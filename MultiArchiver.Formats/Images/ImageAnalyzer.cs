using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System.Drawing;

namespace IS4.MultiArchiver.Analyzers
{
    public class ImageAnalyzer : BinaryFormatAnalyzer<Image>
    {
        public ImageAnalyzer() : base(Classes.ImageObject)
        {

        }

        public override string Analyze(ILinkedNode node, Image image, ILinkedNodeFactory nodeFactory)
        {
            return null;
        }
    }
}
