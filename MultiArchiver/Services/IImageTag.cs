namespace IS4.MultiArchiver.Services
{
    public interface IImageTag
    {
        bool StoreDimensions { get; }
        bool MakeThumbnail { get; }
        bool ComputeHash { get; }
    }

    public class ImageTag : IImageTag
    {
        public bool StoreDimensions { get; set; } = true;
        public bool MakeThumbnail { get; set; } = true;
        public bool ComputeHash { get; set; } = true;
    }
}
