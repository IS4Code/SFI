using IS4.MultiArchiver.Formats.Data;
using IS4.MultiArchiver.Services;
using System.IO;
using System.Text;

namespace IS4.MultiArchiver.Formats
{
    public class DelphiFormFormat : SignatureFormat<DelphiObject>
    {
        public DelphiFormFormat() : base("TPF0", "application/x-delphi-form", "dfm")
        {

        }

        public override TResult Match<TResult>(Stream stream, ResultFactory<DelphiObject, TResult> resultFactory)
        {
            var encoding = Encoding.GetEncoding(1252); //TODO guess from context
            return resultFactory(DelphiFormReader.Read(stream, encoding));
        }
    }
}
