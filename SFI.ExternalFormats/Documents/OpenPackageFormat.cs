using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using NPOI.OpenXml4Net.Exceptions;
using NPOI.OpenXml4Net.OPC;
using NPOI.OpenXml4Net.OPC.Internal;
using System;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the OOXML package format, matching packages with the
    /// <see cref="ContentTypeManager.CONTENT_TYPES_PART_NAME"/> file.
    /// </summary>
    public sealed class OpenPackageFormat : ContainerFileFormat<IFileNodeInfo, OpenPackageFormat.PackageInfo>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public OpenPackageFormat() : base("application/vnd.openxmlformats-package", "ooxml")
        {

        }

        /// <inheritdoc/>
        protected override IContainerAnalyzer? MatchRoot(IFileNodeInfo file, AnalysisContext context)
        {
            if(file.Path != null && ContentTypeManager.CONTENT_TYPES_PART_NAME.Equals(file.Name, StringComparison.OrdinalIgnoreCase))
            {
                return new PackageInfo(file);
            }
            return null;
        }

        /// <summary>
        /// Represents the content types in the package.
        /// </summary>
        public class PackageInfo : EntityAnalyzer,
            IContainerAnalyzer<IContainerNode, IFileNodeInfo>,
            IContainerAnalyzer<IContainerNode, IDataObject>,
            IContainerAnalyzer
        {
            /// <summary>
            /// The root path of the package.
            /// </summary>
            public string Root { get; }

            /// <summary>
            /// The instance of <see cref="NPOI.OpenXml4Net.OPC.Internal.ContentTypeManager"/>
            /// providing content types of the files within the package.
            /// </summary>
            public ContentTypeManager? ContentTypeManager { get; private set; }

            /// <summary>
            /// Creates a new instance of the info.
            /// </summary>
            /// <param name="contentTypes">The <see cref="ContentTypeManager.CONTENT_TYPES_PART_NAME"/> file.</param>
            public PackageInfo(IFileNodeInfo contentTypes)
            {
                Root = contentTypes.Path!.Substring(0, contentTypes.Path.Length - contentTypes.Name!.Length);
            }

            /// <inheritdoc/>
            public async ValueTask<AnalysisResult> Analyze(IContainerNode? parentNode, IFileNodeInfo file, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzers analyzers)
            {
                if(file.Path!.StartsWith(Root) && ContentTypeManager != null)
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
                            var typeObject = ImmutableContentType.GetCached(contentType);
                            var typeNode = (await analyzers.Analyze<System.Net.Mime.ContentType>(typeObject, context.WithParent(node))).Node;
                            if(typeNode != null)
                            {
                                node.Set(Properties.EncodingFormat, typeNode);
                            }
                        }
                    }catch(InvalidFormatException)
                    {

                    }
                }
                return await inner(ContainerBehaviour.FollowChildren);
            }

            /// <inheritdoc/>
            public async ValueTask<AnalysisResult> Analyze(IContainerNode? parentNode, IDataObject dataObject, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzers analyzers)
            {
                if(dataObject.Source is IFileNodeInfo file)
                {
                    if(ContentTypeManager.CONTENT_TYPES_PART_NAME.Equals(file.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        using var stream = dataObject.StreamFactory.Open();
                        ContentTypeManager = new ZipContentTypeManager(stream, null);
                    }
                }
                return await inner(ContainerBehaviour.None);
            }

            ValueTask<AnalysisResult> IContainerAnalyzer.Analyze<TParent, TEntity>(TParent? parentNode, TEntity entity, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzers analyzers) where TParent : default
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
