using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TagLib;
using TagLib.Image;
using File = TagLib.File;
using Properties = IS4.SFI.Vocabulary.Properties;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// Analyzes instances of <see cref="File"/> as containers of tags.
    /// </summary>
    [Description("Analyzes containers of tags.")]
    public class TagLibAnalyzer : MediaObjectAnalyzer<File>, IPropertyUriFormatter<string>
    {
        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(File file, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);

            var components = new List<string?>();

            var properties = file.Properties;
            if(properties != null)
            {
                Set(node, Properties.Width, properties.PhotoWidth);
                Set(node, Properties.Height, properties.PhotoHeight);
                Set(node, Properties.Width, properties.VideoWidth);
                Set(node, Properties.Height, properties.VideoHeight);
                Set(node, Properties.BitsPerSample, properties.BitsPerSample);
                Set(node, Properties.SampleRate, properties.AudioSampleRate, Datatypes.Hertz);
                Set(node, Properties.Channels, properties.AudioChannels);
                Set(node, Properties.Duration, properties.Duration);

                string? typeLabel = null;
                if(properties.PhotoWidth != 0 && properties.PhotoHeight != 0)
                {
                    if(properties.BitsPerSample != 0)
                    {
                        typeLabel = $"{properties.PhotoWidth}×{properties.PhotoHeight}, {properties.BitsPerSample} bpp";
                    }else{
                        typeLabel = $"{properties.PhotoWidth}×{properties.PhotoHeight}";
                    }
                }else if(properties.VideoWidth != 0 && properties.VideoHeight != 0)
                {
                    typeLabel = $"{properties.VideoWidth}×{properties.VideoHeight}";
                }else if(properties.AudioSampleRate != 0)
                {
                    if(properties.AudioChannels != 0)
                    {
                        typeLabel = $"{properties.AudioSampleRate} Hz, {properties.AudioChannels} channel{(properties.AudioChannels == 1 ? "" : "s")}";
                    }else{
                        typeLabel = $"{properties.AudioSampleRate} Hz";
                    }
                }
                components.Add(typeLabel);

                if(properties.Codecs.Any())
                {
                    if(!properties.Codecs.Skip(1).Any())
                    {
                        var codec = properties.Codecs.First();
                        components.Add(Analyze(node, codec));
                    }else{
                        var codecCounters = new Dictionary<MediaTypes, int>();

                        foreach(var codec in properties.Codecs)
                        {
                            if(codec == null) continue;
                            if(!codecCounters.TryGetValue(codec.MediaTypes, out int counter))
                            {
                                counter = 0;
                            }
                            codecCounters[codec.MediaTypes] = counter + 1;

                            var codecNode = node[codec.MediaTypes.ToString() + "/" + counter];
                            var codecLabel = Analyze(codecNode, codec);
                            codecNode.SetClass(Classes.MediaStream);
                            node.Set(Properties.HasMediaStream, codecNode);
                            if(codecLabel != null)
                            {
                                codecNode.Set(Properties.PrefLabel, $"{counter}:{codec.MediaTypes} ({codecLabel})");
                            }else{
                                codecNode.Set(Properties.PrefLabel, $"{counter}:{codec.MediaTypes}");
                            }
                        }
                    }
                }
            }

            if((file.TagTypes & (TagTypes.Id3v1 | TagTypes.Id3v2)) != 0)
            {
                if(
                    (file.GetTag(TagTypes.Id3v1) is IEnumerable<object> e1 && e1.Any()) ||
                    (file.GetTag(TagTypes.Id3v2) is IEnumerable<object> e2 && e2.Any()))
                {
                    node.SetClass(Classes.ID3Audio);
                }
            }

            if(file.Tag is Tag tag)
            {
                components.Add(tag.Title);

                foreach(var prop in tag.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if(!propertyNames.TryGetValue(prop.Name, out var name)) continue;
                    var value = prop.GetValue(tag);
                    if(value is ICollection collection)
                    {
                        foreach(var elem in collection)
                        {
                            Set(node, name, elem);
                        }
                        continue;
                    }
                    if(value == null || "".Equals(value) || Double.NaN.Equals(value) || 0.Equals(value) || 0u.Equals(value) || false.Equals(value)) continue;
                    Set(node, name, value);
                }

                foreach(var inner in GetTags(tag))
                {
                    if((await analyzers.TryAnalyze(inner, context.WithNode(node))).Label is string s)
                    {
                        components.Add(s);
                    }
                }
            }

            return new AnalysisResult(node, components.JoinAsLabel());
        }

        void Set(ILinkedNode node, string name, object value)
        {
            switch(value)
            {
                case ValueType simpleValue:
                    node.TrySet(this, name, simpleValue);
                    break;
                case Uri uriValue:
                    node.Set(this, name, uriValue);
                    break;
                case string stringValue:
                    node.Set(this, name, stringValue);
                    break;
            }
        }

        static IEnumerable<Tag> GetTags(Tag tag)
        {
            switch(tag)
            {
                case CombinedTag combined:
                    return combined.Tags.SelectMany(GetTags);
                case CombinedImageTag combinedImage:
                    return combinedImage.AllTags.SelectMany(GetTags);
                default:
                    return new[] { tag };
            }
        }

        static readonly Dictionary<string, string> propertyNames = new()
        {
            { nameof(Tag.Album), "albumTitle" },
            { nameof(Tag.BeatsPerMinute), "beatsPerMinute" },
            { nameof(Tag.Composers), "composer" },
            { nameof(Tag.Conductor), "conductor" },
            { nameof(Tag.Performers), "leadArtist" },
            { nameof(Tag.Genres), "contentType" },
            { nameof(Tag.Track), "trackNumber" },
            { nameof(Tag.Disc), "partOfSet" },
            { nameof(Tag.Grouping), "contentGroupDescription" },
            { nameof(Tag.AlbumArtists), "backgroundArtist" },
            { nameof(Tag.Comment), "comments" },
            { nameof(Tag.Copyright), "copyrightMessage" },
            { nameof(Tag.DateTagged), "comments" },
            { nameof(Tag.InitialKey), "initialKey" },
            { nameof(Tag.ISRC), "internationalStandardRecordingCode" },
            { nameof(Tag.RemixedBy), "interpretedBy" },
            { nameof(Tag.Publisher), "publisher" },
            { nameof(Tag.Subtitle), "subtitle" },
            { nameof(Tag.Title), "title" },
            { nameof(Tag.Year), "recordingYear" },

        };

        Uri IUriFormatter<string>.this[string name] => new(Vocabularies.Uri.Nid3 + name, UriKind.Absolute);

        string? Analyze(ILinkedNode node, ICodec codec)
        {
            string? result = null;
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
                Set(node, Properties.AverageBitrate, audio.AudioBitrate, Datatypes.KilobitPerSecond);
                Set(node, Properties.Channels, audio.AudioChannels);
                Set(node, Properties.SampleRate, audio.AudioSampleRate, Datatypes.Hertz);
                if(audio.AudioSampleRate != 0)
                {
                    if(audio.AudioChannels != 0)
                    {
                        result = $"{audio.AudioSampleRate} Hz, {audio.AudioChannels} channel{(audio.AudioChannels == 1 ? "" : "s")}";
                    }else{
                        result = $"{audio.AudioSampleRate} Hz";
                    }
                }
            }
            if(codec is IVideoCodec video)
            {
                node.SetClass(Classes.Video);
                Set(node, Properties.Width, video.VideoWidth);
                Set(node, Properties.Height, video.VideoHeight);

                if(video.VideoWidth != 0 && video.VideoHeight != 0)
                {
                    result = $"{video.VideoWidth}×{video.VideoHeight}";
                }
            }
            if(codec is IPhotoCodec photo)
            {
                node.SetClass(Classes.Image);
                Set(node, Properties.Width, photo.PhotoWidth);
                Set(node, Properties.Height, photo.PhotoHeight);

                if(photo.PhotoWidth != 0 && photo.PhotoHeight != 0)
                {
                    result = $"{photo.PhotoWidth}×{photo.PhotoHeight}";
                }
            }
            return result;
        }

        private void Set<T>(ILinkedNode node, PropertyUri prop, T valueOrDefault) where T : struct, IEquatable<T>, IFormattable
        {
            if(valueOrDefault.Equals(default)) return;
            node.Set(prop, valueOrDefault);
        }

        private void Set<T>(ILinkedNode node, PropertyUri prop, T valueOrDefault, DatatypeUri datatype) where T : struct, IEquatable<T>, IFormattable
        {
            if(valueOrDefault.Equals(default)) return;
            node.Set(prop, valueOrDefault, datatype);
        }
    }
}
