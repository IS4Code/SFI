namespace IS4.SFI.Tags
{
    /// <summary>
    /// A tag providing information about the resource a particular image is coming from.
    /// </summary>
    public interface IImageResourceTag
    {
        /// <summary>
        /// <see langword="true"/> if the image should be transparent, <see langword="false"/> otherwise.
        /// </summary>
        bool IsTransparent { get; }
    }
}
