namespace IS4.MultiArchiver.Services
{
    public interface IFormatObject<out T>
    {
        string Extension { get; }
        string MediaType { get; }
        T Value { get; }
    }

    public class FormatObject<T> : IFormatObject<T>
    {
        readonly IFileFormat format;
        public string Extension => format is IFileFormat<T> fmt ? fmt.GetExtension(Value) : format.GetExtension(Value);
        public string MediaType => format is IFileFormat<T> fmt ? fmt.GetMediaType(Value) : format.GetMediaType(Value);
        public T Value { get; }

        public FormatObject(IFileFormat format, T value)
        {
            this.format = format;
            Value = value;
        }
    }
}
