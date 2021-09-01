using IS4.MultiArchiver.Formats.Archives;
using IS4.MultiArchiver.Media;
using SharpCompress.Readers.Rar;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class RarFormat : SignatureFormat<IArchiveReader>
    {
        public RarFormat() : base("Rar!", "application/vnd.rar", "rar")
        {

        }

        public override TResult Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IArchiveReader, TResult, TArgs> resultFactory, TArgs args)
        {
            using(var reader = RarReader.Open(stream))
            {
                return resultFactory(new ArchiveReaderAdapter(reader), args);
            }
        }
    }
}
