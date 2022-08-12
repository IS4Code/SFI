namespace IS4.MultiArchiver.Tags
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
        public bool StoreDimensions { get; set; } = true;
        public bool MakeThumbnail { get; set; } = true;
        public bool LowFrequencyHash { get; set; } = true;
        public bool HighFrequencyHash { get; set; } = true;
        public bool ByteHash { get; set; } = true;
    }
}
