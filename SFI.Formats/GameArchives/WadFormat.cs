using IS4.SFI.Services;
using nz.doom.WadParser;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the WAD archive format, as an instance of <see cref="Wad"/>.
    /// </summary>
    [Description("Represents the WAD archive format used in Doom and other games.")]
    public class WadFormat : BinaryFileFormat<Wad>
    {
        static readonly byte[][] signatures = new[] { "IWAD", "PWAD", "WAD" }.Select(h => Encoding.ASCII.GetBytes(h)).ToArray();

        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public WadFormat() : base(5, "application/x-doom", "wad")
        {

        }

        /// <inheritdoc/>
        public override bool CheckHeader(ReadOnlySpan<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
        {
            if(header.Length < 5)
            {
                return false;
            }
            foreach(var sig in signatures)
            {
                if(header.StartsWith(sig.AsSpan()))
                {
                    return true;
                }
            }
            return false;
        }

        public override async ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<Wad, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            if(stream is FileStream fileStream)
            {
                return await resultFactory(WadParser.Parse(fileStream), args);
            }
            using var tmpPath = FileTools.GetTemporaryFile("wad");
            using(var file = new FileStream(tmpPath, FileMode.CreateNew))
            {
                stream.CopyTo(file);
            }
            return await resultFactory(WadParser.Parse(tmpPath), args);
        }
    }
}
