using IS4.SFI.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents an instance of <see cref="IFileFormat"/> for a format
    /// based on binary data.
    /// </summary>
    public interface IBinaryFileFormat : IFileFormat
    {
        /// <summary>
        /// The minimum length of the header that should be read from a file
        /// before calling <see cref="CheckHeader(Span{byte}, bool, IEncodingDetector)"/>.
        /// </summary>
        int HeaderLength { get; }

        /// <summary>
        /// Determines whether a file beginning with <paramref name="header"/> could
        /// be in a format represented by this instance.
        /// </summary>
        /// <param name="header">A collection of bytes from the beginning of the file.</param>
        /// <param name="isBinary">Whether the file was detected as binary or not.</param>
        /// <param name="encodingDetector">The specific instance of <see cref="IEncodingDetector"/> used to determine the encoding.</param>
        /// <returns>False if the file cannot possibly be in this format, true otherwise.</returns>
        bool CheckHeader(ArraySegment<byte> header, bool isBinary, IEncodingDetector? encodingDetector);

        /// <summary>
        /// Determines whether a file beginning with <paramref name="header"/> could
        /// be in a format represented by this instance.
        /// </summary>
        /// <param name="header">A collection of bytes from the beginning of the file.</param>
        /// <param name="isBinary">Whether the file was detected as binary or not.</param>
        /// <param name="encodingDetector">The specific instance of <see cref="IEncodingDetector"/> used to determine the encoding.</param>
        /// <returns>False if the file cannot possibly be in this format, true otherwise.</returns>
        bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector? encodingDetector);

        /// <summary>
        /// Attempts to match this format from a file, producing an object that describes
        /// the media object stored in the file. The object is obtained
        /// using the provided <see cref="IResultFactory{TResult, TArgs}"/>.
        /// </summary>
        /// <typeparam name="TResult">User-specified result type passed to <paramref name="resultFactory"/>.</typeparam>
        /// <typeparam name="TArgs">User-specified arguments type passed to <paramref name="resultFactory"/>.</typeparam>
        /// <param name="stream">The stream to analyze.</param>
        /// <param name="context">Additional information relevant to the match.</param>
        /// <param name="resultFactory">A receiver object that is provided the result of the match, if any.</param>
        /// <param name="args">User-specified arguments passed to <paramref name="resultFactory"/>.</param>
        /// <returns>
        /// The result of <see cref="IResultFactory{TResult, TArgs}.Invoke{T}(T, TArgs)"/> when given the produced object,
        /// or the default value of <typeparamref name="TResult"/> when the match isn't successful.
        /// </returns>
        /// <exception cref="Exception">
        /// Any exception may be caused during the internal parsing of the format.
        /// </exception>
        ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, IResultFactory<TResult, TArgs> resultFactory, TArgs args);
    }

    /// <summary>
    /// Represents an instance of <see cref="IFileFormat{T}"/> for a format
    /// based on binary data, producing instances of <typeparamref name="T"/>
    /// to describe the media object.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the instances produced as a result
    /// of parsing the format.
    /// </typeparam>
    public interface IBinaryFileFormat<T> : IFileFormat<T>, IBinaryFileFormat where T : class
    {
        /// <summary>
        /// Attempts to match this format from a file, producing an object that describes
        /// the media object stored in the file. The object is obtained
        /// using the provided <see cref="ResultFactory{T, TResult, TArgs}"/>.
        /// </summary>
        /// <typeparam name="TResult">User-specified result type passed to <paramref name="resultFactory"/>.</typeparam>
        /// <typeparam name="TArgs">User-specified arguments type passed to <paramref name="resultFactory"/>.</typeparam>
        /// <param name="stream">The stream to analyze.</param>
        /// <param name="context">Additional information relevant to the match.</param>
        /// <param name="resultFactory">A receiver object that is provided the result of the match, if any.</param>
        /// <param name="args">User-specified arguments passed to <paramref name="resultFactory"/>.</param>
        /// <returns>
        /// The result of <see cref="ResultFactory{T, TResult, TArgs}.Invoke(T, TArgs)"/> when given the produced object,
        /// or the default value of <typeparamref name="TResult"/> when the match isn't successful.
        /// </returns>
        /// <exception cref="Exception">
        /// Any exception may be caused during the internal parsing of the format.
        /// </exception>
        ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<T, TResult, TArgs> resultFactory, TArgs args);
    }

    /// <summary>
    /// Provides a base implementation of <see cref="IBinaryFileFormat{T}"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the instances produced as a result
    /// of parsing the format.
    /// </typeparam>
    public abstract class BinaryFileFormat<T> : FileFormat<T>, IBinaryFileFormat<T> where T : class
    {
        /// <inheritdoc/>
        public int HeaderLength { get; }

        /// <param name="headerLength">The value of <see cref="HeaderLength"/>.</param>
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        /// <param name="extension"><inheritdoc cref="FileFormat{T}.FileFormat(string, string)" path="/param[@name='extension']"/></param>
        /// <param name="mediaType"><inheritdoc cref="FileFormat{T}.FileFormat(string, string)" path="/param[@name='mediaType']"/></param>
        public BinaryFileFormat(int headerLength, string? mediaType, string? extension) : base(mediaType, extension)
        {
            HeaderLength = headerLength;
        }

        /// <inheritdoc/>
        public virtual bool CheckHeader(ArraySegment<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
        {
            return CheckHeader(header.AsSpan(), isBinary, encodingDetector);
        }

        /// <inheritdoc/>
        public abstract bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector? encodingDetector);

        /// <inheritdoc/>
        public abstract ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<T, TResult, TArgs> resultFactory, TArgs args);

        /// <inheritdoc/>
        public ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, IResultFactory<TResult, TArgs> resultFactory, TArgs args)
        {
            return Match(stream, context, resultFactory.Invoke, args);
        }
    }
}
