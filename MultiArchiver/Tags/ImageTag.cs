namespace IS4.MultiArchiver.Tags
{
    public interface IImageTag
    {
        bool StoreDimensions { get; }
        bool MakeThumbnail { get; }
        bool LowFrequencyHash { get; }
        bool HighFrequencyHash { get; }
        bool ByteHash { get; }
    }

    public class ImageTag : IImageTag
    {
        public bool StoreDimensions { get; set; } = true;
        public bool MakeThumbnail { get; set; } = true;
        public bool LowFrequencyHash { get; set; } = true;
        public bool HighFrequencyHash { get; set; } = true;
        public bool ByteHash { get; set; } = true;
    }
}
