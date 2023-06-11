using IS4.SFI.Analyzers;
using IS4.SFI.Application;
using IS4.SFI.Application.Plugins;
using IS4.SFI.Application.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.ConsoleApp
{
    /// <summary>
    /// The specific implementation of <see cref="Inspector"/> for the console application.
    /// </summary>
    class ConsoleInspector : ExtensibleInspector
    {
        string baseDirectory = Path.Combine(AppContext.BaseDirectory, "plugins");

        /// <summary>
        /// The default image analyzer.
        /// </summary>
        public ImageAnalyzer ImageAnalyzer { get; }

        /// <inheritdoc/>
        public ConsoleInspector()
        {
            Analyzers.Add(ImageAnalyzer = new ImageAnalyzer());

            var algorithms = ImageAnalyzer.DataHashAlgorithms;
            foreach(var algorithm in DataAnalyzer.HashAlgorithms)
            {
                algorithms.Add(algorithm);
            }
        }

        public async override ValueTask AddDefault()
        {
            await LoadAssembly(BaseFormats.Assembly);
            await LoadAssembly(ExternalFormats.Assembly);
            await LoadAssembly(AccessoriesFormats.Assembly);
            await LoadAssembly(MediaAnalysisFormats.Assembly);
            await LoadAssembly(WindowsFormats.Assembly);

            Plugins.Clear();
            foreach(var plugin in LoadPlugins())
            {
                Plugins.Add(plugin);
            }

            await base.AddDefault();
        }

        IEnumerable<Plugin> LoadPlugins()
        {
            if(Directory.Exists(baseDirectory))
            {
                foreach(var dir in Directory.EnumerateDirectories(baseDirectory))
                {
                    yield return PluginResolvers.GetPluginFromDirectory(dir);
                }

                foreach(var zip in Directory.EnumerateFiles(baseDirectory, "*.zip"))
                {
                    yield return PluginResolvers.GetPluginFromZip(zip);
                }
            }
        }

        /// <inheritdoc/>
        protected override void AddLoadDirectories(PluginLoadContext context)
        {
            context.AddDirectory(baseDirectory);
            context.AddDirectory(AppContext.BaseDirectory);
        }
    }
}
