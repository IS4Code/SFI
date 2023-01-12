using IS4.SFI.Application;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IS4.SFI
{
    /// <summary>
    /// An implementation of <see cref="Inspector"/> allowing automatic management
    /// of component collections.
    /// </summary>
    public class ComponentInspector : Inspector
    {
        readonly HashSet<ComponentCollection> componentCollections = new HashSet<ComponentCollection>();

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
        protected void CaptureCollections(object instance, bool updateExisting = false)
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
    }
}
