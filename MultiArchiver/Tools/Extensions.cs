using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace IS4.MultiArchiver.Tools
{
    public static class Extensions
    {
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
        {
            key = pair.Key;
            value = pair.Value;
        }

        public static bool Is<T>(this Exception exception) where T : Exception
        {
            if(exception == null)
            {
                return false;
            }
            if(exception is T)
            {
                return true;
            }
            if(exception is AggregateException aggr)
            {
                 return aggr.InnerExceptions.Any(Is<T>);
            }
            if(exception.InnerException != null && exception.InnerException != exception)
            {
                return exception.InnerException.Is<T>();
            }
            return false;
        }

        public static ArraySegment<byte> GetData(this MemoryStream memoryStream)
        {
            if(memoryStream.TryGetBuffer(out var buffer)) return buffer;
            return new ArraySegment<byte>(memoryStream.ToArray());
        }

        public static string GetString(this Encoding encoding, ArraySegment<byte> bytes)
        {
            return encoding.GetString(bytes.Array, bytes.Offset, bytes.Count);
        }

        public static string ToBase64String(this ArraySegment<byte> bytes)
        {
            return Convert.ToBase64String(bytes.Array, bytes.Offset, bytes.Count);
        }

        public static int IndexOf<T>(this ArraySegment<T> segment, T value)
        {
            return Array.IndexOf(segment.Array, value);
        }

        public static int IndexOf<T>(this ArraySegment<T> segment, T value, int startIndex)
        {
            return Array.IndexOf(segment.Array, value, startIndex);
        }

        public static int IndexOf<T>(this ArraySegment<T> segment, T value, int startIndex, int count)
        {
            return Array.IndexOf(segment.Array, value, startIndex, count);
        }

        public static void CopyTo<T>(this ArraySegment<T> segment, T[] array, int index = 0)
        {
            Array.Copy(segment.Array, segment.Offset, array, index, segment.Count);
        }

        public static void Write(this Stream stream, ArraySegment<byte> buffer)
        {
            stream.Write(buffer.Array, buffer.Offset, buffer.Count);
        }

        public static MemoryStream AsStream(this ArraySegment<byte> buffer, bool writable)
        {
            return new MemoryStream(buffer.Array, buffer.Offset, buffer.Count, writable);
        }

        public static ArraySegment<T> Slice<T>(this ArraySegment<T> segment, int start)
        {
            if(start < 0 || start > segment.Count) throw new ArgumentOutOfRangeException(nameof(start));
            return new ArraySegment<T>(segment.Array, segment.Offset + start, segment.Count - start);
        }

        public static ArraySegment<T> Slice<T>(this ArraySegment<T> segment, int start, int length)
        {
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            if(start + length > segment.Count) throw new ArgumentOutOfRangeException(nameof(length));
            return new ArraySegment<T>(segment.Array, segment.Offset + start, length);
        }

        public static ArraySegment<T> Slice<T>(this T[] array, int start)
        {
            if(start < 0 || start > array.Length) throw new ArgumentOutOfRangeException(nameof(start));
            return new ArraySegment<T>(array, start, array.Length - start);
        }

        public static ArraySegment<T> Slice<T>(this T[] array, int start, int length)
        {
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            if(start + length > array.Length) throw new ArgumentOutOfRangeException(nameof(length));
            return new ArraySegment<T>(array, start, length);
        }

        #region MemoryCast overloads
        public static Span<T> MemoryCast<T>(this Span<bool> span) where T : struct
        {
            return MemoryMarshal.Cast<bool, T>(span);
        }

        public static Span<T> MemoryCast<T>(this Span<byte> span) where T : struct
        {
            return MemoryMarshal.Cast<byte, T>(span);
        }

        public static Span<T> MemoryCast<T>(this Span<sbyte> span) where T : struct
        {
            return MemoryMarshal.Cast<sbyte, T>(span);
        }

        public static Span<T> MemoryCast<T>(this Span<char> span) where T : struct
        {
            return MemoryMarshal.Cast<char, T>(span);
        }

        public static Span<T> MemoryCast<T>(this Span<short> span) where T : struct
        {
            return MemoryMarshal.Cast<short, T>(span);
        }

        public static Span<T> MemoryCast<T>(this Span<ushort> span) where T : struct
        {
            return MemoryMarshal.Cast<ushort, T>(span);
        }

        public static Span<T> MemoryCast<T>(this Span<long> span) where T : struct
        {
            return MemoryMarshal.Cast<long, T>(span);
        }

        public static Span<T> MemoryCast<T>(this Span<ulong> span) where T : struct
        {
            return MemoryMarshal.Cast<ulong, T>(span);
        }

        public static Span<T> MemoryCast<T>(this Span<float> span) where T : struct
        {
            return MemoryMarshal.Cast<float, T>(span);
        }

        public static Span<T> MemoryCast<T>(this Span<double> span) where T : struct
        {
            return MemoryMarshal.Cast<double, T>(span);
        }

        public static Span<T> MemoryCast<T>(this Span<IntPtr> span) where T : struct
        {
            return MemoryMarshal.Cast<IntPtr, T>(span);
        }

        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<bool> span) where T : struct
        {
            return MemoryMarshal.Cast<bool, T>(span);
        }

        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<byte> span) where T : struct
        {
            return MemoryMarshal.Cast<byte, T>(span);
        }

        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<sbyte> span) where T : struct
        {
            return MemoryMarshal.Cast<sbyte, T>(span);
        }

        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<char> span) where T : struct
        {
            return MemoryMarshal.Cast<char, T>(span);
        }

        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<short> span) where T : struct
        {
            return MemoryMarshal.Cast<short, T>(span);
        }

        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<ushort> span) where T : struct
        {
            return MemoryMarshal.Cast<ushort, T>(span);
        }

        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<long> span) where T : struct
        {
            return MemoryMarshal.Cast<long, T>(span);
        }

        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<ulong> span) where T : struct
        {
            return MemoryMarshal.Cast<ulong, T>(span);
        }

        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<float> span) where T : struct
        {
            return MemoryMarshal.Cast<float, T>(span);
        }

        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<double> span) where T : struct
        {
            return MemoryMarshal.Cast<double, T>(span);
        }

        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<IntPtr> span) where T : struct
        {
            return MemoryMarshal.Cast<IntPtr, T>(span);
        }
        #endregion
    }
}
