using System;
using System.IO;

namespace IS4.SFI
{
    /// <summary>
    /// Stores utility methods for manipulating files.
    /// </summary>
    public static class FileTools
    {
        /// <summary>
        /// Produces a disposable instance of <see cref="TemporaryFile"/> from a newly created
        /// temporary file whose name is based on <paramref name="identifier"/>.
        /// </summary>
        /// <param name="identifier">Part of the temporary file's name to distinguish it among others.</param>
        /// <returns>A new instance of <see cref="TemporaryFile"/> representing the file.</returns>
        public static TemporaryFile GetTemporaryFile(string identifier)
        {
            return new TemporaryFile(identifier);
        }

        /// <summary>
        /// Managed the lifetime of a temporary file.
        /// </summary>
        public struct TemporaryFile : IDisposable, IEquatable<TemporaryFile>
        {
            /// <summary>
            /// The path to the file.
            /// </summary>
            public string Path { get; }

            internal TemporaryFile(string identifier)
            {
                Path = System.IO.Path.GetTempPath() + "sfi_" + identifier + "_" + Guid.NewGuid().ToString();
            }

            /// <summary>
            /// Returns the value of the <see cref="Path"/> property.
            /// </summary>
            /// <param name="file">The instance to retrieve the value from.</param>
            public static implicit operator string(TemporaryFile file)
            {
                return file.Path;
            }

            /// <summary>
            /// If the temporary file still exists, deletes it.
            /// </summary>
            public void Dispose()
            {
                if(Path != null && File.Exists(Path))
                {
                    try{
                        File.Delete(Path);
                    }catch(FileNotFoundException)
                    {

                    }
                }
            }

            /// <inheritdoc/>
            public bool Equals(TemporaryFile other)
            {
                return Path == other.Path;
            }
        }
    }
}
