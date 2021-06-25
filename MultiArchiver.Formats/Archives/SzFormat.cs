using IS4.MultiArchiver.Formats.Archives;
using IS4.MultiArchiver.Services;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace IS4.MultiArchiver.Formats
{
    public class SzFormat : SignatureFormat<SzReader>
    {
        public SzFormat() : base(10, "SZ", null, "sz")
        {

        }

        public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            if(header.Length < HeaderLength || !base.CheckHeader(header, isBinary, encodingDetector)) return false;
            var sig = MemoryMarshal.Cast<byte, uint>(header);
            if(sig[0] == 0x44445A53 && sig[1] == 0x3327F088 && header[8] == 0x41)
            {
                return true;
            }
            if(sig[0] == 0x88205A53 && sig[1] == 0xD13327F0)
            {
                return true;
            }
            return false;
        }

        public override string GetMediaType(SzReader value)
        {
            return value.QBasicVariant ? "application/x-ms-compress-sz" : "application/x-ms-compress-szdd";
        }

        public override TResult Match<TResult>(Stream stream, ResultFactory<SzReader, TResult> resultFactory)
        {
            return resultFactory(new SzReader(stream));
        }
    }
}
