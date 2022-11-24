using IS4.SFI.Formats.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the DFM format used by Delphi, producing instances of <see cref="DelphiObject"/>.
    /// </summary>
    public class DelphiFormFormat : SignatureFormat<DelphiObject>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public DelphiFormFormat() : base("TPF0", "application/x-delphi-form", "dfm")
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<DelphiObject, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            var encoding = Encoding.GetEncoding(1252); //TODO: guess from context
            return await resultFactory(DelphiFormReader.Read(stream, encoding), args);
        }
    }
}
