using IS4.SFI.Vocabulary;
using System.ComponentModel;

namespace IS4.SFI.Hashes
{
    /// <summary>
    /// A hash algorithm derived from <see cref="PaddedBlockHash"/>
    /// that uses 256-KiB blocks.
    /// </summary>
    [Description("Hashes 256-KiB chunks using SHA-1.")]
    public class Padded256BlockSHA1Hash : PaddedBlockHash
    {
        public Padded256BlockSHA1Hash() : base(BitTorrentIndividuals.BSHA1_256, "urn:bsha1-256:", 262144)
        {

        }
    }
}
