using IS4.SFI.Application;
using IS4.SFI.Application.Tools;
using IS4.SFI.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
#if WINDOWS || NETFRAMEWORK
using System.Windows.Forms;
#endif

namespace IS4.SFI.ConsoleApp
{
    /// <summary>
    /// The main console application class and its environment.
    /// </summary>
    class Program : IApplicationEnvironment
    {
        /// <inheritdoc/>
        public int WindowWidth => Console.WindowWidth;

        /// <inheritdoc/>
        public TextWriter LogWriter => Console.Error;

        /// <inheritdoc/>
        public string NewLine => Environment.NewLine;

        /// <inheritdoc/>
        public string? ExecutableName => Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule?.ModuleName);

        /// <summary>
        /// The entry point of the program.
        /// </summary>
        /// <param name="args">The arguments to the application.</param>
        static async Task Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture =
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

            var application = new Application<ConsoleInspector>(new Program());
            await application.Run(args);
        }

        public IEnumerable<IFileNodeInfo> GetFiles(string path)
        {
            if(StandardPaths.IsInput(path))
            {
                return new IFileNodeInfo[] { new DeviceFileInfo(Console.OpenStandardInput) };
            }else if(StandardPaths.IsClipboard(path))
            {
#if WINDOWS || NETFRAMEWORK
                return new IFileNodeInfo[] { new DeviceFileInfo(() => new ClipboardStream()) };
#else
                ShowPlatformFileUnavailableException("clipboard");
#endif
            }else if(StandardPaths.IsFilePicker(path))
            {
#if WINDOWS || NETFRAMEWORK
                return new IFileNodeInfo[] { new DeviceFileInfo(() => {
                    var path = ShowDialog<OpenFileDialog>("Load input file", null);
                    return File.OpenRead(path);
                }) };
#else
                ShowPlatformFileUnavailableException("file picker");
#endif
            }else if(StandardPaths.IsDirectoryPicker(path))
            {
#if WINDOWS || NETFRAMEWORK
                const string title = "Load input folder";
                return StaThread.Invoke(() => {
                    using var dialog = new FolderBrowserDialog();
                    dialog.Description = title;
                    dialog.ShowNewFolderButton = false;
#if NET5_0_OR_GREATER
                    dialog.ClientGuid = DataTools.GuidFromName(appGuidNS, title);
                    dialog.AutoUpgradeEnabled = true;
                    dialog.UseDescriptionForTitle = true;
#endif

                    DialogResult result;
                    var window = ConsoleWindow.Instance;
                    if(window.Handle != IntPtr.Zero)
                    {
                        result = dialog.ShowDialog(window);
                    }else{
                        result = dialog.ShowDialog();
                    }
                    if(result != DialogResult.OK)
                    {
                        throw new ApplicationException($"{title} dialog canceled!");
                    }
                    return new IFileNodeInfo[] {
                        new DirectoryInfoWrapper(new DirectoryInfo(dialog.SelectedPath))
                    };
                });
#else
                ShowPlatformFileUnavailableException("folder picker");
#endif
            }
            var fileName = Path.GetFileName(path);
            if(fileName.Contains('*') || fileName.Contains('?'))
            {
                var directory = Path.GetDirectoryName(path);
                if(String.IsNullOrEmpty(directory))
                {
                    directory = Environment.CurrentDirectory;
                }
                IEnumerable<IFileNodeInfo> files = Directory.GetFiles(directory, fileName).Select(f => new FileInfoWrapper(new FileInfo(f)));
                var directories = Directory.GetDirectories(directory, fileName).Select(d => new DirectoryInfoWrapper(new DirectoryInfo(d)));
                return files.Concat(directories);
            }
            if(File.Exists(path))
            {
                return new IFileNodeInfo[] { new FileInfoWrapper(new FileInfo(path)) };
            }
            if(Directory.Exists(path))
            {
                return new IFileNodeInfo[] { new DirectoryInfoWrapper(new DirectoryInfo(path)) };
            }
            return Array.Empty<IFileNodeInfo>();
        }

        public Stream CreateFile(string path, string mediaType)
        {
            if(StandardPaths.IsOutput(path))
            {
                return Console.OpenStandardOutput();
            }else if(StandardPaths.IsError(path))
            {
                return Console.OpenStandardError();
            }else if(StandardPaths.IsNull(path))
            {
                return Stream.Null;
            }else if(StandardPaths.IsClipboard(path))
            {
#if WINDOWS || NETFRAMEWORK
                return new ClipboardStream();
#else
                ShowPlatformFileUnavailableException("clipboard");
#endif
            } else if(StandardPaths.IsFilePicker(path))
            {
#if WINDOWS || NETFRAMEWORK
                path = ShowDialog<SaveFileDialog>("Save output file", mediaType);
                return File.Create(path);
#else
                ShowPlatformFileUnavailableException("file picker");
#endif
            }
            var dir = Path.GetDirectoryName(path);
            if(!String.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return File.Create(path);
        }

        public ValueTask Update()
        {
            return default;
        }

#if WINDOWS || NETFRAMEWORK
        static readonly byte[] appGuidNS = Guid.Parse("b1ebfec4-0e2f-42e3-8357-6724abc06db3").ToByteArray();

        static string ShowDialog<TDialog>(string title, string? filter) where TDialog : FileDialog, new()
        {
            return StaThread.Invoke(() => {
                using var dialog = new TDialog();
                dialog.DereferenceLinks = false;
#if NET5_0_OR_GREATER
                dialog.ClientGuid = DataTools.GuidFromName(appGuidNS, title);
#endif
                dialog.Title = title;
                if(filter != null)
                {
                    dialog.Filter = filter + "|*";
                }

                DialogResult result;
                var window = ConsoleWindow.Instance;
                if(window.Handle != IntPtr.Zero)
                {
                    result = dialog.ShowDialog(window);
                }else{
                    result = dialog.ShowDialog();
                }
                if(result != DialogResult.OK)
                {
                    throw new ApplicationException($"{title} dialog canceled!");
                }
                return dialog.FileName;
            });
        }

        class ConsoleWindow : IWin32Window
        {
            public static readonly ConsoleWindow Instance = new ConsoleWindow();

            readonly Process process = Process.GetCurrentProcess();

            public IntPtr Handle => process.MainWindowHandle;
        }
#else
        static void ShowPlatformFileUnavailableException(string name)
        {
            if(System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                throw new ApplicationException($"The special {name} file is available only in distributions targeting Windows.");
            }
            throw new ApplicationException($"The special {name} file is available only in Windows.");
        }
#endif
    }
}
