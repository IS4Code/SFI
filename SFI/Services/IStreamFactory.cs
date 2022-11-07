using IS4.SFI.Tools;
using System;
using System.IO;

namespace IS4.SFI.Services
{
    /// <summary>
    /// Represents any object that can be opened for reading.
    /// </summary>
    public interface IStreamFactory : IPersistentKey
    {
        /// <summary>
        /// The assumed length of the data; might be different from the actual
        /// length of the stream returned by <see cref="Open"/>.
        /// </summary>
        long Length { get; }

        /// <summary>
        /// The type of access the <see cref="Open"/> method permits.
        /// </summary>
        StreamFactoryAccess Access { get; }

        /// <summary>
        /// Opens a new stream from the object and returns it.
        /// </summary>
        /// <returns>A newly created stream pointing to the beginning of the data.</returns>
        Stream Open();
    }

    /// <summary>
    /// Describes the type of access permitted by <see cref="IStreamFactory.Open"/>.
    /// </summary>
    public enum StreamFactoryAccess
    {
        /// <summary>
        /// The method can be called only once.
        /// </summary>
        Single,

        /// <summary>
        /// The method can be called multiple times, but only after the previous stream
        /// has reached the end.
        /// </summary>
        Reentrant,

        /// <summary>
        /// The method can be called any number of times in parallel.
        /// </summary>
        Parallel
    }

    /// <summary>
    /// Produces new instances of <see cref="MemoryStream"/> from an array buffer.
    /// </summary>
    public sealed class MemoryStreamFactory : IStreamFactory
    {
        readonly ArraySegment<byte> buffer;

        /// <inheritdoc/>
        public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

        /// <inheritdoc/>
        public object? ReferenceKey { get; }

        /// <inheritdoc/>
        public object? DataKey { get; }

        /// <inheritdoc/>
        public long Length => buffer.Count;

        /// <summary>
        /// Creates a new instance of the stream factory from the given buffer.
        /// </summary>
        /// <param name="buffer">The array buffer to use for the stream.</param>
        /// <param name="key">The object to provide the <see cref="IPersistentKey"/> implementation.</param>
        public MemoryStreamFactory(ArraySegment<byte> buffer, IPersistentKey? key) : this(buffer, key?.ReferenceKey, key?.DataKey)
        {

        }

        /// <summary>
        /// Creates a new instance of the stream factory from the given buffer.
        /// </summary>
        /// <param name="buffer">The array buffer to use for the stream.</param>
        /// <param name="referenceKey">The value of <see cref="ReferenceKey"/>.</param>
        /// <param name="dataKey">The value of <see cref="DataKey"/>.</param>
        public MemoryStreamFactory(ArraySegment<byte> buffer, object? referenceKey, object? dataKey)
        {
            this.buffer = buffer;
            ReferenceKey = referenceKey;
            DataKey = dataKey;
        }

        /// <summary>
        /// Opens the stored array buffer as a read-only instance of <see cref="MemoryStream"/>.
        /// </summary>
        /// <returns>The stream pointing to the stored array buffer.</returns>
        public MemoryStream Open()
        {
            return buffer.AsStream(false);
        }

        Stream IStreamFactory.Open()
        {
            return Open();
        }
    }
}
