using DiscUtils.Iso9660;
using IS4.MultiArchiver.Services;
using System;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class IsoFormat : FileFormat<CDReader>
    {
        public IsoFormat() : base(0, null, "iso")
        {

        }

        public override bool Match(Span<byte> header)
        {
            return true;
        }

        public override TResult Match<TResult>(Stream stream, Func<CDReader, TResult> resultFactory)
        {
            using(var reader = new CDReader(stream, true))
            {
                return resultFactory(reader);
            }
        }
    }
}
