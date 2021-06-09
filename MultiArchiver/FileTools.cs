using System;
using System.IO;

namespace IS4.MultiArchiver
{
    public static class FileTools
    {
        public static TemporaryFile GetTemporaryFile(string identifier)
        {
            return new TemporaryFile(identifier);
        }

        public struct TemporaryFile : IDisposable, IEquatable<TemporaryFile>
        {
            public string Path { get; }

            internal TemporaryFile(string identifier)
            {
                Path = System.IO.Path.GetTempPath() + "ma_" + identifier + "_" + Guid.NewGuid().ToString();
            }

            public static implicit operator string(TemporaryFile file)
            {
                return file.Path;
            }

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

            public bool Equals(TemporaryFile other)
            {
                return Path == other.Path;
            }
        }
    }
}
