using IS4.MultiArchiver.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TagLib;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// Represents tagged formats, as instances of <see cref="TagLib.File"/>.
    /// </summary>
    public class TagLibFormat : BinaryFileFormat<TagLib.File>
    {
        public TagLibFormat() : base(0, null, null)
        {

        }

        static IEnumerable<SupportedMimeType> GetMimeTypes(TagLib.File file)
        {
            if(file is TagLib.Image.NoMetadata.File) return Array.Empty<SupportedMimeType>();
            if(file is TagLib.Riff.File riff)
            {
                if(file.Properties.Codecs.All(c => c is TagLib.Riff.WaveFormatEx))
                {
                    return new[] { new SupportedMimeType("audio/vnd.wave", "wav") };
                }
                if(file.Properties.Codecs.Any(c => c is TagLib.Riff.BitmapInfoHeader))
                {
                    return new[] { new SupportedMimeType("video/vnd.avi", "avi") };
                }
                return Array.Empty<SupportedMimeType>();
            }
            return file.GetType().GetCustomAttributes(typeof(SupportedMimeType), true).OfType<SupportedMimeType>();
        }

        public override string GetExtension(TagLib.File file)
        {
            var attributes = GetMimeTypes(file).Select(a => a.Extension);
            return attributes.FirstOrDefault(e => !String.IsNullOrEmpty(e));
        }

        public override string GetMediaType(TagLib.File file)
        {
            if(String.IsNullOrEmpty(file.MimeType))
            {
                var attributes = GetMimeTypes(file).Select(a => a.MimeType);
                attributes = attributes.Where(m => !m.StartsWith("taglib/", StringComparison.OrdinalIgnoreCase));
                attributes = attributes.Where(m => m.Contains("/"));
                attributes = attributes.OrderBy(m => m.Substring(m.IndexOf('/') + 1).StartsWith("x-", StringComparison.OrdinalIgnoreCase));
                return attributes.FirstOrDefault();
            }
            return file.MimeType;
        }

        public async override ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<TagLib.File, TResult, TArgs> resultFactory, TArgs args)
        {
            var file = new File(stream, context);
            if(file.Name != null)
            {
                var ext = Path.GetExtension(file.Name);
                if(ext.StartsWith(".", StringComparison.Ordinal))
                {
                    var type = "taglib/" + ext.Substring(1).ToLowerInvariant();
                    if(FileTypes.AvailableTypes.TryGetValue(type, out var fileType))
                    {
                        try{
                            var tagFile = (TagLib.File)Activator.CreateInstance(fileType, file, ReadStyle.Average);

                            if(tagFile.Properties != null || (tagFile.Tag != null && tagFile.TagTypes != TagTypes.None))
                            {
                                return await resultFactory(tagFile, args);
                            }
                        }catch(System.Reflection.TargetInvocationException e)
                        {
                            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                            throw;
                        }
                    }
                }
            }
            return default;
        }

        public override bool CheckHeader(ArraySegment<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            return true;
        }

        public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            return true;
        }

        protected class File : TagLib.File.IFileAbstraction
        {
            public string Name { get; }

            public Stream ReadStream { get; }

            public Stream WriteStream => throw new NotSupportedException();

            public File(Stream stream, string name)
            {
                Name = name;
                ReadStream = stream;
            }

            public File(Stream stream, IFileNodeInfo source) : this(stream, source?.Name)
            {

            }

            public File(Stream stream, MatchContext context) : this(stream, context.GetService<IFileNodeInfo>())
            {

            }

            public void CloseStream(Stream stream)
            {
                
            }
        }
    }
}
