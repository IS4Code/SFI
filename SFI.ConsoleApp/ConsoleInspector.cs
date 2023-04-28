using IS4.SFI.Analyzers;
using IS4.SFI.Application;
using IS4.SFI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
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
                    yield return GetPluginFromDirectory(dir);
                }

                foreach(var zip in Directory.EnumerateFiles(baseDirectory, "*.zip"))
                {
                    yield return GetPluginFromZip(zip);
                }
            }
        }

        Plugin GetPluginFromDirectory(string dir)
        {
            // Look for a file with .dll and the same name as the directory
            var name = Path.GetFileName(dir) + ".dll";
            var info = new DirectoryInfo(dir);
            return new Plugin(GetDirectory(info), name);
        }

        Plugin GetPluginFromZip(string file)
        {
            // Look for a file with .zip changed to .dll
            var name = Path.ChangeExtension(Path.GetFileName(file), "dll");
            ZipArchive archive;
            try{
                archive = ZipFile.OpenRead(file);
            }catch(Exception e)
            {
                OutputLog?.WriteLine($"An error occurred while opening plugin archive {Path.GetFileName(file)}: " + e.Message);
                return default;
            }
            return new Plugin(GetDirectory(archive), name);
        }

        protected override Assembly LoadFromFile(IFileInfo file, IDirectoryInfo mainDirectory)
        {
            // Add directories for assembly lookup:
            var context = new PluginLoadContext();
            context.AddDirectory(mainDirectory);
            context.AddDirectory(baseDirectory);
            context.AddDirectory(AppContext.BaseDirectory);
            return context.LoadFromFile(file);
        }
    }
}
