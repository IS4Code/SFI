using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.Tools;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// Represents the SZ archive format.
    /// </summary>
    public class SzFormat : SignatureFormat<SzReader>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public SzFormat() : base(10, "SZ", null, "sz")
        {

        }

        public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            if(header.Length < HeaderLength || !base.CheckHeader(header, isBinary, encodingDetector)) return false;
            var sig = header.MemoryCast<uint>();
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

        public override async ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<SzReader, TResult, TArgs> resultFactory, TArgs args)
        {
            return await resultFactory(new SzReader(stream), args);
        }
    }
}
