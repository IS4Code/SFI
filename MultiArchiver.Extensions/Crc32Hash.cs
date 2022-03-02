using Force.Crc32;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver
{
    public class Crc32Hash : DataHashAlgorithm
    {
        public static readonly Crc32Hash Instance = new Crc32Hash();

        public Crc32Hash() : base(Individuals.Crc32, 4, "urn:crc32:", FormattingMethod.Hex)
        {

        }

        public override async ValueTask<byte[]> ComputeHash(Stream input, IPersistentKey key = null)
        {
            uint aggregate = 0;
            if(input is IEnumerator<ArraySegment<byte>> collection)
            {
                while(collection.MoveNext())
                {
                    var segment = collection.Current;
                    if(segment.Count > 0)
                    {
                        aggregate = Crc32Algorithm.Append(aggregate, segment.Array, segment.Offset, segment.Count);
                    }
                }
            }else{
                var buffer = ArrayPool<byte>.Shared.Rent(16384);
                try{
                    int read;
                    while((read = await input.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        aggregate = Crc32Algorithm.Append(aggregate, buffer, 0, read);
                    }
                }finally{
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
            return BitConverter.GetBytes(aggregate);
        }

        public override ValueTask<byte[]> ComputeHash(byte[] data, IPersistentKey key = null)
        {
            return new ValueTask<byte[]>(BitConverter.GetBytes(Crc32Algorithm.Compute(data)));
        }

        public override ValueTask<byte[]> ComputeHash(byte[] data, int offset, int count, IPersistentKey key = null)
        {
            return new ValueTask<byte[]>(BitConverter.GetBytes(Crc32Algorithm.Compute(data, offset, count)));
        }
    }
}
