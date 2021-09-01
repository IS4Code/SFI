using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using NPOI.OpenXml4Net.Exceptions;
using NPOI.OpenXml4Net.OPC;
using NPOI.OpenXml4Net.OPC.Internal;
using System;
using System.Collections.Generic;

namespace IS4.MultiArchiver.Formats
{
    public sealed class OpenPackageFormat : PackageFormat<IFileNodeInfo, OpenPackageFormat.PackageInfo, OpenPackageFormat.PackageInfo>
    {
        public OpenPackageFormat() : base("application/vnd.openxmlformats-package", "ooxml")
        {

        }

        public override PackageInfo Match(IFileNodeInfo file, MatchContext context)
        {
            if(ContentTypeManager.CONTENT_TYPES_PART_NAME.Equals(file.Name, StringComparison.OrdinalIgnoreCase))
            {
                return new PackageInfo(file);
            }
            return null;
        }

        public class PackageInfo : EntityAnalyzerBase, IEntityAnalyzer<IFileNodeInfo>, IEntityAnalyzer<IDataObject>, IEntityAnalyzerProvider
        {
            public string Root { get; }
            public ContentTypeManager ContentTypeManager { get; private set; }

            public PackageInfo(IFileNodeInfo contentTypes)
            {
                Root = contentTypes.Path.Substring(0, contentTypes.Path.Length - contentTypes.Name.Length);
            }

            public AnalysisResult Analyze(IFileNodeInfo file, AnalysisContext context, IEntityAnalyzerProvider globalAnalyzer)
            {
                if(file.Path.StartsWith(Root) && ContentTypeManager != null)
                {
                    var node = GetNode(context);
                    var relPath = file.Path.Substring(Root.Length);

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
                return default;
            }

            public AnalysisResult Analyze(IDataObject dataObject, AnalysisContext context, IEntityAnalyzerProvider globalAnalyzer)
            {
                if(dataObject.Source is IFileNodeInfo file)
                {
                    if(ContentTypeManager.CONTENT_TYPES_PART_NAME.Equals(file.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        using(var stream = dataObject.StreamFactory.Open())
                        {
                            ContentTypeManager = new ZipContentTypeManager(stream, null);
                        }
                    }
                }
                return default;
            }

            IEnumerable<IEntityAnalyzer<T>> IEntityAnalyzerProvider.GetAnalyzers<T>()
            {
                if(this is IEntityAnalyzer<T> analyzer)
                {
                    yield return analyzer;
                }
            }
        }
    }
}
