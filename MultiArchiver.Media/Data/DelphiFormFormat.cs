using IS4.MultiArchiver.Media.Data;
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

        public override TResult Match<TResult, TArgs>(Stream stream, ResultFactory<DelphiObject, TResult, TArgs> resultFactory, TArgs args)
        {
            var encoding = Encoding.GetEncoding(1252); //TODO guess from context
            return resultFactory(DelphiFormReader.Read(stream, encoding), args);
        }
    }
}
