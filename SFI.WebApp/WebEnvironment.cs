using IS4.SFI.Application;
using IS4.SFI.Services;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IS4.SFI.WebApp
{
    /// <summary>
    /// The implementation of <see cref="IApplicationEnvironment"/>
    /// for the web environment.
    /// </summary>
    public class WebEnvironment : IApplicationEnvironment, IDisposable
    {
        readonly IJSInProcessRuntime js;
        readonly IReadOnlyDictionary<string, IBrowserFile> inputFiles;
        readonly IDictionary<string, BlobArrayStream> outputFiles;
        readonly Action stateChanged;

        /// <inheritdoc/>
        public int WindowWidth => Int32.MaxValue;

        /// <inheritdoc/>
        public TextWriter LogWriter { get; }

        /// <inheritdoc/>
        public string NewLine { get; }

        /// <summary>
        /// True if <see cref="Dispose"/> has been called.
        /// </summary>
        public bool Disposed { get; private set; }

        /// <inheritdoc/>
        public string? ExecutableName => "";

        /// <summary>
        /// Creates a new instance of the environment.
        /// </summary>
        /// <param name="js">The JavaScript runtime to use.</param>
        /// <param name="writer">The writer for logging.</param>
        /// <param name="inputFiles">The collection of input files.</param>
        /// <param name="outputFiles">The collection of output files, which may be modified during the lifetime of the instance.</param>
        /// <param name="stateChanged">An action that causes the page to be updated.</param>
        public WebEnvironment(IJSInProcessRuntime js, TextWriter writer, IReadOnlyDictionary<string, IBrowserFile> inputFiles, IDictionary<string, BlobArrayStream> outputFiles, Action stateChanged)
        {
            this.js = js;
            this.inputFiles = inputFiles;
            this.outputFiles = outputFiles;
            this.stateChanged = stateChanged;
            LogWriter = writer;
            NewLine = js.Invoke<string>("getNewline");
        }

        /// <inheritdoc/>
        public IEnumerable<IFileInfo> GetFiles(string path)
        {
            if(inputFiles == null)
            {
                return Array.Empty<IFileInfo>();
            }
            var match = DataTools.ConvertWildcardToRegex(path);
            return inputFiles.Where(f => match.IsMatch(f.Key)).Select(f => new BrowserFileInfo(f.Value));
        }

        /// <inheritdoc/>
        public Stream CreateFile(string path, string mediaType)
        {
            if(path == "-")
            {
                return new TextStream(LogWriter, Encoding.UTF8, this);
            }
            if(path.Equals("nul", StringComparison.OrdinalIgnoreCase) || path == "/dev/null")
            {
                return Stream.Null;
            }
            return outputFiles[path] = new BlobArrayStream(js, mediaType);
        }

        /// <inheritdoc/>
        public async ValueTask Update()
        {
            if(Disposed) throw new InternalApplicationException(new OperationCanceledException());
            stateChanged?.Invoke();
            await Task.Delay(1);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Disposed = true;
        }

        class TextStream : Stream
        {
            readonly TextWriter writer;
            readonly Encoding encoding;
            readonly IApplicationEnvironment? environment;

            public TextStream(TextWriter writer, Encoding encoding, IApplicationEnvironment? environment)
            {
                this.writer = writer;
                this.encoding = encoding;
                this.environment = environment;
            }

            public override bool CanRead => false;

            public override bool CanSeek => false;

            public override bool CanWrite => true;

            public override long Length => throw new NotSupportedException();

            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

            public override void Flush()
            {
                writer.Flush();
                environment?.Update();
            }

            public override Task FlushAsync(CancellationToken cancellationToken)
            {
                if(cancellationToken.IsCancellationRequested) return Task.FromCanceled(cancellationToken);
                return Inner();
                async Task Inner()
                {
                    await writer.FlushAsync();
                    await (environment?.Update()).GetValueOrDefault();
                }
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
                var text = encoding.GetString(buffer, offset, count);
                writer.Write(text);
            }

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                if(cancellationToken.IsCancellationRequested) return Task.FromCanceled(cancellationToken);
                var text = encoding.GetString(buffer, offset, count);
                return writer.WriteAsync(text);
            }
        }
    }
}
