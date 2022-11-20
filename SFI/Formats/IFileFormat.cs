using System;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents a general file format or a collection of related file formats.
    /// </summary>
    public interface IFileFormat
    {
        /// <summary>
        /// Returns the media type of an object describing an instance of this formats.
        /// </summary>
        /// <param name="value">An object compatible with this format.</param>
        /// <returns>A MIME type based on <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the argument is not compatible with the format.
        /// </exception>
        string? GetMediaType(object value);

        /// <summary>
        /// Returns the common extension of an object describing an instance of this formats.
        /// </summary>
        /// <param name="value">An object compatible with this format.</param>
        /// <returns>An extension based on <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the argument is not compatible with the format.
        /// </exception>
        string? GetExtension(object value);
    }

    /// <summary>
    /// Represents a general file format whose media objects can be described
    /// using instances of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the instances produced as a result
    /// of parsing the format.
    /// </typeparam>
    public interface IFileFormat<in T> : IFileFormat where T : class
    {
        /// <summary>
        /// Returns the media type of an object describing an instance of this formats.
        /// </summary>
        /// <param name="value">An object compatible with this format.</param>
        /// <returns>A MIME type based on <paramref name="value"/>.</returns>
        string? GetMediaType(T value);

        /// <summary>
        /// Returns the common extension of an object describing an instance of this formats.
        /// </summary>
        /// <param name="value">An object compatible with this format.</param>
        /// <returns>A MIME type based on <paramref name="value"/>.</returns>
        string? GetExtension(T value);
    }

    /// <summary>
    /// Provides a base implementation of <see cref="IFileFormat{T}"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the instances produced as a result
    /// of parsing the format.
    /// </typeparam>
    public abstract class FileFormat<T> : IFileFormat<T> where T : class
    {
        /// <summary>
        /// The common media type, used if there is no other implementation
        /// of <see cref="GetMediaType(T)"/>.
        /// </summary>
        public string? MediaType { get; }

        /// <summary>
        /// The common extension, used if there is no other implementation
        /// of <see cref="GetExtension(T)"/>.
        /// </summary>
        public string? Extension { get; }

        /// <summary>
        /// Creates a new instance of the format.
        /// </summary>
        /// <param name="mediaType">The value of <see cref="MediaType"/>.</param>
        /// <param name="extension">The value of <see cref="Extension"/>.</param>
        public FileFormat(string? mediaType, string? extension)
        {
            MediaType = mediaType;
            Extension = extension;
        }

        /// <inheritdoc/>
        public virtual string? GetExtension(T value)
        {
            return Extension;
        }

        /// <inheritdoc/>
        public virtual string? GetMediaType(T value)
        {
            return MediaType;
        }

        string? IFileFormat.GetExtension(object value)
        {
            if(value is not T obj) throw new ArgumentException(null, nameof(value));
            return GetExtension(obj);
        }

        string? IFileFormat.GetMediaType(object value)
        {
            if(value is not T obj) throw new ArgumentException(null, nameof(value));
            return GetMediaType(obj);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return MediaType ?? TextTools.GetFakeMediaTypeFromType<T>() ?? base.ToString();
        }
    }
}
