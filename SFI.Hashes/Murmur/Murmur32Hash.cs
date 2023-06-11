using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using Murmur;
using System;
using System.ComponentModel;

namespace IS4.SFI.Hashes
{
    /// <summary>
    /// The 32-bit MurmurHash non-cryptographic hash algorithm, using <see cref="Murmur32"/>.
    /// </summary>
    [Description("The 32-bit MurmurHash non-cryptographic hash algorithm.")]
    public class Murmur32Hash : BuiltInHash<Murmur32>
    {
        /// <summary>
        /// <c><see cref="Vocabularies.Uri.At"/>:murmur32</c>.
        /// </summary>
        [Uri(Vocabularies.Uri.At)]
        public static readonly IndividualUri Murmur32;

        /// <inheritdoc cref="HashAlgorithm.HashAlgorithm(IndividualUri, int, string, FormattingMethod)"/>
        public Murmur32Hash() : base(() => MurmurHash.Create32(), Murmur32, "urn:murmur32:", 0x23, null, FormattingMethod.Base64)
        {

        }

        static Murmur32Hash()
        {
            typeof(Murmur32Hash).InitializeUris();
        }
    }
}
