using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace IS4.SFI.Services
{
    /// <summary>
    /// Contains properties relevant for saving a file as a result of <see cref="ILinkedNode.Match(out INodeMatchProperties)"/>,
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
            return TypeDescriptor.GetProperties(properties).Cast<PropertyDescriptor>().Select(p => new KeyValuePair<string, PropertyDescriptor>(TextTools.FormatMimeName(p.Name).Replace("-", "_"), p));
        }
    }
}
