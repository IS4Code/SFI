using IS4.MultiArchiver.Media;

namespace IS4.MultiArchiver.Formats
{
    public abstract class WinModuleFormat : ModuleFormat<IModule>
    {
        public WinModuleFormat(string signature, string mediaType, string extension) : base(signature, mediaType, extension)
        {

        }

        public override string GetExtension(IModule module)
        {
            switch(module.Type)
            {
                case ModuleType.System: return "sys";
                case ModuleType.Library: return "dll";
                case ModuleType.Executable: return "exe";
                default: return null;
            }
        }
    }
}
