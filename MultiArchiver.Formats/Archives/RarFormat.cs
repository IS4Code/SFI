using IS4.MultiArchiver.Services;
using SharpCompress.Readers.Rar;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class RarFormat : SignatureFormat<RarReader>
    {
        public RarFormat() : base("Rar!", "application/vnd.rar", "rar")
        {

        }

        public override TResult Match<TResult>(Stream stream, ResultFactory<RarReader, TResult> resultFactory)
        {
            using(var reader = RarReader.Open(stream))
            {
                return resultFactory(reader);
            }
        }
    }
}
