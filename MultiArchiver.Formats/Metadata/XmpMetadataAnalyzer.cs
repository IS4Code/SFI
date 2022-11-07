using IS4.MultiArchiver.Services;
using MetadataExtractor.Formats.Xmp;
using System.IO;
using System.Threading.Tasks;
using XmpCore.Impl;
using XmpCore.Options;

namespace IS4.MultiArchiver.Analyzers
{
    /// <summary>
    /// An analyzer of XMP metadata, as instances
    /// of <see cref="XmpDirectory"/>.
    /// </summary>
    public class XmpMetadataAnalyzer : EntityAnalyzer<XmpDirectory>
    {
        public SerializeOptions XmpSerializeOptions = new SerializeOptions
        {
            Indent = "",
            Newline = " "
        };

        public async override ValueTask<AnalysisResult> Analyze(XmpDirectory directory, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            var serializer = new XmpSerializerRdf();
            using(var stream = new MemoryStream())
            {
                // Store as RDF/XML in <x:xmpmeta>
                serializer.Serialize(directory.XmpMeta, stream, XmpSerializeOptions);
                stream.Position = 0;
                DataTools.DescribeAsXmp(node, stream);
            }
            return new AnalysisResult(node);
        }
    }
}
