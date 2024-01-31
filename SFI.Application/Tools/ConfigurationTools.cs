using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IS4.SFI.Application.Tools
{
    /// <summary>
    /// Provides method useful for configuration of arbitrary objects.
    /// </summary>
    public static class ConfigurationTools
    {
        /// <summary>
        /// Registers additional <see cref="TypeDescriptionProvider"/>
        /// instances.
        /// </summary>
        /// <remarks>
        /// At the moment, the only new provider is for
        /// <see cref="Encoding"/> which adds support
        /// for conversions between <see cref="string"/>.
        /// </remarks>
        public static void RegisterCustomDescriptors()
        {
            var provider = TypeDescriptor.GetProvider(typeof(Encoding));
            TypeDescriptor.AddProvider(new EncodingTypeDescriptionProvider(provider), typeof(Encoding));
        }

        /// <summary>
        /// Returns the collection of all properties on a component
        /// that can be configured from the command line.
        /// </summary>
        /// <remarks>
        /// Configurable properties are those properties that can be set (not read-only),
        /// which do not have <c>[<see cref="BrowsableAttribute"/>(<see langword="false"/>)]</c>, and their type
        /// can be converted to and from <see cref="string"/>.
        /// </remarks>
        public static IEnumerable<PropertyDescriptor> GetConfigurableProperties(object component)
        {
            return TypeDescriptor.GetProperties(component).Cast<PropertyDescriptor>().Where(
                p =>
                    !p.IsReadOnly &&
                    p.IsBrowsable &&
                    IsStringConvertible(p.Converter)
            );
        }

        static readonly Type stringType = typeof(string);

        /// <summary>
        /// Checks whether <paramref name="converter"/> can be used to convert to and from
        /// <see cref="string"/>.
        /// </summary>
        /// <param name="converter">The converter to check.</param>
        /// <returns><see langword="true"/> if the conversion is permitted, <see langword="false"/> otherwise.</returns>
        public static bool IsStringConvertible(TypeConverter converter)
        {
            if(converter != null)
            {
                return converter.CanConvertFrom(stringType) && converter.CanConvertTo(stringType);
            }
            return false;
        }

        static readonly Type obsoleteType = typeof(ObsoleteAttribute);

        /// <summary>
        /// Checks whether <paramref name="member"/> is obsolete.
        /// </summary>
        /// <param name="member">The member to check.</param>
        /// <param name="attribute">The variable to receive the attribute instance.</param>
        /// <returns><see langword="true"/> is marked as obsolete, <see langword="false"/> otherwise.</returns>
        public static bool IsObsolete(this MemberDescriptor member, [MaybeNullWhen(false)] out ObsoleteAttribute attribute)
        {
            attribute = member.Attributes[obsoleteType] as ObsoleteAttribute;
            return attribute != null;
        }

        /// <summary>
        /// Assigns the values of properties on a component.
        /// </summary>
        /// <param name="component">The component to assign to.</param>
        /// <param name="componentName">The name of the component, for diagnostics.</param>
        /// <param name="valueMap">The function that maps property names to their values.</param>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        public static void SetProperties(object component, string componentName, Func<string, string?> valueMap, ILogger? logger)
        {
            var batch = component as ISupportInitialize;
            batch?.BeginInit();

            try{ 
                foreach(var prop in GetConfigurableProperties(component))
                {
				    var name = TextTools.FormatComponentName(prop.Name);
				    if(valueMap(name) is string value)
				    {
                        if(prop.IsObsolete(out var info))
                        {
                            logger?.Log(info.IsError ? LogLevel.Error : LogLevel.Warning, $"Property {componentName}:{name} is obsolete: " + info.Message);
                        }
					    // This property is given a value
					    var converter = prop.Converter;
					    object? convertedValue = null;
					    Exception? conversionException = null;
					    try{
						    convertedValue = converter.ConvertFromInvariantString(value);
                        }catch(Exception e)
                        {
						    // Conversion failed (for any reason)
						    conversionException = e;
					    }
					    if(convertedValue == null && !(String.IsNullOrEmpty(value) && conversionException == null))
                        {
						    throw new ApplicationException($"Cannot convert value '{value}' for property {componentName}:{name} to type {TextTools.GetIdentifierFromType(prop.PropertyType)}!", conversionException);
                        }
                        try{
						    prop.SetValue(component, convertedValue);
					    }catch(Exception e)
					    {
						    throw new ApplicationException($"Cannot assign value '{value}' to property {componentName}:{name}: {e.Message}", e);
					    }
				    }
                }
            }finally{ 
			    batch?.EndInit();
            }
        }

        /// <summary>
        /// Retrieves the collection of standard values for a simple type.
        /// </summary>
        /// <param name="type">The type to retrieve the values of.</param>
        /// <param name="converter">The type's converter to retrieve the values from.</param>
        /// <param name="standardValues">The variable to assign the result in case of success.</param>
        /// <returns>Whether any standard values were retrieved.</returns>
        public static bool GetStandardValues(Type type, TypeConverter converter, out ICollection standardValues)
        {
            if(!type.IsPrimitive && (type.IsEnum || Type.GetTypeCode(type) == TypeCode.Object))
            {
                if(converter.GetStandardValuesSupported() && converter.GetStandardValues() is { Count: > 0 } values)
                {
                    standardValues = values;
                    return true;
                }
            }
            standardValues = Array.Empty<object>();
            return false;
        }

        static readonly Dictionary<Type, string> primaryProperties = new()
        {
            { typeof(ObsoleteAttribute), nameof(ObsoleteAttribute.Message) }
        };

        /// <summary>
        /// Retrieves a collection of key-value pairs for each textual attribute from <paramref name="attributes"/>.
        /// </summary>
        /// <param name="attributes">The attribute collection to browse.</param>
        /// <returns>A sequence of key-value pairs with the attributes.</returns>
        public static IEnumerable<KeyValuePair<string, string>> GetTextAttributes(IEnumerable<Attribute> attributes)
        {
			foreach(var attr in attributes)
			{
				const string attributeSuffix = nameof(Attribute);

				var attrType = attr.GetType();
				var attrName = attrType.Name;
                if(!attrName.EndsWith(attributeSuffix))
				{
					// Non-standard name
					continue;
                }
                attrName = attrName.Substring(0, attrName.Length - attributeSuffix.Length);

                var mainProperty = attrType.GetProperty(attrName, BindingFlags.Public | BindingFlags.Instance);
				if(mainProperty == null || !stringType.Equals(mainProperty.PropertyType))
				{
                    if(!primaryProperties.TryGetValue(attrType, out var mainPropertyName) || (mainProperty = attrType.GetProperty(mainPropertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) == null)
                    {
					    var properties = attrType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => stringType.Equals(p.PropertyType)).Take(2).ToList();
					    if(properties.Count != 1)
                        {
						    // Can't find single string property
                            continue;
                        }
					    mainProperty = properties[0];
					    if(!attrName.Contains(mainProperty.Name))
					    {
						    // Name is not similar enough
						    continue;
					    }
                    }
				}

				var value = mainProperty.GetValue(attr) as string;
				if(!String.IsNullOrWhiteSpace(value))
				{
					var newline = value!.IndexOf('\n');
					if(newline != -1)
					{
						// Strip after newline character
						value = value.Substring(0, newline);
						if(String.IsNullOrWhiteSpace(value))
						{
							continue;
						}
                    }
                    value = value.Trim();
                    yield return new(mainProperty.Name, value);
				}
			}
        }
    }
}
