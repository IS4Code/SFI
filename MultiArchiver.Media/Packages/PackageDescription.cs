namespace IS4.MultiArchiver.Media.Packages
{
    public class PackageDescription
    {
        public string Value { get; }

        public PackageDescription(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
