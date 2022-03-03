using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver
{
    public abstract class StreamDataHash<T> : DataHashAlgorithm
    {
        public StreamDataHash(IndividualUri identifier, int hashSize, string prefix, FormattingMethod formatting) : base(identifier, hashSize, prefix, formatting)
        {

        }

        protected abstract T Initialize();

        protected abstract void Finalize(T instance);

        protected abstract void Append(ref T instance, ArraySegment<byte> segment);

        protected abstract byte[] Output(T instance);

        public override async ValueTask<byte[]> ComputeHash(Stream input, IPersistentKey key = null)
        {
            var instance = Initialize();
            try{
                switch(input)
                {
                    case IAsyncEnumerator<ArraySegment<byte>> collection:
                    {
                        while(await collection.MoveNextAsync())
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
                        var buffer = ArrayPool<byte>.Shared.Rent(16384);
                        try{
                            int read;
                            while((read = await input.ReadAsync(buffer, 0, buffer.Length)) != 0)
                            {
                                Append(ref instance, new ArraySegment<byte>(buffer, 0, read));
                            }
                        }finally{
                            ArrayPool<byte>.Shared.Return(buffer);
                        }
                        break;
                    }
                }
                return Output(instance);
            }finally{
                Finalize(instance);
            }
        }
    }
}
