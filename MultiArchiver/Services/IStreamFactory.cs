using IS4.MultiArchiver.Tools;
using System;
using System.IO;

namespace IS4.MultiArchiver.Services
{
    public interface IStreamFactory : IPersistentKey
    {
        long Length { get; }
        StreamFactoryAccess Access { get; }
        Stream Open();
    }

    public enum StreamFactoryAccess
    {
        Single,
        Reentrant,
        Parallel
    }

    public sealed class MemoryStreamFactory : IStreamFactory
    {
        readonly ArraySegment<byte> buffer;

        public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

        public object ReferenceKey { get; }

        public object DataKey { get; }

        public long Length => buffer.Count;

        public MemoryStreamFactory(ArraySegment<byte> buffer, IPersistentKey key) : this(buffer, key.ReferenceKey, key.DataKey)
        {

        }

        public MemoryStreamFactory(ArraySegment<byte> buffer, object referenceKey, object dataKey)
        {
            this.buffer = buffer;
            ReferenceKey = referenceKey;
            DataKey = dataKey;
        }

        public Stream Open()
        {
            return buffer.AsStream(false);
        }
    }
}
