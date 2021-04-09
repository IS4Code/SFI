using System;

namespace IS4.MultiArchiver.Services
{
    public interface IUriFormatter<in T>
    {
        Uri FormatUri(T value);
    }

    public class UriFormatter : IUriFormatter<string>
    {
        public static readonly IUriFormatter<string> Instance = new UriFormatter();

        private UriFormatter()
        {

        }

        public Uri FormatUri(string value)
        {
            return new Uri(value, UriKind.Absolute);
        }
    }

    public class IdentityUriFormatter : IUriFormatter<Uri>
    {
        public static readonly IUriFormatter<Uri> Instance = new IdentityUriFormatter();

        private IdentityUriFormatter()
        {

        }

        public Uri FormatUri(Uri value)
        {
            return value;
        }
    }
}
