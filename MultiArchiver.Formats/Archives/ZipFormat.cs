using IS4.MultiArchiver.Services;
using SharpCompress.Readers.Zip;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class ZipFormat : SignatureFormat<ZipReader>
    {
        public ZipFormat() : base("PK", "application/zip", "zip")
        {

        }

        public override TResult Match<TResult, TArgs>(Stream stream, ResultFactory<ZipReader, TResult, TArgs> resultFactory, TArgs args)
        {
            using(var reader = ZipReader.Open(stream))
            {
                return resultFactory(reader, args);
            }
        }
    }
}
