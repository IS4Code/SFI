using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using SwfDotNet.IO;
using SwfDotNet.IO.Tags;
using System.Linq;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of Adobe Shockwave Flash objects, as instances of <see cref="Swf"/>.
    /// </summary>
    public class ShockwaveFlashAnalyzer : MediaObjectAnalyzer<Swf>, IEntityAnalyzer<DefineBitsJpeg2Tag>, IEntityAnalyzer<DefineSoundTag>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public ShockwaveFlashAnalyzer() : base(Common.ApplicationClasses.Concat(Common.VideoClasses))
        {

        }

        /// <inheritdoc/>
        public override async ValueTask<AnalysisResult> Analyze(Swf swf, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);

            if(swf.Header is SwfHeader header)
            {
                if(IsDefined(header.Width, out var width))
                {
                    node.Set(Properties.Width, width);
                }

                if(IsDefined(header.Height, out var height))
                {
                    node.Set(Properties.Height, height);
                }

                if(IsDefined(header.Frames, out var frames))
                {
                    node.Set(Properties.FrameCount, frames);
                }

                if(IsDefined(header.Fps, out var fps))
                {
                    node.Set(Properties.FrameRate, fps);
                }
            }

            foreach(BaseTag tag in swf.Tags)
            {
                if(tag is DefineTag defineTag)
                {
                    var childNode = (await analyzers.TryAnalyze(defineTag, context.WithParent(node))).Node;
                    if(childNode != null)
                    {
                        node.Set(Properties.HasPart, childNode);
                    }
                }
            }

            return new AnalysisResult(node);
        }

        /// <inheritdoc/>
        public ValueTask<AnalysisResult> Analyze(DefineBitsJpeg2Tag entity, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            return analyzers.Analyze(entity.JpegData, context);
        }

        /// <inheritdoc/>
        public ValueTask<AnalysisResult> Analyze(DefineSoundTag entity, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            return analyzers.Analyze(entity.SoundData, context);
        }
    }
}
