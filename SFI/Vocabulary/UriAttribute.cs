using System;

namespace IS4.SFI.Vocabulary
{
    /// <summary>
    /// Provides information for constructing an RDF term when initializing
    /// a field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class UriAttribute : Attribute
    {
        /// <summary>
        /// The instance of <see cref="VocabularyUri"/> identifying the vocabulary
        /// containing the term identified by the field.
        /// </summary>
        public VocabularyUri Vocabulary { get; }

        /// <summary>
        /// The proper local name of the term inside the vocabulary, if it is
        /// different from the name of the field.
        /// </summary>
        public string? LocalName { get; }

        /// <summary>
        /// Creates a new instance of the attribute.
        /// </summary>
        /// <param name="vocabulary">The vocabulary URI, stored in <see cref="Vocabulary"/>.</param>
        /// <param name="localName">The value of <see cref="LocalName"/>.</param>
        public UriAttribute(string vocabulary, string? localName = null)
        {
            Vocabulary = new VocabularyUri(vocabulary);
            LocalName = localName;
        }
    }
}
