﻿using IS4.SFI.Services;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Application
{
    /// <summary>
    /// Provides environment-specific properties and methods.
    /// </summary>
    public interface IApplicationEnvironment
    {
        /// <summary>
        /// The width of the console window or screen in characters.
        /// </summary>
        int WindowWidth { get; }

        /// <summary>
        /// The sequence of characters to use as a newline.
        /// </summary>
        string NewLine { get; }

        /// <summary>
        /// The writer to use for log messages or diagnostics.
        /// </summary>
        TextWriter LogWriter { get; }

        /// <summary>
        /// The name of the executable.
        /// </summary>
        string? ExecutableName { get; }

        /// <summary>
        /// Retrieves a collection of files identified by <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to the files, supporting wildcards.</param>
        /// <returns>The collection of files with names matching <paramref name="path"/>.</returns>
        IEnumerable<IFileNodeInfo> GetFiles(string path);

        /// <summary>
        /// Creates an output file and opens a write stream to it.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="mediaType">The media type of the file.</param>
        /// <returns>A stream to the newly created file.</returns>
        Stream CreateFile(string path, string mediaType);

        /// <summary>
        /// Updates the application with new results.
        /// </summary>
        ValueTask Update();
    }
}
