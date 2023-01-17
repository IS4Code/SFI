using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using Murmur;
using System;

namespace IS4.SFI.SamplePlugin
{
    /// <summary>
    /// The 32-bit MurmurHash non-cryptographic hash algorithm, using <see cref="Murmur32"/>.
    /// </summary>
    public class Murmur32Hash : BuiltInHash<Murmur32>
    {
        /// <summary>
        /// <see cref="Vocabularies.Uri.At"/>:murmur32.
        /// </summary>
        [Uri(Vocabularies.Uri.At)]
        public static readonly IndividualUri Murmur32;

        /// <summary>
        /// The singleton instance of the algorithm.
        /// </summary>
        public static readonly Murmur32Hash Instance = new Murmur32Hash();

        private Murmur32Hash() : base(() => MurmurHash.Create32(), Murmur32, "murmur32", 0x23, null, Services.FormattingMethod.Base64)
        {

        }

        static Murmur32Hash()
        {
            typeof(Murmur32Hash).InitializeUris();
        }
    }
}
