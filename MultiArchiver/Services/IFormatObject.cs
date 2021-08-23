using System;

namespace IS4.MultiArchiver.Services
{
    public interface IFormatObject : IIndividualUriFormatter<Uri>
    {
        string Extension { get; }
        string MediaType { get; }
        IFileFormat Format { get; }

        TResult GetValue<TResult, TArgs>(IResultFactory<TResult, TArgs> resultFactory, TArgs args);
    }

    public sealed class FormatObject<T> : IFormatObject where T : class
    {
        public IFileFormat Format { get; }
        public string Extension => Format is IFileFormat<T> fmt ? fmt.GetExtension(Value) : Format.GetExtension(Value);
        public string MediaType => Format is IFileFormat<T> fmt ? fmt.GetMediaType(Value) : Format.GetMediaType(Value);
        public T Value { get; }

        public FormatObject(IFileFormat format, T value)
        {
            Format = format;
            Value = value;
        }

        public override string ToString()
        {
            return $"Media object ({MediaType ?? Extension ?? Format?.ToString()})";
        }

        TResult IFormatObject.GetValue<TResult, TArgs>(IResultFactory<TResult, TArgs> resultFactory, TArgs args)
        {
            return resultFactory.Invoke<T>(Value, args);
        }

        static readonly char[] dataChars = { ';', ',' };
        static readonly char[] splitChar = { '/' };

        public Uri this[Uri value] {
            get {
                if(value.Scheme.Equals("data", StringComparison.OrdinalIgnoreCase))
                {
                    var type = MediaType?.ToLowerInvariant();
                    if(type == null || type.IndexOf('/') == -1) return null;
                    var builder = new UriBuilder(value);
                    int rest = builder.Path.IndexOfAny(dataChars);
                    if(rest == -1) throw new ArgumentException(null, nameof(value));
                    builder.Path = type + builder.Path.Substring(rest);
                    return builder.Uri;
                }
                if(String.IsNullOrEmpty(value.Authority)) throw new ArgumentException(null, nameof(value));
                var sub = Extension?.ToLowerInvariant();
                if(sub == null)
                {
                    sub = MediaType?.ToLowerInvariant();
                    if(sub == null || sub.IndexOf('/') == -1) return null;
                    sub = sub.Split(splitChar)[1];
                    if(sub.StartsWith("prs.") || sub.StartsWith("vnd."))
                    {
                        sub = sub.Substring(4);
                    }else if(sub.StartsWith("x-"))
                    {
                        sub = sub.Substring(2);
                    }
                    int plus = sub.IndexOf('+');
                    if(plus != -1)
                    {
                        sub = sub.Substring(0, plus);
                    }
                }
                return new Uri(value.AbsoluteUri + "/" + sub);
            }
        }
    }
}
