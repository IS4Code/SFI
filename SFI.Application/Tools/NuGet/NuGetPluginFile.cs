using IS4.SFI.Services;
using IS4.SFI.Tools;
using NuGet.Packaging;
using System;
using System.IO;

namespace IS4.SFI.Application.Tools.NuGet
{
    /// <summary>
    /// Represents a file located within a NuGet package.
    /// </summary>
    public class NuGetPluginFile : IFileInfo
    {
        readonly PackageReaderBase package;

        /// <inheritdoc/>
        public string? Name { get; }

        /// <inheritdoc/>
        public string? Path { get; }

        /// <inheritdoc/>
        public long Length => data.Count;

        readonly ArraySegment<byte> data;

        /// <summary>
        /// Creates a new instance of the file.
        /// </summary>
        /// <param name="path">The path of the file in <paramref name="package"/>.</param>
        /// <param name="package">The package containing the file.</param>
        public NuGetPluginFile(string path, PackageReaderBase package)
        {
            this.package = package;
            Path = path;
            var index = path.LastIndexOf('/');
            Name = index != -1 ? path.Substring(index + 1) : path;

            using var buffer = new MemoryStream();
            using var stream = package.GetStream(path);
            stream.CopyTo(buffer);

            data = buffer.GetData();
        }

        string? IFileNodeInfo.SubName => null;

        int? IFileNodeInfo.Revision => null;

        DateTime? IFileNodeInfo.CreationTime => null;

        DateTime? IFileNodeInfo.LastWriteTime => null;

        DateTime? IFileNodeInfo.LastAccessTime => null;

        FileKind IFileNodeInfo.Kind => FileKind.ArchiveItem;

        FileAttributes IFileNodeInfo.Attributes => FileAttributes.ReadOnly;

        StreamFactoryAccess IStreamFactory.Access => StreamFactoryAccess.Parallel;

        object? IIdentityKey.ReferenceKey => package;

        object? IIdentityKey.DataKey => Path;

        /// <inheritdoc/>
        public Stream Open()
        {
            return new MemoryStream(data.Array!, data.Offset, data.Count, false);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"/{Path}";
        }
    }
}
