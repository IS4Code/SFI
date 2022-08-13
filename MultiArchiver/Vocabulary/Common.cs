using System.Collections.Generic;

namespace IS4.MultiArchiver.Vocabulary
{
    /// <summary>
    /// Commonly used collections of RDF terms.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Classes denoting audio objects.
        /// </summary>
        public static IReadOnlyCollection<ClassUri> AudioClasses = new[]
        {
            Classes.AudioObject, Classes.Audio
        };

        /// <summary>
        /// Classes denoting video objects.
        /// </summary>
        public static IReadOnlyCollection<ClassUri> VideoClasses = new[]
        {
            Classes.VideoObject, Classes.Video
        };

        /// <summary>
        /// Classes denoting image objects.
        /// </summary>
        public static IReadOnlyCollection<ClassUri> ImageClasses = new[]
        {
            Classes.ImageObject, Classes.Image
        };

        /// <summary>
        /// Classes denoting application or executable objects.
        /// </summary>
        public static IReadOnlyCollection<ClassUri> ApplicationClasses = new[]
        {
            Classes.SoftwareApplication, Classes.Executable
        };

        /// <summary>
        /// Classes denoting archives.
        /// </summary>
        public static IReadOnlyCollection<ClassUri> ArchiveClasses = new[]
        {
            Classes.Archive
        };
    }
}
