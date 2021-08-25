using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using NPOI.OpenXml4Net.Exceptions;
using NPOI.OpenXml4Net.OPC;
using NPOI.OpenXml4Net.OPC.Internal;
using System;

namespace IS4.MultiArchiver.Formats
{
    public class OpenPackageFormat : PackageFormat<IStreamFactory, OpenPackageFormat.PackageInfo, OpenPackageFormat.PackageInfo>
    {
        public OpenPackageFormat() : base("application/vnd.openxmlformats-package", "ooxml")
        {

        }

        public override PackageInfo Match(IStreamFactory stream, MatchContext context)
        {
            if(stream is IFileNodeInfo file)
            {
                if(file.Name.Equals(ContentTypeManager.CONTENT_TYPES_PART_NAME, StringComparison.OrdinalIgnoreCase))
                {
                    return new PackageInfo(file);
                }
            }
            return null;
        }

        public class PackageInfo : EntityAnalyzer<IStreamFactory>, IIndividualUriFormatter<Uri>
        {
            public string Root { get; }
            public ContentTypeManager ContentTypeManager { get; private set; }

            Uri IUriFormatter<Uri>.this[Uri value] => value;

            public PackageInfo(IFileNodeInfo contentTypes)
            {
                Root = contentTypes.Path.Substring(0, contentTypes.Path.Length - contentTypes.Name.Length);
            }

            public override AnalysisResult Analyze(IStreamFactory entity, AnalysisContext context, IEntityAnalyzer globalAnalyzer)
            {
                if(entity is IFileNodeInfo file)
                {
                    if(file.Path.StartsWith(Root))
                    {
                        var node = GetNode(this, context);
                        var relPath = file.Path.Substring(Root.Length);

                        if(file.Name == ContentTypeManager.CONTENT_TYPES_PART_NAME)
                        {
                            ContentTypeManager = new ZipContentTypeManager(context.MatchContext.Stream, null);
                        }
                        if(ContentTypeManager != null)
                        {
                            var packUri = new Uri("/" + Uri.EscapeUriString(relPath), UriKind.Relative);
                            try
                            {
                                var partName = new PackagePartName(packUri, true);
                                var contentType = ContentTypeManager.GetContentType(partName);

                                if(contentType != null)
                                {
                                    node.Set(Properties.EncodingFormat, Vocabularies.Urim, Uri.EscapeUriString(contentType));
                                }
                            }catch(InvalidFormatException)
                            {

                            }
                        }
                    }
                }
                return default;
            }
        }
    }
}
