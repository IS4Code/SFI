using System;

namespace IS4.MultiArchiver.Vocabulary
{
    public struct VocabularyUri : IEquatable<VocabularyUri>
    {
        public string Value { get; }

        public VocabularyUri(string value)
        {
            Value = value;
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

        public static bool operator ==(VocabularyUri a, VocabularyUri b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(VocabularyUri a, VocabularyUri b)
        {
            return !a.Equals(b);
        }

        public static implicit operator VocabularyUri(string value)
        {
            return new VocabularyUri(value);
        }
    }
}
