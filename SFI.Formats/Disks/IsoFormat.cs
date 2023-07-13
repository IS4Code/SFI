using DiscUtils.Iso9660;
using System.ComponentModel;
using System.IO;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the ISO CD image format, as an instance of <see cref="CDReader"/>.
    /// </summary>
    [Description("Represents the ISO CD image format.")]
    public class IsoFormat : DiscFormat<CDReader>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public IsoFormat() : base("application/x-iso9660-image", "iso")
        {

        }

        /// <inheritdoc/>
        protected override CDReader Create(Stream stream)
        {
            return new CDReader(stream, true);
        }
    }
}
