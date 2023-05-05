using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Tools
{
    /// <summary>
    /// Represents an incremental hash algorithm that uses a value of
    /// type <typeparamref name="T"/> to build the hash.
    /// </summary>
    /// <typeparam name="T">The type of the aggregation value.</typeparam>
    public abstract class StreamDataHash<T> : DataHashAlgorithm
    {
        /// <inheritdoc/>
        public StreamDataHash(IndividualUri identifier, int hashSize, string prefix, FormattingMethod formatting) : base(identifier, hashSize, prefix, formatting)
        {

        }

        /// <summary>
        /// Creates a new instance of <typeparamref name="T"/> and
        /// initializes it for new hashing.
        /// </summary>
        /// <returns>A newly initialized instance of <typeparamref name="T"/>.</returns>
        protected abstract T Initialize();

        /// <summary>
        /// Performs cleanup on an instance of <typeparamref name="T"/>
        /// when it is no longer required.
        /// </summary>
        /// <param name="instance">The variable storing the instance to dispose.</param>
        protected abstract void Finalize(ref T instance);

        /// <summary>
        /// Appends new data to an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="instance">The variable storing the instance to use.</param>
        /// <param name="segment">The new data to append.</param>
        protected abstract void Append(ref T instance, ArraySegment<byte> segment);

        /// <summary>
        /// Retrieves the hash as a byte array from an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="instance">The variable storing the instance to use.</param>
        /// <returns>The hash stored in the instance.</returns>
        protected abstract byte[] Output(ref T instance);

        /// <inheritdoc/>
        public async override ValueTask<byte[]> ComputeHash(Stream input, IIdentityKey? key = null)
        {
            var instance = Initialize();
            try{
                switch(input)
                {
                    case IAsyncEnumerator<ArraySegment<byte>> collection:
                    {
                        while(await collection.MoveNextAsync().ConfigureAwait(false))
                        {
                            var segment = collection.Current;
                            if(segment.Count > 0)
                            {
                                Append(ref instance, segment);
                            }
                        }
                        break;
                    }
                    case IEnumerator<ArraySegment<byte>> collection:
                    {
                        while(collection.MoveNext())
                        {
                            var segment = collection.Current;
                            if(segment.Count > 0)
                            {
                                Append(ref instance, segment);
                            }
                        }
                        break;
                    }
                    default:
                    {
                        using var bufferLease = ArrayPool<byte>.Shared.Rent(16384, out var buffer);
                        int read;
                        while((read = await input.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        {
                            Append(ref instance, new ArraySegment<byte>(buffer, 0, read));
                        }
                        break;
                    }
                }
                return Output(ref instance);
            }finally{
                Finalize(ref instance);
            }
        }
    }
}
