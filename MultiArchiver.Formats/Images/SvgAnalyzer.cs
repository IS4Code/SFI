using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using Svg;
using System.Drawing;

namespace IS4.MultiArchiver.Analyzers
{
    public class SvgAnalyzer : XmlFormatAnalyzer<SvgDocument>
    {
        public SvgAnalyzer() : base(Common.ImageClasses)
        {

        }

        public override string Analyze(ILinkedNode parent, ILinkedNode node, SvgDocument svg, object source, ILinkedNodeFactory nodeFactory)
        {
            if(svg.Width.Type == SvgUnitType.Pixel)
            {
                node.Set(Properties.Width, (decimal)svg.Width.Value);
            }
            if(svg.Height.Type == SvgUnitType.Pixel)
            {
                node.Set(Properties.Height, (decimal)svg.Height.Value);
            }
            using(var bmp = svg.Draw())
            {
                bmp.Tag = new ImageTag
                {
                    StoreDimensions = false
                };
                var imageObj = new LinkedObject<Image>(node, source, bmp);
                nodeFactory.Create<ILinkedObject<Image>>(parent, imageObj);
            }
            if(!svg.Width.IsEmpty && !svg.Height.IsEmpty)
            {
                return $"{svg.Width.Value}×{svg.Height.Value}";
            }
            return null;
        }
    }
}
