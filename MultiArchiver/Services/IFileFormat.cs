using System;
using System.IO;

namespace IS4.MultiArchiver.Services
{
    public interface IFileFormat
    {
        int HeaderLength { get; }
        string MediaType { get; }
        string Extension { get; }

        bool Match(Span<byte> header);
    }

    public interface IFileLoader : IFileFormat
    {
        IDisposable Match(Stream stream);
    }

    public interface IFileReader : IFileFormat
    {
        ILinkedNode Match(Stream stream, ILinkedNodeFactory analyzer);
    }

    public abstract class FileFormat : IFileFormat
    {
        public int HeaderLength { get; }
        public string MediaType { get; }
        public string Extension { get; }

        public FileFormat(int headerLength, string mediaType, string extension)
        {
            HeaderLength = headerLength;
            MediaType = mediaType;
            Extension = extension;
        }

        public abstract bool Match(Span<byte> header);
    }
}
