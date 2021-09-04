using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using NPOI.OpenXml4Net.Exceptions;
using NPOI.OpenXml4Net.OPC;
using NPOI.OpenXml4Net.OPC.Internal;
using System;

namespace IS4.MultiArchiver.Formats
{
    public sealed class OpenPackageFormat : LegacyPackageFileFormat<IFileNodeInfo, OpenPackageFormat.PackageInfo, OpenPackageFormat.PackageInfo>
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

        public class PackageInfo : EntityAnalyzer,
            IContainerAnalyzer<IContainerNode, IFileNodeInfo>,
            IContainerAnalyzer<IContainerNode, IDataObject>,
            IContainerAnalyzer
        {
            public string Root { get; }
            public ContentTypeManager ContentTypeManager { get; private set; }

            public PackageInfo(IFileNodeInfo contentTypes)
            {
                Root = contentTypes.Path.Substring(0, contentTypes.Path.Length - contentTypes.Name.Length);
            }

            public AnalysisResult Analyze(IContainerNode parentNode, IFileNodeInfo file, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzerProvider analyzers)
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
                return inner(ContainerBehaviour.FollowChildren);
            }

            public AnalysisResult Analyze(IContainerNode parentNode, IDataObject dataObject, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzerProvider analyzers)
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
                return inner(ContainerBehaviour.None);
            }

            AnalysisResult IContainerAnalyzer.Analyze<TParent, TEntity>(TParent parentNode, TEntity entity, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzerProvider analyzers)
            {
                if(this is IContainerAnalyzer<TParent, TEntity> analyzer)
                {
                    return analyzer.Analyze(parentNode, entity, context, inner, analyzers);
                }
                return inner(ContainerBehaviour.None);
            }
        }
    }
}
