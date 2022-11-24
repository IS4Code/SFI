namespace IS4.SFI.Formats.Packages
{
    /// <summary>
    /// Stores the description of a package, as identified by metadata within.
    /// </summary>
    public class PackageDescription
    {
        /// <summary>
        /// The text of the description.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Creates a new instance of the description.
        /// </summary>
        /// <param name="value">The value of <see cref="Value"/>.</param>
        public PackageDescription(string value)
        {
            Value = value;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Value;
        }
    }
}
