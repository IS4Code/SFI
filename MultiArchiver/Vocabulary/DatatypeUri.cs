﻿using System;

namespace IS4.MultiArchiver.Vocabulary
{
    /// <summary>
    /// Represents an RDF datatype term in a vocabulary.
    /// </summary>
    public struct DatatypeUri : ITermUri, IEquatable<DatatypeUri>
    {
        public VocabularyUri Vocabulary { get; }
        public string Term { get; }

        public string Value => Vocabulary.Value + Term;

        /// <summary>
        /// Creates a new instance of the term.
        /// </summary>
        /// <param name="vocabulary">The value of <see cref="Vocabulary"/>.</param>
        /// <param name="term">The value of <see cref="Term"/>.</param>
        public DatatypeUri(VocabularyUri vocabulary, string term)
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
        public DatatypeUri(UriAttribute uriAttribute, string fieldName)
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

        public bool Equals(DatatypeUri other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is DatatypeUri uri && Equals(uri);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// Compares two instances of <see cref="DatatypeUri"/> for equality.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns>The result of <see cref="Equals(DatatypeUri)"/>.</returns>
        public static bool operator ==(DatatypeUri a, DatatypeUri b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Compares two instances of <see cref="DatatypeUri"/> for inequality.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns>The negated result of <see cref="Equals(DatatypeUri)"/>.</returns>
        public static bool operator !=(DatatypeUri a, DatatypeUri b)
        {
            return !a.Equals(b);
        }
    }
}
