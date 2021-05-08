using SharpCompress.Archives.Rar;
using System;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class RarFormat : ArchiveFormat<RarArchive>
    {
        public RarFormat() : base("Rar!", "application/vnd.rar", "rar")
        {

        }

        public override TResult Match<TResult>(Stream stream, Func<RarArchive, TResult> resultFactory)
        {
            using(var archive = RarArchive.Open(stream))
            {
                if(!CheckArchive(archive)) return null;
                return resultFactory(archive);
            }
        }
    }
}
