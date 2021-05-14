using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TagLib;
using Properties = IS4.MultiArchiver.Vocabulary.Properties;

namespace IS4.MultiArchiver.Analyzers
{
    public class TagLibAnalyzer : BinaryFormatAnalyzer<File>, IEntityAnalyzer<ICodec>
    {
        static readonly ConditionalWeakTable<ICodec, string> codecPosition = new ConditionalWeakTable<ICodec, string>();

        public override string Analyze(ILinkedNode node, File file, ILinkedNodeFactory nodeFactory)
        {
            var properties = file.Properties;
            Set(node, Properties.Width, properties.PhotoWidth);
            Set(node, Properties.Height, properties.PhotoHeight);
            Set(node, Properties.Width, properties.VideoWidth);
            Set(node, Properties.Height, properties.VideoHeight);
            Set(node, Properties.BitsPerSample, properties.BitsPerSample);
            Set(node, Properties.SampleRate, properties.AudioSampleRate, Datatypes.Hertz);
            Set(node, Properties.Channels, properties.AudioChannels);
            Set(node, Properties.Duration, properties.Duration);

            var codecCounters = new Dictionary<char, int>();

            foreach(var codec in properties.Codecs)
            {
                char streamType;
                switch(codec.MediaTypes)
                {
                    case MediaTypes.Video:
                        streamType = 'v';
                        break;
                    case MediaTypes.Audio:
                        streamType = 'a';
                        break;
                    case MediaTypes.Photo:
                        streamType = 't';
                        break;
                    case MediaTypes.Text:
                        streamType = 's';
                        break;
                    default:
                        continue;
                }
                if(!codecCounters.TryGetValue(streamType, out int counter))
                {
                    counter = 0;
                }
                codecCounters[streamType] = counter + 1;
                codecPosition.Add(codec, streamType + ":" + counter);

                var codecNode = nodeFactory.Create(node, codec);
                if(codecNode != null)
                {
                    node.Set(Properties.HasMediaStream, codecNode);
                }
            }

            return null;
        }

        public ILinkedNode Analyze(ILinkedNode parent, ICodec codec, ILinkedNodeFactory nodeFactory)
        {
            if(!codecPosition.TryGetValue(codec, out var pos)) return null;
            var node = parent[pos];
            if(node != null)
            {
                node.SetClass(Classes.MediaStream);
                Set(node, Properties.Duration, codec.Duration);
                if(codec is ILosslessAudioCodec losslessAudio)
                {
                    node.SetClass(Classes.Audio);
                    Set(node, Properties.BitsPerSample, losslessAudio.BitsPerSample);
                    node.Set(Properties.CompressionType, Individuals.LosslessCompressionType);
                }
                if(codec is IAudioCodec audio)
                {
                    node.SetClass(Classes.Audio);
                    Set(node, Properties.AverageBitrate, audio.AudioBitrate, Datatypes.BitPerSecond);
                    Set(node, Properties.Channels, audio.AudioChannels);
                    Set(node, Properties.SampleRate, audio.AudioSampleRate, Datatypes.Hertz);
                }
                if(codec is IVideoCodec video)
                {
                    node.SetClass(Classes.Video);
                    Set(node, Properties.Width, video.VideoWidth);
                    Set(node, Properties.Height, video.VideoHeight);
                }
                if(codec is IPhotoCodec photo)
                {
                    node.SetClass(Classes.Image);
                    Set(node, Properties.Width, photo.PhotoWidth);
                    Set(node, Properties.Height, photo.PhotoHeight);
                }
            }
            return node;
        }

        private void Set<T>(ILinkedNode node, Properties prop, T valueOrDefault) where T : struct, IEquatable<T>, IFormattable
        {
            if(valueOrDefault.Equals(default)) return;
            node.Set(prop, valueOrDefault);
        }

        private void Set<T>(ILinkedNode node, Properties prop, T valueOrDefault, Datatypes datatype) where T : struct, IEquatable<T>, IFormattable
        {
            if(valueOrDefault.Equals(default)) return;
            node.Set(prop, valueOrDefault, datatype);
        }
    }
}
