using IS4.MultiArchiver.Formats.Archives;
using IS4.MultiArchiver.Media;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.Tools;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// Represents the SZ archive format.
    /// </summary>
    public class SzFormat : SignatureFormat<IArchiveReader>
    {
        /// <summary>
        /// Stores the inner type of each <see cref="ArchiveReaderAdapter"/> instance.
        /// </summary>
        static readonly ConditionalWeakTable<IArchiveReader, string> storedTypes = new ConditionalWeakTable<IArchiveReader, string>();

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

        public override string GetMediaType(IArchiveReader value)
        {
            return storedTypes.TryGetValue(value, out var type) ? type : base.GetMediaType(value);
        }

        public async override ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IArchiveReader, TResult, TArgs> resultFactory, TArgs args)
        {
            using(var reader = new SzReader(stream))
            {
                var adapter = new ArchiveReaderAdapter(reader);
                storedTypes.Add(adapter, reader.QBasicVariant ? "application/x-ms-compress-sz" : "application/x-ms-compress-szdd");
                return await resultFactory(adapter, args);
            }
        }
    }
}
