using IS4.SFI.Services;

namespace IS4.SFI.Application
{
    /// <summary>
    /// Stores information about a plugin.
    /// </summary>
    public struct Plugin
    {
        /// <summary>
        /// The main directory of the plugin.
        /// </summary>
        public IDirectoryInfo Directory { get; }

        /// <summary>
        /// The name of the entry file of the plugin.
        /// </summary>
        public string MainFile { get; }

        /// <summary>
        /// Creates a new instance of the plugin.
        /// </summary>
        /// <param name="directory">The value of <see cref="Directory"/>.</param>
        /// <param name="mainFile">The value of <see cref="MainFile"/>.</param>
        public Plugin(IDirectoryInfo directory, string mainFile)
        {
            Directory = directory;
            MainFile = mainFile;
        }
    }
}
