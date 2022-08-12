using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace IS4.MultiArchiver.Tools
{
    /// <summary>
    /// Stores miscellaneous extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Deconstructs <see cref="KeyValuePair{TKey, TValue}"/> into its components.
        /// </summary>
        /// <typeparam name="TKey">The type of <see cref="KeyValuePair{TKey, TValue}.Key"/>.</typeparam>
        /// <typeparam name="TValue">The type of <see cref="KeyValuePair{TKey, TValue}.Value"/>.</typeparam>
        /// <param name="pair">The deconstructed pair of values.</param>
        /// <param name="key">The variable receiving <see cref="KeyValuePair{TKey, TValue}.Key"/>.</param>
        /// <param name="value">The variable receiving <see cref="KeyValuePair{TKey, TValue}.Value"/></param>
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
        {
            key = pair.Key;
            value = pair.Value;
        }

        /// <summary>
        /// Returns true if <paramref name="exception"/>, or one of its inner
        /// exceptions, is an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The exception type to check.</typeparam>
        /// <param name="exception">The input instance of <see cref="Exception"/>.</param>
        /// <returns>
        /// True if <paramref name="exception"/> has <typeparamref name="T"/>
        /// as one of its base classes, or if the same holds for its
        /// <see cref="Exception.InnerException"/> or one of
        /// <see cref="AggregateException.InnerExceptions"/>.
        /// </returns>
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

        /// <summary>
        /// Obtains the byte array buffer stored in an instance of
        /// <see cref="MemoryStream"/>, without copying if possible.
        /// </summary>
        /// <param name="memoryStream">The stream to obtain the data from.</param>
        /// <returns>
        /// Either the result of <see cref="MemoryStream.TryGetBuffer(out ArraySegment{byte})"/>,
        /// or <see cref="MemoryStream.ToArray"/> if that fails.
        /// </returns>
        public static ArraySegment<byte> GetData(this MemoryStream memoryStream)
        {
            if(memoryStream.TryGetBuffer(out var buffer)) return buffer;
            return new ArraySegment<byte>(memoryStream.ToArray());
        }

        /// <summary>
        /// Decodes a sequence of bytes into a string based on the encoding.
        /// </summary>
        /// <param name="encoding">The instance of <see cref="Encoding"/> to use.</param>
        /// <param name="bytes">The byte sequence to decode.</param>
        /// <returns>
        /// The result of <see cref="Encoding.GetString(byte[], int, int)"/>
        /// applied on the array segment.
        /// </returns>
        public static string GetString(this Encoding encoding, ArraySegment<byte> bytes)
        {
            return encoding.GetString(bytes.Array, bytes.Offset, bytes.Count);
        }

        /// <summary>
        /// Converts the given byte sequence to a base64-encoded string.
        /// </summary>
        /// <param name="bytes">The byte sequence to convert.</param>
        /// <returns>
        /// The result of <see cref="Convert.ToBase64String(byte[], int, int)"/>
        /// applied on the array segment.
        /// </returns>
        public static string ToBase64String(this ArraySegment<byte> bytes)
        {
            return Convert.ToBase64String(bytes.Array, bytes.Offset, bytes.Count);
        }

        /// <summary>
        /// Searches for the given element <paramref name="value"/> in the input
        /// array segment, and returns its index.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="segment">The used <see cref="ArraySegment{T}"/> insntace.</param>
        /// <param name="value">The element to search.</param>
        /// <returns>The index of the searched element within the segment, or -1 if not found.</returns>
        public static int IndexOf<T>(this ArraySegment<T> segment, T value)
        {
            var index = Array.IndexOf(segment.Array, value, segment.Offset, segment.Count);
            if(index == -1) return -1;
            return index - segment.Offset;
        }

        /// <summary>
        /// Searches for the given element <paramref name="value"/> in the input
        /// array segment, and returns its index.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="segment">The used <see cref="ArraySegment{T}"/> insntace.</param>
        /// <param name="value">The element to search.</param>
        /// <param name="startIndex">The index in the segment where to begin searching.</param>
        /// <returns>The index of the searched element within the segment, or -1 if not found.</returns>
        public static int IndexOf<T>(this ArraySegment<T> segment, T value, int startIndex)
        {
            var index = Array.IndexOf(segment.Array, value, segment.Offset + startIndex, segment.Count - startIndex);
            if(index == -1) return -1;
            return index - segment.Offset;
        }

        /// <summary>
        /// Searches for the given element <paramref name="value"/> in the input
        /// array segment, and returns its index.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="segment">The used <see cref="ArraySegment{T}"/> instance.</param>
        /// <param name="value">The element to search.</param>
        /// <param name="startIndex">The index in the segment where to begin searching.</param>
        /// <param name="count">The maximum number of elements to search.</param>
        /// <returns>The index of the searched element within the segment, or -1 if not found.</returns>
        public static int IndexOf<T>(this ArraySegment<T> segment, T value, int startIndex, int count)
        {
            var index = Array.IndexOf(segment.Array, value, segment.Offset + startIndex, Math.Min(segment.Count - startIndex, count));
            if(index == -1) return -1;
            return index - segment.Offset;
        }

        /// <summary>
        /// Retrieves the element in an array segment at a particular index.
        /// </summary>
        /// <typeparam name="T">The type of the element.</typeparam>
        /// <param name="segment">The used <see cref="ArraySegment{T}"/> instance.</param>
        /// <param name="index">The index of the element.</param>
        /// <returns>The value of the element at <paramref name="index"/>.</returns>
        public static T At<T>(this ArraySegment<T> segment, int index)
        {
            return AtList<ArraySegment<T>, T>(segment, index);
        }

        private static T AtList<TList, T>(TList list, int index) where TList : struct, IReadOnlyList<T>
        {
            return list[index];
        }

        /// <summary>
        /// Copies the elements from an array segment to an array, by calling
        /// <see cref="Array.Copy(Array, int, Array, int, int)"/> on the underlying
        /// array.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="segment">The used <see cref="ArraySegment{T}"/> instance.</param>
        /// <param name="array">The destination array.</param>
        /// <param name="index">The index in the destination array.</param>
        public static void CopyTo<T>(this ArraySegment<T> segment, T[] array, int index = 0)
        {
            Array.Copy(segment.Array, segment.Offset, array, index, segment.Count);
        }

        /// <summary>
        /// Writes the bytes from an array segment to a stream.
        /// </summary>
        /// <param name="stream">The used <see cref="Stream"/> instance.</param>
        /// <param name="buffer">The data to write.</param>
        public static void Write(this Stream stream, ArraySegment<byte> buffer)
        {
            stream.Write(buffer.Array, buffer.Offset, buffer.Count);
        }

        /// <summary>
        /// Creates a new instance of <see cref="MemoryStream"/> using the bytes
        /// from the array segment as the backing buffer.
        /// </summary>
        /// <param name="buffer">The array segment buffer to use.</param>
        /// <param name="writable">Whether the stream should allow writing or not.</param>
        /// <returns>The newly created instance.</returns>
        public static MemoryStream AsStream(this ArraySegment<byte> buffer, bool writable)
        {
            return new MemoryStream(buffer.Array, buffer.Offset, buffer.Count, writable);
        }

        /// <summary>
        /// Creates an instance of <see cref="ArraySegment{T}"/> over a range of
        /// elements in a parent <see cref="ArraySegment{T}"/>.
        /// </summary>
        /// <typeparam name="T">The element type of the array segment.</typeparam>
        /// <param name="segment">The used <see cref="ArraySegment{T}"/> instance.</param>
        /// <param name="start">The starting index of the sub-segment.</param>
        /// <returns>
        /// A new <see cref="ArraySegment{T}"/> starting at index <paramref name="start"/>
        /// in the original segment.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="start"/> is less than 0 or greater than
        /// <see cref="ArraySegment{T}.Count"/>.
        /// </exception>
        public static ArraySegment<T> Slice<T>(this ArraySegment<T> segment, int start)
        {
            if(start < 0 || start > segment.Count) throw new ArgumentOutOfRangeException(nameof(start));
            return new ArraySegment<T>(segment.Array, segment.Offset + start, segment.Count - start);
        }

        /// <summary>
        /// Creates an instance of <see cref="ArraySegment{T}"/> over a range of
        /// elements in a parent <see cref="ArraySegment{T}"/>.
        /// </summary>
        /// <typeparam name="T">The element type of the array segment.</typeparam>
        /// <param name="segment">The used <see cref="ArraySegment{T}"/> instance.</param>
        /// <param name="start">The starting index of the sub-segment.</param>
        /// <param name="length">The length of the sub-segment.</param>
        /// <returns>
        /// A new <see cref="ArraySegment{T}"/> starting at index <paramref name="start"/>
        /// in the original segment, spanning <paramref name="length"/> elements.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="start"/> is less than 0 or greater than
        /// <see cref="ArraySegment{T}.Count"/>, or <paramref name="length"/>
        /// exceeds the size of the segment.
        /// </exception>
        public static ArraySegment<T> Slice<T>(this ArraySegment<T> segment, int start, int length)
        {
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            if(start + length > segment.Count) throw new ArgumentOutOfRangeException(nameof(length));
            return new ArraySegment<T>(segment.Array, segment.Offset + start, length);
        }

        /// <summary>
        /// Creates an instance of <see cref="ArraySegment{T}"/> over a range of
        /// elements in an array.
        /// </summary>
        /// <typeparam name="T">The element type of the array segment.</typeparam>
        /// <param name="array">The used array to create the segment from.</param>
        /// <param name="start">The starting index of the segment.</param>
        /// <returns>
        /// A new <see cref="ArraySegment{T}"/> starting at index <paramref name="start"/>
        /// in the original segment.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="start"/> is less than 0 or greater than
        /// the length of the array.
        /// </exception>
        public static ArraySegment<T> Slice<T>(this T[] array, int start)
        {
            if(start < 0 || start > array.Length) throw new ArgumentOutOfRangeException(nameof(start));
            return new ArraySegment<T>(array, start, array.Length - start);
        }

        /// <summary>
        /// Creates an instance of <see cref="ArraySegment{T}"/> over a range of
        /// elements in an array.
        /// </summary>
        /// <typeparam name="T">The element type of the array segment.</typeparam>
        /// <param name="array">The used array to create the segment from.</param>
        /// <param name="start">The starting index of the segment.</param>
        /// <param name="length">The length of the segment.</param>
        /// <returns>
        /// A new <see cref="ArraySegment{T}"/> starting at index <paramref name="start"/>
        /// in the array, spanning <paramref name="length"/> elements.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="start"/> is less than 0 or greater than
        /// the length of the array, or <paramref name="length"/>
        /// exceeds the size of the array.
        /// </exception>
        public static ArraySegment<T> Slice<T>(this T[] array, int start, int length)
        {
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            if(start + length > array.Length) throw new ArgumentOutOfRangeException(nameof(length));
            return new ArraySegment<T>(array, start, length);
        }

        #region MemoryCast overloads

        /// <summary>
        /// Memory-casts the input span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(Span{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="Span{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static Span<T> MemoryCast<T>(this Span<bool> span) where T : struct
        {
            return MemoryMarshal.Cast<bool, T>(span);
        }

        /// <summary>
        /// Memory-casts the input span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(Span{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="Span{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static Span<T> MemoryCast<T>(this Span<byte> span) where T : struct
        {
            return MemoryMarshal.Cast<byte, T>(span);
        }

        /// <summary>
        /// Memory-casts the input span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(Span{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="Span{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static Span<T> MemoryCast<T>(this Span<sbyte> span) where T : struct
        {
            return MemoryMarshal.Cast<sbyte, T>(span);
        }

        /// <summary>
        /// Memory-casts the input span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(Span{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="Span{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static Span<T> MemoryCast<T>(this Span<char> span) where T : struct
        {
            return MemoryMarshal.Cast<char, T>(span);
        }

        /// <summary>
        /// Memory-casts the input span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(Span{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="Span{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static Span<T> MemoryCast<T>(this Span<short> span) where T : struct
        {
            return MemoryMarshal.Cast<short, T>(span);
        }

        /// <summary>
        /// Memory-casts the input span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(Span{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="Span{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static Span<T> MemoryCast<T>(this Span<ushort> span) where T : struct
        {
            return MemoryMarshal.Cast<ushort, T>(span);
        }

        /// <summary>
        /// Memory-casts the input span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(Span{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="Span{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static Span<T> MemoryCast<T>(this Span<int> span) where T : struct
        {
            return MemoryMarshal.Cast<int, T>(span);
        }

        /// <summary>
        /// Memory-casts the input span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(Span{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="Span{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static Span<T> MemoryCast<T>(this Span<uint> span) where T : struct
        {
            return MemoryMarshal.Cast<uint, T>(span);
        }

        /// <summary>
        /// Memory-casts the input span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(Span{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="Span{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static Span<T> MemoryCast<T>(this Span<long> span) where T : struct
        {
            return MemoryMarshal.Cast<long, T>(span);
        }

        /// <summary>
        /// Memory-casts the input span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(Span{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="Span{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static Span<T> MemoryCast<T>(this Span<ulong> span) where T : struct
        {
            return MemoryMarshal.Cast<ulong, T>(span);
        }

        /// <summary>
        /// Memory-casts the input span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(Span{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="Span{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static Span<T> MemoryCast<T>(this Span<float> span) where T : struct
        {
            return MemoryMarshal.Cast<float, T>(span);
        }

        /// <summary>
        /// Memory-casts the input span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(Span{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="Span{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static Span<T> MemoryCast<T>(this Span<double> span) where T : struct
        {
            return MemoryMarshal.Cast<double, T>(span);
        }

        /// <summary>
        /// Memory-casts the input span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(Span{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="Span{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static Span<T> MemoryCast<T>(this Span<IntPtr> span) where T : struct
        {
            return MemoryMarshal.Cast<IntPtr, T>(span);
        }

        /// <summary>
        /// Memory-casts the input read-only span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(ReadOnlySpan{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="ReadOnlySpan{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<bool> span) where T : struct
        {
            return MemoryMarshal.Cast<bool, T>(span);
        }


        /// <summary>
        /// Memory-casts the input read-only span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(ReadOnlySpan{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="ReadOnlySpan{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<byte> span) where T : struct
        {
            return MemoryMarshal.Cast<byte, T>(span);
        }


        /// <summary>
        /// Memory-casts the input read-only span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(ReadOnlySpan{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="ReadOnlySpan{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<sbyte> span) where T : struct
        {
            return MemoryMarshal.Cast<sbyte, T>(span);
        }


        /// <summary>
        /// Memory-casts the input read-only span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(ReadOnlySpan{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="ReadOnlySpan{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<char> span) where T : struct
        {
            return MemoryMarshal.Cast<char, T>(span);
        }


        /// <summary>
        /// Memory-casts the input read-only span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(ReadOnlySpan{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="ReadOnlySpan{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<short> span) where T : struct
        {
            return MemoryMarshal.Cast<short, T>(span);
        }


        /// <summary>
        /// Memory-casts the input read-only span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(ReadOnlySpan{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="ReadOnlySpan{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<ushort> span) where T : struct
        {
            return MemoryMarshal.Cast<ushort, T>(span);
        }


        /// <summary>
        /// Memory-casts the input read-only span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(ReadOnlySpan{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="ReadOnlySpan{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<int> span) where T : struct
        {
            return MemoryMarshal.Cast<int, T>(span);
        }


        /// <summary>
        /// Memory-casts the input read-only span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(ReadOnlySpan{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="ReadOnlySpan{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<uint> span) where T : struct
        {
            return MemoryMarshal.Cast<uint, T>(span);
        }


        /// <summary>
        /// Memory-casts the input read-only span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(ReadOnlySpan{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="ReadOnlySpan{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<long> span) where T : struct
        {
            return MemoryMarshal.Cast<long, T>(span);
        }


        /// <summary>
        /// Memory-casts the input read-only span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(ReadOnlySpan{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="ReadOnlySpan{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<ulong> span) where T : struct
        {
            return MemoryMarshal.Cast<ulong, T>(span);
        }


        /// <summary>
        /// Memory-casts the input read-only span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(ReadOnlySpan{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="ReadOnlySpan{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<float> span) where T : struct
        {
            return MemoryMarshal.Cast<float, T>(span);
        }


        /// <summary>
        /// Memory-casts the input read-only span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(ReadOnlySpan{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="ReadOnlySpan{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<double> span) where T : struct
        {
            return MemoryMarshal.Cast<double, T>(span);
        }


        /// <summary>
        /// Memory-casts the input read-only span to a different element type,
        /// via <see cref="MemoryMarshal.Cast{TFrom, TTo}(ReadOnlySpan{TFrom})"/>.
        /// </summary>
        /// <typeparam name="T">The new element type of the span.</typeparam>
        /// <param name="span">The input <see cref="ReadOnlySpan{T}"/> instance to use.</param>
        /// <returns>
        /// A new span over the same memory range, but with elements
        /// reinterpreted to type <typeparamref name="T"/>.
        /// </returns>
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<IntPtr> span) where T : struct
        {
            return MemoryMarshal.Cast<IntPtr, T>(span);
        }
        #endregion
    }
}
