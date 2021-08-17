using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TagLib;
using Properties = IS4.MultiArchiver.Vocabulary.Properties;

namespace IS4.MultiArchiver.Analyzers
{
    public class TagLibAnalyzer : BinaryFormatAnalyzer<File>, IPropertyUriFormatter<string>
    {
        public override string Analyze(ILinkedNode node, File file, ILinkedNodeFactory nodeFactory)
        {
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

                string result = null;

                if(properties.PhotoWidth != 0 && properties.PhotoHeight != 0)
                {
                    if(properties.BitsPerSample != 0)
                    {
                        result = $"{properties.PhotoWidth}×{properties.PhotoHeight}, {properties.BitsPerSample} bpp";
                    }else{
                        result = $"{properties.PhotoWidth}×{properties.PhotoHeight}";
                    }
                }else if(properties.VideoWidth != 0 && properties.VideoHeight != 0)
                {
                    result = $"{properties.VideoWidth}×{properties.VideoHeight}";
                }else if(properties.AudioSampleRate != 0)
                {
                    if(properties.AudioChannels != 0)
                    {
                        result = $"{properties.AudioSampleRate} Hz, {properties.AudioChannels} channel{(properties.AudioChannels == 1 ? "" : "s")}";
                    }else{
                        result = $"{properties.AudioSampleRate} Hz";
                    }
                }

                if(properties.Codecs.Any())
                {
                    if(!properties.Codecs.Skip(1).Any())
                    {
                        var codec = properties.Codecs.First();
                        result = result ?? Analyze(node, codec, nodeFactory);
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
                            var label = Analyze(codecNode, codec, nodeFactory);
                            codecNode.SetClass(Classes.MediaStream);
                            node.Set(Properties.HasMediaStream, codecNode);
                            if(label != null)
                            {
                                codecNode.Set(Properties.PrefLabel, $"{counter}:{codec.MediaTypes} ({label})");
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

            if(file.Tag != null)
            {
                foreach(var prop in file.Tag.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if(!propertyNames.TryGetValue(prop.Name, out var name)) continue;
                    var value = prop.GetValue(file.Tag);
                    if(value is ICollection collection)
                    {
                        foreach(var elem in collection)
                        {
                            try{
                                node.Set(this, name, (dynamic)elem);
                            }catch(RuntimeBinderException)
                            {

                            }
                        }
                        continue;
                    }
                    if(value == null || "".Equals(value) || Double.NaN.Equals(value) || 0.Equals(value) || 0u.Equals(value) || false.Equals(value)) continue;
                    try{
                        node.Set(this, name, (dynamic)value);
                    }catch(RuntimeBinderException)
                    {

                    }
                }
            }

            return null;
        }

        static readonly Dictionary<string, string> propertyNames = new Dictionary<string, string>
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

        Uri IUriFormatter<string>.FormatUri(string name)
        {
            return new Uri($"http://www.semanticdesktop.org/ontologies/2007/05/10/nid3#{name}", UriKind.Absolute);
        }

        string Analyze(ILinkedNode node, ICodec codec, ILinkedNodeFactory nodeFactory)
        {
            string result = null;
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
