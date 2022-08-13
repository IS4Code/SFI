using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using System;

namespace IS4.MultiArchiver.Vocabulary
{
    /// <summary>
    /// Stores the URI of an RDF vocabulary.
    /// </summary>
    public struct VocabularyUri : IEquatable<VocabularyUri>, IGenericUriFormatter<string>, IDatatypeUriFormatter<string>, IGraphUriFormatter<string>
    {
        /// <summary>
        /// The absolute URI of the vocabulary.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Produces a new URI from a term inside the vocabulary.
        /// </summary>
        /// <param name="term">The local name of the term.</param>
        /// <returns>
        /// A URI created by concatenating <see cref="Value"/> and
        /// <paramref name="term"/>.
        /// </returns>
        public Uri this[string term] => new EncodedUri(Value + term, UriKind.Absolute);

        /// <summary>
        /// Creates a new instance of the vocabulary.
        /// </summary>
        /// <param name="value">The value of <see cref="Value"/>.</param>
        public VocabularyUri(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Returns <see cref="Value"/> formatted as a URI node.
        /// </summary>
        /// <returns>The formatted value of the instance.</returns>
        public override string ToString()
        {
            return $"<{Value}>";
        }

        public bool Equals(VocabularyUri other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is VocabularyUri uri && Equals(uri);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// Compares two instances of <see cref="VocabularyUri"/> for equality.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns>The result of <see cref="Equals(VocabularyUri)"/>.</returns>
        public static bool operator ==(VocabularyUri a, VocabularyUri b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Compares two instances of <see cref="VocabularyUri"/> for inequality.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns>The negated result of <see cref="Equals(VocabularyUri)"/>.</returns>
        public static bool operator !=(VocabularyUri a, VocabularyUri b)
        {
            return !a.Equals(b);
        }
    }
}
