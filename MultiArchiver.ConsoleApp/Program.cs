using IS4.MultiArchiver.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.ConsoleApp
{
    class Program : IApplicationEnvironment
    {
        public int WindowWidth => Console.WindowWidth;

        public TextWriter LogWriter => Console.Error;

        public string NewLine => Environment.NewLine;

        static async Task Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture =
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

            var application = new Application<ConsoleArchiver>(new Program());
            await application.Run(args);
        }

        public IEnumerable<IFileInfo> GetFiles(string path)
        {
            if(path == "-") return new IFileInfo[] { new StandardInput() };
            var fileName = Path.GetFileName(path);
            if(fileName.Contains('*') || fileName.Contains('?'))
            {
                var directory = Path.GetDirectoryName(path);
                var files = Directory.GetFiles(String.IsNullOrEmpty(directory) ? Environment.CurrentDirectory : directory, fileName);
                return files.Select(f => new FileInfoWrapper(new FileInfo(f)));
            }
            return new IFileInfo[] { new FileInfoWrapper(new FileInfo(path)) };
        }

        public Stream CreateFile(string path, string mediaType)
        {
            if(path == "-") return Console.OpenStandardOutput();
            return File.Create(path);
        }

        class StandardInput : IFileInfo
        {
            public bool IsEncrypted => false;

            public string Name => null;

            public string SubName => null;

            public string Path => null;

            public int? Revision => null;

            public DateTime? CreationTime => null;

            public DateTime? LastWriteTime => null;

            public DateTime? LastAccessTime => null;

            public FileKind Kind => FileKind.None;

            public long Length => -1;

            public StreamFactoryAccess Access => StreamFactoryAccess.Single;

            public object ReferenceKey => null;

            public object DataKey => null;

            public Stream Open()
            {
                return Console.OpenStandardInput();
            }
        }
    }
}
