using IS4.MultiArchiver.Services;
using SharpCompress.Archives;
using System;
using System.IO;
using SharpCompress.Common;
using System.Linq;

namespace IS4.MultiArchiver.Formats
{
    public class ArchiveFormat : BinaryFileFormat<IArchive>
    {
        public ArchiveFormat() : base(0, null, null)
        {

        }

        private (string ext, string type) GetArchiveInfo(IArchive archive)
        {
            switch(archive.Type)
            {
                case ArchiveType.Rar:
                    return ("rar", "application/vnd.rar");
                case ArchiveType.Zip:
                    return ("zip", "application/zip");
                case ArchiveType.Tar:
                    return ("tar", "application/x-tar");
                case ArchiveType.SevenZip:
                    return ("7z", "application/x-7z-compressed");
                case ArchiveType.GZip:
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
                if(archive.TotalSize <= 0 || archive.TotalUncompressSize <= 0)
                {
                    return null;
                }
                if(archive.Type == ArchiveType.Tar)
                {
                    if(archive.TotalUncompressSize > 1024 * (long)Int32.MaxValue)
                    {
                        return null;
                    }
                    if(archive.TotalUncompressSize != archive.Entries.Sum(e => e.Size))
                    {
                        return null;
                    }
                    if(archive.Entries.Any(e => e.Key.Contains('\0')))
                    {
                        return null;
                    }
                }
                return resultFactory(archive);
            }
        }
    }
}
