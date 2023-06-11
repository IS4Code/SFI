using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using Murmur;
using System;
using System.ComponentModel;

namespace IS4.SFI.Hashes
{
    /// <summary>
    /// The 128-bit MurmurHash non-cryptographic hash algorithm, using <see cref="Murmur128"/>.
    /// </summary>
    [Description("The 128-bit MurmurHash non-cryptographic hash algorithm.")]
    public class Murmur128Hash : BuiltInHash<Murmur128>
    {
        /// <summary>
        /// <c><see cref="Vocabularies.Uri.At"/>:murmur128</c>.
        /// </summary>
        [Uri(Vocabularies.Uri.At)]
        public static readonly IndividualUri Murmur128;

        /// <inheritdoc cref="HashAlgorithm.HashAlgorithm(IndividualUri, int, string, FormattingMethod)"/>
        public Murmur128Hash() : base(() => MurmurHash.Create128(), Murmur128, "urn:murmur128:", 0x1022, null, FormattingMethod.Base64)
        {

        }

        static Murmur128Hash()
        {
            typeof(Murmur128Hash).InitializeUris();
        }
    }
}
