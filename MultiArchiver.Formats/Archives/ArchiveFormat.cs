using IS4.MultiArchiver.Services;
using SharpCompress.Archives;
using System;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class ArchiveFormat : FileFormat<IArchive>
    {
        public ArchiveFormat() : base(0, null, null)
        {

        }

        public override bool Match(Span<byte> header)
        {
            return true;
        }

        public override TResult Match<TResult>(Stream stream, Func<IArchive, TResult> resultFactory)
        {
            using(var archive = ArchiveFactory.Open(stream))
            {
                return resultFactory(archive);
            }
        }
    }
}
