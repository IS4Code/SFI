using IS4.SFI.Application.Plugins;
using IS4.SFI.Application.Tools;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace IS4.SFI.Application
{
    /// <summary>
    /// An implementation of <see cref="Inspector"/> allowing loading of plugins.
    /// </summary>
    public abstract class ExtensibleInspector : ComponentInspector, ILogger
    {
        /// <summary>
        /// Contains the collection of plugins loaded by the inspector.
        /// </summary>
        public ICollection<Plugin> Plugins { get; } = new List<Plugin>();

        /// <summary>
        /// Contains the collection of additional plugin identifiers to load.
        /// </summary>
        public ICollection<string> AdditionalPluginIdentifiers { get; } = new List<string>();

        /// <summary>
        /// Provides support for resolving plugins.
        /// </summary>
        protected PluginResolvers PluginResolvers { get; }

        /// <inheritdoc/>
        public ExtensibleInspector()
        {
            PluginResolvers = new(this);
        }

        /// <inheritdoc/>
        public async override ValueTask AddDefault()
        {
            foreach(var pluginId in AdditionalPluginIdentifiers)
            {
                Plugins.Add(await PluginResolvers.GetPluginAsync(pluginId, CancellationToken.None));
            }

            await base.AddDefault();

            await LoadPlugins();
        }

        async ValueTask LoadPlugins()
        {
            CaptureCollections(this, true);

            var loaded = new Dictionary<Assembly, int>();

            var loadContext = new PluginLoadContext();
            AddLoadDirectories(loadContext);

            foreach(var plugin in Plugins)
            {
                if(plugin.Directory == null) continue;

                foreach(var component in LoadPlugin(plugin.Directory, plugin.MainFile, ref loadContext))
                {
                    int componentCount = await LoadIntoCollections(component);

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
                    OutputLog?.LogInformation($"Loaded {count} component{(count == 1 ? "" : "s")} from assembly {asm.GetName().Name}.");
                }
            }
        }

        /// <summary>
        /// Add additional directories for assembly lookup to an instance
        /// of <see cref="PluginLoadContext"/>.
        /// </summary>
        /// <param name="loadContext">The assembly load context to add to.</param>
        protected virtual void AddLoadDirectories(PluginLoadContext loadContext)
        {

        }

        /// <summary>
        /// Loads a plugin from a file in a directory.
        /// </summary>
        /// <param name="mainDirectory">The directory to search.</param>
        /// <param name="mainFile">The name of the main file.</param>
        /// <param name="previousContext">The context used to load the previous plugin.</param>
        /// <returns>A collection of all instantiable types in the assembly, together with their constructor.</returns>
        IEnumerable<ComponentType> LoadPlugin(IDirectoryInfo mainDirectory, string mainFile, ref PluginLoadContext? previousContext)
        {
            // Add DI services:
            var services = new ServiceCollection();
            services.AddSingleton<Inspector>(this);
            services.AddSingleton(OutputLog ?? NullLogger.Instance);
            services.AddSingleton(mainDirectory);
            var serviceProvider = services.BuildServiceProvider();

            Assembly asm;
            try{
                var mainEntry = mainDirectory.Entries.OfType<IFileInfo>().FirstOrDefault(e => mainFile.Equals(e.Name, StringComparison.OrdinalIgnoreCase));

                if(mainEntry == null)
                {
                    OutputLog?.LogWarning($"Cannot find main library file {mainFile} inside the plugin.");
                    return Array.Empty<ComponentType>();
                }

                var context = new PluginLoadContext(previousContext);
                context.AddDirectory(mainDirectory);
                asm = context.LoadFromFile(mainEntry);
                previousContext = context;
            }catch(Exception e)
            {
                var pluginName = Path.GetFileNameWithoutExtension(mainFile);
                OutputLog?.LogError(e, $"An error occurred while loading plugin {pluginName}.");
                return Array.Empty<ComponentType>();
            }

            return OpenAssembly(asm, serviceProvider);
        }

        #region ILogger implementation
        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            OutputLog?.Log(logLevel, eventId, state, exception, formatter);
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return OutputLog?.IsEnabled(logLevel) ?? false;
        }

        IDisposable? ILogger.BeginScope<TState>(TState state)
        {
            return OutputLog?.BeginScope(state);
        }
        #endregion
    }
}
