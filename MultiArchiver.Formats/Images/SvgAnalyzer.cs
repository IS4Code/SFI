using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tags;
using IS4.MultiArchiver.Vocabulary;
using Svg;
using System;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    /// <summary>
    /// Analyzes SVG images as instances of <see cref="SvgDocument"/>.
    /// </summary>
    public class SvgAnalyzer : MediaObjectAnalyzer<SvgDocument>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public SvgAnalyzer() : base(Common.ImageClasses)
        {

        }

        public async override ValueTask<AnalysisResult> Analyze(SvgDocument svg, AnalysisContext context, IEntityAnalyzers analyzers)
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
                    exception = (await analyzers.Analyze(bmp, context)).Exception;
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
