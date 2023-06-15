using Aeon.Emulator;
using Aeon.Emulator.Dos.Programs;
using Aeon.Emulator.Dos.VirtualFileSystem;
using Aeon.Emulator.RuntimeExceptions;
using Aeon.Emulator.Video;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.Tools;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace IS4.SFI.Formats.Modules
{
    /// <summary>
    /// Represents an MS-DOS executable module.
    /// </summary>
    public class DosModule
    {
        readonly BinaryReader reader;

        /// <summary>
        /// Whether the module is a MZ module (EXE) or a plain COM executable.
        /// </summary>
        public bool IsExe { get; }

        /// <summary>
        /// Creates a new instance of the module.
        /// </summary>
        /// <param name="stream">The stream storing the module.</param>
        public DosModule(Stream stream)
        {
            reader = new BinaryReader(stream, Encoding.ASCII, true);
            stream.Position = 0;
            var sig = reader.ReadUInt16();
            if(sig != 0x5A4D && sig != 0x4D5A) return;
            IsExe = true;
            if(stream.Length < 0x3C + 4) return;
            stream.Position = 0x3C;
            var headerOffset = reader.ReadUInt32();
            if(headerOffset <= 1 || headerOffset >= stream.Length - 2) return;
            stream.Position = headerOffset;
            var b = reader.ReadByte();
            if(b < 0x41 || b > 0x5A) return;
            b = reader.ReadByte();
            if(b < 0x41 || b > 0x5A) return;
            throw new ArgumentException("This file uses an extended executable format.", nameof(stream));
        }

        /// <summary>
        /// Attempts to emulate the module and reads the output from the console.
        /// </summary>
        /// <param name="consoleEncoding">The encoding to use to convert the video memory to text.</param>
        /// <param name="step">The number of instructions to emulate in each step.</param>
        /// <param name="max">The maximum number of instructions after which to terminate.</param>
        /// <returns>The text in the console after the program was executed, if any.</returns>
        public string? Emulate(Encoding consoleEncoding, int step, int max)
        {
            using var machine = new VirtualMachine();
            machine.EndInitialization();

            var video = machine.Devices.OfType<VideoHandler>().FirstOrDefault();
            if(video == null)
            {
                return null;
            }

            var stream = reader.BaseStream;
            stream.Position = 0;
            var path = new VirtualPath();
            ProgramImage image = IsExe ? new ExeFile(path, stream) : new ComFile(path, stream);
            machine.LoadImage(image);

            EventHandler endHandler = delegate { throw new EndOfProgramException(); };
            machine.VideoModeChanged += endHandler;
            machine.CurrentProcessChanged += endHandler;

            int instructions = 0;
            while(true)
            {
                try{
                    machine.Emulate(step);
                    instructions += step;
                    if(instructions > max)
                    {
                        break;
                    }
                }catch(Exception e) when (IsVMException(e))
                { 
                    break;
                }
            }

            machine.VideoModeChanged -= endHandler;
            machine.CurrentProcessChanged -= endHandler;

            var console = video.TextConsole;
            var plane = console.GetPlaneData(0);
            return ReadTextMemory(plane, console.Width, console.Height, consoleEncoding);
        }

        bool IsVMException(Exception e)
        {
            return e is EndOfProgramException or NotImplementedException
                or NotSupportedException or ArgumentException
                or InvalidOperationException or InvalidDataException
                or FileNotFoundException or EmulatedException
                or EnableInstructionTrapException;
        }

        /// <summary>
        /// Reads the video text memory lines to string.
        /// </summary>
        /// <param name="memory">The memory span.</param>
        /// <param name="width">The width of each line.</param>
        /// <param name="height">The height of the console.</param>
        /// <param name="encoding">The encoding to use to decode the bytes.</param>
        /// <returns>The string contents of the memory, or <see langword="null"/>.</returns>
        string? ReadTextMemory(Span<byte> memory, int width, int height, Encoding encoding)
        {
            StringBuilder? sb = null;
            int numLines = 0;
            for(int y = 0; y < height; y++)
            {
                var row = memory.Slice(y * width, width);
                var end = row.IndexOf((byte)0);
                if(end != 0)
                {
                    if(end != -1)
                    {
                        row = row.Slice(0, end);
                    }
                    if(numLines > 0)
                    {
                        sb ??= new();
                        for(int i = 0; i < numLines; i++)
                        {
                            sb.Append('\n');
                        }
                        numLines = 0;
                    }
                    (sb ??= new()).Append(encoding.GetString(row.ToArray()));
                }
                numLines++;
            }
            return sb?.ToString();
        }

        /// <summary>
        /// Decompresses the executable.
        /// </summary>
        /// <returns>
        /// The decompressed equivalent of the executable, as an instance
        /// of <see cref="IFileInfo"/>.
        /// </returns>
        public IFileInfo? GetCompressedContents()
        {
            var lzex = new LzExtractedFile(reader);
            if(!lzex.Valid) return null;
            return lzex;
        }

        class LzExtractedFile : LzExtractor, IFileInfo
        {
            readonly Lazy<ArraySegment<byte>> data;

            public LzExtractedFile(BinaryReader reader) : base(reader)
            {
                data = new Lazy<ArraySegment<byte>>(() => {
                    var stream = Decompress();
                    return stream.GetData();
                });
            }

            public string? Name => null;

            public string? SubName => null;

            public string? Path => null;

            public int? Revision => null;

            public DateTime? CreationTime => null;

            public DateTime? LastWriteTime => null;

            public DateTime? LastAccessTime => null;

            public long Length => data.Value.Count;

            public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

            public object? ReferenceKey => this;

            public object? DataKey => null;

            public FileKind Kind => FileKind.Embedded;

            public FileAttributes Attributes => FileAttributes.Normal;

            public Stream Open()
            {
                return this.data.Value.AsStream(false);
            }

            public override string ToString()
            {
                return "LZEXE-compressed executable";
            }
        }
    }
}
