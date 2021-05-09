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

        public override bool Analyze(ILinkedNode node, IReadOnlyList<Directory> entity, ILinkedNodeFactory nodeFactory)
        {
            bool result = false;
            foreach(var dir in entity)
            {
                if(TryDescribe(node, dir, nodeFactory))
                {
                    result = true;
                }

                if(dir.Tags.FirstOrDefault(t => t.Name == "Width" || t.Name == "Image Width") is Tag width)
                {
                    if(dir.TryGetInt32(width.Type, out var value))
                    {
                        node.Set(Properties.Width, value);
                    }
                }
                if(dir.Tags.FirstOrDefault(t => t.Name == "Height" || t.Name == "Image Height") is Tag height)
                {
                    if(dir.TryGetInt32(height.Type, out var value))
                    {
                        node.Set(Properties.Height, value);
                    }
                }
                if(dir.Tags.FirstOrDefault(t => t.Name == "Bits Per Sample") is Tag bits)
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
                    if(dir.TryGetInt32(bits.Type, out var value))
                    {
                        node.Set(prop, value);
                    }
                }
                if(dir.Tags.FirstOrDefault(t => t.Name == "Channels") is Tag channels)
                {
                    if(dir.TryGetInt32(channels.Type, out var value))
                    {
                        node.Set(Properties.Channels, value);
                    }
                }
                if(dir.Tags.FirstOrDefault(t => t.Name == "Sampling Rate") is Tag rate)
                {
                    if(dir.TryGetInt32(rate.Type, out var value))
                    {
                        node.Set(Properties.SampleRate, value);
                    }
                }
                if(dir.Tags.FirstOrDefault(t => t.Name == "Duration") is Tag duration)
                {
                    if(TimeSpan.TryParse(dir.GetString(duration.Type), CultureInfo.InvariantCulture, out var value))
                    {
                        node.Set(Properties.Duration, value);
                    }
                }
            }
            return result;
        }

        public static ImageMetadataAnalyzer CreateDefault()
        {
            var analyzer = new ImageMetadataAnalyzer();

            analyzer.MetadataReaders.Add(new ExifReader());

            return analyzer;
        }

        private bool Describe<T>(ILinkedNode node, T dir, ILinkedNodeFactory nodeFactory) where T : Directory
        {
            foreach(var obj in MetadataReaders)
            {
                if(obj is IMetadataReader<T> reader)
                {
                    if(reader.Describe(node, dir, nodeFactory))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool TryDescribe(ILinkedNode node, Directory dir, ILinkedNodeFactory nodeFactory)
        {
            try
            {
                return Describe(node, (dynamic)dir, nodeFactory);
            }catch(RuntimeBinderException)
            {
                return false;
            }
        }
    }
}
