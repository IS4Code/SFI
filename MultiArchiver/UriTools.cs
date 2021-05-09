using IS4.MultiArchiver.Services;
using System;
using System.Text.RegularExpressions;
using System.Web;

namespace IS4.MultiArchiver
{
    internal static class UriTools
    {
        const string publicid = "publicid:";

        static readonly Regex pubIdRegex = new Regex(@"(^\s+|\s+$)|(\s+)|(\/\/)|(::)|([+:\/;'?#%])", RegexOptions.Compiled);

        public class PublicIdFormatter : IIndividualUriFormatter<string>
        {
            public static readonly PublicIdFormatter Instance = new PublicIdFormatter();

            private PublicIdFormatter()
            {

            }

            public Uri FormatUri(string value)
            {
                return CreatePublicId(value);
            }
        }

        public static Uri CreatePublicId(string id)
        {
            return new Uri("urn:" + publicid + TranscribePublicId(id));
        }

        public static string TranscribePublicId(string id)
        {
            return pubIdRegex.Replace(id, m => {
                if(m.Groups[1].Success)
                {
                    return "";
                }else if(m.Groups[2].Success)
                {
                    return "+";
                }else if(m.Groups[3].Success)
                {
                    return ":";
                }else if(m.Groups[4].Success)
                {
                    return ";";
                }else{
                    return Uri.EscapeDataString(m.Value);
                }
            });
        }

        static readonly Regex uriPubIdRegex = new Regex(@"(\+)|(:)|(;)|((?:%[a-fA-F0-9]{2})+)", RegexOptions.Compiled);

        public static string ExtractPublicId(Uri uri)
        {
            if(uri.IsAbsoluteUri && uri.Scheme == "urn" && String.IsNullOrEmpty(uri.Fragment))
            {
                var path = uri.AbsolutePath;
                if(path.StartsWith(publicid))
                {
                    path = path.Substring(publicid.Length);
                    return uriPubIdRegex.Replace(path, m => {
                        if(m.Groups[1].Success)
                        {
                            return " ";
                        }else if(m.Groups[2].Success)
                        {
                            return "//";
                        }else if(m.Groups[3].Success)
                        {
                            return "::";
                        }else{
                            return Uri.UnescapeDataString(m.Value);
                        }
                    });
                }
            }
            return null;
        }

        static readonly Regex urlRegex = new Regex(@"%[a-f0-9]{2}|\+", RegexOptions.Compiled);

        public static string EscapeDataBytes(byte[] bytes, int offset, int length)
        {
            return urlRegex.Replace(HttpUtility.UrlEncode(bytes, offset, length), m => {
                if(m.Value == "+") return "%20";
                return m.Value.ToUpperInvariant();
            });
        }
    }
}
