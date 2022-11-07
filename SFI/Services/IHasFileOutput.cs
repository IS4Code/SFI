using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Services
{
    /// <summary>
    /// Produces a file as a by-product of an operation.
    /// </summary>
    /// <param name="name">The name of the file.</param>
    /// <param name="isBinary">Whether the file is binary or textual.</param>
    /// <param name="properties">Additional user-defined properties of the file.</param>
    /// <param name="writer">If the file is opened, a stream is created and provided to this function to write the file.</param>
    /// <returns>The result of <paramref name="writer"/>.</returns>
    public delegate ValueTask OutputFileDelegate(string? name, bool isBinary, IReadOnlyDictionary<string, object>? properties, Func<Stream, ValueTask> writer);

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
}
