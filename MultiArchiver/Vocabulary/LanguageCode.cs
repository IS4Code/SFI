using System;
using System.Globalization;

namespace IS4.MultiArchiver.Vocabulary
{
    /// <summary>
    /// A language code used as a language tag of an RDF literal.
    /// </summary>
    public struct LanguageCode : IEquatable<LanguageCode>
    {
        /// <summary>
        /// The language code for the English language.
        /// </summary>
        public static readonly LanguageCode En = new LanguageCode("en");

        /// <summary>
        /// The string value of the language code.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Creates a new instance of the language code.
        /// </summary>
        /// <param name="value">The value of <see cref="Value"/>.</param>
        public LanguageCode(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Creates a new instance of the language code from a culture.
        /// </summary>
        /// <param name="culture">
        /// The culture representing the language, as <see cref="CultureInfo.IetfLanguageTag"/>.
        /// </param>
        public LanguageCode(CultureInfo culture)
        {
            Value = culture.IetfLanguageTag;
        }

        /// <summary>
        /// Returns <see cref="Value"/>.
        /// </summary>
        /// <returns><see cref="Value"/></returns>
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

        /// <summary>
        /// Compares two instances of <see cref="LanguageCode"/> for equality.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns>The result of <see cref="Equals(LanguageCode)"/>.</returns>
        public static bool operator ==(LanguageCode a, LanguageCode b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Compares two instances of <see cref="LanguageCode"/> for inequality.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns>The negated result of <see cref="Equals(LanguageCode)"/>.</returns>
        public static bool operator !=(LanguageCode a, LanguageCode b)
        {
            return !a.Equals(b);
        }
    }
}
