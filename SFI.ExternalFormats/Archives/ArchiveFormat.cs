using SharpCompress.Archives;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents an archive format producing instances of <see cref="IArchive"/>.
    /// </summary>
    /// <typeparam name="TArchive">The supported archive type.</typeparam>
    public abstract class ArchiveFormat<TArchive> : SignatureFormat<IArchiveFile> where TArchive : class, IArchive
    {
        /// <inheritdoc/>
        public ArchiveFormat(int headerLength, string mediaType, string extension) : base(headerLength, mediaType, extension)
        {

        }

        /// <inheritdoc/>
        public ArchiveFormat(string signature, string mediaType, string extension) : base(signature, mediaType, extension)
        {

        }

        /// <inheritdoc/>
        public ArchiveFormat(byte[] signature, string mediaType, string extension) : base(signature, mediaType, extension)
        {

        }

        /// <summary>
        /// Checks that the provided <typeparamref name="TArchive"/> instance was correctly recognized.
        /// </summary>
        /// <param name="archive">The checked archive.</param>
        /// <returns>True if the archive is valid.</returns>
        protected bool CheckArchive(TArchive archive)
        {
            return archive != null && archive.TotalSize >= 0 && archive.TotalUncompressSize >= 0;
        }
    }
}
