﻿using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using MetadataExtractor;
using MetadataExtractor.Formats.FileType;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// Analyzes image metadata represented by collections of <see cref="Directory"/>.
    /// </summary>
    [Description("Analyzes image metadata.")]
    public class ImageMetadataAnalyzer : MediaObjectAnalyzer<IReadOnlyList<Directory>>
    {
        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(IReadOnlyList<Directory> entity, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);

            var components = new List<string?>();

            var streams = IdentifyTags(entity, "Width", "Height", "Bits Per Sample", "Channels", "Sampling Rate", "Duration").ToList();
            
            if(streams.Count > 0)
            {
                if(streams.Count == 1)
                {
                    components.Add(Analyze(node, streams[0].Value, entity));
                }else{
                    var merged = new Dictionary<string, object?>();

                    for(int i = 0; i < streams.Count; i++)
                    {
                        var stream = streams[i];
                        var streamNode = node[i.ToString()];
                        var streamLabel = Analyze(streamNode, stream.Value, entity);
                        streamNode.SetClass(Classes.MediaStream);
                        node.Set(Properties.HasMediaStream, streamNode);
                        if(streamLabel != null)
                        {
                            streamNode.Set(Properties.PrefLabel, $"{i}:{stream.Key.Name} ({streamLabel})");
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

                    components.Add(Analyze(node, merged, entity));
                }
            }

            foreach(var dir in entity)
            {
                if(dir is FileTypeDirectory)
                {
                    continue;
                }
                if((await analyzers.TryAnalyze(dir, context.WithNode(node))).Label is string s)
                {
                    components.Add(s);
                }
            }

            return new AnalysisResult(node, components.JoinAsLabel());
        }

        static IEnumerable<KeyValuePair<Directory, Dictionary<string, object?>>> IdentifyTags(IReadOnlyList<Directory> entity, params string[] tagNames)
        {
            return entity.Select(d => new KeyValuePair<Directory, Dictionary<string, object?>>(d, tagNames.Select(n => (n, t: d.Tags.FirstOrDefault(t => t.Name.EndsWith(n, StringComparison.OrdinalIgnoreCase)))).Where(p => p.t != null).ToDictionary(p => p.n, p => d.GetObject(p.t.Type))))
                .Where(p => p.Value.Values.Any(v => v != null));
        }

        static bool TryGetValue<T>(IReadOnlyDictionary<string, object?> tags, string key, out T? result) where T : struct
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

        string? Analyze(ILinkedNode node, IReadOnlyDictionary<string, object?> tags, IReadOnlyList<Directory> entity)
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

            return components.JoinAsLabel();
        }
    }
}
