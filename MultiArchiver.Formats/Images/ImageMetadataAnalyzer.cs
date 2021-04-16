using IS4.MultiArchiver.Analyzers.MetadataReaders;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using MetadataExtractor;
using Microsoft.CSharp.RuntimeBinder;
using System.Collections.Generic;

namespace IS4.MultiArchiver.Analyzers
{
    public class ImageMetadataAnalyzer : BinaryFormatAnalyzer<IReadOnlyList<Directory>>
    {
        public ICollection<object> MetadataReaders { get; } = new SortedSet<object>(TypeInheritanceComparer<object>.Instance);

        public ImageMetadataAnalyzer() : base(Classes.ImageObject)
        {

        }

        public override bool Analyze(ILinkedNode node, IReadOnlyList<Directory> image, ILinkedNodeFactory nodeFactory)
        {
            bool result = false;
            foreach(var dir in image)
            {
                if(TryDescribe(node, dir, nodeFactory))
                {
                    result = true;
                }
            }
            return result;
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
