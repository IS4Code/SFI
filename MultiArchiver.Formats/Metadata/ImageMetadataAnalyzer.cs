using IS4.MultiArchiver.Analyzers.MetadataReaders;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using MetadataExtractor;
using MetadataExtractor.Formats.FileType;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace IS4.MultiArchiver.Analyzers
{
    public class ImageMetadataAnalyzer : BinaryFormatAnalyzer<IReadOnlyList<Directory>>
    {
        public ICollection<object> MetadataReaders { get; } = new SortedSet<object>(TypeInheritanceComparer<object>.Instance);

        public override string Analyze(ILinkedNode node, IReadOnlyList<Directory> entity, ILinkedNodeFactory nodeFactory)
        {
            string result = null;
            foreach(var dir in entity)
            {
                if(TryDescribe(node, dir, nodeFactory) is string s)
                {
                    result = s;
                }
            }

            var streams = IdentifyTags(entity, "Width", "Image Width", "Height", "Image Height", "Bits Per Sample", "Channels", "Sampling Rate", "Duration").ToList();
            
            if(streams.Count > 0)
            {
                if(streams.Count == 1)
                {
                    Analyze(node, -1, streams[0].Key, streams[0].Value, entity, nodeFactory);
                }else{
                    for(int i = 0; i < streams.Count; i++)
                    {
                        var streamNode = Analyze(node, i, streams[0].Key, streams[0].Value, entity, nodeFactory);
                        if(streamNode != null)
                        {
                            node.Set(Properties.HasMediaStream, streamNode);
                        }
                    }
                }
            }

            return result;
        }

        static IEnumerable<KeyValuePair<Directory, Dictionary<string, object>>> IdentifyTags(IReadOnlyList<Directory> entity, params string[] tagNames)
        {
            return entity.Select(d => new KeyValuePair<Directory, Dictionary<string, object>>(d, tagNames.Select(n => d.Tags.FirstOrDefault(t => t.Name == n)).Where(n => n != null).ToDictionary(t => t.Name, t => d.GetObject(t.Type))))
                .Where(p => p.Value.Values.Any(v => v != null));
        }

        ILinkedNode Analyze(ILinkedNode parent, int index, Directory dir, IReadOnlyDictionary<string, object> tags, IReadOnlyList<Directory> entity, ILinkedNodeFactory nodeFactory)
        {
            var node = index >= 0 ? parent[index.ToString()] : parent;

            if(tags.TryGetValue("Width", out var width) || tags.TryGetValue("Image Width", out width))
            {
                node.Set(Properties.Width, Convert.ToInt32(width));
            }

            if(tags.TryGetValue("Height", out var height) || tags.TryGetValue("Image Height", out height))
            {
                node.Set(Properties.Height, Convert.ToInt32(height));
            }

            if(tags.TryGetValue("Bits Per Sample", out var bits))
            {
                Properties prop;
                switch(entity.OfType<FileTypeDirectory>().FirstOrDefault()?.GetString(FileTypeDirectory.TagDetectedFileMimeType)?.Substring(0, 6).ToLowerInvariant())
                {
                    case "audio/":
                        prop = Properties.BitsPerSample;
                        break;
                    case "image/":
                        prop = Properties.ColorDepth;
                        break;
                    default:
                        prop = Properties.BitDepth;
                        break;
                }
                node.Set(prop, Convert.ToInt32(bits));
            }

            if(tags.TryGetValue("Channels", out var channels))
            {
                node.Set(Properties.Channels, Convert.ToInt32(channels));
            }

            if(tags.TryGetValue("Sampling Rate", out var rate))
            {
                node.Set(Properties.SampleRate, Convert.ToInt32(rate), Datatypes.Hertz);
            }

            if(tags.TryGetValue("Duration", out var duration))
            {
                if(TimeSpan.TryParse(duration.ToString(), CultureInfo.InvariantCulture, out var value))
                {
                    node.Set(Properties.Duration, value);
                }
            }

            return node;
        }

        public static ImageMetadataAnalyzer CreateDefault()
        {
            var analyzer = new ImageMetadataAnalyzer();

            analyzer.MetadataReaders.Add(new ExifReader());

            return analyzer;
        }

        private string Describe<T>(ILinkedNode node, T dir, ILinkedNodeFactory nodeFactory) where T : Directory
        {
            foreach(var obj in MetadataReaders)
            {
                if(obj is IMetadataReader<T> reader)
                {
                    if(reader.Describe(node, dir, nodeFactory) is string s)
                    {
                        return s;
                    }
                }
            }
            return null;
        }

        private string TryDescribe(ILinkedNode node, Directory dir, ILinkedNodeFactory nodeFactory)
        {
            try
            {
                return Describe(node, (dynamic)dir, nodeFactory);
            }catch(RuntimeBinderException)
            {
                return null;
            }
        }
    }
}
