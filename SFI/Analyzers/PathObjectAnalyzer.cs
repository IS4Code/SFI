using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of file locations, expressed as instances of <see cref="PathObject"/>.
    /// </summary>
    public class PathObjectAnalyzer : EntityAnalyzer<PathObject>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public PathObjectAnalyzer()
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(PathObject path, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            if(context.Node is not ILinkedNode node)
            {
                if(path.IsRootDirectory)
                {
                    node = context.NodeFactory.Create(RootDirectoryUri.Instance, default);
                }else{
                    var local = UriTools.EscapePathString(path.Value.Value ?? "");
                    node = context.NodeFactory.Create(Vocabularies.File, local);
                }
            }
            node = InitNewNode(node, context);

            if(!path.IsRootDirectory)
            {
                if(path.Broader is PathObject broader)
                {
                    await analyzers.Analyze(broader, context.WithParentLink(node, Properties.PathObject));
                }else if(path.Extension is ExtensionObject extension)
                {
                    await analyzers.Analyze(extension, context.WithParentLink(node, Properties.ExtensionObject));
                }
            }

            return new(node);
        }

        /// <summary>
        /// This class is used to provide a fake URI with the value of
        /// <see cref="RootDirectoryUri.Value"/> when .NET would like to change it.
        /// </summary>
        class RootDirectoryUri : Uri, IIndividualUriFormatter<ValueTuple>
        {
            public const string Value = "file:///./";

            public static readonly RootDirectoryUri Instance = new();

            private RootDirectoryUri() : base(Value, UriKind.Absolute)
            {

            }

            public Uri this[ValueTuple value] => this;

            public override string ToString()
            {
                return Value;
            }
        }
    }
}
