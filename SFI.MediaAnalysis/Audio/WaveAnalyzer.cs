using IS4.SFI.MediaAnalysis.Audio;
using IS4.SFI.Formats;
using IS4.SFI.Services;
using IS4.SFI.Tags;
using IS4.SFI.Vocabulary;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Drawing;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of audio as instances of <see cref="WaveStream"/>.
    /// </summary>
    public class WaveAnalyzer : MediaObjectAnalyzer<WaveStream>
    {
        readonly PolarSpectrumGenerator generator = new PolarSpectrumGenerator(512, 512);

        /// <summary>
        /// Whether to produce spectrograms from the audio.
        /// </summary>
        public bool CreateSpectrum { get; set; } = true;

        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public WaveAnalyzer() : base(Common.AudioClasses)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(WaveStream wave, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            if(wave is ICustomWaveFormat customFormat)
            {
                if(customFormat.ChannelCount is int channels)
                {
                    node.Set(Properties.Channels, channels);
                }
                if(customFormat.BitsPerSample is int bits)
                {
                    node.Set(Properties.BitsPerSample, bits);
                }
                if(customFormat.SampleRate is int sampleRate)
                {
                    node.Set(Properties.SampleRate, sampleRate);
                }
            }else{
                node.Set(Properties.Channels, wave.WaveFormat.Channels);
                node.Set(Properties.BitsPerSample, wave.WaveFormat.BitsPerSample);
                node.Set(Properties.SampleRate, wave.WaveFormat.SampleRate, Datatypes.Hertz);
            }

            node.Set(Properties.Duration, wave.TotalTime);

            ISampleProvider? provider = null;
            switch(wave.WaveFormat.Encoding)
            {
                case WaveFormatEncoding.Pcm:
                    switch(wave.WaveFormat.BitsPerSample)
                    {
                        case 8:
                            provider = new Pcm8BitToSampleProvider(wave);
                            break;
                        case 16:
                            provider = new Pcm16BitToSampleProvider(wave);
                            break;
                        case 24:
                            provider = new Pcm24BitToSampleProvider(wave);
                            break;
                        case 32:
                            provider = new Pcm32BitToSampleProvider(wave);
                            break;
                    }
                    break;
                case WaveFormatEncoding.IeeeFloat:
                    provider = new WaveToSampleProvider(wave);
                    break;
            }
            if(provider != null && CreateSpectrum && PolarSpectrumGenerator.IsSupported)
            {
                var result = generator.CreateSpectrum(wave.WaveFormat.SampleRate, wave.WaveFormat.Channels, provider);

                Bitmap bmp;
                switch(wave.WaveFormat.Channels)
                {
                    case 1:
                        bmp = generator.DrawMono(result[0]);
                        break;
                    case 2:
                        bmp = generator.DrawStereo(result[0], result[1]);
                        break;
                    default:
                        return default;
                }

                bmp.Tag = new ImageTag
                {
                    StoreDimensions = false,
                    HighFrequencyHash = false,
                    ByteHash = false
                };

                await analyzers.Analyze(bmp, context.AsInitialized());
            }

            return new AnalysisResult(node);
        }
    }
}
