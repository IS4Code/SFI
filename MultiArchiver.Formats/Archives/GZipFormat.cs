using IS4.MultiArchiver.Services;
using SharpCompress.Readers.GZip;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class GZipFormat : SignatureFormat<GZipReader>
    {
        public GZipFormat() : base(new byte[] { 0x1F, 0x8B }, "application/gzip", "gz")
        {

        }

        public override TResult Match<TResult, TArgs>(Stream stream, ResultFactory<GZipReader, TResult, TArgs> resultFactory, TArgs args)
        {
            using(var reader = GZipReader.Open(stream))
            {
                return resultFactory(reader, args);
            }
        }
    }
}
