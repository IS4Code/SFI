using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using System;
using System.Text.RegularExpressions;
using System.Web;

namespace IS4.MultiArchiver
{
    public static class UriTools
    {
        const string publicid = "publicid:";

        static readonly Regex pubIdRegex = new Regex(@"(^\s+|\s+$)|(\s+)|(\/\/)|(::)|([+:\/;'?#%])", RegexOptions.Compiled);

        public static readonly IIndividualUriFormatter<string> PublicIdFormatter = new PublicIdFormatterClass();

        class PublicIdFormatterClass : IIndividualUriFormatter<string>
        {
            public Uri this[string value] => CreatePublicId(value);
        }

        public static readonly IIndividualUriFormatter<(string mediaType, string charset, ArraySegment<byte> data)> DataUriFormatter = new DataUriFormatterClass();

        class DataUriFormatterClass : IIndividualUriFormatter<(string, string, ArraySegment<byte>)>
        {
            public Uri this[(string, string, ArraySegment<byte>) value] {
                get {
                    var (mediaType, charset, bytes) = value;
                    string base64Encoded = ";base64," + bytes.ToBase64String();
                    string uriEncoded = "," + EscapeDataBytes(bytes);

                    string data = uriEncoded.Length <= base64Encoded.Length ? uriEncoded : base64Encoded;

                    switch(charset?.ToLowerInvariant())
                    {
                        case null:
                            return new Uri("data:" + (mediaType ?? "application/octet-stream") + data, UriKind.Absolute);
                        case "ascii":
                        case "us-ascii":
                            return new EncodedUri("data:" + mediaType + data, UriKind.Absolute);
                        default:
                            return new EncodedUri("data:" + mediaType + ";charset=" + charset + data, UriKind.Absolute);
                    }
                }
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

        public static string EscapeDataBytes(ArraySegment<byte> bytes)
        {
            return EscapeDataBytes(bytes.Array, bytes.Offset, bytes.Count);
        }

        public static string EscapeDataBytes(byte[] bytes, int offset, int length)
        {
            return urlRegex.Replace(HttpUtility.UrlEncode(bytes, offset, length), m => {
                if(m.Value == "+") return "%20";
                return m.Value.ToUpperInvariant();
            });
        }
    }
}
