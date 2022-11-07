using System;
using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace IS4.SFI.Tools.IO
{
    /// <summary>
    /// An implementation of <see cref="ChannelStream{TSequence}"/> for an
    /// instance of <see cref="ArraySegment{T}"/> of <see cref="Byte"/>.
    /// </summary>
    public sealed class ChannelArrayStream : ChannelStream<ArraySegment<byte>>
    {
        /// <inheritdoc/>
        public ChannelArrayStream(ChannelReader<ArraySegment<byte>> channelReader) : base(channelReader)
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="ChannelArrayStream"/> and retrieves
        /// the writer to its underlying channel.
        /// </summary>
        /// <returns>The stream to read from the channel.</returns>
        /// <inheritdoc cref="ChannelStream{TSequence}.CreateReader(out ChannelWriter{TSequence}, int?)"/>
        public static ChannelArrayStream Create(out ChannelWriter<ArraySegment<byte>> writer, int? capacity = null)
        {
            return new ChannelArrayStream(CreateReader(out writer, capacity));
        }

        /// <inheritdoc/>
        protected override void ReadFrom(ref ArraySegment<byte> current, byte[] buffer, int offset, int len)
        {
            current.Slice(0, len).CopyTo(buffer, offset);
            current = current.Slice(len);
        }

        /// <inheritdoc/>
        protected override byte ReadFrom(ref ArraySegment<byte> current)
        {
            var result = current.At(0);
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
        /// <inheritdoc/>
        public ChannelMemoryStream(ChannelReader<UnmanagedMemoryRange> channelReader) : base(channelReader)
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="ChannelMemoryStream"/> and retrieves
        /// the writer to its underlying channel.
        /// </summary>
        /// <returns>The stream to read from the channel.</returns>
        /// <inheritdoc cref="ChannelStream{TSequence}.CreateReader(out ChannelWriter{TSequence}, int?)"/>
        public static ChannelMemoryStream Create(out ChannelWriter<UnmanagedMemoryRange> writer, int? capacity = null)
        {
            return new ChannelMemoryStream(CreateReader(out writer, capacity));
        }

        /// <inheritdoc/>
        protected override void ReadFrom(ref UnmanagedMemoryRange current, byte[] buffer, int offset, int len)
        {
            Marshal.Copy(current.Address, buffer, offset, len);
            current = new UnmanagedMemoryRange(current.Address + len, current.Count - len);
        }

        /// <inheritdoc/>
        protected override byte ReadFrom(ref UnmanagedMemoryRange current)
        {
            var result = Marshal.ReadByte(current.Address);
            current = new UnmanagedMemoryRange(current.Address + 1, current.Count - 1);
            return result;
        }
    }
}
