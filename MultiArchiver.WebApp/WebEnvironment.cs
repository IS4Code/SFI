using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;

namespace IS4.MultiArchiver.WebApp
{
    public class WebEnvironment : IApplicationEnvironment
    {
        readonly IJSInProcessRuntime js;
        readonly IReadOnlyDictionary<string, IBrowserFile> inputFiles;
        readonly IDictionary<string, IJSInProcessObjectReference> outputFiles;

        public int WindowWidth => Int32.MaxValue;

        public TextWriter LogWriter { get; }

        public string NewLine { get; }

        public WebEnvironment(IJSInProcessRuntime js, TextWriter writer, IReadOnlyDictionary<string, IBrowserFile> inputFiles, IDictionary<string, IJSInProcessObjectReference> outputFiles)
        {
            this.js = js;
            LogWriter = writer;
            this.inputFiles = inputFiles;
            this.outputFiles = outputFiles;
            NewLine = js.Invoke<string>("getNewline");
        }

        public Stream OpenInputFile(string path)
        {
            if(inputFiles == null || !inputFiles.TryGetValue(path, out var file))
            {
                throw new FileNotFoundException();
            }
            return file.OpenReadStream(Int64.MaxValue);
        }

        public Stream OpenOutputFile(string path)
        {
            var data = js.Invoke<IJSInProcessObjectReference>("createArray");
            outputFiles[path] = data;
            return new OutputStream(js, data);
        }

        class OutputStream : Stream
        {
            public override bool CanRead => false;

            public override bool CanSeek => false;

            public override bool CanWrite => true;

            long position;

            public override long Length => position;

            public override long Position { get => position; set => throw new NotSupportedException(); }

            readonly IJSInProcessRuntime js;
            readonly IJSInProcessObjectReference data;

            public OutputStream(IJSInProcessRuntime js, IJSInProcessObjectReference data)
            {
                this.js = js;
                this.data = data;
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
                position += count;
                var segment = new ArraySegment<byte>(buffer, offset, count);
                js.InvokeVoid("appendBytes", data, segment);
            }
        }
    }
}
