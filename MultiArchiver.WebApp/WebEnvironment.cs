using IS4.MultiArchiver.Services;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;

namespace IS4.MultiArchiver.WebApp
{
    public class WebEnvironment : IApplicationEnvironment
    {
        readonly IJSInProcessRuntime js;
        readonly IReadOnlyDictionary<string, IBrowserFile> inputFiles;
        readonly IDictionary<string, BlobArrayStream> outputFiles;

        public int WindowWidth => Int32.MaxValue;

        public TextWriter LogWriter { get; }

        public string NewLine { get; }

        public WebEnvironment(IJSInProcessRuntime js, TextWriter writer, IReadOnlyDictionary<string, IBrowserFile> inputFiles, IDictionary<string, BlobArrayStream> outputFiles)
        {
            this.js = js;
            LogWriter = writer;
            this.inputFiles = inputFiles;
            this.outputFiles = outputFiles;
            NewLine = js.Invoke<string>("getNewline");
        }

        public IFileInfo GetFile(string path)
        {
            if(inputFiles == null || !inputFiles.TryGetValue(path, out var file))
            {
                throw new FileNotFoundException();
            }
            return new BrowserFileInfo(file);
        }

        public Stream CreateFile(string path, string mediaType)
        {
            return outputFiles[path] = new BlobArrayStream(js, mediaType);
        }
    }
}
