using IS4.MultiArchiver.Analysis.Audio;
using IS4.MultiArchiver.Media;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tags;
using IS4.MultiArchiver.Vocabulary;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Drawing;

namespace IS4.MultiArchiver.Analyzers
{
    public class WaveAnalyzer : BinaryFormatAnalyzer<WaveStream>
    {
        readonly PolarSpectrumGenerator generator = new PolarSpectrumGenerator(512, 512);

        public WaveAnalyzer() : base(Common.AudioClasses)
        {

        }

        public override string Analyze(ILinkedNode parent, ILinkedNode node, WaveStream wave, object source, ILinkedNodeFactory nodeFactory)
        {
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

            ISampleProvider provider = null;
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
            if(provider != null)
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
                        return null;
                }

                bmp.Tag = new ImageTag
                {
                    StoreDimensions = false,
                    HighFrequencyHash = false,
                    ByteHash = false
                };

                var imageObj = new LinkedObject<Image>(node, source, bmp);
                nodeFactory.Create<ILinkedObject<Image>>(parent, imageObj);
            }

            return null;
        }
    }
}
