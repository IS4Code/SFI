using Microsoft.Extensions.Primitives;
using System;

namespace IS4.SFI.Services
{
    public readonly record struct PathObject(StringSegment Value) : IIdentityKey
    {
        public PathObject? Broader {
            get {
                var slash = Value.IndexOf('/');
                if(slash != -1 && slash != Value.Length - 1)
                {
                    return new(Value.Subsegment(slash + 1));
                }
                return null;
            }
        }

        public ExtensionObject? Extension {
            get {
                var dot = Value.LastIndexOf('.');
                if(dot != -1 && dot != Value.Length - 1)
                {
                    if(Value.IndexOf('/', dot + 1) == -1)
                    {
                        return new(Value.Subsegment(dot + 1));
                    }
                }
                return null;
            }
        }

        public bool IsRootDirectory => Value.Length == 1 && Value[0] == '/';

        /// <inheritdoc/>
        public override string ToString() => $"/{Value}";

        static readonly Type type = typeof(PathObject);

        object? IIdentityKey.ReferenceKey => type;

        object? IIdentityKey.DataKey => Value;
    }
}
