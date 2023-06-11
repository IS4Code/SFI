using IS4.SFI.Vocabulary;
using System;

#pragma warning disable 649

namespace IS4.SFI.Hashes
{
    internal static class Individuals
    {
        const string At = Vocabularies.Uri.At;

        /// <summary>
        /// <c><see cref="At"/>:btih</c>.
        /// </summary>
        [Uri(At, "btih")]
        public static readonly IndividualUri BTIH;

        /// <summary>
        /// <c><see cref="At"/>:bsha1-256</c>.
        /// </summary>
        [Uri(At, "bsha1-256")]
        public static readonly IndividualUri BSHA1_256;

        /// <summary>
        /// <c><see cref="At"/>:bsha1-512</c>.
        /// </summary>
        [Uri(At, "bsha1-512")]
        public static readonly IndividualUri BSHA1_512;

        /// <summary>
        /// <c><see cref="At"/>:bsha1-1024</c>.
        /// </summary>
        [Uri(At, "bsha1-1024")]
        public static readonly IndividualUri BSHA1_1024;

        static Individuals()
        {
            typeof(Individuals).InitializeUris();
        }
    }
}
