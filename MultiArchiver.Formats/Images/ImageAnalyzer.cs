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

        public override ILinkedNode Analyze(ILinkedNode parent, Image image, ILinkedNodeFactory nodeFactory)
        {
            var node = base.Analyze(parent, image, nodeFactory);
            if(node != null)
            {

            }
            return node;
        }
    }
}
