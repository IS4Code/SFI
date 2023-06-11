using IS4.SFI.Application.Plugins;
using IS4.SFI.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace IS4.SFI.Application.Tools.NuGet
{
    /// <summary>
    /// Stores the combined directory of a plugin obtained through NuGet
    /// merged from the individual references packages.
    /// </summary>
    public class NuGetPlugin : IDirectoryInfo
    {
        /// <summary>
        /// The <see cref="Plugin"/> instance that represents the plugin.
        /// </summary>
        public Plugin Plugin { get; }

        readonly ConcurrentDictionary<string, NuGetPluginFile> files = new();

        /// <summary>
        /// Creates a new instance from the package name.
        /// </summary>
        /// <param name="name">The name of the package.</param>
        public NuGetPlugin(string name)
        {
            Plugin = new(this, name + ".dll");
        }

        /// <summary>
        /// Adds a new file to the plugin.
        /// </summary>
        /// <param name="file">The file to add.</param>
        /// <exception cref="InvalidOperationException">The file with the same path already exists.</exception>
        public void AddFile(NuGetPluginFile file)
        {
            if(!files.TryAdd(file.Path!, file))
            {
                throw new InvalidOperationException($"The file '{file.Path}' already exists.");
            }
        }

        IEnumerable<IFileNodeInfo> IDirectoryInfo.Entries => files.Values;

        Environment.SpecialFolder? IDirectoryInfo.SpecialFolderType => null;

        string? IFileNodeInfo.Name => null;

        string? IFileNodeInfo.SubName => null;

        string? IFileNodeInfo.Path => null;

        int? IFileNodeInfo.Revision => null;

        DateTime? IFileNodeInfo.CreationTime => null;

        DateTime? IFileNodeInfo.LastWriteTime => null;

        DateTime? IFileNodeInfo.LastAccessTime => null;

        FileKind IFileNodeInfo.Kind => FileKind.None;

        FileAttributes IFileNodeInfo.Attributes => FileAttributes.Directory;

        object? IIdentityKey.ReferenceKey => this;

        object? IIdentityKey.DataKey => null;
    }
}
