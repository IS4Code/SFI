using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using Svg;

namespace IS4.MultiArchiver.Analyzers
{
    public class SvgAnalyzer : XmlFormatAnalyzer<SvgDocument>
    {
        public override string Analyze(ILinkedNode node, SvgDocument svg, ILinkedNodeFactory nodeFactory)
        {
            if(svg.Width.Type == SvgUnitType.Pixel)
            {
                node.Set(Properties.Width, (decimal)svg.Width.Value);
            }
            if(svg.Height.Type == SvgUnitType.Pixel)
            {
                node.Set(Properties.Height, (decimal)svg.Height.Value);
            }
            return null;
        }
    }
}
