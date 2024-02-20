using IS4.SFI.Vocabulary;
using System;

using static IS4.SFI.Vocabulary.Vocabularies.Uri;

namespace IS4.SFI.Vocabulary
{
    /// <summary>
    /// Instances of <see cref="IndividualUri"/> related to non-cryptographic hash algorithms.
    /// </summary>
    public static class NonCryptographicHashIndividuals
    {
        /// <summary>
        /// <c><see cref="At"/>:crc32</c>.
        /// </summary>
        [Uri(At)]
        public static readonly IndividualUri Crc32;

        /// <summary>
        /// <c><see cref="At"/>:crc64</c>.
        /// </summary>
        [Uri(At)]
        public static readonly IndividualUri Crc64;

        /// <summary>
        /// <c><see cref="At"/>:xxHash32</c>.
        /// </summary>
        [Uri(At)]
        public static readonly IndividualUri XxHash32;

        /// <summary>
        /// <c><see cref="At"/>:xxHash64</c>.
        /// </summary>
        [Uri(At)]
        public static readonly IndividualUri XxHash64;

        /// <summary>
        /// <c><see cref="At"/>:xxHash3</c>.
        /// </summary>
        [Uri(At)]
        public static readonly IndividualUri XxHash3;

        /// <summary>
        /// <c><see cref="At"/>:xxHash64</c>.
        /// </summary>
        [Uri(At)]
        public static readonly IndividualUri XxHash128;

        static NonCryptographicHashIndividuals()
        {
            typeof(NonCryptographicHashIndividuals).InitializeUris();
        }
    }
}
