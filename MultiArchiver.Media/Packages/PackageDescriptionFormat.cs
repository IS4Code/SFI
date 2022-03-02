using IS4.MultiArchiver.Media.Packages;
using IS4.MultiArchiver.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    public class PackageDescriptionFormat : PackageFormat<PackageDescriptionFormat.PackageInfo>
    {
        public ICollection<string> FileNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public PackageDescriptionFormat()
        {
            FileNames.Add("FILE_ID.DIZ");
        }

        protected override PackageInfo MatchRoot(object root, AnalysisContext context)
        {
            return new PackageInfo(this);
        }

        public class PackageInfo : Analyzer
        {
            readonly PackageDescriptionFormat format;
            readonly List<IDataObject> dataObjects = new List<IDataObject>();

            public PackageInfo(PackageDescriptionFormat format)
            {
                this.format = format;
            }

            protected override async ValueTask<AnalysisResult> Analyze<TPath, TNode>(TPath parentPath, TNode node, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzerProvider analyzers)
            {
                if(parentPath != null)
                {
                    if(node is IDataObject dataObject)
                    {
                        if(dataObject.StringValue != null && dataObject.Source is IFileInfo file)
                        {
                            if(format.FileNames.Contains(file.Name))
                            {
                                dataObjects.Add(dataObject);
                            }
                        }
                        return await inner(ContainerBehaviour.None);
                    }
                    return await inner(ContainerBehaviour.FollowChildren);
                }
                var result = await inner(ContainerBehaviour.FollowChildren);
                if(result.Node != null)
                {
                    foreach(var dataObject in dataObjects)
                    {
                        var file = (IFileInfo)dataObject.Source;
                        if(file.Path == $"{Root}/{file.Name}")
                        {
                            var value = DataTools.ReplaceControlCharacters(dataObject.StringValue, dataObject.Encoding);

                            analyzers.Analyze(new PackageDescription(value), context.WithNode(result.Node));
                        }
                    }
                }
                return result;
            }
        }
    }
}
