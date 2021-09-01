using System;

namespace IS4.MultiArchiver.Formats
{
    public interface IFileFormat
    {
        string GetMediaType(object value);
        string GetExtension(object value);
    }

    public interface IFileFormat<in T> : IFileFormat where T : class
    {
        string GetMediaType(T value);
        string GetExtension(T value);
    }

    public abstract class FileFormat<T> : IFileFormat<T> where T : class
    {
        public string MediaType { get; }
        public string Extension { get; }

        public FileFormat(string mediaType, string extension)
        {
            MediaType = mediaType;
            Extension = extension;
        }

        public virtual string GetExtension(T value)
        {
            return Extension;
        }

        public virtual string GetMediaType(T value)
        {
            return MediaType;
        }

        string IFileFormat.GetExtension(object value)
        {
            if(!(value is T obj)) throw new ArgumentException(null, nameof(value));
            return GetExtension(obj);
        }

        string IFileFormat.GetMediaType(object value)
        {
            if(!(value is T obj)) throw new ArgumentException(null, nameof(value));
            return GetMediaType(obj);
        }
    }
}
