using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using IS4.MultiArchiver.Windows;

namespace IS4.MultiArchiver.Analyzers
{
    public class Win16ModuleAnalyzer : BinaryFormatAnalyzer<NeReader>
    {
        public override string Analyze(ILinkedNode node, NeReader reader, ILinkedNodeFactory nodeFactory)
        {
            foreach(var info in reader.ReadResources())
            {
                var infoNode = nodeFactory.Create<IFileInfo>(node[info.Type.ToString()], info);
                if(infoNode != null)
                {
                    infoNode.SetClass(Classes.EmbeddedFileDataObject);
                    node.Set(Properties.HasMediaStream, infoNode);
                }
            }
            return null;
        }
    }
}
