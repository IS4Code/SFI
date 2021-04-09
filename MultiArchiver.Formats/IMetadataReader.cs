using MetadataExtractor;

namespace IS4.MultiArchiver.Services
{
    public interface IMetadataReader<in T> where T : Directory
    {
        bool Describe(ILinkedNode node, T directory, ILinkedNodeFactory nodeFactory);
    }
}
