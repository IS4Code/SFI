using IS4.MultiArchiver.Vocabulary;
using System.IO;

namespace IS4.MultiArchiver.Services
{
    public interface IHashAlgorithm : IUriFormatter<byte[]>
    {
        Individuals Identifier { get; }
        byte[] ComputeHash(Stream input);
    }
}
