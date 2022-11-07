namespace IS4.SFI.Tags
{
    /// <summary>
    /// A tag storing configuration for analyzing images.
    /// </summary>
    public interface IImageTag
    {
        /// <summary>
        /// Whether to add the image's dimensions to the output.
        /// </summary>
        bool StoreDimensions { get; }

        /// <summary>
        /// Whether to add the thumbnail to the output.
        /// </summary>
        bool MakeThumbnail { get; }

        /// <summary>
        /// Whether to compute low-frequency hashes from the image.
        /// </summary>
        bool LowFrequencyHash { get; }

        /// <summary>
        /// Whether to compute high-frequency hashes from the image.
        /// </summary>
        bool HighFrequencyHash { get; }

        /// <summary>
        /// Whether to compute byte-based hashes from the image.
        /// </summary>
        bool ByteHash { get; }
    }

    /// <summary>
    /// A default implementation of <see cref="IImageTag"/> with
    /// all options enabled.
    /// </summary>
    public class ImageTag : IImageTag
    {
        /// <inheritdoc/>
        public bool StoreDimensions { get; set; } = true;

        /// <inheritdoc/>
        public bool MakeThumbnail { get; set; } = true;

        /// <inheritdoc/>
        public bool LowFrequencyHash { get; set; } = true;

        /// <inheritdoc/>
        public bool HighFrequencyHash { get; set; } = true;

        /// <inheritdoc/>
        public bool ByteHash { get; set; } = true;
    }
}
