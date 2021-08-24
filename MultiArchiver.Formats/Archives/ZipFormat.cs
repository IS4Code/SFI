using IS4.MultiArchiver.Formats.Archives;
using IS4.MultiArchiver.Media;
using IS4.MultiArchiver.Services;
using SharpCompress.Readers.Zip;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class ZipFormat : SignatureFormat<IArchiveReader>
    {
        public ZipFormat() : base("PK", "application/zip", "zip")
        {

        }

        public override TResult Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IArchiveReader, TResult, TArgs> resultFactory, TArgs args)
        {
            using(var reader = ZipReader.Open(stream))
            {
                return resultFactory(new ArchiveReaderAdapter(reader), args);
            }
        }
    }
}
