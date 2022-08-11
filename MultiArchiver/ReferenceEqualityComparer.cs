﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace IS4.MultiArchiver
{
    /// <summary>
    /// Compares two instances of <typeparamref name="T"/> based on their reference
    /// equality, i.e. their identity, disregarding any potential overridden
    /// implementations of <see cref="Object.Equals(object)"/> or
    /// <see cref="Object.GetHashCode"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReferenceEqualityComparer<T> : EqualityComparer<T> where T : class
    {
        /// <summary>
        /// The default instance of the comparer.
        /// </summary>
        public new static readonly IEqualityComparer<T> Default = new ReferenceEqualityComparer<T>();
            
        public override bool Equals(T x, T y)
        {
            return Object.ReferenceEquals(x, y);
        }

        public override int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
