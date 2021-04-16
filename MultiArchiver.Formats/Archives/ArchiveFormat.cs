using IS4.MultiArchiver.Services;
using SharpCompress.Archives;
using Archives = SharpCompress.Archives;
using System;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class ArchiveFormat : FileFormat<IArchive>
    {
        public ArchiveFormat() : base(0, null, null)
        {

        }

        private (string ext, string type) GetArchiveInfo(IArchive archive)
        {
            switch(archive)
            {
                case Archives.Rar.RarArchive _:
                    return ("rar", "application/vnd.rar");
                case Archives.Zip.ZipArchive _:
                    return ("zip", "application/zip");
                case Archives.Tar.TarArchive _:
                    return ("tar", "application/x-tar");
                case Archives.SevenZip.SevenZipArchive _:
                    return ("7z", "application/x-7z-compressed");
                case Archives.GZip.GZipArchive _:
                    return ("gz", "application/gzip");
                default:
                    return default;
            }
        }

        public override string GetExtension(IArchive value)
        {
            return GetArchiveInfo(value).ext;
        }

        public override string GetMediaType(IArchive value)
        {
            return GetArchiveInfo(value).type;
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
