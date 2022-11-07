using IS4.MultiArchiver.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace IS4.MultiArchiver.ConsoleApp
{
    /// <summary>
    /// Manages plugins for the console application.
    /// </summary>
    class Plugins
    {
        readonly Inspector inspector;

        TextWriter OutputLog => inspector.OutputLog;

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="inspector">The instance of <see cref="Inspector"/> to use.</param>
        public Plugins(Inspector inspector)
        {
            this.inspector = inspector;
        }

        /// <summary>
        /// Loads all plugins in a folder in <see cref="AppContext.BaseDirectory"/>.
        /// </summary>
        /// <param name="folder">The name of the folder.</param>
        /// <returns>A sequence of all exported types from the plugin assemblies, together with their DI constructors.</returns>
        public IEnumerable<(Type type, Func<object> ctor)> LoadPlugins(string folder)
        {
            var plugins = Path.Combine(AppContext.BaseDirectory, folder);

            foreach(var dir in Directory.EnumerateDirectories(plugins))
            {
                foreach(var elem in LoadPluginFromDirectory(plugins, dir))
                {
                    yield return elem;
                }
            }

            foreach(var zip in Directory.EnumerateFiles(plugins, "*.zip"))
            {
                foreach(var elem in LoadPluginFromZip(plugins, zip))
                {
                    yield return elem;
                }
            }
        }

        IEnumerable<(Type, Func<object>)> LoadPluginFromDirectory(string baseDir, string dir)
        {
            // Look for a file with .dll and the same name as the directory
            var name = Path.GetFileName(dir) + ".dll";
            var info = new DirectoryInfo(dir);
            return LoadPlugin(baseDir, new DirectoryInfoWrapper(info), name);
        }

        IEnumerable<(Type, Func<object>)> LoadPluginFromZip(string baseDir, string file)
        {
            // Look for a file with .zip changed to .dll
            var name = Path.ChangeExtension(Path.GetFileName(file), "dll");
            ZipArchive archive;
            try{
                archive = ZipFile.OpenRead(file);
            }catch(Exception e)
            {
                OutputLog?.WriteLine($"An error occurred while opening plugin archive {Path.GetFileName(file)}: " + e);
                return Array.Empty<(Type, Func<object>)>();
            }
            return LoadPlugin(baseDir, new ZipArchiveWrapper(archive), name);
        }

        IEnumerable<(Type, Func<object>)> LoadPlugin(string baseDir, IDirectoryInfo mainDirectory, string mainFile)
        {
            // Add DI services:
            var services = new ServiceCollection();
            services.AddSingleton(inspector);
            services.AddSingleton(OutputLog);
            services.AddSingleton(mainDirectory);
            var serviceProvider = services.BuildServiceProvider();

            // Add directories for assembly lookup:
            var context = new PluginLoadContext();
            context.AddDirectory(mainDirectory);
            context.AddDirectory(baseDir);
            context.AddDirectory(AppContext.BaseDirectory);

            Assembly asm;
            try
            {
                var mainEntry = mainDirectory.Entries.OfType<IFileInfo>().FirstOrDefault(e => mainFile.Equals(e.Name, StringComparison.OrdinalIgnoreCase));

                if(mainEntry == null)
                {
                    OutputLog?.WriteLine($"Cannot find main library file {mainFile} inside the plugin.");
                    yield break;
                }

                asm = context.LoadFromFile(mainEntry);
            }catch(Exception e)
            {
                var pluginName = Path.GetFileNameWithoutExtension(mainFile);
                OutputLog?.WriteLine($"An error occurred while loading plugin {pluginName}: " + e);
                yield break;
            }

            foreach(var type in asm.ExportedTypes)
            {
                // Only yield concrete instantiable types
                if(!type.IsAbstract && type.IsClass && !type.IsGenericTypeDefinition)
                {
                    yield return (type, () => ActivatorUtilities.CreateInstance(serviceProvider, type));
                }
            }
        }
        
        class ZipArchiveWrapper : IDirectoryInfo
        {
            readonly ZipArchive archive;

            public ZipArchiveWrapper(ZipArchive archive)
            {
                this.archive = archive;
            }

            public IEnumerable<IFileNodeInfo> Entries => archive.Entries.Select(e => new ZipEntryWrapper(e));

            public string Name => null;
            public string SubName => null;
            public string Path => null;
            public int? Revision => null;
            public DateTime? CreationTime => null;
            public DateTime? LastWriteTime => null;
            public DateTime? LastAccessTime => null;
            public FileKind Kind => FileKind.ArchiveItem;
            public object ReferenceKey => archive;
            public object DataKey => null;

            class ZipEntryWrapper : IFileInfo
            {
                readonly ZipArchiveEntry entry;

                public ZipEntryWrapper(ZipArchiveEntry entry)
                {
                    this.entry = entry;
                }

                public bool IsEncrypted => false;
                public string Name => entry.Name;
                public string SubName => null;
                public string Path => entry.FullName;
                public int? Revision => null;
                public DateTime? CreationTime => null;
                public DateTime? LastWriteTime => entry.LastWriteTime.UtcDateTime;
                public DateTime? LastAccessTime => null;
                public FileKind Kind => FileKind.ArchiveItem;
                public long Length => entry.Length;
                public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;
                public object ReferenceKey => entry;
                public object DataKey => null;

                public Stream Open()
                {
                    // DEFLATE stream does not support Length, so use a buffer
                    var buffer = new MemoryStream();
                    using(var stream = entry.Open())
                    {
                        stream.CopyTo(buffer);
                    }
                    if(!buffer.TryGetBuffer(out var segment))
                    {
                        segment = buffer.ToArray();
                    }
                    return new MemoryStream(segment.Array, segment.Offset, segment.Count, false);
                }
            }
        }
    }
}
