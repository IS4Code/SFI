using System.Collections.Generic;

namespace IS4.MultiArchiver.Vocabulary
{
    public static class Common
    {
        public static IReadOnlyCollection<ClassUri> AudioClasses = new[]
        {
            Classes.AudioObject, Classes.Audio
        };

        public static IReadOnlyCollection<ClassUri> VideoClasses = new[]
        {
            Classes.VideoObject, Classes.Video
        };

        public static IReadOnlyCollection<ClassUri> ImageClasses = new[]
        {
            Classes.ImageObject, Classes.Image
        };

        public static IReadOnlyCollection<ClassUri> ApplicationClasses = new[]
        {
            Classes.SoftwareApplication, Classes.Executable
        };

        public static IReadOnlyCollection<ClassUri> ArchiveClasses = new[]
        {
            Classes.Archive
        };
    }
}
