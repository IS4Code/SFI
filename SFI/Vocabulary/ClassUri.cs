using System;

namespace IS4.SFI.Vocabulary
{
    /// <summary>
    /// Represents an RDF class term in a vocabulary.
    /// </summary>
    public struct ClassUri : ITermUri, IEquatable<ClassUri>
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
        public ClassUri(VocabularyUri vocabulary, string term)
        {
            Vocabulary = vocabulary;
            Term = term;
        }

        /// <summary>
        /// Creates a new instance of the term from a field.
        /// </summary>
        /// <param name="uriAttribute">The attribute identifying the vocabulary and local name of the term.</param>
        /// <param name="fieldName">
        /// The name of the field, used as a fallback for <see cref="Term"/>.
        /// </param>
        public ClassUri(UriAttribute uriAttribute, string fieldName)
            : this(uriAttribute.Vocabulary, uriAttribute.LocalName ?? fieldName)
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
        public bool Equals(ClassUri other)
        {
            return Value == other.Value;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is ClassUri uri && Equals(uri);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// Compares two instances of <see cref="ClassUri"/> for equality.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns>The result of <see cref="Equals(ClassUri)"/>.</returns>
        public static bool operator ==(ClassUri a, ClassUri b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Compares two instances of <see cref="ClassUri"/> for inequality.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns>The negated result of <see cref="Equals(ClassUri)"/>.</returns>
        public static bool operator !=(ClassUri a, ClassUri b)
        {
            return !a.Equals(b);
        }
    }
}
