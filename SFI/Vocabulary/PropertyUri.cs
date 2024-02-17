using System;

namespace IS4.SFI.Vocabulary
{
    /// <summary>
    /// Represents an RDF property term in a vocabulary.
    /// </summary>
    public struct PropertyUri : IDirectionalTermUri<PropertyUri>
    {
        const char invertChar = '^';

        readonly string vocabularyRaw;

        /// <summary>
        /// Whether the property is an inverse, i.e. it links object to subject in a triple.
        /// </summary>
        public bool IsInverse => !String.IsNullOrEmpty(vocabularyRaw) && vocabularyRaw[0] == invertChar;

        string VocabularyUri => IsInverse ? String.Intern(vocabularyRaw.Substring(1)) : vocabularyRaw;

        /// <inheritdoc/>
        public VocabularyUri Vocabulary => new VocabularyUri(VocabularyUri);

        /// <inheritdoc/>
        public string Term { get; }

        /// <inheritdoc/>
        public string Value => Vocabulary.Value + Term;

        /// <summary>
        /// Creates a new instance of the term.
        /// </summary>
        /// <param name="vocabulary">The value of <see cref="Vocabulary"/>.</param>
        /// <param name="term">The value of <see cref="Term"/>.</param>
        public PropertyUri(VocabularyUri vocabulary, string term)
        {
            vocabularyRaw = vocabulary.Value;
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
        public PropertyUri(UriAttribute uriAttribute, string fieldName)
            : this(uriAttribute.Vocabulary, uriAttribute.LocalName ?? fieldName.ToCamelCase())
        {

        }

        /// <summary>
        /// Inverts the direction of the property, i.e. the value of <see cref="IsInverse"/>.
        /// </summary>
        /// <returns>A new <see cref="PropertyUri"/> with the inverted direction.</returns>
        public PropertyUri AsInverse()
        {
            return IsInverse
                ? new(Vocabulary, Term)
                : new(new VocabularyUri(invertChar + vocabularyRaw), Term);
        }

        IDirectionalTermUri IDirectionalTermUri.AsInverse()
        {
            return AsInverse();
        }

        /// <summary>
        /// Returns <see cref="Value"/> formatted as a URI node.
        /// </summary>
        /// <returns>The formatted value of the instance.</returns>
        public override string ToString()
        {
            return $"{(IsInverse ? invertChar : null)}<{Value}>";
        }

        /// <inheritdoc/>
        public bool Equals(PropertyUri other)
        {
            return vocabularyRaw == other.vocabularyRaw && Term == other.Term;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is PropertyUri uri && Equals(uri);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(vocabularyRaw, Term);
        }

        /// <summary>
        /// Compares two instances of <see cref="ClassUri"/> for equality.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns>The result of <see cref="Equals(PropertyUri)"/>.</returns>
        public static bool operator ==(PropertyUri a, PropertyUri b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Compares two instances of <see cref="PropertyUri"/> for inequality.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns>The negated result of <see cref="Equals(PropertyUri)"/>.</returns>
        public static bool operator !=(PropertyUri a, PropertyUri b)
        {
            return !a.Equals(b);
        }
    }
}
