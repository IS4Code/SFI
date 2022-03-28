using Microsoft.JSInterop;
using System;
using System.IO;

namespace IS4.MultiArchiver.WebApp
{
    public class BlobArrayStream : Stream
    {
        const string createArray = "createArray";
        const string appendBytes = "appendBytes";
        const string finalizeBlob = "finalizeBlob";

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        long position;

        public override long Length => position;

        public override long Position { get => position; set => throw new NotSupportedException(); }

        readonly IJSInProcessRuntime js;
        readonly string mediaType;
        readonly IJSInProcessObjectReference data;

        public BlobArrayStream(IJSInProcessRuntime js, string mediaType)
        {
            this.js = js;
            this.mediaType = mediaType;
            data = js.Invoke<IJSInProcessObjectReference>(createArray);
        }

        public override void Flush()
        {

        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var segment = new ArraySegment<byte>(buffer, offset, count);
            js.InvokeVoid(appendBytes, data, segment);
            position += count;
        }

        public string CreateBlob()
        {
            return js.Invoke<string>(finalizeBlob, data, mediaType);
        }
    }
}
