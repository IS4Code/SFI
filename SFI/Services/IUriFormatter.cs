using System;

namespace IS4.SFI.Services
{
    /// <summary>
    /// Allows production of URIs from values of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of values used by the formatter.</typeparam>
    public interface IUriFormatter<in T>
    {
        /// <summary>
        /// Creates a new URI based on <paramref name="value"/> in a way specific
        /// to the implementation.
        /// </summary>
        /// <param name="value">The value serving as the basis for the URI.</param>
        /// <returns>A new URI incorporating <paramref name="value"/>.</returns>
        Uri? this[T value] { get; }
    }

    /// <summary>
    /// Allows production of URIs from values with the intention of using
    /// them as individuals in RDF.
    /// </summary>
    /// <typeparam name="T">The type of values used by the formatter.</typeparam>
    public interface IIndividualUriFormatter<in T> : IUriFormatter<T>
    {

    }

    /// <summary>
    /// Allows production of URIs from values with the intention of using
    /// them as properties in RDF.
    /// </summary>
    /// <typeparam name="T">The type of values used by the formatter.</typeparam>
    public interface IPropertyUriFormatter<in T> : IUriFormatter<T>
    {

    }

    /// <summary>
    /// Allows production of URIs from values with the intention of using
    /// them as classes in RDF.
    /// </summary>
    /// <typeparam name="T">The type of values used by the formatter.</typeparam>
    public interface IClassUriFormatter<in T> : IUriFormatter<T>
    {

    }

    /// <summary>
    /// Allows production of URIs from values with the intention of using
    /// them as individuals, properties, or classes in RDF.
    /// </summary>
    /// <typeparam name="T">The type of values used by the formatter.</typeparam>
    public interface IGenericUriFormatter<in T> : IIndividualUriFormatter<T>, IPropertyUriFormatter<T>, IClassUriFormatter<T>
    {

    }

    /// <summary>
    /// Allows production of URIs from values with the intention of using
    /// them as datatypes in RDF.
    /// </summary>
    /// <typeparam name="T">The type of values used by the formatter.</typeparam>
    public interface IDatatypeUriFormatter<in T> : IUriFormatter<T>
    {

    }

    /// <summary>
    /// Allows production of URIs from values with the intention of using
    /// them as graphs in RDF.
    /// </summary>
    /// <typeparam name="T">The type of values used by the formatter.</typeparam>
    public interface IGraphUriFormatter<in T> : IUriFormatter<T>
    {

    }

    /// <summary>
    /// Allows production of URIs from values with the intention of using
    /// them as individuals, properties, classes, datatypes, or graphs in RDF.
    /// </summary>
    /// <typeparam name="T">The type of values used by the formatter.</typeparam>
    public interface IUniversalUriFormatter<in T> : IGenericUriFormatter<T>, IDatatypeUriFormatter<T>, IGraphUriFormatter<T>
    {

    }

    /// <summary>
    /// A simple implementation of <see cref="IUriFormatter{T}"/> that produces
    /// URIs from <see cref="String"/> or just returning the input <see cref="Uri"/>.
    /// </summary>
    public sealed class UriFormatter :
        IUniversalUriFormatter<string>,
        IUniversalUriFormatter<Uri>
    {
        /// <summary>
        /// The singleton instance of the formatter.
        /// </summary>
        public static readonly UriFormatter Instance = new();

        private UriFormatter()
        {

        }

        /// <summary>
        /// Creates a new absolute URI from <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The absolute URI string.</param>
        /// <returns>The created URI.</returns>
        public Uri this[string value] => new(value, UriKind.Absolute);

        /// <summary>
        /// Returns <paramref name="value"/> unchanged.
        /// </summary>
        /// <param name="value">The URI instance to use.</param>
        /// <returns><paramref name="value"/></returns>
        public Uri this[Uri value] => value;
    }
}
