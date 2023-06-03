using Microsoft.Extensions.Primitives;
using System;

namespace IS4.SFI.Services
{
    public readonly record struct ExtensionObject(StringSegment Value) : IIdentityKey
    {
        static readonly Type type = typeof(ExtensionObject);

        /// <inheritdoc/>
        public override string ToString() => $".{Value}";

        object? IIdentityKey.ReferenceKey => type;

        object? IIdentityKey.DataKey => Value;
    }
}
