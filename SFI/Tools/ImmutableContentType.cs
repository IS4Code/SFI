using IS4.SFI.Services;
using System;
using System.Net.Mime;
using System.Runtime.CompilerServices;

namespace IS4.SFI.Tools
{
    /// <summary>
    /// Provides an implementation of <see cref="ContentType"/> that preserves the value
    /// it was constructed from and prevents changes to it from taking effects.
    /// </summary>
    /// <remarks>
    /// The <see cref="ToString"/> method returns the value passed to
    /// <see cref="ImmutableContentType(string)"/>, but if the object is changed,
    /// the method throws an exception, signifying changes to the object.
    /// </remarks>
    public class ImmutableContentType : ContentType, ICloneable, IEquatable<ContentType>, IPersistentKey
    {
        static readonly ConditionalWeakTable<string, ImmutableContentType> cache = new();

        /// <summary>
        /// Obtains an instance of <see cref="ImmutableContentType"/> constructed
        /// from a particular string, potentially reusing an earlier constructed object.
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static ImmutableContentType GetCached(string contentType)
        {
            return cache.GetValue(contentType, ct => new ImmutableContentType(ct));
        }

        readonly string value;
        readonly string reparsedValue;

        bool Changed => reparsedValue != base.ToString();

        object? IPersistentKey.ReferenceKey => null;

        object? IPersistentKey.DataKey => ToString();

        /// <inheritdoc/>
        public ImmutableContentType(string contentType) : base(contentType)
        {
            value = contentType;
            reparsedValue = base.ToString();
        }

        /// <inheritdoc cref="ICloneable.Clone"/>
        public virtual ImmutableContentType Clone()
        {
            return new ImmutableContentType(value);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <inheritdoc cref="Equals(object)"/>
        public virtual bool Equals(ContentType other)
        {
            return ToString() == other.ToString();
        }

        /// <inheritdoc/>
        public override bool Equals(object rparam)
        {
            return rparam is ContentType other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if(Changed)
            {
                throw new InvalidOperationException("The instance was modified during its lifetime.");
            }
            return value;
        }
    }
}
