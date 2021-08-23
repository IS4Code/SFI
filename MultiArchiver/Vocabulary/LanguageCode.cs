using System;
using System.Globalization;

namespace IS4.MultiArchiver.Vocabulary
{
    public struct LanguageCode : IEquatable<LanguageCode>
    {
        public static readonly LanguageCode En = new LanguageCode("en");

        public string Value { get; }

        public LanguageCode(string value)
        {
            Value = value;
        }

        public LanguageCode(CultureInfo culture)
        {
            Value = culture.IetfLanguageTag;
        }

        public override string ToString()
        {
            return Value;
        }

        public bool Equals(LanguageCode other)
        {
            return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return obj is LanguageCode code && Equals(code);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
        }

        public static bool operator ==(LanguageCode a, LanguageCode b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(LanguageCode a, LanguageCode b)
        {
            return !a.Equals(b);
        }
    }
}
