using Microsoft.JSInterop;
using System;
using System.IO;

namespace IS4.SFI.WebApp
{
    /// <summary>
    /// A writeable stream serializing the input data
    /// to a JavaScript array and producing a blob.
    /// </summary>
    public class BlobArrayStream : Stream
    {
        const string createArray = "createArray";
        const string appendBytes = "appendBytes";
        const string finalizeBlob = "finalizeBlob";

        /// <inheritdoc/>
        public override bool CanRead => false;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => true;

        long position;

        /// <inheritdoc/>
        public override long Length => position;

        /// <inheritdoc/>
        public override long Position { get => position; set => throw new NotSupportedException(); }

        readonly IJSInProcessRuntime js;
        readonly string mediaType;
        readonly IJSInProcessObjectReference data;

        /// <summary>
        /// Creates a new instance of the stream.
        /// </summary>
        /// <param name="js">The JavaScript instance to use.</param>
        /// <param name="mediaType">The media type of the resulting blob.</param>
        public BlobArrayStream(IJSInProcessRuntime js, string mediaType)
        {
            this.js = js;
            this.mediaType = mediaType;
            data = js.Invoke<IJSInProcessObjectReference>(createArray);
        }

        /// <inheritdoc/>
        public override void Flush()
        {

        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            var segment = new ArraySegment<byte>(buffer, offset, count);
            js.InvokeVoid(appendBytes, data, segment);
            position += count;
        }

        /// <summary>
        /// Finalizes the blob and creates its URI.
        /// </summary>
        /// <returns>The blob URI.</returns>
        public string CreateBlob()
        {
            return js.Invoke<string>(finalizeBlob, data, mediaType);
        }
    }
}
