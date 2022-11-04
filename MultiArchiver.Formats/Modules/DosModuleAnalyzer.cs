using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using IS4.Tools;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    /// <summary>
    /// An analyzer of MS-DOS modules, as instances of <see cref="Module"/>.
    /// </summary>
    public class DosModuleAnalyzer : MediaObjectAnalyzer<DosModuleAnalyzer.Module>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public DosModuleAnalyzer() : base(Common.ApplicationClasses)
        {

        }

        public async override ValueTask<AnalysisResult> Analyze(Module module, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            var uncompressed = module.GetCompressedContents();
            if(uncompressed != null)
            {
                var infoNode = (await analyzers.Analyze<IFileInfo>(uncompressed, context.WithParent(node))).Node;
                if(infoNode != null)
                {
                    node.Set(Properties.BelongsToContainer, infoNode);
                }
            }
            return new AnalysisResult(node);
        }

        /// <summary>
        /// Represents an MS-DOS executable module.
        /// </summary>
        public class Module
        {
            readonly BinaryReader reader;

            /// <summary>
            /// Creates a new instance of the module.
            /// </summary>
            /// <param name="stream">The stream storing the module in the MZ format.</param>
            public Module(Stream stream)
            {
                reader = new BinaryReader(stream, Encoding.ASCII, true);

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
            /// Decompresses the executable.
            /// </summary>
            /// <returns>
            /// The decompressed equivalent of the executable, as an instance
            /// of <see cref="IFileInfo"/>.
            /// </returns>
            public IFileInfo GetCompressedContents()
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

                public bool IsEncrypted => false;

                public string Name => null;

                public string SubName => null;

                public string Path => null;

                public int? Revision => null;

                public DateTime? CreationTime => null;

                public DateTime? LastWriteTime => null;

                public DateTime? LastAccessTime => null;

                public long Length => data.Value.Count;

                public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

                public object ReferenceKey => this;

                public object DataKey => null;

                public FileKind Kind => FileKind.Embedded;

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
}
