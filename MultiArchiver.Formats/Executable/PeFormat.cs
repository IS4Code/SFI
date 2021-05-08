using PeNet;
using System;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class PeFormat : SignatureFormat<PeFile>
    {
        public PeFormat() : base(3, "MZ", "application/vnd.microsoft.portable-executable", null)
        {

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
