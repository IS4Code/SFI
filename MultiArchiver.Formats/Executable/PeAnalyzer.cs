using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using PeNet;

namespace IS4.MultiArchiver.Analyzers
{
    public class PeAnalyzer : BinaryFormatAnalyzer<PeFile>
    {
        public PeAnalyzer() : base(Classes.Executable)
        {

        }

        public override bool Analyze(ILinkedNode node, PeFile file, ILinkedNodeFactory nodeFactory)
        {
            /*var res = file.ImageResourceDirectory;
            foreach(var entry in res.DirectoryEntries)
            {
                if(entry.ResolvedName == "Icon")
                {
                    entry.
                }
            }*/
            return false;
        }
    }
}
