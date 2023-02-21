using System;

namespace IS4.SFI
{
    /// <summary>
    /// This attribute is applied to properties, indicating that the property
    /// contains a collection of configurable components.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ComponentCollectionAttribute : Attribute
    {
        /// <summary>
        /// The prefix identifying the collection.
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// The common type of the objects in the collection, or <see langword="null"/>
        /// if it can be deduced from the collection.
        /// </summary>
        public Type? CommonType { get; }

        /// <summary>
        /// Creates a new instance of the attribute.
        /// </summary>
        /// <param name="prefix">The value of <see cref="Prefix"/>.</param>
        /// <param name="commonType">The value of <see cref="CommonType"/>.</param>
        public ComponentCollectionAttribute(string prefix, Type? commonType = null)
        {
            Prefix = prefix;
            CommonType = commonType;
        }
    }
}
