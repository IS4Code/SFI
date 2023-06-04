using Microsoft.Extensions.Primitives;
using System;

namespace IS4.SFI.Services
{
    /// <summary>
    /// Represents a file name extension.
    /// </summary>
    /// <param name="Value">The textual value of the extension, excluding the <c>.</c>.</param>
    public readonly record struct ExtensionObject(StringSegment Value) : IIdentityKey
    {
        static readonly Type type = typeof(ExtensionObject);

        /// <inheritdoc/>
        public override string ToString() => $".{Value}";

        object? IIdentityKey.ReferenceKey => type;

        object? IIdentityKey.DataKey => Value;
    }
}
