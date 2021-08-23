using System;

namespace IS4.MultiArchiver.Vocabulary
{
    public struct ClassUri : ITermUri, IEquatable<ClassUri>
    {
        public VocabularyUri Vocabulary { get; }
        public string Term { get; }

        public string Value => Vocabulary.Value + Term;

        public ClassUri(VocabularyUri vocabulary, string term)
        {
            Vocabulary = vocabulary;
            Term = term;
        }

        public ClassUri(UriAttribute uriAttribute, string fieldName)
            : this(uriAttribute.Vocabulary, uriAttribute.LocalName ?? fieldName)
        {

        }

        public override string ToString()
        {
            return $"<{Value}>";
        }

        public bool Equals(ClassUri other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is ClassUri uri && Equals(uri);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(ClassUri a, ClassUri b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ClassUri a, ClassUri b)
        {
            return !a.Equals(b);
        }
    }
}
