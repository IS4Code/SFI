using IS4.MultiArchiver.Services;
using System;
using System.IO;
using TagLib;

namespace IS4.MultiArchiver.Formats
{
    public class TagLibFormat : BinaryFileFormat<TagLib.File>
    {
        public TagLibFormat() : base(0, null, null)
        {

        }

        /*public override string GetExtension(TagLib.File file)
        {
            
        }*/

        public override string GetMediaType(TagLib.File file)
        {
            return file.MimeType;
        }

        public override TResult Match<TResult>(Stream stream, Func<TagLib.File, TResult> resultFactory)
        {
            return Match(stream, null, resultFactory);
        }

        public override TResult Match<TResult>(Stream stream, object source, Func<TagLib.File, TResult> resultFactory)
        {
            return resultFactory(TagLib.File.Create(new File(stream, source)));
        }

        public override bool Match(ArraySegment<byte> header)
        {
            return true;
        }

        public override bool Match(Span<byte> header)
        {
            return true;
        }

        class File : TagLib.File.IFileAbstraction
        {
            public string Name { get; }

            public Stream ReadStream { get; }

            public Stream WriteStream => throw new NotSupportedException();

            public File(Stream stream, object source)
            {
                Name = source is IFileNodeInfo file ? file.Name : "";
                ReadStream = stream;
            }

            public void CloseStream(Stream stream)
            {
                
            }
        }
    }
}
