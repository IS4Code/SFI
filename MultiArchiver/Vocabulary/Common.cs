using System.Collections.Generic;

namespace IS4.MultiArchiver.Vocabulary
{
    public static class Common
    {
        public static IEnumerable<Classes> AudioClasses = new[]
        {
            Classes.AudioObject, Classes.Audio
        };

        public static IEnumerable<Classes> VideoClasses = new[]
        {
            Classes.VideoObject, Classes.Video
        };

        public static IEnumerable<Classes> ImageClasses = new[]
        {
            Classes.ImageObject, Classes.Image
        };

        public static IEnumerable<Classes> ApplicationClasses = new[]
        {
            Classes.SoftwareApplication, Classes.Executable
        };
    }
}
