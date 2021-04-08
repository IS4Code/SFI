using System;
using System.IO;

namespace IS4.MultiArchiver.Services
{
    public interface IFileFormat
    {
        int HeaderLength { get; }
        string MediaType { get; }
        string Extension { get; }

        bool Match(ArraySegment<byte> header);
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

        public virtual bool Match(ArraySegment<byte> header)
        {
            return Match(header.AsSpan());
        }

        public abstract bool Match(Span<byte> header);
    }
}
