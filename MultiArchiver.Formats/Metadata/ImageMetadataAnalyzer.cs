using IS4.MultiArchiver.Formats.Metadata;
using IS4.MultiArchiver.Formats.Metadata.MetadataReaders;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using MetadataExtractor;
using MetadataExtractor.Formats.FileType;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

            var streams = IdentifyTags(entity, "Width", "Height", "Bits Per Sample", "Channels", "Sampling Rate", "Duration").ToList();
            
            if(streams.Count > 0)
            {
                if(streams.Count == 1)
                {
                    return result ?? Analyze(node, streams[0].Value, entity, nodeFactory);
                }else{
                    var merged = new Dictionary<string, object>();

                    for(int i = 0; i < streams.Count; i++)
                    {
                        var stream = streams[i];
                        var streamNode = node[i.ToString()];
                        var label = Analyze(streamNode, stream.Value, entity, nodeFactory);
                        streamNode.SetClass(Classes.MediaStream);
                        node.Set(Properties.HasMediaStream, streamNode);
                        if(label != null)
                        {
                            streamNode.Set(Properties.PrefLabel, $"{i}:{stream.Key.Name} ({label})");
                        }else{
                            streamNode.Set(Properties.PrefLabel, $"{i}:{stream.Key.Name}");
                        }

                        foreach(var pair in stream.Value)
                        {
                            if(merged.TryGetValue(pair.Key, out var existing))
                            {
                                if(existing != null && !existing.Equals(pair.Value))
                                {
                                    merged[pair.Key] = null;
                                }
                            }else{
                                merged[pair.Key] = pair.Value;
                            }
                        }
                    }

                    return result ?? Analyze(node, merged, entity, nodeFactory);
                }
            }

            return result;
        }

        static IEnumerable<KeyValuePair<Directory, Dictionary<string, object>>> IdentifyTags(IReadOnlyList<Directory> entity, params string[] tagNames)
        {
            return entity.Select(d => new KeyValuePair<Directory, Dictionary<string, object>>(d, tagNames.Select(n => (n, t: d.Tags.FirstOrDefault(t => t.Name.EndsWith(n, StringComparison.OrdinalIgnoreCase)))).Where(p => p.t != null).ToDictionary(p => p.n, p => d.GetObject(p.t.Type))))
                .Where(p => p.Value.Values.Any(v => v != null));
        }

        static bool TryGetValue<T>(IReadOnlyDictionary<string, object> tags, string key, out T? result) where T : struct
        {
            if(tags.TryGetValue(key, out var obj) && obj != null)
            {
                try{
                    result = (T)Convert.ChangeType(obj, typeof(T), CultureInfo.InvariantCulture);
                    return true;
                }catch(FormatException)
                {

                }catch(InvalidCastException)
                {

                }
                try{
                    var converter = TypeDescriptor.GetConverter(typeof(T));
                    if(converter.CanConvertFrom(obj.GetType()))
                    {
                        result = (T)converter.ConvertFrom(null, CultureInfo.InvariantCulture, obj);
                        return true;
                    }
                }catch(FormatException)
                {

                }catch(NotSupportedException)
                {

                }catch(InvalidCastException)
                {

                }
            }
            result = null;
            return false;
        }

        string Analyze(ILinkedNode node, IReadOnlyDictionary<string, object> tags, IReadOnlyList<Directory> entity, ILinkedNodeFactory nodeFactory)
        {
            if(TryGetValue<int>(tags, "Width", out var width))
            {
                node.Set(Properties.Width, width.GetValueOrDefault());
            }

            if(TryGetValue<int>(tags, "Height", out var height))
            {
                node.Set(Properties.Height, height.GetValueOrDefault());
            }

            if(TryGetValue<int>(tags, "Bits Per Sample", out var bits))
            {
                switch(entity.OfType<FileTypeDirectory>().FirstOrDefault()?.GetString(FileTypeDirectory.TagDetectedFileMimeType)?.Substring(0, 6).ToLowerInvariant())
                {
                    case "audio/":
                        node.Set(Properties.BitsPerSample, bits.GetValueOrDefault());
                        break;
                    case "image/":
                        //node.Set(Properties.ColorDepth, bits.GetValueOrDefault());
                        break;
                    default:
                        node.Set(Properties.BitDepth, bits.GetValueOrDefault());
                        break;
                }
            }

            if(TryGetValue<int>(tags, "Channels", out var channels))
            {
                node.Set(Properties.Channels, channels.GetValueOrDefault());
            }

            if(TryGetValue<int>(tags, "Sampling Rate", out var rate))
            {
                node.Set(Properties.SampleRate, rate.GetValueOrDefault(), Datatypes.Hertz);
            }

            if(TryGetValue<TimeSpan>(tags, "Duration", out var duration))
            {
                node.Set(Properties.Duration, duration.GetValueOrDefault());
            }

            var components = new List<string>();

            if(width != null && height != null)
            {
                components.Add($"{width}×{height}");
            }

            if(bits != null)
            {
                components.Add($"{bits}-bit");
            }

            if(rate != null)
            {
                components.Add($"{rate} Hz");
            }

            if(channels != null)
            {
                components.Add($"{channels} channel{(channels == 1 ? "" : "s")}");
            }

            return components.Count > 0 ? String.Join(", ", components) : null;
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
