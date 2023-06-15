namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents a format for Windows modules.
    /// </summary>
    public abstract class WinModuleFormat : ModuleFormat<IModule>
    {
        /// <inheritdoc/>
        public WinModuleFormat(string signature, string? mediaType, string? extension) : base(signature, mediaType, extension)
        {

        }

        /// <inheritdoc/>
        public override string? GetExtension(IModule module)
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
