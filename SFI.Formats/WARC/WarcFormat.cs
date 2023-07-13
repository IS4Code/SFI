using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Toimik.WarcProtocol;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the Web ARCive format, producing instances of <see cref="IAsyncEnumerable{T}"/>
    /// of <see cref="Record"/>.
    /// </summary>
    [Description("Represents the Web ARCive format.")]
    public class WarcFormat : SignatureFormat<IAsyncEnumerable<Record>>
    {
        /// <summary>
        /// The parser used for processing input files.
        /// </summary>
        public WarcParser Parser { get; } = new WarcParser();

        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public WarcFormat() : base("WARC/", "application/warc", "warc")
        {

        }

        /// <inheritdoc/>
        public override async ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IAsyncEnumerable<Record>, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            return await resultFactory(Parser.Parse(stream, false), args);
        }
    }
}
