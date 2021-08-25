using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tags;
using IS4.MultiArchiver.Vocabulary;
using Svg;
using System;

namespace IS4.MultiArchiver.Analyzers
{
    public class SvgAnalyzer : MediaObjectAnalyzer<SvgDocument>
    {
        public SvgAnalyzer() : base(Common.ImageClasses)
        {

        }

        public override AnalysisResult Analyze(SvgDocument svg, AnalysisContext context, IEntityAnalyzer globalAnalyzer)
        {
            var node = GetNode(context);
            if(svg.Width.Type == SvgUnitType.Pixel)
            {
                node.Set(Properties.Width, (decimal)svg.Width.Value);
            }
            if(svg.Height.Type == SvgUnitType.Pixel)
            {
                node.Set(Properties.Height, (decimal)svg.Height.Value);
            }
            Exception exception = null;
            try
            {
                using(var bmp = svg.Draw())
                {
                    bmp.Tag = new ImageTag
                    {
                        StoreDimensions = false
                    };
                    exception = globalAnalyzer.Analyze(bmp, context).Exception;
                }
            }catch(Exception e)
            {
                exception = e;
            }
            if(!svg.Width.IsEmpty && !svg.Height.IsEmpty)
            {
                return new AnalysisResult(node, $"{svg.Width.Value}×{svg.Height.Value}", exception);
            }
            return new AnalysisResult(node, exception: exception);
        }
    }
}
