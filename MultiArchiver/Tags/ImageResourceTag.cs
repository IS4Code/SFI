namespace IS4.MultiArchiver.Tags
{
    /// <summary>
    /// A tag providing information about the resource a particular image is coming from.
    /// </summary>
    public interface IImageResourceTag
    {
        /// <summary>
        /// True if the image should be transparent, false otherwise.
        /// </summary>
        bool IsTransparent { get; }
    }
}
