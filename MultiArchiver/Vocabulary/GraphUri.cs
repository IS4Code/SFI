using System;

namespace IS4.MultiArchiver.Vocabulary
{
    public struct GraphUri : ITermUri, IEquatable<GraphUri>
    {
        public VocabularyUri Vocabulary { get; }
        public string Term { get; }

        public string Value => Vocabulary.Value + Term;

        public GraphUri(VocabularyUri vocabulary, string term)
        {
            Vocabulary = vocabulary;
            Term = term;
        }

        public GraphUri(UriAttribute uriAttribute, string fieldName)
            : this(uriAttribute.Vocabulary, uriAttribute.LocalName ?? fieldName.ToCamelCase())
        {

        }

        public bool Equals(GraphUri other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is GraphUri uri && Equals(uri);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(GraphUri a, GraphUri b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(GraphUri a, GraphUri b)
        {
            return !a.Equals(b);
        }
    }
}
