using IS4.SFI.Services;
using MetadataExtractor.Formats.Xmp;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using XmpCore.Impl;
using XmpCore.Options;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of XMP metadata, as instances
    /// of <see cref="XmpDirectory"/>.
    /// </summary>
    [Description("An analyzer of XMP metadata.")]
    public class XmpMetadataAnalyzer : EntityAnalyzer<XmpDirectory>
    {
        /// <summary>
        /// The options to use when serializing XMP metadata to XML.
        /// </summary>
        public SerializeOptions XmpSerializeOptions = new()
        {
            Indent = "",
            Newline = " "
        };

        /// <inheritdoc/>
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
