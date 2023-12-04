using System;
using System.Runtime.InteropServices;

namespace IS4.SFI.Application.Tools
{
    /// <summary>
    /// Provides helper methods for checking whether given paths match certain special destinations.
    /// </summary>
    public static class StandardPaths
    {
        static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        
        /// <summary>
        /// Checks whether <paramref name="path"/> identifies the null stream.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>
        /// Whether the path equals <c>/dev/null</c>, or <c>NUL</c> (case-insensitive)
        /// on Windows systems.
        /// </returns>
        public static bool IsNull(string path)
        {
            return path == "/dev/null" || (IsWindows && path.Equals("NUL", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Checks whether <paramref name="path"/> identifies the standard input.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>
        /// Whether the path equals <c>-</c>, <c>/dev/fd/0</c>, or <c>/dev/stdin</c>.
        /// </returns>
        public static bool IsInput(string path)
        {
            return path is "-" or "/dev/fd/0" or "/dev/stdin";
        }

        /// <summary>
        /// Checks whether <paramref name="path"/> identifies the standard output.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>
        /// Whether the path equals <c>-</c>, <c>/dev/fd/1</c>, or <c>/dev/stdout</c>.
        /// </returns>
        public static bool IsOutput(string path)
        {
            return path is "-" or "/dev/fd/1" or "/dev/stdout";
        }

        /// <summary>
        /// Checks whether <paramref name="path"/> identifies the standard error.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>
        /// Whether the path equals <c>/dev/fd/2</c>, or <c>/dev/stderr</c>.
        /// </returns>
        public static bool IsError(string path)
        {
            return path is "/dev/fd/2" or "/dev/stderr";
        }

        /// <summary>
        /// Checks whether <paramref name="path"/> identifies the clipboard device.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>
        /// Whether the path equals <c>/dev/clipboard</c>.
        /// </returns>
        public static bool IsClipboard(string path)
        {
            return path is "/dev/clipboard";
        }

        /// <summary>
        /// Checks whether <paramref name="path"/> identifies the file picker dialog.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>
        /// Whether the path equals <c>/dev/picker</c>.
        /// </returns>
        public static bool IsFilePicker(string path)
        {
            return path is "/dev/picker";
        }

        /// <summary>
        /// Checks whether <paramref name="path"/> identifies the directory picker dialog.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>
        /// Whether the path equals <c>/dev/folderpicker</c>.
        /// </returns>
        public static bool IsDirectoryPicker(string path)
        {
            return path is "/dev/folderpicker";
        }
    }
}
