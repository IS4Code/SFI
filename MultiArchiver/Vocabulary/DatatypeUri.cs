using System;

namespace IS4.MultiArchiver.Vocabulary
{
    public struct DatatypeUri : ITermUri, IEquatable<DatatypeUri>
    {
        public VocabularyUri Vocabulary { get; }
        public string Term { get; }

        public string Value => Vocabulary.Value + Term;

        public DatatypeUri(VocabularyUri vocabulary, string term)
        {
            Vocabulary = vocabulary;
            Term = term;
        }

        public DatatypeUri(UriAttribute uriAttribute, string fieldName)
            : this(uriAttribute.Vocabulary, uriAttribute.LocalName ?? fieldName.ToCamelCase())
        {

        }

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

        public static bool operator ==(DatatypeUri a, DatatypeUri b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(DatatypeUri a, DatatypeUri b)
        {
            return !a.Equals(b);
        }
    }
}
