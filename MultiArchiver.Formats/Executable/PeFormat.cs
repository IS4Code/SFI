using IS4.MultiArchiver.Services;
using PeNet;
using System;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class PeFormat : BinaryFileFormat<PeFile>
    {
        public PeFormat() : base(2, "application/vnd.microsoft.portable-executable", null)
        {

        }

        public override bool Match(Span<byte> header)
        {
            return header.Length >= 2 && header[0] == 'M' && header[1] == 'Z';
        }

        public override TResult Match<TResult>(Stream stream, Func<PeFile, TResult> resultFactory)
        {
            using(var buffer = new MemoryStream())
            {
                stream.CopyTo(buffer);
                buffer.Position = 0;
                var data = buffer.ToArray();
                var file = new PeFile(data);
                return resultFactory(file);
            }
        }

        public override string GetExtension(PeFile value)
        {
            return value.IsDLL ? "dll" : value.IsEXE ? "exe" : null;
        }
    }
}
