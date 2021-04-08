using IS4.MultiArchiver.Vocabulary;
using System.IO.Compression;

namespace IS4.MultiArchiver.Analyzers
{
    public class ArchiveAnalyzer : ClassRecognizingAnalyzer<ZipArchive>
    {
        public ArchiveAnalyzer() : base(Classes.Archive)
        {

        }
    }
}
