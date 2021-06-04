using System;

namespace IS4.MultiArchiver.Tools
{
    public sealed class EncodedUri : Uri
    {
        public EncodedUri(string uriString) : base(uriString)
        {

        }

        public EncodedUri(string uriString, UriKind uriKind) : base(uriString, uriKind)
        {

        }

        public override string ToString()
        {
            return IsAbsoluteUri ? AbsoluteUri : OriginalString;
        }
    }
}
