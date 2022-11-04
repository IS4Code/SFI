using IS4.MultiArchiver.Media.Packages;
using IS4.MultiArchiver.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// Provides description of based on the presence of plaintext-based description files.
    /// The names of the used files are stored in <see cref="FileNames"/>.
    /// </summary>
    public class PackageDescriptionProvider : PackageProvider<PackageDescriptionProvider.PackageInfo>
    {
        /// <summary>
        /// The collection of file names which are looked up in a directory.
        /// By default it contains only "FILE_ID.DIZ".
        /// </summary>
        public ICollection<string> FileNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Creates a new instance of the provider.
        /// </summary>
        public PackageDescriptionProvider()
        {
            FileNames.Add("FILE_ID.DIZ");
        }

        protected override PackageInfo MatchRoot(object root, AnalysisContext context)
        {
            return new PackageInfo(this);
        }

        /// <summary>
        /// Stores information about a package.
        /// </summary>
        public class PackageInfo : Analyzer
        {
            readonly PackageDescriptionProvider provider;
            readonly List<IDataObject> dataObjects = new List<IDataObject>();

            /// <summary>
            /// Creates a new instance of the class.
            /// </summary>
            /// <param name="provider">The outer instance of <see cref="PackageDescriptionProvider"/>.</param>
            public PackageInfo(PackageDescriptionProvider provider)
            {
                this.provider = provider;
            }

            protected async override ValueTask<AnalysisResult> Analyze<TPath, TNode>(TPath parentPath, TNode node, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzers analyzers)
            {
                if(parentPath != null)
                {
                    // This is not the root
                    if(node is IDataObject dataObject)
                    {
                        if(dataObject.StringValue != null && dataObject.Source is IFileInfo file)
                        {
                            if(provider.FileNames.Contains(file.Name))
                            {
                                // The filename is recognized
                                dataObjects.Add(dataObject);
                            }
                        }
                        // Don't follow into the data
                        return await inner(ContainerBehaviour.None);
                    }
                    if(node is IFormatObject)
                    {
                        // Don't follow into the media object
                        return await inner(ContainerBehaviour.None);
                    }
                    // Follow into the nested nodes
                    return await inner(ContainerBehaviour.FollowChildren);
                }
                var result = await inner(ContainerBehaviour.FollowChildren);
                if(result.Node != null)
                {
                    // The directory yielded a node
                    foreach(var dataObject in dataObjects)
                    {
                        var file = (IFileInfo)dataObject.Source;
                        if(file.Path == $"{Root}/{file.Name}" || (String.IsNullOrEmpty(Root) && file.Path == file.Name))
                        {
                            // The file is in the root directory
                            var value = DataTools.ReplaceControlCharacters(dataObject.StringValue, dataObject.Encoding);

                            // Describe the node using PackageDescription
                            await analyzers.Analyze(new PackageDescription(value), context.WithNode(result.Node));
                        }
                    }
                }
                return result;
            }
        }
    }
}
