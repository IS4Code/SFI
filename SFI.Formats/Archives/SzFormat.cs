using IS4.SFI.Formats.Archives;
using IS4.SFI.Media;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.Tools;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the SZ archive format.
    /// </summary>
    public class SzFormat : SignatureFormat<IArchiveReader>
    {
        /// <summary>
        /// Stores the inner type of each <see cref="ArchiveReaderAdapter"/> instance.
        /// </summary>
        static readonly ConditionalWeakTable<IArchiveReader, string> storedTypes = new();

        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public SzFormat() : base(10, "SZ", null, "sz")
        {

        }

        /// <inheritdoc/>
        public override bool CheckHeader(ReadOnlySpan<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
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

        /// <inheritdoc/>
        public override string? GetMediaType(IArchiveReader value)
        {
            return storedTypes.TryGetValue(value, out var type) ? type : base.GetMediaType(value);
        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IArchiveReader, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            using var reader = new SzReader(stream);
            var adapter = new ArchiveReaderAdapter(reader);
            storedTypes.Add(adapter, reader.QBasicVariant ? "application/x-ms-compress-sz" : "application/x-ms-compress-szdd");
            return await resultFactory(adapter, args);
        }
    }
}
