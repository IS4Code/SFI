using IS4.SFI.Application.Tools;
using IS4.SFI.Application.Tools.NuGet;
using IS4.SFI.Services;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Compression;
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

        static readonly string sfiPrefix = $"{nameof(IS4)}.{nameof(SFI)}.";

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

            id = uri.AbsolutePath;
            switch(uri.Scheme.ToLowerInvariant())
            {
                case "file":
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
                    id = sfiPrefix + id.Replace("-", "");
                    goto case "nuget";
                case "nuget":
                    return await GetPluginFromNuGet(id, cancellationToken);
                default:
                    throw new ArgumentException($"The plugin identifier '{id}' does not correspond to a known format.", nameof(id));
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
    }
}
