using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System.Drawing;

namespace IS4.MultiArchiver.Analyzers
{
    public class ImageAnalyzer : ClassRecognizingAnalyzer<Image>
    {
        public ImageAnalyzer() : base(Classes.ImageObject)
        {

        }

        public override ILinkedNode Analyze(Image image, ILinkedNodeFactory nodeFactory)
        {
            var node = base.Analyze(image, nodeFactory);
            if(node != null)
            {

            }
            return node;
        }
    }
}
