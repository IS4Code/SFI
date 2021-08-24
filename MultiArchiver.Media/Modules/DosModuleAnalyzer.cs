using IS4.MultiArchiver.Media;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;
using System.Text;

namespace IS4.MultiArchiver.Analyzers
{
    public class DosModuleAnalyzer : MediaObjectAnalyzer<DosModuleAnalyzer.Module>
    {
        public DosModuleAnalyzer() : base(Common.ApplicationClasses)
        {

        }

        public override AnalysisResult Analyze(Module module, AnalysisContext context, IEntityAnalyzer globalAnalyzer)
        {
            var node = GetNode(context);
            var uncompressed = module.GetCompressedContents();
            if(uncompressed != null)
            {
                var infoNode = globalAnalyzer.Analyze<IFileInfo>(uncompressed, context.WithParent(node)).Node;
                if(infoNode != null)
                {
                    node.Set(Properties.BelongsToContainer, infoNode);
                }
            }
            return new AnalysisResult(node);
        }

        public class Module
        {
            readonly Stream stream;
            readonly BinaryReader reader;

            public Module(Stream stream)
            {
                this.stream = stream;
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

                public object ReferenceKey => Reader;

                public object DataKey => null;

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
