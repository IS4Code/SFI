using IS4.SFI.Services;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace IS4.SFI.Tools
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
        {
            key = pair.Key;
            value = pair.Value;
        }

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="exception"/>, or one of its inner
        /// exceptions, is an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The exception type to check.</typeparam>
        /// <param name="exception">The input instance of <see cref="Exception"/>.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="exception"/> has <typeparamref name="T"/>
        /// as one of its base classes, or if the same holds for its
        /// <see cref="Exception.InnerException"/> or one of
        /// <see cref="AggregateException.InnerExceptions"/>.
        /// </returns>
        public static bool Is<T>(this Exception? exception) where T : Exception
        {
            switch(exception)
            {
                case null:
                    return false;
                case T:
                    return true;
                case AggregateException aggr:
                    return aggr.InnerExceptions.Any(Is<T>);
                case { InnerException: { } inner } when inner != exception:
                    return inner.Is<T>();
                default:
                    return false;
            }
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToBase64String(this ArraySegment<byte> bytes)
        {
            return Convert.ToBase64String(bytes.Array, bytes.Offset, bytes.Count);
        }

        /// <inheritdoc cref="IndexOf{T}(ArraySegment{T}, T, int, int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this ArraySegment<T> segment, T value)
        {
            var index = Array.IndexOf(segment.Array, value, segment.Offset, segment.Count);
            if(index == -1) return -1;
            return index - segment.Offset;
        }

        /// <inheritdoc cref="IndexOf{T}(ArraySegment{T}, T, int, int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T At<T>(this ArraySegment<T> segment, int index)
        {
            return AtList<ArraySegment<T>, T>(segment, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this ArraySegment<T> segment, T[] array, int index = 0)
        {
            Array.Copy(segment.Array, segment.Offset, array, index, segment.Count);
        }

        /// <summary>
        /// Writes the bytes from an array segment to a stream.
        /// </summary>
        /// <param name="stream">The used <see cref="Stream"/> instance.</param>
        /// <param name="buffer">The data to write.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        static readonly Type entityAnalyzerType = typeof(IEntityAnalyzer<>);

        /// <summary>
        /// Checks whether <paramref name="type"/> implements a concrete
        /// <see cref="IEntityAnalyzer{T}"/> type.
        /// </summary>
        /// <param name="type">The type instance to check.</param>
        /// <returns>
        /// <see langword="true"/> if one of the implemented interfaces is a concrete type
        /// instantiated from <see cref="IEntityAnalyzer{T}"/>.
        /// </returns>
        public static bool IsEntityAnalyzerType(this Type type)
        {
            return GetEntityAnalyzerTypes(type).Any();
        }

        /// <summary>
        /// Returns the type parameters of all implementations of
        /// <see cref="IEntityAnalyzer{T}"/> on a type.
        /// </summary>
        /// <param name="type">The type instance to check.</param>
        /// <returns>A collection of types that can be analyzed
        /// by the analyzer.</returns>
        public static IEnumerable<Type> GetEntityAnalyzerTypes(this Type type)
        {
            return type.GetInterfaces().Where(i => i.IsGenericType && entityAnalyzerType.Equals(i.GetGenericTypeDefinition())).Select(t => t.GetGenericArguments()[0]);
        }

        /// <summary>
        /// Joins all non-empty elements in a sequence to a label, using a separator.
        /// </summary>
        /// <param name="components">The sequence of texts to join.</param>
        /// <param name="separator">The separator placed between non-empty elements.</param>
        /// <returns>A non-empty label, or <see langword="null"/>.</returns>
        public static string? JoinAsLabel(this IEnumerable<string?> components, string separator = ", ")
        {
            var label = String.Join(separator, components.Where(c => !String.IsNullOrWhiteSpace(c)).Distinct());
            return label != "" ? label : null;
        }

        /// <summary>
        /// Rents an array from the instance of <see cref="ArrayPool{T}"/>
        /// with a particular minimum size, and returns an instance of
        /// <see cref="IDisposable"/> that can be used to return it back to the
        /// pool.
        /// </summary>
        /// <typeparam name="T">The element type of the array.</typeparam>
        /// <param name="arrayPool">The instance of <see cref="ArrayPool{T}"/> to use.</param>
        /// <param name="minimumLength">The minimum length of the desired array.</param>
        /// <param name="array">A variable to receive the array from <paramref name="arrayPool"/>.</param>
        /// <returns>An instance of <see cref="IDisposable"/> that should be used to return the array.</returns>
        /// <remarks>
        /// This method internally calls <see cref="ArrayPool{T}.Rent(int)"/>
        /// and <see cref="ArrayPool{T}.Return(T[], bool)"/>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArrayPoolLease<T> Rent<T>(this ArrayPool<T> arrayPool, int minimumLength, out T[] array)
        {
            array = arrayPool.Rent(minimumLength);
            return new ArrayPoolLease<T>(arrayPool, array);
        }

        /// <summary>
        /// Stores an array rented from an instance of <see cref="ArrayPool{T}"/>
        /// by calling <see cref="ArrayPool{T}.Rent(int)"/>, implementing
        /// <see cref="IDisposable"/> to automatically return the array to the pool.
        /// </summary>
        /// <typeparam name="T">The element type of the array.</typeparam>
        public struct ArrayPoolLease<T> : IDisposable
        {
            readonly ArrayPool<T> arrayPool;
            T[]? array;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ArrayPoolLease(ArrayPool<T> arrayPool, T[] array)
            {
                this.arrayPool = arrayPool;
                this.array = array;
            }

            /// <summary>
            /// Calls <see cref="ArrayPool{T}.Return(T[], bool)"/> on the rented array.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                var arr = array;
                array = null;
                if(arr != null)
                {
                    arrayPool.Return(arr);
                }
            }
        }

        /// <summary>
        /// Attempts to decode a sequence of bytes to string based on the encoding,
        /// while taking the preamble into account.
        /// </summary>
        /// <param name="encoding">The instance of <see cref="Encoding"/> to use.</param>
        /// <param name="data">The byte sequence to decode.</param>
        /// <returns>The decoded string, or <see langword="null"/> if it could not be produced.</returns>
        /// <remarks>
        /// The preamble, as returned by <see cref="Encoding.GetPreamble"/>, is
        /// attempted to be matched at the beginning of the data, and if it is found,
        /// only the remaining part of the data is decoded.
        /// </remarks>
        public static string? TryGetString(this Encoding encoding, ArraySegment<byte> data)
        {
            try{
                var preamble = encoding.GetPreamble();
                if(preamble?.Length > 0 && data.AsSpan().StartsWith(preamble))
                {
                    return encoding.GetString(data.Slice(preamble.Length));
                }
                return encoding.GetString(data);
            }catch(ArgumentException)
            {
                return null;
            }
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> MemoryCast<T>(this Span<bool> span) where T : struct
        {
            return MemoryMarshal.Cast<bool, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> MemoryCast<T>(this Span<byte> span) where T : struct
        {
            return MemoryMarshal.Cast<byte, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> MemoryCast<T>(this Span<sbyte> span) where T : struct
        {
            return MemoryMarshal.Cast<sbyte, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> MemoryCast<T>(this Span<char> span) where T : struct
        {
            return MemoryMarshal.Cast<char, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> MemoryCast<T>(this Span<short> span) where T : struct
        {
            return MemoryMarshal.Cast<short, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> MemoryCast<T>(this Span<ushort> span) where T : struct
        {
            return MemoryMarshal.Cast<ushort, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> MemoryCast<T>(this Span<int> span) where T : struct
        {
            return MemoryMarshal.Cast<int, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> MemoryCast<T>(this Span<uint> span) where T : struct
        {
            return MemoryMarshal.Cast<uint, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> MemoryCast<T>(this Span<long> span) where T : struct
        {
            return MemoryMarshal.Cast<long, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> MemoryCast<T>(this Span<ulong> span) where T : struct
        {
            return MemoryMarshal.Cast<ulong, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> MemoryCast<T>(this Span<float> span) where T : struct
        {
            return MemoryMarshal.Cast<float, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> MemoryCast<T>(this Span<double> span) where T : struct
        {
            return MemoryMarshal.Cast<double, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> MemoryCast<T>(this Span<IntPtr> span) where T : struct
        {
            return MemoryMarshal.Cast<IntPtr, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<bool> span) where T : struct
        {
            return MemoryMarshal.Cast<bool, T>(span);
        }


        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<byte> span) where T : struct
        {
            return MemoryMarshal.Cast<byte, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<sbyte> span) where T : struct
        {
            return MemoryMarshal.Cast<sbyte, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<char> span) where T : struct
        {
            return MemoryMarshal.Cast<char, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<short> span) where T : struct
        {
            return MemoryMarshal.Cast<short, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<ushort> span) where T : struct
        {
            return MemoryMarshal.Cast<ushort, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<int> span) where T : struct
        {
            return MemoryMarshal.Cast<int, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<uint> span) where T : struct
        {
            return MemoryMarshal.Cast<uint, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<long> span) where T : struct
        {
            return MemoryMarshal.Cast<long, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<ulong> span) where T : struct
        {
            return MemoryMarshal.Cast<ulong, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<float> span) where T : struct
        {
            return MemoryMarshal.Cast<float, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<double> span) where T : struct
        {
            return MemoryMarshal.Cast<double, T>(span);
        }

        /// <inheritdoc cref="MemoryCast{T}(Span{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> MemoryCast<T>(this ReadOnlySpan<IntPtr> span) where T : struct
        {
            return MemoryMarshal.Cast<IntPtr, T>(span);
        }
        #endregion
    }
}
