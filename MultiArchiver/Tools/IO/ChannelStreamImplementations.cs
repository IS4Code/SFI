using System;
using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace IS4.MultiArchiver.Tools.IO
{
    public sealed class ChannelArrayStream : ChannelStream<ArraySegment<byte>>
    {
        public ChannelArrayStream(ChannelReader<ArraySegment<byte>> channelReader) : base(channelReader)
        {

        }

        public static ChannelArrayStream Create(out ChannelWriter<ArraySegment<byte>> writer, int? capacity = null)
        {
            return new ChannelArrayStream(CreateReader(out writer, capacity));
        }

        protected override void ReadFrom(ref ArraySegment<byte> current, byte[] buffer, int offset, int len)
        {
            current.Slice(0, len).CopyTo(buffer, offset);
            current = current.Slice(len);
        }

        protected override byte ReadFrom(ref ArraySegment<byte> current)
        {
            var result = current.Array[current.Offset];
            current = current.Slice(1);
            return result;
        }
    }
    
    public sealed class ChannelMemoryStream : ChannelStream<UnmanagedMemoryRange>
    {
        public ChannelMemoryStream(ChannelReader<UnmanagedMemoryRange> channelReader) : base(channelReader)
        {

        }

        public static ChannelMemoryStream Create(out ChannelWriter<UnmanagedMemoryRange> writer, int? capacity = null)
        {
            return new ChannelMemoryStream(CreateReader(out writer, capacity));
        }
        
        protected override void ReadFrom(ref UnmanagedMemoryRange current, byte[] buffer, int offset, int len)
        {
            Marshal.Copy(current.Address, buffer, offset, len);
            current = new UnmanagedMemoryRange(current.Address + len, current.Count - len);
        }

        protected override byte ReadFrom(ref UnmanagedMemoryRange current)
        {
            var result = Marshal.ReadByte(current.Address);
            current = new UnmanagedMemoryRange(current.Address + 1, current.Count - 1);
            return result;
        }
    }
}
