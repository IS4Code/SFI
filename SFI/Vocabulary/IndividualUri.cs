using System;

namespace IS4.SFI.Vocabulary
{
    /// <summary>
    /// Represents an RDF individual term in a vocabulary.
    /// </summary>
    public struct IndividualUri : ITermUri, IEquatable<IndividualUri>
    {
        /// <inheritdoc/>
        public VocabularyUri Vocabulary { get; }

        /// <inheritdoc/>
        public string Term { get; }

        /// <inheritdoc/>
        public string Value => Vocabulary.Value + Term;

        /// <summary>
        /// Creates a new instance of the term.
        /// </summary>
        /// <param name="vocabulary">The value of <see cref="Vocabulary"/>.</param>
        /// <param name="term">The value of <see cref="Term"/>.</param>
        public IndividualUri(VocabularyUri vocabulary, string term)
        {
            Vocabulary = vocabulary;
            Term = term;
        }

        /// <summary>
        /// Creates a new instance of the term from a field.
        /// </summary>
        /// <param name="uriAttribute">The attribute identifying the vocabulary and local name of the term.</param>
        /// <param name="fieldName">
        /// The name of the field, used as a fallback for <see cref="Term"/>,
        /// converted via <see cref="Extensions.ToCamelCase(string)"/>.
        /// </param>
        public IndividualUri(UriAttribute uriAttribute, string fieldName)
            : this(uriAttribute.Vocabulary, uriAttribute.LocalName ?? fieldName.ToCamelCase())
        {

        }

        /// <summary>
        /// Returns <see cref="Value"/> formatted as a URI node.
        /// </summary>
        /// <returns>The formatted value of the instance.</returns>
        public override string ToString()
        {
            return $"<{Value}>";
        }

        /// <inheritdoc/>
        public bool Equals(IndividualUri other)
        {
            return Vocabulary == other.Vocabulary && Term == other.Term;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is IndividualUri uri && Equals(uri);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Vocabulary.Value, Term);
        }

        /// <summary>
        /// Compares two instances of <see cref="IndividualUri"/> for equality.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns>The result of <see cref="Equals(IndividualUri)"/>.</returns>
        public static bool operator ==(IndividualUri a, IndividualUri b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Compares two instances of <see cref="IndividualUri"/> for inequality.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns>The negated result of <see cref="Equals(IndividualUri)"/>.</returns>
        public static bool operator !=(IndividualUri a, IndividualUri b)
        {
            return !a.Equals(b);
        }
    }
}
