using IS4.MultiArchiver.Formats;
using System;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Services
{
    /// <summary>
    /// Contains information about a recognized format from data.
    /// </summary>
    public interface IFormatObject : IIndividualUriFormatter<Uri>
    {
        /// <summary>
        /// The common extension of the format.
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// The media type of the format.
        /// </summary>
        string MediaType { get; }

        /// <summary>
        /// The format recognized from the data.
        /// </summary>
        IFileFormat Format { get; }

        /// <summary>
        /// Obtains the specific value parsed via the format,
        /// by calling the provided instance of <see cref="IResultFactory{TResult, TArgs}"/>.
        /// </summary>
        /// <typeparam name="TResult">The user-defined type of the result of <see cref="IResultFactory{TResult, TArgs}.Invoke{T}(T, TArgs)"/>.</typeparam>
        /// <typeparam name="TArgs">The user-defined type of the arguments to <see cref="IResultFactory{TResult, TArgs}.Invoke{T}(T, TArgs)"/>.</typeparam>
        /// <param name="resultFactory">The object which receives the value stored in the instance.</param>
        /// <param name="args">Additional arguments provided to <paramref name="resultFactory"/>.</param>
        /// <returns>The result of <paramref name="resultFactory"/>.</returns>
        ValueTask<TResult> GetValue<TResult, TArgs>(IResultFactory<TResult, TArgs> resultFactory, TArgs args);
    }

    /// <summary>
    /// Contains information about a recognized format from data,
    /// alongside a particular instance parsed from the data.
    /// </summary>
    /// <typeparam name="T">The type of the stored object.</typeparam>
    public interface IFormatObject<out T> : IFormatObject
    {
        /// <summary>
        /// The parsed value, also obtained via <see cref="IFormatObject.GetValue{TResult, TArgs}(IResultFactory{TResult, TArgs}, TArgs)"/>.
        /// </summary>
        T Value { get; }
    }

    /// <summary>
    /// Represents a format object for a binary format, created as a result of
    /// parsing data described by <see cref="IDataObject"/>.
    /// </summary>
    public interface IBinaryFormatObject : IFormatObject
    {
        /// <summary>
        /// Contains information about the raw data.
        /// </summary>
        IDataObject Data { get; }
    }

    /// <summary>
    /// Represents a format object for a binary format, created as a result of
    /// parsing data described by <see cref="IDataObject"/>,
    /// alongside a particular instance parsed from the data.
    /// </summary>
    /// <typeparam name="T">The type of the stored object.</typeparam>
    public interface IBinaryFormatObject<out T> : IBinaryFormatObject, IFormatObject<T>
    {

    }

    /// <summary>
    /// The base implementation of <see cref="IFormatObject{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the stored object.</typeparam>
    public class FormatObject<T> : IFormatObject<T> where T : class
    {
        public IFileFormat Format { get; }
        public string Extension => Format is IFileFormat<T> fmt ? fmt.GetExtension(Value) : Format.GetExtension(Value);
        public string MediaType => Format is IFileFormat<T> fmt ? fmt.GetMediaType(Value) : Format.GetMediaType(Value);
        public T Value { get; }

        /// <summary>
        /// Creates a new instance of the format object.
        /// </summary>
        /// <param name="format">The value of <see cref="Format"/>.</param>
        /// <param name="value">The value of <see cref="Value"/>.</param>
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

        /// <summary>
        /// Applies the format referenced by this object to a URI
        /// identifying the raw data.
        /// </summary>
        /// <param name="value">The URI identifying the original raw data.</param>
        /// <returns>The new URI reflecting the format.</returns>
        public Uri this[Uri value] {
            get {
                if(value is IIndividualUriFormatter<IFormatObject> formatter)
                {
                    // Custom instances of Uris may be formatters, such as the data: URI
                    try{
                        if(formatter[this] is Uri result)
                        {
                            return result;
                        }
                    }catch(UriFormatException) when(GlobalOptions.SuppressNonCriticalExceptions)
                    {
                        return null;
                    }
                }
                if(String.IsNullOrEmpty(value.Authority)) return null;
                var sub = Extension?.ToLowerInvariant();
                if(sub == null)
                {
                    // The extension is not defined, use bare media type
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
                // Append it after the whole URI
                return new Uri(value.AbsoluteUri + "/" + sub);
            }
        }
    }

    /// <summary>
    /// The base implementation of <see cref="IBinaryFormatObject{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the stored object.</typeparam>
    public sealed class BinaryFormatObject<T> : FormatObject<T>, IBinaryFormatObject<T> where T : class
    {
        public IDataObject Data { get; }

        /// <inheritdoc cref="FormatObject{T}.Format"/>
        public new IBinaryFileFormat Format => (IBinaryFileFormat)base.Format;

        /// <param name="data">The value of <see cref="Data"/>.</param>
        /// <inheritdoc cref="FormatObject{T}.FormatObject(IFileFormat, T)"/>
        public BinaryFormatObject(IDataObject data, IBinaryFileFormat format, T value) : base(format, value)
        {
            Data = data;
        }
    }
}
