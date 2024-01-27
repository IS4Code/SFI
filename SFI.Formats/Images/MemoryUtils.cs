using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace IS4.SFI.Tools
{
    static class MemoryUtils
    {
        public static Memory<TTo> Cast<TFrom, TTo>(Memory<TFrom> from) where TFrom : unmanaged where TTo : unmanaged
        {
            if(MemoryMarshal.TryGetArray<TFrom>(from, out var array) && array.Array is TTo[] compatible)
            {
                return compatible.AsMemory(array.Offset, array.Count);
            }
            return new CastMemoryManager<TFrom, TTo>(from).Memory;
        }

        sealed class CastMemoryManager<TFrom, TTo> : MemoryManager<TTo> where TFrom : unmanaged where TTo : unmanaged
        {
            Memory<TFrom> memory;

            public CastMemoryManager(Memory<TFrom> memory)
            {
                this.memory = memory;
            }

            public override Span<TTo> GetSpan()
            {
                return MemoryMarshal.Cast<TFrom, TTo>(memory.Span);
            }

            public override MemoryHandle Pin(int elementIndex = 0)
            {
                if(elementIndex != 0)
                {
                    throw new NotImplementedException();
                }
                return memory.Pin();
            }

            public override void Unpin()
            {

            }

            protected override void Dispose(bool disposing)
            {
                if(disposing)
                {
                    memory = Memory<TFrom>.Empty;
                }
            }
        }

        public static Memory<byte> GetObjectMemory(object obj)
        {
            if(obj is Array array)
            {
                return GetArrayMemory(array);
            }
            if(!obj.GetType().IsValueType)
            {
                throw new ArgumentException("Argument must be an array or a boxed value type.", nameof(obj));
            }
            return new ObjectMemoryManager(obj).Memory;
        }

        [DynamicDependency(nameof(GetDynamicArrayMemory), typeof(MemoryUtils))]
        static Memory<byte> GetArrayMemory(Array array)
        {
            if(array is byte[] byteArray)
            {
                return byteArray.AsMemory();
            }
            try{
                return GetDynamicArrayMemory((dynamic)array);
            }catch(RuntimeBinderException)
            {

            }
            return new ArrayMemoryManager(array).Memory;
        }

        static Memory<byte> GetDynamicArrayMemory<T>(T[] array) where T : unmanaged
        {
            return Cast<T, byte>(array.AsMemory());
        }

        public static Memory<byte> GetValueMemory<T>(T value) where T : unmanaged
        {
            return GetDynamicArrayMemory(new[] { value });
        }

        sealed class ArrayMemoryManager : MemoryManager<byte>
        {
            readonly Array array;
            GCHandle handle;

            public ArrayMemoryManager(Array array)
            {
                this.array = array;
                handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            }

            public unsafe override Span<byte> GetSpan()
            {
                return new Span<byte>((void*)handle.AddrOfPinnedObject(), Buffer.ByteLength(array));
            }

            public unsafe override MemoryHandle Pin(int elementIndex = 0)
            {
                if(elementIndex < 0 || elementIndex >= Buffer.ByteLength(array))
                {
                    throw new ArgumentOutOfRangeException(nameof(elementIndex));
                }
                return new MemoryHandle((byte*)handle.AddrOfPinnedObject() + elementIndex, handle, this);
            }

            public override void Unpin()
            {
                handle.Free();
            }

            protected override void Dispose(bool disposing)
            {
                if(handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }
        
        sealed class ObjectMemoryManager : MemoryManager<byte>
        {
            readonly object obj;
            readonly int length;
            GCHandle handle;

            static readonly MethodInfo sizeOf = typeof(Unsafe).GetMethod(nameof(Unsafe.SizeOf), BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);

            public ObjectMemoryManager(object obj)
            {
                this.obj = obj;
                length = (int)sizeOf.MakeGenericMethod(obj.GetType()).Invoke(null, null);
                handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
            }

            public unsafe override Span<byte> GetSpan()
            {
                return new Span<byte>((void*)handle.AddrOfPinnedObject(), length);
            }

            public unsafe override MemoryHandle Pin(int elementIndex = 0)
            {
                if(elementIndex < 0 || elementIndex >= length)
                {
                    throw new ArgumentOutOfRangeException(nameof(elementIndex));
                }
                return new MemoryHandle((byte*)handle.AddrOfPinnedObject() + elementIndex, handle, this);
            }

            public override void Unpin()
            {
                handle.Free();
            }

            protected override void Dispose(bool disposing)
            {
                if(handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }
    }
}
