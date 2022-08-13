﻿using System;

namespace IS4.MultiArchiver.Vocabulary
{
    using static Vocabularies.Uri;

    /// <summary>
    /// Contains common RDF graphs, with vocabulary prefixes
    /// taken from <see cref="Vocabularies.Uri"/>.
    /// </summary>
    public static class Graphs
    {
        /// <summary>
        /// <see cref="At"/>:autoGenerated. This graph stores
        /// data generated by the corresponding analyzers based on the
        /// information gathered from input.
        /// </summary>
        [Uri(At)]
        public static readonly GraphUri AutoGenerated;

        /// <summary>
        /// <see cref="At"/>:humanLabelled. This graph stores
        /// data that has been entered directly by the user.
        /// </summary>
        [Uri(At)]
        public static readonly GraphUri HumanLabelled;

        /// <summary>
        /// <see cref="At"/>:autoGuessed. This graph stores
        /// data that was guessed or auto-recognized
        /// using some heuristics.
        /// </summary>
        [Uri(At)]
        public static readonly GraphUri AutoGuessed;

        /// <summary>
        /// <see cref="At"/>:metadata. This graph stores
        /// information about the analysis process itself.
        /// </summary>
        [Uri(At)]
        public static readonly GraphUri Metadata;

        /// <summary>
        /// <see cref="At"/>:shortenedLinks. This graph stores
        /// links between any URIs that had to be shortened and their
        /// original form, using <see cref="Properties.SameAs"/>.
        /// </summary>
        [Uri(At)]
        public static readonly GraphUri ShortenedLinks;

        static Graphs()
        {
            typeof(Graphs).InitializeUris();
        }
    }
}
