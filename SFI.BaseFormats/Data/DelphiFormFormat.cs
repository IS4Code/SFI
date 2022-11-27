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
        /// <summary>
        /// Contains the encoding used for reading strings.
        /// </summary>
        public Encoding DefaultEncoding { get; set; }

        /// <summary>
        /// Contains the name of <see cref="DefaultEncoding"/>.
        /// </summary>
        public string DefaultEncodingName {
            get {
                return DefaultEncoding.WebName;
            }
            set {
                DefaultEncoding = Encoding.GetEncoding(value);
            }
        }

        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public DelphiFormFormat() : base("TPF0", "application/x-delphi-form", "dfm")
        {
            try{
                DefaultEncoding = Encoding.GetEncoding(1252);
            }catch{
                DefaultEncoding = Encoding.ASCII;
            }
        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<DelphiObject, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            return await resultFactory(DelphiFormReader.Read(stream, DefaultEncoding), args);
        }
    }
}
