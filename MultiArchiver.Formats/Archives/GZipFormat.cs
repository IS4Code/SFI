using SharpCompress.Archives.GZip;
using System;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class GZipFormat : ArchiveFormat<GZipArchive>
    {
        public GZipFormat() : base(new byte[] { 0x1F, 0x8B }, "application/gzip", "gz")
        {

        }

        public override TResult Match<TResult>(Stream stream, Func<GZipArchive, TResult> resultFactory)
        {
            using(var archive = GZipArchive.Open(stream))
            {
                if(!CheckArchive(archive)) return null;
                return resultFactory(archive);
            }
        }
    }
}
