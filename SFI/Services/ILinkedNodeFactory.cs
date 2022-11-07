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
        /// True if the string is safe to use, false if it must be escaped or discarded.
        /// </returns>
        bool IsSafeString(string str);
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
    }
}
