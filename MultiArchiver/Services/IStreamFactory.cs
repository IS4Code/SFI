using System.IO;

namespace IS4.MultiArchiver.Services
{
    public interface IStreamFactory : IPersistentKey
    {
        bool IsThreadSafe { get; }
        Stream Open();
    }
}
