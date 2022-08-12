using System;
using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace IS4.MultiArchiver.Tools.IO
{
    /// <summary>
    /// An implementation of <see cref="ChannelStream{TSequence}"/> for an
    /// instance of <see cref="ArraySegment{T}"/> of <see cref="Byte"/>.
    /// </summary>
    public sealed class ChannelArrayStream : ChannelStream<ArraySegment<byte>>
    {
        /// <summary>
        /// Creates a new stream instance from a channel reader.
        /// </summary>
        /// <param name="channelReader">
        /// The reader for the channel providing the byte sequences to read.
        /// </param>
        public ChannelArrayStream(ChannelReader<ArraySegment<byte>> channelReader) : base(channelReader)
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="ChannelArrayStream"/> and retrieves
        /// the writer to its underlying channel.
        /// </summary>
        /// <param name="writer">The variable to receive the writer for the created channel.</param>
        /// <param name="capacity">The capacity of the channel, if it should be bounded.</param>
        /// <returns>The stream to read from the channel.</returns>
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

    /// <summary>
    /// An implementation of <see cref="ChannelStream{TSequence}"/> for an
    /// instance of <see cref="UnmanagedMemoryRange"/>.
    /// </summary>
    public sealed class ChannelMemoryStream : ChannelStream<UnmanagedMemoryRange>
    {
        /// <summary>
        /// Creates a new stream instance from a channel reader.
        /// </summary>
        /// <param name="channelReader">
        /// The reader for the channel providing the byte sequences to read.
        /// </param>
        public ChannelMemoryStream(ChannelReader<UnmanagedMemoryRange> channelReader) : base(channelReader)
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="ChannelMemoryStream"/> and retrieves
        /// the writer to its underlying channel.
        /// </summary>
        /// <param name="writer">The variable to receive the writer for the created channel.</param>
        /// <param name="capacity">The capacity of the channel, if it should be bounded.</param>
        /// <returns>The stream to read from the channel.</returns>
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
