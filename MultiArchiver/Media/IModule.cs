using System.Collections.Generic;

namespace IS4.MultiArchiver.Media
{
    public interface IModule
    {
        ModuleType Type { get; }
        IEnumerable<IModuleResource> ReadResources();
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
}
