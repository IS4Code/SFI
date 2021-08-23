using System;

namespace IS4.MultiArchiver.Vocabulary
{
    public struct IndividualUri : ITermUri, IEquatable<IndividualUri>
    {
        public VocabularyUri Vocabulary { get; }
        public string Term { get; }

        public string Value => Vocabulary.Value + Term;

        public IndividualUri(VocabularyUri vocabulary, string term)
        {
            Vocabulary = vocabulary;
            Term = term;
        }

        public IndividualUri(UriAttribute uriAttribute, string fieldName)
            : this(uriAttribute.Vocabulary, uriAttribute.LocalName ?? fieldName.ToCamelCase())
        {

        }

        public override string ToString()
        {
            return $"<{Value}>";
        }

        public bool Equals(IndividualUri other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is IndividualUri uri && Equals(uri);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(IndividualUri a, IndividualUri b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(IndividualUri a, IndividualUri b)
        {
            return !a.Equals(b);
        }
    }
}
