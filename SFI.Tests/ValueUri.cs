using System;
using System.Globalization;

namespace IS4.SFI.Tests
{
    /// <summary>
    /// Wraps a value converted to a URI.
    /// </summary>
    public class ValueUri : EquatableUri, IEquatable<ValueUri>
    {
        /// <summary>
        /// The inner value.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Creates a new URI instance from a value.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        public ValueUri(object value) : base(Format(value))
        {
            Value = value;
        }

        bool IEquatable<ValueUri>.Equals(ValueUri? other)
        {
            return Equals(other);
        }

        static string Format(object value)
        {
            if(value is IFormattable formattable)
            {
                return $"x.value:{formattable.ToString(null, CultureInfo.InvariantCulture)}";
            }
            return $"x.value:{value}";
        }
    }
}
