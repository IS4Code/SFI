using System;

namespace IS4.MultiArchiver.Vocabulary
{
    public struct PropertyUri : ITermUri, IEquatable<PropertyUri>
    {
        public VocabularyUri Vocabulary { get; }
        public string Term { get; }

        public string Value => Vocabulary.Value + Term;

        public PropertyUri(VocabularyUri vocabulary, string term)
        {
            Vocabulary = vocabulary;
            Term = term;
        }

        public PropertyUri(UriAttribute uriAttribute, string fieldName)
            : this(uriAttribute.Vocabulary, uriAttribute.LocalName ?? fieldName.ToCamelCase())
        {

        }

        public override string ToString()
        {
            return $"<{Value}>";
        }

        public bool Equals(PropertyUri other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is PropertyUri uri && Equals(uri);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(PropertyUri a, PropertyUri b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(PropertyUri a, PropertyUri b)
        {
            return !a.Equals(b);
        }
    }
}
