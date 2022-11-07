using System;

namespace IS4.SFI.Tools
{
    /// <summary>
    /// A class derived from <see cref="Uri"/> changing the default
    /// <see cref="Uri.ToString"/> implementation to use
    /// <see cref="Uri.AbsoluteUri"/> or <see cref="Uri.OriginalString"/>
    /// instead, as long as <see cref="Enabled"/> is true.
    /// </summary>
    public class EncodedUri : Uri
    {
        /// <summary>
        /// Whether to enable the overriding behaviour of
        /// <see cref="ToString"/>, globally.
        /// </summary>
        public static bool Enabled { get; set; } = true;

        /// <summary>
        /// Creates a new instance of the URI.
        /// </summary>
        /// <param name="uriString">The string representation of the URI, passed to <see cref="Uri.Uri(string)"/>.</param>
        public EncodedUri(string uriString) : base(uriString)
        {

        }

        /// <summary>
        /// Creates a new instance of the URI.
        /// </summary>
        /// <param name="uriString">The string representation of the URI, passed to <see cref="Uri.Uri(string, UriKind)"/>.</param>
        /// <param name="uriKind">The kind of the URI.</param>
        public EncodedUri(string uriString, UriKind uriKind) : base(uriString, uriKind)
        {

        }

        /// <summary>
        /// If <see cref="Enabled"/> is falls, calls the base implementation of
        /// <see cref="Uri.ToString"/>. Otherwise, returns either
        /// <see cref="Uri.AbsoluteUri"/> or <see cref="Uri.OriginalString"/>
        /// depending on the value of <see cref="Uri.IsAbsoluteUri"/>.
        /// </summary>
        /// <returns>The string value of the URI.</returns>
        public override string ToString()
        {
            return Enabled ? (IsAbsoluteUri ? AbsoluteUri : OriginalString) : base.ToString();
        }
    }
}
