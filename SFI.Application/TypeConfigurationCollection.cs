using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace IS4.SFI.Application
{
    /// <summary>
    /// Provides an implementation of <see cref="ComponentCollection{T}"/> of
    /// <see cref="Configuration"/> instances, storing configurable properties
    /// for instances of arbitrary types.
    /// </summary>
    public class TypeConfigurationCollection : ComponentCollection<TypeConfigurationCollection.Configuration>
    {
        readonly Dictionary<Type, Configuration> typeMap;

        /// <summary>
        /// The dictionary used to retrieve a particular instance of <see cref="Configuration"/>
        /// for a type.
        /// </summary>
        public IReadOnlyDictionary<Type, Configuration> TypeMap => typeMap;

        /// <summary>
        /// Creates a new instance of the collection using a custom prefix.
        /// </summary>
        /// <param name="prefix">The prefix to assign to all components inside the collection.</param>
        public TypeConfigurationCollection(string prefix) : base(GetCollection(out var typeMap), new(prefix))
        {
            this.typeMap = typeMap;
        }

        /// <summary>
        /// Adds a new configuration for a type.
        /// </summary>
        /// <param name="type">The type to configure.</param>
        /// <param name="identifier">The desired identifier of the component.</param>
        public void AddType(Type type, string? identifier = null)
        {
            if(!typeMap.ContainsKey(type))
            {
                typeMap.Add(type, new(type, identifier));
            }
        }

        static ICollection<Configuration> GetCollection(out Dictionary<Type, Configuration> typeMap)
        {
            typeMap = new();
            return typeMap.Values;
        }

        /// <summary>
        /// Holds configuration properties for an instance of <see cref="Type"/>.
        /// </summary>
        [TypeDescriptionProvider(typeof(ConfigurationDescriptionProvider))]
        public class Configuration : ICustomTypeDescriptor
        {
            /// <summary>
            /// The type to configure.
            /// </summary>
            public Type Type { get; }

            /// <summary>
            /// The identifier of the component.
            /// </summary>
            public string Identifier { get; }

            readonly Dictionary<PropertyDescriptor, object?> properties = new();

            /// <summary>
            /// The configured properties to assign for new instances of <see cref="Type"/>.
            /// </summary>
            public IReadOnlyDictionary<PropertyDescriptor, object?> Properties => properties;

            readonly Type[] interfaces;

            /// <summary>
            /// Creates a new instance of the component.
            /// </summary>
            /// <param name="type">The value of <see cref="Type"/>.</param>
            /// <param name="identifier">The value of <see cref="Identifier"/>.</param>
            public Configuration(Type type, string? identifier = null)
            {
                Type = type;
                interfaces = type.GetInterfaces();
                Identifier = identifier ?? TextTools.GetIdentifierFromType(Type);
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                return Identifier;
            }
            
            AttributeCollection ICustomTypeDescriptor.GetAttributes()
            {
                return TypeDescriptor.GetAttributes(Type);
            }

            string ICustomTypeDescriptor.GetClassName()
            {
                return TypeDescriptor.GetClassName(Type);
            }

            string ICustomTypeDescriptor.GetComponentName()
            {
                return TypeDescriptor.GetComponentName(Type);
            }

            TypeConverter ICustomTypeDescriptor.GetConverter()
            {
                return TypeDescriptor.GetConverter(Type);
            }

            EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
            {
                return TypeDescriptor.GetDefaultEvent(Type);
            }

            PropertyDescriptor? ICustomTypeDescriptor.GetDefaultProperty()
            {
                return null;
            }

            object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
            {
                return TypeDescriptor.GetEditor(Type, editorBaseType);
            }

            EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
            {
                return EventDescriptorCollection.Empty;
            }

            EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
            {
                return EventDescriptorCollection.Empty;
            }

            PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
            {
                var properties = TypeDescriptor.GetProperties(Type);
                return TransformProperties(properties);
            }

            PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
            {
                var properties = TypeDescriptor.GetProperties(Type, attributes);
                return TransformProperties(properties);
            }

            PropertyDescriptorCollection TransformProperties(PropertyDescriptorCollection properties)
            {
                var array = new PropertyDescriptor[properties.Count];
                properties.CopyTo(array, 0);
                for(int i = 0; i < array.Length; i++)
                {
                    array[i] = new ProxyDescriptor(array[i]);
                }
                return new(array);
            }

            object? ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
            {
                return null;
            }

            class ProxyDescriptor : PropertyDescriptor
            {
                readonly PropertyDescriptor inner;

                public ProxyDescriptor(PropertyDescriptor inner) : base(inner)
                {
                    this.inner = inner;
                }

                public override Type ComponentType => inner.ComponentType;

                public override bool IsReadOnly => inner.IsReadOnly;

                public override Type PropertyType => inner.PropertyType;

                public override TypeConverter Converter => inner.Converter;

                public override bool CanResetValue(object component)
                {
                    return component is Configuration || inner.CanResetValue(null);
                }

                public override object? GetValue(object component)
                {
                    if(component is Configuration config)
                    {
                        return config.Properties.TryGetValue(inner, out var value) ? value : null;
                    }
                    return inner.GetValue(component);
                }

                public override void ResetValue(object component)
                {
                    if(component is Configuration config)
                    {
                        config.properties.Remove(inner);
                        return;
                    }
                    inner.ResetValue(component);
                }

                public override void SetValue(object component, object value)
                {
                    if(component is Configuration config)
                    {
                        config.properties[inner] = value;
                        return;
                    }
                    inner.SetValue(component, value);
                }

                public override bool ShouldSerializeValue(object component)
                {
                    return inner.ShouldSerializeValue(null);
                }
            }
        }

        class ConfigurationDescriptionProvider : TypeDescriptionProvider
        {
            public override bool IsSupportedType(Type type)
            {
                return typeof(Configuration).Equals(type);
            }

            public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
            {
                if(instance is Configuration config)
                {
                    return config;
                }
                return base.GetTypeDescriptor(objectType, instance);
            }

            public override Type GetReflectionType(Type objectType, object instance)
            {
                if(instance is Configuration config)
                {
                    return config.Type;
                }
                return base.GetReflectionType(objectType, instance);
            }
        }
    }
}
