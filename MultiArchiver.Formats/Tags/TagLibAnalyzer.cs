using IS4.MultiArchiver.Services;
using TagLib;

namespace IS4.MultiArchiver.Analyzers
{
    public class TagLibAnalyzer : BinaryFormatAnalyzer<TagLib.File>
    {
        public override bool Analyze(ILinkedNode node, File entity, ILinkedNodeFactory nodeFactory)
        {
            return base.Analyze(node, entity, nodeFactory);
        }
    }
}
