using IS4.SFI.Tools;
using System;
using System.Collections.Generic;
using VDS.RDF;

namespace IS4.SFI.Tests
{
    /// <summary>
    /// A subclass of <see cref="Uri"/>, providing explicit comparisons with other instances
    /// by implementing <see cref="IEquatable{T}"/>.
    /// </summary>
    public class EquatableUri : EncodedUri, IEquatable<EquatableUri>, IEquatable<EncodedUri>, IEquatable<Uri>
    {
        /// <summary>
        /// The comparer used to compare <see cref="Uri"/> instances.
        /// </summary>
        public static IEqualityComparer<Uri> Comparer { get; } = new UriComparer();

        /// <inheritdoc/>
        public EquatableUri(string uriString) : base(uriString)
        {

        }

        /// <inheritdoc/>
        public EquatableUri(string uriString, UriKind uriKind) : base(uriString, uriKind)
        {

        }

        /// <summary>
        /// Obtains an instance of <see cref="EquatableUri"/> from <see cref="Uri"/>.
        /// </summary>
        /// <param name="original">The URI instance to use.</param>
        /// <returns>
        /// If <paramref name="original"/> is actually an instance of <see cref="EquatableUri"/>,
        /// returns it directly, otherwise creates a new instance based on the <see cref="Uri.OriginalString"/>.
        /// </returns>
        public static EquatableUri Create(Uri original)
        {
            return original is EquatableUri uri ? uri : new EquatableUri(original.OriginalString, original.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
        }

        /// <inheritdoc/>
        public override bool Equals(object? comparand)
        {
            return comparand is Uri uri ? Equals(uri) : base.Equals(comparand);
        }

        /// <inheritdoc/>
        public virtual bool Equals(Uri? other)
        {
            return Comparer.Equals(this, other);
        }

        bool IEquatable<EquatableUri>.Equals(EquatableUri? other)
        {
            return Equals(other);
        }

        bool IEquatable<EncodedUri>.Equals(EncodedUri? other)
        {
            return Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Comparer.GetHashCode(this);
        }
    }
}
