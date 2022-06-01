using System;

namespace IS4.MultiArchiver.Tools
{
    public class EncodedUri : Uri
    {
        public static bool Enabled { get; set; } = true;

        public EncodedUri(string uriString) : base(uriString)
        {

        }

        public EncodedUri(string uriString, UriKind uriKind) : base(uriString, uriKind)
        {

        }

        public override string ToString()
        {
            return Enabled ? (IsAbsoluteUri ? AbsoluteUri : OriginalString) : base.ToString();
        }
    }
}
