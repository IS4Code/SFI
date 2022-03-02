using Blake3;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver
{
    public class Blake3Hash : DataHashAlgorithm
    {
        public static readonly Blake3Hash Instance = new Blake3Hash();

        public override int? NumericIdentifier => 0x1e;

        public Blake3Hash() : base(Individuals.Blake3, 32, "urn:blake3:", FormattingMethod.Base32)
        {

        }

        public override async ValueTask<byte[]> ComputeHash(Stream input, IPersistentKey key = null)
        {
            using(var hasher = Hasher.New())
            {
                if(input is IEnumerator<ArraySegment<byte>> collection)
                {
                    while(collection.MoveNext())
                    {
                        var segment = collection.Current;
                        if(segment.Count > 0)
                        {
                            hasher.Update(segment.AsSpan());
                        }
                    }
                }else{
                    var buffer = ArrayPool<byte>.Shared.Rent(16384);
                    try{
                        int read;
                        while((read = await input.ReadAsync(buffer, 0, 16384)) != 0)
                        {
                            hasher.Update(new ReadOnlySpan<byte>(buffer, 0, read));
                        }
                    }finally{
                        ArrayPool<byte>.Shared.Return(buffer);
                    }
                }
                var hash = hasher.Finalize();
                return hash.AsSpan().ToArray();
            }
        }

        public override ValueTask<byte[]> ComputeHash(byte[] data, IPersistentKey key = null)
        {
            var hash = Hasher.Hash(new ReadOnlySpan<byte>(data));
            return new ValueTask<byte[]>(hash.AsSpan().ToArray());
        }

        public override ValueTask<byte[]> ComputeHash(byte[] data, int offset, int count, IPersistentKey key = null)
        {
            var hash = Hasher.Hash(new ReadOnlySpan<byte>(data, offset, count));
            return new ValueTask<byte[]>(hash.AsSpan().ToArray());
        }
    }
}
