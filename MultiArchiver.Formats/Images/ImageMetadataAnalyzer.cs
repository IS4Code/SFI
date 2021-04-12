using IS4.MultiArchiver.Analyzers.MetadataReaders;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using MetadataExtractor;
using Microsoft.CSharp.RuntimeBinder;
using System.Collections.Generic;

namespace IS4.MultiArchiver.Analyzers
{
    public class ImageMetadataAnalyzer : FormatAnalyzer<IReadOnlyList<Directory>>
    {
        public ICollection<object> MetadataReaders { get; } = new SortedSet<object>(TypeInheritanceComparer<object>.Instance);

        public ImageMetadataAnalyzer() : base(Classes.ImageObject)
        {

        }

        public override ILinkedNode Analyze(ILinkedNode parent, IReadOnlyList<Directory> image, ILinkedNodeFactory nodeFactory)
        {
            var node = base.Analyze(parent, image, nodeFactory);
            if(node != null)
            {
                foreach(var dir in image)
                {
                    TryDescribe(node, dir, nodeFactory);
                }
            }
            return node;
        }

        public static ImageMetadataAnalyzer CreateDefault()
        {
            var analyzer = new ImageMetadataAnalyzer();

            analyzer.MetadataReaders.Add(new ExifReader());

            return analyzer;
        }

        private bool Describe<T>(ILinkedNode node, T dir, ILinkedNodeFactory nodeFactory) where T : Directory
        {
            foreach(var obj in MetadataReaders)
            {
                if(obj is IMetadataReader<T> reader)
                {
                    if(reader.Describe(node, dir, nodeFactory))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool TryDescribe(ILinkedNode node, Directory dir, ILinkedNodeFactory nodeFactory)
        {
            try
            {
                return Describe(node, (dynamic)dir, nodeFactory);
            }catch(RuntimeBinderException)
            {
                return false;
            }
        }
    }
}
