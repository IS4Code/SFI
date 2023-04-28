using IS4.SFI.Application;
using Microsoft.Extensions.DependencyInjection;
using MorseCode.ITask;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IS4.SFI
{
    /// <summary>
    /// An implementation of <see cref="Inspector"/> allowing automatic management
    /// of component collections.
    /// </summary>
    public class ComponentInspector : Inspector, IResultFactory<object?, IEnumerable>
    {
        readonly HashSet<ComponentCollection> componentCollections = new();

        /// <summary>
        /// Stores all component collections managed by the instance.
        /// </summary>
        public IReadOnlyCollection<ComponentCollection> ComponentCollections => componentCollections;

        /// <inheritdoc/>
        public ComponentInspector()
        {
            CaptureCollections(this);
        }

        /// <inheritdoc/>
        public async override ValueTask AddDefault()
        {
            await base.AddDefault();

            CaptureCollections(this, true);
        }

        static readonly Type enumerableType = typeof(IEnumerable);
        static readonly Type collectionType = typeof(ICollection<>);

        /// <summary>
        /// Browses an object, looking for all properties with <see cref="ComponentCollectionAttribute"/>
        /// and loading the component collections within.
        /// </summary>
        /// <param name="instance">The instance to browse.</param>
        /// <param name="updateExisting">Whether to update previously found collections.</param>
        protected virtual void CaptureCollections(object instance, bool updateExisting = false)
        {
            foreach(System.ComponentModel.PropertyDescriptor property in System.ComponentModel.TypeDescriptor.GetProperties(instance))
            {
                if(property.IsBrowsable && enumerableType.IsAssignableFrom(property.PropertyType))
                {
                    var attribute = property.Attributes.OfType<ComponentCollectionAttribute>().FirstOrDefault();
                    if(attribute != null)
                    {
                        var collection = (IEnumerable)property.GetValue(instance);
                        if(collection != null)
                        {
                            var type = collection.GetType().GetInterfaces().FirstOrDefault(i => i.IsGenericType && collectionType.Equals(i.GetGenericTypeDefinition()));
                            if(type != null)
                            {
                                var itemType = type.GetGenericArguments()[0];
                                var components = ComponentCollection.Create(itemType, collection, attribute);
                                if(componentCollections.Add(components) || updateExisting)
                                {
                                    foreach(var element in collection)
                                    {
                                        CaptureCollections(element);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds the given <see cref="ComponentType"/> instance into the existing collections.
        /// </summary>
        /// <param name="component">The component to add.</param>
        /// <returns>The number of individual component items that were added.</returns>
        protected virtual async ValueTask<int> LoadIntoCollections(ComponentType component)
        {
            int componentCount = 0;

            foreach(var collection in ComponentCollections.ToArray())
            {
                await foreach(var instance in collection.CreateInstance(component, this, collection.Collection))
                {
                    if(instance != null)
                    {
                        componentCount++;
                        CaptureCollections(instance);
                    }
                }
            }

            return componentCount;
        }

        async ITask<object?> IResultFactory<object?, IEnumerable>.Invoke<T>(T value, IEnumerable sequence)
        {
            if(sequence is ICollection<T> collection)
            {
                collection.Add(value);
                return value;
            }
            return null;
        }

        /// <summary>
        /// Retrieves all types in an assembly that could be loaded as components.
        /// </summary>
        /// <param name="assembly">The assembly to browse.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> instance that is used for initializing the components.</param>
        /// <returns>The collection of all applicable component types.</returns>
        protected virtual IEnumerable<ComponentType> OpenAssembly(Assembly assembly, IServiceProvider serviceProvider)
        {
            foreach(var type in assembly.ExportedTypes)
            {
                // Only yield concrete instantiable browsable types
                if(!type.IsAbstract && !type.IsGenericTypeDefinition && IsTypeBrowsable(type))
                {
                    yield return new ComponentType(type, () => ActivatorUtilities.CreateInstance(serviceProvider, type), this);
                }
            }
        }

        /// <summary>
        /// Loads all components in an assembly.
        /// </summary>
        /// <param name="assembly">The assembly to load from.</param>
        /// <returns>The number of individual component items that were added.</returns>
        protected virtual async ValueTask<int> LoadAssembly(Assembly assembly)
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            int count = 0;

            foreach(var component in OpenAssembly(assembly, serviceProvider))
            {
                count += await LoadIntoCollections(component);
            }

            return count;
        }

        static readonly Type BrowsableAttributeType = typeof(System.ComponentModel.BrowsableAttribute);

        static bool IsTypeBrowsable(Type type)
        {
            return
                (System.ComponentModel.TypeDescriptor.GetAttributes(type)[BrowsableAttributeType] as System.ComponentModel.BrowsableAttribute)
                ?.Browsable ?? true;
        }
    }
}
