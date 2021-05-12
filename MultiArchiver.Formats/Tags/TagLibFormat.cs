using IS4.MultiArchiver.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TagLib;

namespace IS4.MultiArchiver.Formats
{
    public class TagLibFormat : BinaryFileFormat<TagLib.File>
    {
        public TagLibFormat() : base(0, null, null)
        {

        }

        static IEnumerable<SupportedMimeType> GetMimeTypes(TagLib.File file)
        {
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

        public override TResult Match<TResult>(Stream stream, Func<TagLib.File, TResult> resultFactory)
        {
            return Match(stream, null, resultFactory);
        }

        public override TResult Match<TResult>(Stream stream, object source, Func<TagLib.File, TResult> resultFactory)
        {
            var file = new File(stream, source);
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

                            return resultFactory(tagFile);
                        }catch(System.Reflection.TargetInvocationException e)
                        {
                            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                            throw;
                        }
                    }
                }
            }
            return null;
        }

        public override bool Match(ArraySegment<byte> header)
        {
            return true;
        }

        public override bool Match(Span<byte> header)
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

            public File(Stream stream, object source) : this(stream, source as IFileNodeInfo)
            {

            }

            public void CloseStream(Stream stream)
            {
                
            }
        }
    }
}
