using Microsoft.Extensions.Primitives;
using System;

namespace IS4.SFI.Services
{
    /// <summary>
    /// Represents a path in any file system.
    /// </summary>
    /// <param name="Value">The textual value of the path.</param>
    public readonly record struct PathObject(StringSegment Value) : IIdentityKey
    {
        /// <summary>
        /// The less specific path contained within this path, if existing.
        /// </summary>
        /// <remarks>
        /// The path is obtained by splitting the value at the first <c>/</c>
        /// and retrieving the second segment.
        /// </remarks>
        public PathObject? Broader {
            get {
                var slash = Value.IndexOf('/');
                if(slash != -1 && !(slash == 0 && Value.Length == 1))
                {
                    if(slash == Value.Length - 1)
                    {
                        return new(Value.Subsegment(slash));
                    }else{
                        return new(Value.Subsegment(slash + 1));
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// The parent path, as a directory containing this path.
        /// </summary>
        /// <remarks>
        /// The path is obtained by splitting the value at the last <c>/</c>
        /// and retrieving the first segment.
        /// </remarks>
        public PathObject? Parent {
            get {
                var slash = Value.LastIndexOf('/');
                if(slash != -1)
                {
                    if(slash == Value.Length - 1)
                    {
                        return new(Value.Subsegment(0, slash));
                    }else{
                        return new(Value.Subsegment(0, slash + 1));
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// The extension indicated by the path, if any.
        /// </summary>
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

        /// <summary>
        /// Whether the path is equal to <c>/</c>, indicating the root directory.
        /// </summary>
        public bool IsRootDirectory => Value.Length == 1 && Value[0] == '/';

        /// <summary>
        /// Whether the path is equal to the empty string, indicating the root.
        /// </summary>
        public bool IsRoot => Value.Length == 0;

        /// <inheritdoc/>
        public override string ToString() => $"/{Value}";

        static readonly Type type = typeof(PathObject);

        object? IIdentityKey.ReferenceKey => type;

        object? IIdentityKey.DataKey => Value;
    }
}
