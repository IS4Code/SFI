using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

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

        string VocabularyUri => IsInverse ? GetInverseVocabulary(vocabularyRaw) : vocabularyRaw;

        /// <inheritdoc/>
        public VocabularyUri Vocabulary => new VocabularyUri(VocabularyUri);

        /// <inheritdoc/>
        public string Term { get; }

        /// <inheritdoc/>
        public string Value => Vocabulary.Value + Term;

        /// <inheritdoc cref="PropertyUri(SFI.Vocabulary.VocabularyUri, string, bool)"/>
        public PropertyUri(VocabularyUri vocabulary, string term) : this(vocabulary, term, false)
        {

        }

        /// <summary>
        /// Creates a new instance of the term.
        /// </summary>
        /// <param name="vocabulary">The value of <see cref="Vocabulary"/>.</param>
        /// <param name="term">The value of <see cref="Term"/>.</param>
        /// <param name="isInverse">The value of <see cref="IsInverse"/>.</param>
        public PropertyUri(VocabularyUri vocabulary, string term, bool isInverse)
        {
            if(String.IsNullOrEmpty(vocabulary.Value) || vocabulary.Value[0] == invertChar)
            {
                throw new ArgumentException("The vocabulary URI has invalid format.", nameof(vocabulary));
            }
            vocabularyRaw = isInverse ? GetInverseVocabulary(vocabulary.Value) : vocabulary.Value;
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
            return new(Vocabulary, Term, !IsInverse);
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

        static readonly ConditionalWeakTable<string, string> inverseMap = new();
        static readonly ConditionalWeakTable<string, string>.CreateValueCallback valueFactory = s => {
            var result = CreateInverseVocabulary(s);
            try{
                inverseMap.Add(result, s);
            }catch(ArgumentException)
            {
                // Should not happen since result is a new unique string
            }
            return result;
        };

        static readonly ConcurrentDictionary<string, string> internedInverseMap = new(ReferenceEqualityComparer<string>.Default);
        static readonly Func<string, string> internedValueFactory = s => {
            var result = String.Intern(CreateInverseVocabulary(s));
            internedInverseMap[result] = s;
            return result;
        };

        static string GetInverseVocabulary(string str)
        {
            if(String.IsInterned(str) is string interned)
            {
                return internedInverseMap.GetOrAdd(interned, internedValueFactory);
            }else{
                return inverseMap.GetValue(str, valueFactory);
            }
        }

        static string CreateInverseVocabulary(string str)
        {
            if(str[0] == invertChar)
            {
                return str.Substring(1);
            }else{
                return invertChar + str;
            }
        }
    }
}
