using IS4.MultiArchiver.Formats;
using System;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Services
{
    public interface IFormatObject : IIndividualUriFormatter<Uri>
    {
        string Extension { get; }
        string MediaType { get; }
        IFileFormat Format { get; }

        ValueTask<TResult> GetValue<TResult, TArgs>(IResultFactory<TResult, TArgs> resultFactory, TArgs args);
    }

    public interface IFormatObject<out T> : IFormatObject
    {
        T Value { get; }
    }

    public interface IBinaryFormatObject : IFormatObject
    {
        IDataObject Data { get; }
    }

    public interface IBinaryFormatObject<out T> : IBinaryFormatObject, IFormatObject<T>
    {

    }

    public class FormatObject<T> : IFormatObject<T> where T : class
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

        async ValueTask<TResult> IFormatObject.GetValue<TResult, TArgs>(IResultFactory<TResult, TArgs> resultFactory, TArgs args)
        {
            return await resultFactory.Invoke<T>(Value, args);
        }

        static readonly char[] splitChar = { '/' };

        public Uri this[Uri value] {
            get {
                if(value is IIndividualUriFormatter<IFormatObject> formatter)
                {
                    if(formatter[this] is Uri result)
                    {
                        return result;
                    }
                }
                if(String.IsNullOrEmpty(value.Authority)) return UriTools.CreateUuid(Guid.NewGuid());
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

    public sealed class BinaryFormatObject<T> : FormatObject<T>, IBinaryFormatObject<T> where T : class
    {
        public IDataObject Data { get; }

        public new IBinaryFileFormat Format => (IBinaryFileFormat)base.Format;

        public BinaryFormatObject(IDataObject data, IBinaryFileFormat format, T value) : base(format, value)
        {
            Data = data;
        }
    }
}
