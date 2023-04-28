using IS4.SFI.Application;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using Microsoft.Extensions.DependencyInjection;
using MorseCode.ITask;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IS4.SFI
{
    /// <summary>
    /// An implementation of <see cref="Inspector"/> allowing loading of plugins.
    /// </summary>
    public abstract class ExtensibleInspector : ComponentInspector, IResultFactory<object?, IEnumerable>
    {
        /// <summary>
        /// Contains the collection of plugins loaded by the inspector.
        /// </summary>
        public ICollection<Plugin> Plugins { get; } = new List<Plugin>();

        /// <inheritdoc/>
        public async override ValueTask AddDefault()
        {
            await base.AddDefault();

            await LoadPlugins();
        }

        async ValueTask LoadPlugins()
        {
            CaptureCollections(this, true);

            var loaded = new Dictionary<Assembly, int>();

            foreach(var plugin in Plugins)
            {
                if(plugin.Directory == null) continue;

                foreach(var component in LoadPlugin(plugin.Directory, plugin.MainFile))
                {
                    int componentCount = 0;

                    foreach(var collection in ComponentCollections)
                    {
                        await foreach(var instance in collection.CreateInstance(component, this, collection.Collection))
                        {
                            if(instance != null)
                            {
                                componentCount++;
                                CaptureCollections(instance);
                            }
                        }
                    }

                    if(componentCount > 0)
                    {
                        var assembly = component.Type.Assembly;
                        if(!loaded.TryGetValue(assembly, out var count))
                        {
                            count = 0;
                        }
                        loaded[assembly] = count + componentCount;
                    }
                }
            }

            if(loaded.Count > 0)
            {
                foreach(var (asm, count) in loaded)
                {
                    OutputLog?.WriteLine($"Loaded {count} component{(count == 1 ? "" : "s")} from assembly {asm.GetName().Name}.");
                }
            }
        }

        async ITask<object?> IResultFactory<object?, IEnumerable>.Invoke<T>(T value, IEnumerable sequence)
        {
            if(sequence is ICollection<T> collection)
            {
                collection.Add(value);
                return value;
            }
            return null;
        }

        /// <summary>
        /// Loads an assembly from a file.
        /// </summary>
        /// <param name="file">The assembly file.</param>
        /// <param name="mainDirectory">The directory storing the file, used for dependency lookup.</param>
        /// <returns>The newly loaded assembly.</returns>
        protected abstract Assembly LoadFromFile(IFileInfo file, IDirectoryInfo mainDirectory);

        /// <summary>
        /// Opens a ZIP archive as a directory.
        /// </summary>
        /// <param name="archive">The ZIP archive to use.</param>
        /// <returns>An instance of <see cref="IDirectoryInfo"/> for the archive.</returns>
        protected IDirectoryInfo GetDirectory(ZipArchive archive)
        {
            return new ZipArchiveWrapper(archive);
        }

        /// <summary>
        /// Opens an instance of <see cref="DirectoryInfo"/> as a directory.
        /// </summary>
        /// <param name="directory">The directory to use.</param>
        /// <returns>An instance of <see cref="IDirectoryInfo"/> for the directory.</returns>
        protected IDirectoryInfo GetDirectory(DirectoryInfo directory)
        {
            return new DirectoryInfoWrapper(directory);
        }

        /// <summary>
        /// Loads a plugin from a file in a directory.
        /// </summary>
        /// <param name="mainDirectory">The directory to search.</param>
        /// <param name="mainFile">The name of the main file.</param>
        /// <returns>A collection of all instantiable types in the assembly, together with their constructor.</returns>
        IEnumerable<ComponentType> LoadPlugin(IDirectoryInfo mainDirectory, string mainFile)
        {
            // Add DI services:
            var services = new ServiceCollection();
            services.AddSingleton<Inspector>(this);
            services.AddSingleton(OutputLog);
            services.AddSingleton(mainDirectory);
            var serviceProvider = services.BuildServiceProvider();

            Assembly asm;
            try{
                var mainEntry = mainDirectory.Entries.OfType<IFileInfo>().FirstOrDefault(e => mainFile.Equals(e.Name, StringComparison.OrdinalIgnoreCase));

                if(mainEntry == null)
                {
                    OutputLog?.WriteLine($"Cannot find main library file {mainFile} inside the plugin.");
                    yield break;
                }

                asm = LoadFromFile(mainEntry, mainDirectory);
            }catch(Exception e)
            {
                var pluginName = Path.GetFileNameWithoutExtension(mainFile);
                OutputLog?.WriteLine($"An error occurred while loading plugin {pluginName}: " + e);
                yield break;
            }

            foreach(var type in asm.ExportedTypes)
            {
                // Only yield concrete instantiable browsable types
                if(!type.IsAbstract && !type.IsGenericTypeDefinition && IsTypeBrowsable(type))
                {
                    yield return new ComponentType(type, () => ActivatorUtilities.CreateInstance(serviceProvider, type), this);
                }
            }
        }

        static readonly Type BrowsableAttributeType = typeof(BrowsableAttribute);

        static bool IsTypeBrowsable(Type type)
        {
            return
                (TypeDescriptor.GetAttributes(type)[BrowsableAttributeType] as BrowsableAttribute)
                ?.Browsable ?? true;
        }

        class ZipArchiveWrapper : IDirectoryInfo
        {
            readonly ZipArchive archive;

            public ZipArchiveWrapper(ZipArchive archive)
            {
                this.archive = archive;
            }

            public IEnumerable<IFileNodeInfo> Entries => archive.Entries.Select(e => new ZipEntryWrapper(e));

            public string? Name => null;
            public string? SubName => null;
            public string? Path => null;
            public int? Revision => null;
            public DateTime? CreationTime => null;
            public DateTime? LastWriteTime => null;
            public DateTime? LastAccessTime => null;
            public FileKind Kind => FileKind.ArchiveItem;
            public FileAttributes Attributes => FileAttributes.Directory;
            public Environment.SpecialFolder? SpecialFolderType => null;
            public object? ReferenceKey => archive;
            public object? DataKey => null;

            class ZipEntryWrapper : IFileInfo
            {
                readonly ZipArchiveEntry entry;

                public ZipEntryWrapper(ZipArchiveEntry entry)
                {
                    this.entry = entry;
                }

                public string? Name => entry.Name;
                public string? SubName => null;
                public string? Path => entry.FullName;
                public int? Revision => null;
                public DateTime? CreationTime => null;
                public DateTime? LastWriteTime => entry.LastWriteTime.UtcDateTime;
                public DateTime? LastAccessTime => null;
                public FileKind Kind => FileKind.ArchiveItem;
                public FileAttributes Attributes => FileAttributes.Normal;
                public long Length => entry.Length;
                public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;
                public object? ReferenceKey => entry;
                public object? DataKey => null;

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
                        segment = new ArraySegment<byte>(buffer.ToArray());
                    }
                    return new MemoryStream(segment.Array!, segment.Offset, segment.Count, false);
                }
            }
        }
    }
}
