using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace IS4.MultiArchiver.Tools.IO
{
    public struct UnmanagedMemoryRange : IReadOnlyCollection<byte>
    {
        public IntPtr Address { get; }
        public int Count { get; }

        public UnmanagedMemoryRange(IntPtr address, int count)
        {
            Address = address;
            Count = count;
        }

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
