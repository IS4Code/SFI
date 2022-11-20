using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Services
{
    /// <summary>
    /// Produces a file as a by-product of an operation.
    /// </summary>
    /// <param name="isBinary">Whether the file is binary or textual.</param>
    /// <param name="properties">Additional user-defined properties of the file.</param>
    /// <param name="writer">If the file is opened, a stream is created and provided to this function to write the file.</param>
    /// <returns>The result of <paramref name="writer"/>.</returns>
    public delegate ValueTask OutputFileDelegate(bool isBinary, INodeMatchProperties properties, Func<Stream, ValueTask> writer);

    /// <summary>
    /// Allows extraction of arbitrary files during the operations
    /// performed by this instance.
    /// </summary>
    public interface IHasFileOutput
    {
        /// <summary>
        /// This event is executed when a file could be produced.
        /// </summary>
        event OutputFileDelegate OutputFile;
    }

    /// <summary>
    /// Supports description of arbitrary entites through an instance of <see cref="OutputFileDelegate"/>.
    /// </summary>
    /// <typeparam name="T">The supported entity type.</typeparam>
    public interface IEntityOutputProvider<T> where T : class
    {
        /// <summary>
        /// If <paramref name="entity"/> can be described, invokes <paramref name="output"/>,
        /// providing data related to the entity.
        /// </summary>
        /// <param name="entity">The entity to describe.</param>
        /// <param name="output">The instance of <see cref="OutputFileDelegate"/> for storing the data.</param>
        /// <param name="properties">Additional properties passed to <paramref name="output"/>.</param>
        /// <returns>Whether the entity was recognized.</returns>
        ValueTask<bool> DescribeEntity(T entity, OutputFileDelegate? output, INodeMatchProperties properties);
    }
}
