using IS4.SFI.Services;
using SwfDotNet.IO;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the Adobe Shockwave Flash format, as an instance of <see cref="Swf"/>.
    /// </summary>
    [Description("Represents the Adobe Shockwave Flash format.")]
    public class ShockwaveFlashFormat : BinaryFileFormat<Swf>
    {
        static readonly byte[][] signatures = new[] { "FWS", "CWS", "ZWS" }.Select(h => Encoding.ASCII.GetBytes(h)).ToArray();

        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public ShockwaveFlashFormat() : base(4, "application/vnd.adobe.flash-movie", "swf")
        {

        }

        /// <inheritdoc/>
        public override bool CheckHeader(ReadOnlySpan<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
        {
            if(header.Length < 4)
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

        /// <inheritdoc/>
        public override async ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<Swf, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            var reader = new SwfReader(stream);
            return await resultFactory(reader.ReadSwf(), args);
        }
    }
}
