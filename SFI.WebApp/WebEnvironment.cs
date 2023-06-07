using IS4.SFI.Application;
using IS4.SFI.Application.Tools;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System;
using System.Buffers;
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
        /// <see langword="true"/> if <see cref="Dispose"/> has been called.
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
        public IEnumerable<IFileNodeInfo> GetFiles(string path)
        {
            if(StandardPaths.IsClipboard(path))
            {
                return new IFileNodeInfo[] { new DeviceInput(() => new ClipboardStream(js)) };
            }
            if(inputFiles == null)
            {
                return Array.Empty<IFileNodeInfo>();
            }
            var match = TextTools.ConvertWildcardToRegex(path);
            return inputFiles.Where(f => match.IsMatch(f.Key)).Select(f => new BrowserFileInfo(f.Value));
        }

        /// <inheritdoc/>
        public Stream CreateFile(string path, string mediaType)
        {
            if(StandardPaths.IsOutput(path) || StandardPaths.IsError(path))
            {
                return new TextStream(LogWriter, Encoding.UTF8, this);
            }else if(StandardPaths.IsNull(path))
            {
                return Stream.Null;
            }else if(StandardPaths.IsClipboard(path))
            {
                return new ClipboardStream(js);
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

        /// <summary>
        /// Provides a stream that uses a <see cref="TextWriter"/> instance
        /// to write the characters in a given encoding.
        /// </summary>
        class TextStream : Stream
        {
            readonly TextWriter writer;
            readonly IApplicationEnvironment? environment;
            readonly Decoder decoder;

            public TextStream(TextWriter writer, Encoding encoding, IApplicationEnvironment? environment)
            {
                this.writer = writer;
                this.environment = environment;

                decoder = encoding.GetDecoder();
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
                int size = decoder.GetCharCount(buffer, offset, count);
                using var charsLease = ArrayPool<char>.Shared.Rent(size, out var chars);
                size = decoder.GetChars(buffer, offset, count, chars, 0);
                writer.Write(chars, 0, size);
            }

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                if(cancellationToken.IsCancellationRequested) return Task.FromCanceled(cancellationToken);
                return Inner();
                async Task Inner()
                {
                    int size = decoder.GetCharCount(buffer, offset, count);
                    using var charsLease = ArrayPool<char>.Shared.Rent(size, out var chars);
                    size = decoder.GetChars(buffer, offset, count, chars, 0);
                    await writer.WriteAsync(chars, 0, size);
                }
            }
        }

        /// <summary>
        /// This class represents a device as a file.
        /// </summary>
        class DeviceInput : IFileInfo
        {
            readonly Func<Stream> openFunc;

            public DeviceInput(Func<Stream> openFunc)
            {
                this.openFunc = openFunc;
            }

            public string? Name => null;

            public string? SubName => null;

            public string? Path => null;

            public int? Revision => null;

            public DateTime? CreationTime => null;

            public DateTime? LastWriteTime => null;

            public DateTime? LastAccessTime => null;

            public FileKind Kind => FileKind.None;

            public long Length => -1;

            public FileAttributes Attributes => FileAttributes.Device;

            public StreamFactoryAccess Access => StreamFactoryAccess.Single;

            public object? ReferenceKey => this;

            public object? DataKey => null;

            public Stream Open()
            {
                return openFunc();
            }

            public override string? ToString()
            {
                return null;
            }
        }
    }
}
