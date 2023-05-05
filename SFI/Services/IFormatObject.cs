using IS4.SFI.Formats;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IS4.SFI.Services
{
    /// <summary>
    /// Contains information about a recognized format from data.
    /// </summary>
    public interface IFormatObject : IIndividualUriFormatter<Uri>, IPersistentKey
    {
        /// <summary>
        /// The common extension of the format.
        /// </summary>
        string? Extension { get; }

        /// <summary>
        /// The media type of the format.
        /// </summary>
        string? MediaType { get; }

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
        /// <inheritdoc/>
        public IFileFormat Format { get; }

        /// <inheritdoc/>
        public string? Extension => Format is IFileFormat<T> fmt ? fmt.GetExtension(Value) : Format.GetExtension(Value);

        /// <inheritdoc/>
        public string? MediaType => Format is IFileFormat<T> fmt ? fmt.GetMediaType(Value) : Format.GetMediaType(Value);

        /// <inheritdoc/>
        public T Value { get; }

        /// <inheritdoc cref="IPersistentKey.ReferenceKey"/>
        protected virtual object? ReferenceKey => Value is IPersistentKey key ? key.ReferenceKey : Value;

        /// <inheritdoc cref="IPersistentKey.DataKey"/>
        protected virtual object? DataKey => Value is IPersistentKey key ? (Format, key.DataKey) : Format;

        object? IPersistentKey.ReferenceKey => ReferenceKey;

        object? IPersistentKey.DataKey => DataKey;

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

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Media object ({MediaType ?? Extension ?? Format?.ToString()})";
        }

        async ValueTask<TResult> IFormatObject.GetValue<TResult, TArgs>(IResultFactory<TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            return await resultFactory.Invoke<T>(Value, args);
        }

        /// <summary>
        /// Captures the core descriptive component of a MIME type, such as
        /// <c>bsf</c> from <c>application/vnd.3gpp.bsf+xml</c>.
        /// </summary>
        static readonly Regex mimeCoreName = new(@"^[^/]+/(?:[^.]+\.|x-)*([^;+]*[^;+.\d-][^;+]*)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Applies the format referenced by this object to a URI
        /// identifying the raw data.
        /// </summary>
        /// <param name="value">The URI identifying the original raw data.</param>
        /// <returns>The new URI reflecting the format.</returns>
        public Uri? this[Uri value] {
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
                var sub = Extension?.ToLowerInvariant();
                if(sub == null)
                {
                    // The extension is not defined, extract something from the media type
                    sub = MediaType;
                    if(sub == null) return null;
                    if(mimeCoreName.Match(sub) is not { Success: true } match) return null;
                    sub = match.Groups[1].Value.ToLowerInvariant();
                }
                // Append it after the whole URI
                return UriTools.MakeSubUri(value, sub, false);
            }
        }
    }

    /// <summary>
    /// The base implementation of <see cref="IBinaryFormatObject{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the stored object.</typeparam>
    public sealed class BinaryFormatObject<T> : FormatObject<T>, IBinaryFormatObject<T> where T : class
    {
        /// <inheritdoc/>
        public IDataObject Data { get; }

        /// <inheritdoc cref="FormatObject{T}.Format"/>
        public new IBinaryFileFormat Format => (IBinaryFileFormat)base.Format;

        /// <inheritdoc/>
        protected override object? ReferenceKey => Data.ReferenceKey;

        /// <inheritdoc/>
        protected override object? DataKey => (Data.DataKey, base.DataKey);

        /// <param name="data">The value of <see cref="Data"/>.</param>
        /// <inheritdoc cref="FormatObject{T}.FormatObject(IFileFormat, T)"/>
        /// <param name="format"><inheritdoc cref="FormatObject{T}.FormatObject(IFileFormat, T)" path="/param[@name='format']"/></param>
        /// <param name="value"><inheritdoc cref="FormatObject{T}.FormatObject(IFileFormat, T)" path="/param[@name='value']"/></param>
        public BinaryFormatObject(IDataObject data, IBinaryFileFormat format, T value) : base(format, value)
        {
            Data = data;
        }
    }
}
