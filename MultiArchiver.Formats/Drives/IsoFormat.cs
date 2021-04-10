using DiscUtils.Iso9660;
using IS4.MultiArchiver.Services;
using System;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class IsoFormat : FileFormat, IFileReader
    {
        public IsoFormat() : base(0, null, "iso")
        {

        }

        public override bool Match(Span<byte> header)
        {
            return true;
        }

        public ILinkedNode Match(Stream stream, ILinkedNodeFactory nodeFactory)
        {
            using(var reader = new CDReader(stream, true))
            {
                return nodeFactory.Create(reader);
            }
        }
    }
}
