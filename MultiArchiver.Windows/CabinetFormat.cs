using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Windows;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class CabinetFormat : SignatureFormat<CabinetFile>
    {
        public CabinetFormat() : base("MSCF", "application/vnd.ms-cab-compressed", "cab")
        {

        }

        public override TResult Match<TResult, TArgs>(Stream stream, ResultFactory<CabinetFile, TResult, TArgs> resultFactory, TArgs args)
        {
            using(var file = new CabinetFile(stream))
            {
                return resultFactory(file, args);
            }
        }
    }
}
