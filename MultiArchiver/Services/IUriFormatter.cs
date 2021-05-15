using System;

namespace IS4.MultiArchiver.Services
{
    public interface IUriFormatter<in T>
    {
        Uri FormatUri(T value);
    }

    public interface IIndividualUriFormatter<in T> : IUriFormatter<T>
    {

    }

    public interface IPropertyUriFormatter<in T> : IUriFormatter<T>
    {

    }

    public interface IClassUriFormatter<in T> : IUriFormatter<T>
    {

    }

    public interface IDatatypeUriFormatter<in T> : IUriFormatter<T>
    {

    }

    public class UriFormatter : IIndividualUriFormatter<string>, IPropertyUriFormatter<string>, IClassUriFormatter<string>, IDatatypeUriFormatter<string>
    {
        public static readonly UriFormatter Instance = new UriFormatter();

        private UriFormatter()
        {

        }

        public Uri FormatUri(string value)
        {
            return new Uri(value, UriKind.Absolute);
        }
    }

    public class IdentityUriFormatter : IIndividualUriFormatter<Uri>, IPropertyUriFormatter<Uri>, IClassUriFormatter<Uri>, IDatatypeUriFormatter<Uri>
    {
        public static readonly IdentityUriFormatter Instance = new IdentityUriFormatter();

        private IdentityUriFormatter()
        {

        }

        public Uri FormatUri(Uri value)
        {
            return value;
        }
    }
}
