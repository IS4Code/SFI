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
                    return result ?? Analyze(node, streams[0].Key, streams[0].Value, entity, nodeFactory);
                }else{
                    for(int i = 0; i < streams.Count; i++)
                    {
                        var stream = streams[i];
                        var streamNode = node[i.ToString()];
                        var label = Analyze(streamNode, stream.Key, stream.Value, entity, nodeFactory);
                        streamNode.SetClass(Classes.MediaStream);
                        node.Set(Properties.HasMediaStream, streamNode);
                        if(label != null)
                        {
                            streamNode.Set(Properties.PrefLabel, label);
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

        string Analyze(ILinkedNode node, Directory dir, IReadOnlyDictionary<string, object> tags, IReadOnlyList<Directory> entity, ILinkedNodeFactory nodeFactory)
        {
            int? width = null, height = null, bits = null;
            if(tags.TryGetValue("Width", out var widthObj) || tags.TryGetValue("Image Width", out widthObj))
            {
                width = Convert.ToInt32(widthObj);
            }
            if(tags.TryGetValue("Height", out var heightObj) || tags.TryGetValue("Image Height", out heightObj))
            {
                height = Convert.ToInt32(heightObj);
            }
            if(tags.TryGetValue("Bits Per Sample", out var bitsObj))
            {
                bits = Convert.ToInt32(bitsObj);
            }

            if(width != null)
            {
                node.Set(Properties.Width, width.GetValueOrDefault());
            }

            if(height != null)
            {
                node.Set(Properties.Height, height.GetValueOrDefault());
            }

            if(bits != null)
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
                node.Set(prop, bits.GetValueOrDefault());
            }

            int? channels = null, rate = null;
            TimeSpan? duration = null;
            if(tags.TryGetValue("Channels", out var channelsObj))
            {
                channels = Convert.ToInt32(channelsObj);
            }
            if(tags.TryGetValue("Sampling Rate", out var rateObj))
            {
                rate = Convert.ToInt32(rateObj);
            }
            if(tags.TryGetValue("Duration", out var durationObj))
            {
                if(TimeSpan.TryParse(durationObj.ToString(), CultureInfo.InvariantCulture, out var value))
                {
                    duration = value;
                }
            }

            if(channels != null)
            {
                node.Set(Properties.Channels, channels.GetValueOrDefault());
            }

            if(rate != null)
            {
                node.Set(Properties.SampleRate, rate.GetValueOrDefault(), Datatypes.Hertz);
            }

            if(duration != null)
            {
                node.Set(Properties.Duration, duration.GetValueOrDefault());
            }

            if(width != null && height != null)
            {
                if(bits != null)
                {
                    return $"{width}x{height}, {bits}bpp";
                }
                return $"{width}x{height}";
            }

            if(rate != null)
            {
                if(channels != null)
                {
                    return $"{rate} Hz, {channels} channels";
                }
                return $"{rate} Hz";
            }
            return null;
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
