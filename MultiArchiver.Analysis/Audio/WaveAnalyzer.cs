using IS4.MultiArchiver.Analysis.Audio;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Drawing;

namespace IS4.MultiArchiver.Analyzers
{
    public class WaveAnalyzer : BinaryFormatAnalyzer<WaveStream>
    {
        readonly PolarSpectrumGenerator generator = new PolarSpectrumGenerator(512, 512);

        public override string Analyze(ILinkedNode parent, ILinkedNode node, WaveStream wave, object source, ILinkedNodeFactory nodeFactory)
        {
            node.Set(Properties.Duration, wave.TotalTime);
            node.Set(Properties.Channels, wave.WaveFormat.Channels);
            node.Set(Properties.BitsPerSample, wave.WaveFormat.BitsPerSample);
            node.Set(Properties.SampleRate, wave.WaveFormat.SampleRate, Datatypes.Hertz);

            if(wave.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
            {
                var provider = new Pcm16BitToSampleProvider(wave);

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
                        return null;
                }

                bmp.Tag = new ImageTag
                {
                    StoreDimensions = false
                };

                var imageObj = new LinkedObject<Image>(node, source, bmp);
                nodeFactory.Create<ILinkedObject<Image>>(parent, imageObj);
            }

            return null;
        }
    }
}
