using System;

namespace IS4.MultiArchiver.Services
{
    public interface IFormatObject<out T, out TFormat> : IUriFormatter<Uri> where T : class where TFormat : class, IFileFormat
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

        public override string ToString()
        {
            return MediaType ?? Format?.ToString();
        }

        static readonly char[] dataChars = { ';', ',' };
        static readonly char[] splitChar = { '/' };

        public Uri FormatUri(Uri value)
        {
            var type = MediaType;
            if(type == null || type.IndexOf('/') == -1) throw new InvalidOperationException();
            if(value.Scheme.Equals("data", StringComparison.OrdinalIgnoreCase))
            {
                var builder = new UriBuilder(value);
                int rest = builder.Path.IndexOfAny(dataChars);
                if(rest == -1) throw new ArgumentException(null, nameof(value));
                builder.Path = type + builder.Path.Substring(rest);
                return builder.Uri;
            }
            if(String.IsNullOrEmpty(value.Authority)) throw new ArgumentException(null, nameof(value));
            return new Uri(value.AbsoluteUri + "/" + type.Split(splitChar)[1]);
        }
    }
}
