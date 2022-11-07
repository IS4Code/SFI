using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace IS4.SFI.Tools.IO
{
    /// <summary>
    /// Represents a range of bytes in the unmanaged memory as a collection.
    /// </summary>
    public struct UnmanagedMemoryRange : IReadOnlyCollection<byte>
    {
        /// <summary>
        /// The starting address of the byte range.
        /// </summary>
        public IntPtr Address { get; }

        /// <summary>
        /// The number of bytes in the range.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Creates a new instance of the range.
        /// </summary>
        /// <param name="address">The value of <see cref="Address"/>.</param>
        /// <param name="count">The value of <see cref="Count"/>.</param>
        public UnmanagedMemoryRange(IntPtr address, int count)
        {
            Address = address;
            Count = count;
        }

        /// <inheritdoc/>
        public IEnumerator<byte> GetEnumerator()
        {
            for(int i = 0; i < Count; i++)
            {
                yield return Marshal.ReadByte(Address, i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
