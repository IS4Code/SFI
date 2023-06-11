using IS4.SFI.Application.Tools;
using IS4.SFI.Application.Tools.NuGet;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using Microsoft.Extensions.Logging;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace IS4.SFI.Application.Plugins
{
    /// <summary>
    /// Provides resolution for plugin identifiers.
    /// </summary>
    public class PluginResolvers : IPluginResolver
    {
        readonly ILogger? logger;
        NuGetPluginResolver? nugetResolver;

        const string sfiPrefix = nameof(IS4) + "." + nameof(SFI) + ".";

        /// <summary>
        /// Creates a new instance of the resolvers.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        public PluginResolvers(ILogger? logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async ValueTask<Plugin> GetPluginAsync(string id, CancellationToken cancellationToken)
        {
            if(!Uri.TryCreate(id, UriKind.RelativeOrAbsolute, out var uri))
            {
                throw new ArgumentException($"'{id}' could not be parsed as a package identifier.");
            }
            if(!uri.IsAbsoluteUri)
            {
                if(!Uri.TryCreate(Path.GetFullPath(id), UriKind.Absolute, out uri))
                {
                    throw new ArgumentException($"'{id}' could not be parsed as a local path.");
                }
            }

            var uriPath = uri.AbsolutePath;
            switch(uri.Scheme.ToLowerInvariant())
            {
                case "file":
                    if(uri.IsUnc || !uri.IsFile)
                    {
                        goto default;
                    }
                    var path = uri.LocalPath;
                    switch(Path.GetExtension(path).ToLowerInvariant())
                    {
                        case ".zip":
                            return GetPluginFromZip(path);
                        default:
                            if((File.GetAttributes(path) & FileAttributes.Directory) != 0)
                            {
                                return GetPluginFromDirectory(path);
                            }
                            throw new ArgumentException($"The plugin identifier '{Path.GetFileName(path)}' does not correspond to a known format.", nameof(id));
                    }
                case "sfi":
                    uriPath = sfiPrefix + uriPath.Replace("-", "");
                    goto case "nuget";
                case "nuget":
                    return await GetPluginFromNuGet(uriPath, cancellationToken);
                default:
                    try{
                        return await GetPluginFromWeb(uri, cancellationToken);
                    }catch(Exception e) when(e is NotSupportedException or UriFormatException)
                    {
                        throw new ArgumentException($"The plugin identifier '{id}' does not correspond to a known format.", nameof(id));
                    }
            }
        }

        const string dllMimeType = "application/vnd.microsoft.portable-executable";

        /// <summary>
        /// Retrieves a new <see cref="Plugin"/> instance from a web request.
        /// </summary>
        /// <param name="uri">The URI pointing to the resource to load the plugin from.</param>
        /// <param name="cancellationToken">The token to cancel the operation.</param>
        /// <returns>A plugin instance based on <paramref name="uri"/>.</returns>
        /// <exception cref="InvalidOperationException">The resource does not have a known plugin format.</exception>
        public async ValueTask<Plugin> GetPluginFromWeb(Uri uri, CancellationToken cancellationToken)
        {
            var request = WebRequest.Create(uri);
            using var response = await request.GetResponseAsync();
            var ct = new ContentType(response.ContentType);
            var path = response.ResponseUri.AbsolutePath;
            if(
                ct.MediaType.Equals(MediaTypeNames.Application.Octet, StringComparison.OrdinalIgnoreCase) ||
                ct.MediaType.Equals(MediaTypeNames.Text.Plain, StringComparison.OrdinalIgnoreCase))
            {
                switch(Path.GetExtension(path).ToLowerInvariant())
                {
                    case ".dll":
                        ct = new ContentType(dllMimeType);
                        break;
                    case ".zip":
                        ct = new ContentType(MediaTypeNames.Application.Zip);
                        break;
                }
            }
            if(ct.MediaType.Equals(MediaTypeNames.Application.Zip, StringComparison.OrdinalIgnoreCase))
            {
                using var stream = response.GetResponseStream();
                return GetPluginFromZip(Path.GetFileName(path), stream);
            }else if(ct.MediaType.Equals(dllMimeType, StringComparison.OrdinalIgnoreCase))
            {
                using var stream = response.GetResponseStream();
                return await GetPluginFromFile(Path.GetFileName(path), stream);
            }else{
                throw new InvalidOperationException($"Could not load plugin from a media type '{ct.MediaType}'.");
            }
        }

        /// <summary>
        /// Retrieves a new <see cref="Plugin"/> instance from a directory.
        /// </summary>
        /// <param name="dir">The directory to load the plugin from.</param>
        /// <returns>A plugin instance based on <paramref name="dir"/>.</returns>
        public Plugin GetPluginFromDirectory(string dir)
        {
            // Look for a file with .dll and the same name as the directory
            var name = Path.GetFileName(dir) + ".dll";
            var info = new DirectoryInfo(dir);
            return new Plugin(new DirectoryInfoWrapper(info), name);
        }

        /// <summary>
        /// Retrieves a new <see cref="Plugin"/> instance from a ZIP archive.
        /// </summary>
        /// <param name="file">The path to the archive to load the plugin from.</param>
        /// <returns>A plugin instance based on <paramref name="file"/>.</returns>
        public Plugin GetPluginFromZip(string file)
        {
            // Look for a file with .zip changed to .dll
            var name = Path.ChangeExtension(Path.GetFileName(file), "dll");
            ZipArchive archive;
            try{
                archive = ZipFile.OpenRead(file);
            }catch(Exception e)
            {
                logger?.LogError(e, $"An error occurred while opening plugin archive {Path.GetFileName(file)}.");
                return default;
            }
            return new Plugin(new ZipArchiveWrapper(archive), name);
        }

        /// <summary>
        /// Retrieves a new <see cref="Plugin"/> instance from a ZIP archive.
        /// </summary>
        /// <param name="name">The name of the archive.</param>
        /// <param name="stream">The stream containing the ZIP data.</param>
        /// <returns>A plugin instance based on <paramref name="name"/> and <paramref name="stream"/>.</returns>
        public Plugin GetPluginFromZip(string name, Stream stream)
        {
            // Look for a file with .zip changed to .dll
            name = Path.ChangeExtension(name, "dll");
            ZipArchive archive;
            try{
                archive = new ZipArchive(stream, ZipArchiveMode.Read);
            }catch(Exception e)
            {
                logger?.LogError(e, $"An error occurred while opening plugin archive {name}.");
                return default;
            }
            return new Plugin(new ZipArchiveWrapper(archive), name);
        }

        /// <summary>
        /// Retrieves a new <see cref="Plugin"/> instance from a file.
        /// </summary>
        /// <param name="file">The path to the file to load the plugin from.</param>
        /// <returns>A plugin instance based on <paramref name="file"/>.</returns>
        public Plugin GetPluginFromFile(string file)
        {
            var name = Path.GetFileName(file);
            var directory = new DirectoryInfo(Path.GetDirectoryName(file));
            return new Plugin(new DirectoryInfoWrapper(directory), name);
        }

        /// <summary>
        /// Retrieves a new <see cref="Plugin"/> instance from a file.
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <param name="stream">The stream containing the file data.</param>
        /// <returns>A plugin instance based on <paramref name="name"/> and <paramref name="stream"/>.</returns>
        public async ValueTask<Plugin> GetPluginFromFile(string name, Stream stream)
        {
            using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer);
            var data = buffer.GetData();
            var file = new MemoryFile(name, data);
            return new Plugin(new MemoryDirectory(file), name);
        }

        /// <summary>
        /// Retrieves a new <see cref="Plugin"/> instance from NuGet.
        /// </summary>
        /// <param name="id">The identifier of the plugin, as <c>{package}/{version}</c>.</param>
        /// <param name="cancellationToken">The token to cancel the operation.</param>
        /// <returns>A plugin instance based on <paramref name="id"/>.</returns>
        public async ValueTask<Plugin> GetPluginFromNuGet(string id, CancellationToken cancellationToken)
        {
            nugetResolver ??= new($"{nameof(SFI)} plugins", logger);

            return await nugetResolver.GetPluginAsync(id, cancellationToken);
        }

        class MemoryDirectory : IDirectoryInfo
        {
            readonly IFileInfo[] entries;

            public MemoryDirectory(IFileInfo file)
            {
                entries = new[] { file };
            }

            IEnumerable<IFileNodeInfo> IDirectoryInfo.Entries => entries;

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

        class MemoryFile : IFileInfo
        {
            public string? Name { get; }

            public string? Path { get; }

            public long Length => data.Count;

            readonly ArraySegment<byte> data;

            public MemoryFile(string path, ArraySegment<byte> data)
            {
                Path = path;
                var index = path.LastIndexOf('/');
                Name = index != -1 ? path.Substring(index + 1) : path;
                this.data = data;
            }

            string? IFileNodeInfo.SubName => null;

            int? IFileNodeInfo.Revision => null;

            DateTime? IFileNodeInfo.CreationTime => null;

            DateTime? IFileNodeInfo.LastWriteTime => null;

            DateTime? IFileNodeInfo.LastAccessTime => null;

            FileKind IFileNodeInfo.Kind => FileKind.None;

            FileAttributes IFileNodeInfo.Attributes => FileAttributes.ReadOnly;

            StreamFactoryAccess IStreamFactory.Access => StreamFactoryAccess.Parallel;

            object? IIdentityKey.ReferenceKey => this;

            object? IIdentityKey.DataKey => null;

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
}
