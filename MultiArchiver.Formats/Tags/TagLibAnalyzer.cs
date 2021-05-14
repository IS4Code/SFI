using IS4.MultiArchiver.Services;
using System;
using TagLib;
using Properties = IS4.MultiArchiver.Vocabulary.Properties;

namespace IS4.MultiArchiver.Analyzers
{
    public class TagLibAnalyzer : BinaryFormatAnalyzer<File>
    {
        public override bool Analyze(ILinkedNode node, File file, ILinkedNodeFactory nodeFactory)
        {
            var properties = file.Properties;
            Set(node, Properties.Width, properties.PhotoWidth);
            Set(node, Properties.Height, properties.PhotoHeight);
            Set(node, Properties.Width, properties.VideoWidth);
            Set(node, Properties.Height, properties.VideoHeight);
            Set(node, Properties.BitsPerSample, properties.BitsPerSample);
            Set(node, Properties.SampleRate, properties.AudioSampleRate);
            Set(node, Properties.Channels, properties.AudioChannels);
            Set(node, Properties.Duration, properties.Duration);
            return false;
        }

        private void Set<T>(ILinkedNode node, Properties prop, T valueOrDefault) where T : struct, IEquatable<T>, IFormattable
        {
            if(valueOrDefault.Equals(default)) return;
            node.Set(prop, valueOrDefault);
        }
    }
}
