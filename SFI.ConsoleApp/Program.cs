﻿using IS4.SFI.Application;
using IS4.SFI.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        public ValueTask Update()
        {
            return default;
        }

        /// <summary>
        /// This class represents the standard input as a file.
        /// </summary>
        class StandardInput : IFileInfo
        {
            public bool IsEncrypted => false;

            public string? Name => null;

            public string? SubName => null;

            public string? Path => null;

            public int? Revision => null;

            public DateTime? CreationTime => null;

            public DateTime? LastWriteTime => null;

            public DateTime? LastAccessTime => null;

            public FileKind Kind => FileKind.None;

            public long Length => -1;

            public StreamFactoryAccess Access => StreamFactoryAccess.Single;

            public object? ReferenceKey => this;

            public object? DataKey => null;

            public Stream Open()
            {
                return Console.OpenStandardInput();
            }
        }
    }
}