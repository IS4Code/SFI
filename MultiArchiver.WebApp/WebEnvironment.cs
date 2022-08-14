using IS4.MultiArchiver.Services;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IS4.MultiArchiver.WebApp
{
    /// <summary>
    /// The implementation of <see cref="IApplicationEnvironment"/>
    /// for the web environment.
    /// </summary>
    public class WebEnvironment : IApplicationEnvironment
    {
        readonly IJSInProcessRuntime js;
        readonly IReadOnlyDictionary<string, IBrowserFile> inputFiles;
        readonly IDictionary<string, BlobArrayStream> outputFiles;

        public int WindowWidth => Int32.MaxValue;

        public TextWriter LogWriter { get; }

        public string NewLine { get; }

        /// <summary>
        /// Creates a new instance of the environment.
        /// </summary>
        /// <param name="js">The JavaScript runtime to use.</param>
        /// <param name="writer">The writer for logging.</param>
        /// <param name="inputFiles">The collection of input files.</param>
        /// <param name="outputFiles">The collection of output files, which may be modified during the lifetime of the instance.</param>
        public WebEnvironment(IJSInProcessRuntime js, TextWriter writer, IReadOnlyDictionary<string, IBrowserFile> inputFiles, IDictionary<string, BlobArrayStream> outputFiles)
        {
            this.js = js;
            LogWriter = writer;
            this.inputFiles = inputFiles;
            this.outputFiles = outputFiles;
            NewLine = js.Invoke<string>("getNewline");
        }

        public IEnumerable<IFileInfo> GetFiles(string path)
        {
            if(inputFiles == null)
            {
                return Array.Empty<IFileInfo>();
            }
            var match = DataTools.ConvertWildcardToRegex(path);
            return inputFiles.Where(f => match.IsMatch(f.Key)).Select(f => new BrowserFileInfo(f.Value));
        }

        public Stream CreateFile(string path, string mediaType)
        {
            return outputFiles[path] = new BlobArrayStream(js, mediaType);
        }
    }
}
