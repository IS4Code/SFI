namespace IS4.MultiArchiver.Services
{
    public interface IFormatObject<out T, out TFormat> where T : class where TFormat : class, IFileFormat
    {
        TFormat Format { get; }
        string Extension { get; }
        string MediaType { get; }
        T Value { get; }
    }

    public sealed class FormatObject<T, TFormat> : IFormatObject<T, TFormat> where T : class where TFormat : class, IFileFormat
    {
        public TFormat Format { get; }
        public string Extension => Format is IFileFormat<T> fmt ? fmt.GetExtension(Value) : Format.GetExtension(Value);
        public string MediaType => Format is IFileFormat<T> fmt ? fmt.GetMediaType(Value) : Format.GetMediaType(Value);
        public T Value { get; }

        public FormatObject(TFormat format, T value)
        {
            Format = format;
            Value = value;
        }
    }
}
