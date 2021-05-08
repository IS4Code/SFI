using SharpCompress.Archives;

namespace IS4.MultiArchiver.Formats
{
    public abstract class ArchiveFormat<TArchive> : SignatureFormat<TArchive> where TArchive : class, IArchive
    {
        public ArchiveFormat(int headerLength, string mediaType, string extension) : base(headerLength, mediaType, extension)
        {

        }

        public ArchiveFormat(string signature, string mediaType, string extension) : base(signature, mediaType, extension)
        {

        }

        public ArchiveFormat(byte[] signature, string mediaType, string extension) : base(signature, mediaType, extension)
        {

        }

        protected bool CheckArchive(TArchive archive)
        {
            return archive != null && archive.TotalSize >= 0 && archive.TotalUncompressSize >= 0;
        }
    }
}
