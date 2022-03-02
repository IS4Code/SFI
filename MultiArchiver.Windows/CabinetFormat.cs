using IS4.MultiArchiver.Windows;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    public class CabinetFormat : SignatureFormat<CabinetFile>
    {
        public CabinetFormat() : base("MSCF", "application/vnd.ms-cab-compressed", "cab")
        {

        }

        public override async ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<CabinetFile, TResult, TArgs> resultFactory, TArgs args)
        {
            using(var file = new CabinetFile(stream))
            {
                return await resultFactory(file, args);
            }
        }
    }
}
