using SharpCompress.Readers.GZip;
using System;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class GZipFormat : SignatureFormat<GZipReader>
    {
        public GZipFormat() : base(new byte[] { 0x1F, 0x8B }, "application/gzip", "gz")
        {

        }

        public override TResult Match<TResult>(Stream stream, Func<GZipReader, TResult> resultFactory)
        {
            using(var reader = GZipReader.Open(stream))
            {
                return resultFactory(reader);
            }
        }
    }
}
