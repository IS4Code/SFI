using IS4.SFI.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace IS4.SFI.Services
{
    /// <summary>
    /// Contains properties relevant for saving a file as a result of <see cref="ILinkedNode.Match"/>,
    /// such as through <see cref="OutputFileDelegate"/>. Additional properties can
    /// be retrieved via <see cref="TypeDescriptor.GetProperties(object)"/>.
    /// </summary>
    public interface INodeMatchProperties
    {
        /// <summary>
        /// The extension of the file, indicating the format.
        /// </summary>
        string? Extension { get; set; }

        /// <summary>
        /// The content type of the file, if downloaded.
        /// </summary>
        string? MediaType { get; set; }

        /// <summary>
        /// The size of the file, if known.
        /// </summary>
        long? Size { get; set; }

        /// <summary>
        /// The desired name of the file.
        /// </summary>
        string? Name { get; set; }

        /// <summary>
        /// The argument to <see cref="TextTools.SubstituteVariables(string, IEnumerable{KeyValuePair{string, object}})"/>
        /// to format the final path.
        /// </summary>
        string? PathFormat { get; set; }
    }

    /// <summary>
    /// Extension methods for <see cref="INodeMatchProperties"/>.
    /// </summary>
    public static class NodeMatchPropertiesExtensions
    {
        /// <summary>
        /// Retrieves the collection of all properties as pairs.
        /// </summary>
        /// <param name="properties">The <see cref="INodeMatchProperties"/> instance to use.</param>
        /// <returns>A sequence of pairs storing all properties of the instance.</returns>
        public static IEnumerable<KeyValuePair<string, PropertyDescriptor>> GetProperties(this INodeMatchProperties properties)
        {
            return TypeDescriptor.GetProperties(properties).Cast<PropertyDescriptor>().Where(p => p.IsBrowsable).Select(p => new KeyValuePair<string, PropertyDescriptor>(TextTools.FormatMimeName(p.Name).Replace("-", "_"), p));
        }

        /// <summary>
        /// Retrieves the collection of all properties and their values as pairs.
        /// </summary>
        /// <param name="properties">The <see cref="INodeMatchProperties"/> instance to use.</param>
        /// <returns>A sequence of pairs storing all properties of the instance and their values.</returns>
        public static IEnumerable<KeyValuePair<string, object?>> GetPropertyValues(this INodeMatchProperties properties)
        {
            return GetProperties(properties).Select(p => new KeyValuePair<string, object?>(p.Key, p.Value.GetValue(properties))).OrderBy(p => p.Key);
        }
    }

    /// <summary>
    /// An implementation of <see cref="IEqualityComparer{T}"/> that compares
    /// instances of <see cref="INodeMatchProperties"/> by value.
    /// </summary>
    public class NodeMatchPropertiesComparer : IEqualityComparer<INodeMatchProperties>
    {
        /// <summary>
        /// The default instance of the comparer.
        /// </summary>
        public static readonly NodeMatchPropertiesComparer Instance = new();

        private NodeMatchPropertiesComparer()
        {

        }

        /// <inheritdoc/>
        public bool Equals(INodeMatchProperties x, INodeMatchProperties y)
        {
            var p1 = x.GetPropertyValues();
            var p2 = y.GetPropertyValues();
            return p1.SequenceEqual(p2, KeyValuePairComparer.Instance);
        }

        /// <inheritdoc/>
        public int GetHashCode(INodeMatchProperties obj)
        {
            int hash = 17;
            foreach(var (key, value) in obj.GetPropertyValues())
            {
                hash = unchecked(hash * 23 + key.GetHashCode());
                if(value != null)
                {
                    hash = unchecked(hash * 23 + value.GetHashCode());
                }
            }
            return hash;
        }

        class KeyValuePairComparer : IEqualityComparer<KeyValuePair<string, object?>>
        {
            public static readonly KeyValuePairComparer Instance = new();

            public bool Equals(KeyValuePair<string, object?> x, KeyValuePair<string, object?> y)
            {
                return x.Key == y.Key && Object.Equals(x.Value, y.Value);
            }

            public int GetHashCode(KeyValuePair<string, object?> obj)
            {
                int hash = 17;
                hash = unchecked(hash * 23 + obj.Key.GetHashCode());
                if(obj.Value is object value)
                {
                    hash = unchecked(hash * 23 + value.GetHashCode());
                }
                return hash;
            }
        }
    }
}
