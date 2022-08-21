using IS4.MultiArchiver.Media.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
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

        public override async ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<DelphiObject, TResult, TArgs> resultFactory, TArgs args)
        {
            var encoding = Encoding.GetEncoding(1252); //TODO: guess from context
            return await resultFactory(DelphiFormReader.Read(stream, encoding), args);
        }
    }
}
