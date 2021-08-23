using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using System;

namespace IS4.MultiArchiver.Vocabulary
{
    public struct VocabularyUri : IEquatable<VocabularyUri>, IClassUriFormatter<string>, IPropertyUriFormatter<string>, IIndividualUriFormatter<string>, IDatatypeUriFormatter<string>, IGraphUriFormatter<string>
    {
        public string Value { get; }

        public Uri this[string value] => new EncodedUri(Value + value, UriKind.Absolute);

        public VocabularyUri(string value)
        {
            Value = value;
        }

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

        public static bool operator ==(VocabularyUri a, VocabularyUri b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(VocabularyUri a, VocabularyUri b)
        {
            return !a.Equals(b);
        }
    }
}
