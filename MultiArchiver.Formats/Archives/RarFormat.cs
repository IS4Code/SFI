using SharpCompress.Readers.Rar;
using System;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class RarFormat : SignatureFormat<RarReader>
    {
        public RarFormat() : base("Rar!", "application/vnd.rar", "rar")
        {

        }

        public override TResult Match<TResult>(Stream stream, Func<RarReader, TResult> resultFactory)
        {
            using(var reader = RarReader.Open(stream))
            {
                return resultFactory(reader);
            }
        }
    }
}
