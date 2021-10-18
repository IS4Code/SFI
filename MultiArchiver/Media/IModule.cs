using IS4.MultiArchiver.Tools;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace IS4.MultiArchiver.Media
{
    public interface IModule
    {
        ModuleType Type { get; }
        IEnumerable<IModuleResource> ReadResources();
        IModuleSignature Signature { get; }
    }

    public enum ModuleType
    {
        Unknown,
        Executable,
        Library,
        System
    }

    public interface IModuleResource
    {
        object Type { get; }
        object Name { get; }
        int Length { get; }
        int Read(byte[] buffer, int offset, int length);
    }

    public interface IModuleSignature
    {
        BuiltInHash HashAlgorithm { get; }
        byte[] ComputeHash(BuiltInHash hash);
        byte[] Hash { get; }
        string SignerSerialNumber { get; }
        X509Certificate2 Certificate { get; }
    }
}
