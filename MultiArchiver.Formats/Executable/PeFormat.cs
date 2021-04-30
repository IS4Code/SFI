﻿using IS4.MultiArchiver.Services;
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
            if(stream.ReadByte() != 'M' || stream.ReadByte() != 'Z') return null;
            using(var buffer = new MemoryStream())
            {
                buffer.WriteByte((byte)'M');
                buffer.WriteByte((byte)'Z');
                stream.CopyTo(buffer);
                buffer.Position = 0;
                var file = new PeFile(buffer.ToArray());
                return resultFactory(file);
            }
        }

        public override string GetExtension(PeFile value)
        {
            return value.IsDLL ? "dll" : value.IsEXE ? "exe" : null;
        }
    }
}