using IS4.MultiArchiver.Services;
using System;
using System.IO;
using System.IO.Compression;

namespace IS4.MultiArchiver.Formats
{
    public class ZipFileFormat : FileFormat, IFileReader
    {
        public ZipFileFormat() : base(2, "application/zip", "zip")
        {

        }

        public override bool Match(Span<byte> header)
        {
            return header[0] == 0x50 && header[1] == 0x4B;
        }

        public ILinkedNode Match(Stream stream, ILinkedNodeFactory nodeFactory)
        {
            using(var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
            {
                return nodeFactory.Create(archive);
            }
        }
    }
}
