using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System.Drawing;

namespace IS4.MultiArchiver.Analyzers
{
    public class ImageAnalyzer : FormatAnalyzer<Image>
    {
        public ImageAnalyzer() : base(Classes.ImageObject)
        {

        }

        public override bool Analyze(ILinkedNode node, Image image, ILinkedNodeFactory nodeFactory)
        {
            return false;
        }
    }
}
