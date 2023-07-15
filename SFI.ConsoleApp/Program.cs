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
using System.Windows.Forms;

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
                return new IFileNodeInfo[] { new DeviceInput(Console.OpenStandardInput) };
            }else if(StandardPaths.IsClipboard(path))
            {
                return new IFileNodeInfo[] { new DeviceInput(() => new ClipboardStream()) };
            }else if(StandardPaths.IsFilePicker(path))
            {
                return new IFileNodeInfo[] { new DeviceInput(() => {
                    var path = ShowDialog<OpenFileDialog>("Load input file", null);
                    return File.OpenRead(path);
                }) };
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
                return new ClipboardStream();
            }else if(StandardPaths.IsFilePicker(path))
            {
                path = ShowDialog<SaveFileDialog>("Save output file", mediaType);
                return File.Create(path);
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

        static readonly byte[] appGuidNS = Guid.Parse("b1ebfec4-0e2f-42e3-8357-6724abc06db3").ToByteArray();

        static string ShowDialog<TDialog>(string title, string? filter) where TDialog : FileDialog, new()
        {
            return StaThread.Invoke(() => {
                var dialog = new TDialog();
                dialog.DereferenceLinks = false;
                dialog.ClientGuid = DataTools.GuidFromName(appGuidNS, title);
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

        /// <summary>
        /// This class represents a device as a file.
        /// </summary>
        class DeviceInput : IFileInfo
        {
            readonly Func<Stream> openFunc;

            public DeviceInput(Func<Stream> openFunc)
            {
                this.openFunc = openFunc;
            }

            public string? Name => null;

            public string? SubName => null;

            public string? Path => null;

            public int? Revision => null;

            public DateTime? CreationTime => null;

            public DateTime? LastWriteTime => null;

            public DateTime? LastAccessTime => null;

            public FileKind Kind => FileKind.None;

            public long Length => -1;

            public FileAttributes Attributes => FileAttributes.Device;

            public StreamFactoryAccess Access => StreamFactoryAccess.Single;

            public object? ReferenceKey => this;

            public object? DataKey => null;

            public Stream Open()
            {
                return openFunc();
            }

            public override string? ToString()
            {
                return null;
            }
        }
    }
}
