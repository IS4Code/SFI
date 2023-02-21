using System;

namespace IS4.SFI.Services
{
    /// <summary>
    /// Factory to create new instances of <see cref="ILinkedNode"/>.
    /// </summary>
    public interface ILinkedNodeFactory
    {
        /// <summary>
        /// The default root of linked nodes, as a formatter using a unique identifier
        /// of the resource.
        /// </summary>
        IIndividualUriFormatter<string> Root { get; }

        /// <summary>
        /// Creates a new linked node from a formatted value.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="value"/>.</typeparam>
        /// <param name="formatter">The formatter to use.</param>
        /// <param name="value">The value to format using <paramref name="formatter"/>.</param>
        /// <returns>A new linked node representing the formatted value.</returns>
        ILinkedNode Create<T>(IIndividualUriFormatter<T> formatter, T value);

        /// <summary>
        /// Determines whether <paramref name="str"/> is safe for storing in a
        /// literal node directly.
        /// </summary>
        /// <param name="str">The string value to check.</param>
        /// <returns>
        /// <see langword="true"/> if the string is safe to use as a literal, <see langword="false"/> if it must be escaped or discarded.
        /// </returns>
        /// <remarks>
        /// A literal can be escaped with methods like <see cref="DataTools.CreateLiteralJsonLd(string)"/>.
        /// </remarks>
        bool IsSafeLiteral(string str);

        /// <summary>
        /// Determines whether <paramref name="uri"/> is safe for using it as a
        /// predicate, for compatibility with formats such as RDF/XML.
        /// </summary>
        /// <param name="uri">The URI value to check.</param>
        /// <returns>
        /// <see langword="true"/> if the URI is safe to use as a predicate, <see langword="false"/> if it must be escaped or discarded.
        /// </returns>
        /// <remarks>
        /// A predicate URI may be encoded using methods like <see cref="UriTools.UriToUuidUri(Uri)"/>.
        /// </remarks>
        bool IsSafePredicate(Uri uri);
    }

    /// <summary>
    /// Additional extension methods for <see cref="ILinkedNodeFactory"/>.
    /// </summary>
    public static class LinkedNodeFactoryExtensions
    {
        /// <summary>
        /// Creates a unique node from a newly-generated UUID using
        /// the <see cref="ILinkedNodeFactory.Root"/>.
        /// </summary>
        /// <param name="factory">The factory instance to use.</param>
        /// <returns>The node for a newly created resource.</returns>
        public static ILinkedNode CreateUnique(this ILinkedNodeFactory factory)
        {
            return factory.Create(factory.Root, Guid.NewGuid().ToString("D")) ?? throw new NotSupportedException();
        }

        /// <summary>
        /// Creates a new unique linked blank node.
        /// </summary>
        /// <param name="factory">The factory instance to use.</param>
        /// <returns>A new linked node corresponding to a blank node.</returns>
        public static ILinkedNode CreateBlank(this ILinkedNodeFactory factory)
        {
            return factory.Create(NullFormatter.Instance, default);
        }

        class NullFormatter : IIndividualUriFormatter<ValueTuple>
        {
            public static readonly NullFormatter Instance = new();

            Uri? IUriFormatter<ValueTuple>.this[ValueTuple value] => null;
        }
    }
}
