using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IS4.SFI.Services
{
    /// <summary>
    /// Represents a concrete directory in a file system.
    /// </summary>
    public interface IDirectoryInfo : IFileNodeInfo
    {
        /// <summary>
        /// Contains the collection of all files and directories
        /// in this directory.
        /// </summary>
        IEnumerable<IFileNodeInfo> Entries { get; }

        /// <summary>
        /// The type of the folder, as a value from <see cref="Environment.SpecialFolder"/>
        /// or <see cref="VirtualFolders"/>, if applicable.
        /// </summary>
        Environment.SpecialFolder? SpecialFolderType { get; }
    }

    /// <summary>
    /// Additional values of <see cref="Environment.SpecialFolder"/> for
    /// virtual folders, usable in <see cref="IDirectoryInfo.SpecialFolderType"/>.
    /// </summary>
    public static class VirtualFolders
    {
        /// <summary>
        /// A virtual folder for Internet Explorer.
        /// </summary>
        public const Environment.SpecialFolder InternetExplorer = (Environment.SpecialFolder)1;

        /// <summary>
        /// The virtual folder that contains icons for the Control Panel applications.
        /// </summary>
        public const Environment.SpecialFolder ControlPanel = (Environment.SpecialFolder)3;

        /// <summary>
        /// The virtual folder that contains installed printers.
        /// </summary>
        public const Environment.SpecialFolder Printers = (Environment.SpecialFolder)4;

        /// <summary>
        /// The virtual folder that contains the objects in the user's Recycle Bin.
        /// </summary>
        public const Environment.SpecialFolder RecycleBin = (Environment.SpecialFolder)10;

        /// <summary>
        /// A virtual folder that represents Network Neighborhood, the root of the network namespace hierarchy.
        /// </summary>
        public const Environment.SpecialFolder NetworkNeighborhood = (Environment.SpecialFolder)18;

        /// <summary>
        /// The virtual folder that represents Network Connections, that contains network and dial-up connections.
        /// </summary>
        public const Environment.SpecialFolder NetworkConnections = (Environment.SpecialFolder)48;
    }

    /// <summary>
    /// Wraps a <see cref="DirectoryInfo"/> instance.
    /// </summary>
    public class DirectoryInfoWrapper : FileSystemInfoWrapper<DirectoryInfo>, IDirectoryInfo
    {
        /// <inheritdoc/>
        public DirectoryInfoWrapper(DirectoryInfo baseInfo, IPersistentKey? key = null) : base(baseInfo, key)
        {

        }

        /// <inheritdoc/>
        public IEnumerable<IFileNodeInfo> Entries =>
            BaseInfo.EnumerateFiles().Select(f => (IFileNodeInfo)new FileInfoWrapper(f)).Concat(
                BaseInfo.EnumerateDirectories().Select(d => new DirectoryInfoWrapper(d))
                );

        /// <inheritdoc/>
        public Environment.SpecialFolder? SpecialFolderType => null;
    }
}
