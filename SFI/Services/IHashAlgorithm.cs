using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace IS4.SFI.Services
{
    /// <summary>
    /// Specifies the formatting method used when converting the hash output
    /// to a URI.
    /// </summary>
    public enum FormattingMethod
    {
        /// <summary>
        /// The bytes of the output are convered to uppercase hex characters.
        /// </summary>
        Hex,

        /// <summary>
        /// <see cref="DataTools.Base32{TList}(TList, StringBuilder, string)"/> is used to format the bytes.
        /// </summary>
        Base32,

        /// <summary>
        /// <see cref="DataTools.Base58{TList}(TList, StringBuilder, string)"/> is used to format the bytes.
        /// </summary>
        Base58,

        /// <summary>
        /// <see cref="DataTools.Base64Url(ArraySegment{byte}, StringBuilder)"/> is used to format the bytes.
        /// </summary>
        Base64,

        /// <summary>
        /// The bytes are formatted as an unsigned decimal value.
        /// </summary>
        Decimal
    }

    /// <summary>
    /// Describes the properties of a hash algorithm.
    /// </summary>
    public interface IHashAlgorithm : IIndividualUriFormatter<ArraySegment<byte>>
    {
        /// <summary>
        /// The human-readable name of the algorithm.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Calculates the estimated size of the digest depending on the size of the input data.
        /// </summary>
        /// <param name="dataSize">The size of the input data.</param>
        /// <returns>The hash size in bytes.</returns>
        int GetHashSize(long dataSize);

        /// <summary>
        /// Estimates the length of the URI that would be formatted from the hash of a particular length.
        /// </summary>
        /// <param name="hashSize">The length of the hash.</param>
        /// <returns>The size of the URI in characters.</returns>
        int EstimateUriSize(int hashSize);

        /// <summary>
        /// The individual identifier of this hash algorithm.
        /// </summary>
        IndividualUri Identifier { get; }

        /// <summary>
        /// The multihash identifier of the algorithm; see
        /// https://github.com/multiformats/multicodec/blob/master/table.csv.
        /// </summary>
        int? NumericIdentifier { get; }

        /// <summary>
        /// The URI prefix for producing URIs of individual hashes.
        /// </summary>
        string Prefix { get; }

        /// <summary>
        /// The formatting method to use when appending the hash to
        /// <see cref="Prefix"/>.
        /// </summary>
        FormattingMethod FormattingMethod { get; }

        /// <summary>
        /// The name of the algorithm in the Named Information Hash Algorithm Registry
        /// (https://www.iana.org/assignments/named-information/named-information.xhtml).
        /// </summary>
        string? NiName { get; }
    }

    /// <summary>
    /// Represents a hash algorithm that accepts raw data as input.
    /// </summary>
    public interface IDataHashAlgorithm : IHashAlgorithm
    {
        /// <summary>
        /// Computes the value of the hash from an input stream.
        /// </summary>
        /// <param name="input">The input stream to compute the hash from.</param>
        /// <param name="key">The <see cref="IPersistentKey"/> identifying the data, if needed for caching.</param>
        /// <returns>The bytes of the hash.</returns>
        ValueTask<byte[]> ComputeHash(Stream input, IPersistentKey? key = null);

        /// <summary>
        /// Computes the value of the hash from a byte buffer.
        /// </summary>
        /// <param name="buffer">The array of bytes to compute the hash from.</param>
        /// <param name="key">The <see cref="IPersistentKey"/> identifying the data, if needed for caching.</param>
        /// <returns>The bytes of the hash.</returns>
        ValueTask<byte[]> ComputeHash(byte[] buffer, IPersistentKey? key = null);

        /// <summary>
        /// Computes the value of the hash from a byte buffer.
        /// </summary>
        /// <param name="buffer">The array of bytes to compute the hash from.</param>
        /// <param name="offset">The index in the array to start reading.</param>
        /// <param name="count">The number of bytes to read from the array.</param>
        /// <param name="key">The <see cref="IPersistentKey"/> identifying the data, if needed for caching.</param>
        /// <returns>The bytes of the hash.</returns>
        /// <returns></returns>
        ValueTask<byte[]> ComputeHash(byte[] buffer, int offset, int count, IPersistentKey? key = null);

        /// <summary>
        /// Computes the value of the hash from a byte buffer.
        /// </summary>
        /// <param name="buffer">The sequence of bytes to compute the hash from.</param>
        /// <param name="key">The <see cref="IPersistentKey"/> identifying the data, if needed for caching.</param>
        /// <returns>The bytes of the hash.</returns>
        ValueTask<byte[]> ComputeHash(ArraySegment<byte> buffer, IPersistentKey? key = null);
    }

    /// <summary>
    /// Represents a hash algorithm that accepts files or directories as input.
    /// </summary>
    public interface IFileHashAlgorithm : IHashAlgorithm
    {
        /// <summary>
        /// Computes the value of the hash from a file, described by <see cref="IFileInfo"/>.
        /// </summary>
        /// <param name="file">The file to compute the hash from.</param>
        /// <returns>The bytes of the hash.</returns>
        ValueTask<byte[]> ComputeHash(IFileInfo file);

        /// <summary>
        /// Computes the value of the hash from a directory, described by <see cref="IDirectoryInfo"/>.
        /// </summary>
        /// <param name="directory">The directory to compute the hash from.</param>
        /// <param name="contentOnly">
        /// <see langword="true"/> if the directory should be used only as container of its entries
        /// and not be itself a part in the hashed hierarchy.
        /// </param>
        /// <returns>The bytes of the hash.</returns>
        ValueTask<byte[]> ComputeHash(IDirectoryInfo directory, bool contentOnly);
    }

    /// <summary>
    /// Represents a hash algorithm that accepts arbitrary objects as input.
    /// </summary>
    /// <typeparam name="T">The type of the accepted objects.</typeparam>
    public interface IObjectHashAlgorithm<in T> : IHashAlgorithm
    {
        /// <summary>
        /// Computes the value of the hash from <paramref name="object"/>.
        /// </summary>
        /// <param name="object">The object to compute the hash from.</param>
        /// <returns>The bytes of the hash.</returns>
        ValueTask<byte[]> ComputeHash(T @object);
    }

    /// <summary>
    /// A base implementation of <see cref="IHashAlgorithm"/>.
    /// </summary>
    public abstract class HashAlgorithm : IHashAlgorithm
    {
        /// <inheritdoc/>
        public string Name { get; }

        /// <summary>
        /// The usual size of the hash.
        /// </summary>
        public int HashSize { get; }

        /// <inheritdoc/>
        public IndividualUri Identifier { get; }

        /// <inheritdoc/>
        public string Prefix { get; }

        /// <inheritdoc/>
        public FormattingMethod FormattingMethod { get; }

        /// <inheritdoc/>
        public virtual int? NumericIdentifier { get; }

        /// <inheritdoc/>
        public virtual string? NiName { get; }

        /// <summary>
        /// Creates a new instance of the hash algorithm.
        /// </summary>
        /// <param name="identifier">The value of <see cref="Identifier"/>.</param>
        /// <param name="hashSize">The value of <see cref="HashSize"/>.</param>
        /// <param name="prefix">The value of <see cref="Prefix"/>.</param>
        /// <param name="formatting">The value of <see cref="FormattingMethod"/>.</param>
        public HashAlgorithm(IndividualUri identifier, int hashSize, string prefix, FormattingMethod formatting)
        {
            Identifier = identifier;
            HashSize = hashSize;
            Prefix = prefix;
            FormattingMethod = formatting;
            Name = String.Concat(new Uri(prefix, UriKind.Absolute).AbsolutePath.Where(Char.IsLetterOrDigit));
        }

        /// <inheritdoc/>
        public virtual int GetHashSize(long fileSize)
        {
            return HashSize;
        }

        static readonly double log10byte = Math.Log10(256);
        static readonly double log58byte = Math.Log(256, 58);

        /// <inheritdoc/>
        public virtual int EstimateUriSize(int hashSize)
        {
            var prefix = Prefix.Length;
            switch(FormattingMethod)
            {
                case FormattingMethod.Hex:
                    return prefix + hashSize * 2;
                case FormattingMethod.Base32:
                    return prefix + (hashSize + 4) / 5 * 8;
                case FormattingMethod.Base58:
                    return prefix + (int)Math.Ceiling(hashSize * log58byte);
                case FormattingMethod.Base64:
                    return prefix + (hashSize + 2) / 3 * 4;
                case FormattingMethod.Decimal:
                    return prefix + (int)Math.Ceiling(hashSize * log10byte);
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Produces a URI identifying the result of the hashing.
        /// </summary>
        /// <param name="data">The bytes of the hash.</param>
        /// <returns>
        /// A new <see cref="Uri"/> instance, using the
        /// <see cref="Prefix"/> property alongside <see cref="FormattingMethod"/>
        /// to format <paramref name="data"/>.
        /// </returns>
        public Uri this[ArraySegment<byte> data] {
            get {
                var sb = new StringBuilder(EstimateUriSize(data.Count));
                sb.Append(Prefix);
                if(data.Count > 0)
                {
                    switch(FormattingMethod)
                    {
                        case FormattingMethod.Hex:
                            foreach(byte b in data)
                            {
                                sb.Append(b.ToString("X2"));
                            }
                            break;
                        case FormattingMethod.Base32:
                            DataTools.Base32(data, sb);
                            break;
                        case FormattingMethod.Base58:
                            DataTools.Base58(data, sb);
                            break;
                        case FormattingMethod.Base64:
                            DataTools.Base64Url(data, sb);
                            break;
                        case FormattingMethod.Decimal:
                            switch(data.Count)
                            {
                                case sizeof(byte):
                                    sb.Append(data.Array[data.Offset]);
                                    break;
                                case sizeof(ushort):
                                    sb.Append(BitConverter.ToUInt16(data.Array, data.Offset));
                                    break;
                                case sizeof(uint):
                                    sb.Append(BitConverter.ToUInt32(data.Array, data.Offset));
                                    break;
                                case sizeof(ulong):
                                    sb.Append(BitConverter.ToUInt64(data.Array, data.Offset));
                                    break;
                                default:
                                    var dataCopy = new byte[data.Count + 1];
                                    data.CopyTo(dataCopy, 0);
                                    sb.Append(new BigInteger(dataCopy).ToString());
                                    break;
                            }
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }
                return new Uri(sb.ToString());
            }
        }

        /// <summary>
        /// An estimate on the number of triples used in
        /// <see cref="AddHash(ILinkedNode, IHashAlgorithm, ArraySegment{byte}, ILinkedNodeFactory, OutputFileDelegate)"/>
        /// to initialize the hash node and assign it to a node. 
        /// </summary>
        public const int TriplesPerHash = 4;

        /// <inheritdoc cref="AddHash(ILinkedNode, IHashAlgorithm, ArraySegment{byte}, ILinkedNodeFactory, OutputFileDelegate)"/>
        public static ValueTask<ILinkedNode?> AddHash(ILinkedNode node, IHashAlgorithm? algorithm, byte[] hash, ILinkedNodeFactory nodeFactory, OutputFileDelegate? output)
        {
            return AddHash(node, algorithm, new ArraySegment<byte>(hash), nodeFactory, output);
        }

        /// <summary>
        /// Creates a <see cref="ILinkedNode"/> representing a particular hash
        /// and assigns it to <paramref name="node"/>, via
        /// <see cref="Properties.Digest"/>.
        /// </summary>
        /// <param name="node">The node to assign the hash to.</param>
        /// <param name="algorithm">The particular algorithm used to produce the hash.</param>
        /// <param name="hash">The bytes of the hash.</param>
        /// <param name="nodeFactory">The factory to use when creating the <see cref="ILinkedNode"/>.</param>
        /// <param name="output">An instance of <see cref="OutputFileDelegate"/> handling arbitrary files related to the hash.</param>
        /// <returns>The node for the hash.</returns>
        public static async ValueTask<ILinkedNode?> AddHash(ILinkedNode node, IHashAlgorithm? algorithm, ArraySegment<byte> hash, ILinkedNodeFactory nodeFactory, OutputFileDelegate? output)
        {
            if(algorithm == null || hash.Count == 0) return null;

            var hashNode = nodeFactory.Create(algorithm, hash);

            hashNode.SetClass(Classes.Digest);

            hashNode.Set(Properties.DigestAlgorithm, algorithm.Identifier);
            hashNode.Set(Properties.DigestValue, hash.ToBase64String(), Datatypes.Base64Binary);

            node.Set(Properties.Digest, hashNode);

            if(algorithm is IEntityOutputProvider<byte[]> descProvider && output != null)
            {
                foreach(var properties in hashNode.Match())
                {
                    await descProvider.DescribeEntity(hash.Array, output, properties);
                }
            }

            return hashNode;
        }

        /// <summary>
        /// Returns a particular instance of a <see cref="BuiltInHash"/>
        /// that has the provided size of the output.
        /// </summary>
        /// <param name="length">The hash size in bytes.</param>
        /// <returns>The respective instance, or <see langword="null"/> if there is none.</returns>
        public static BuiltInHash? FromLength(int length)
        {
            switch(length)
            {
                case 16:
                    return BuiltInHash.MD5;
                case 20:
                    return BuiltInHash.SHA1;
                case 32:
                    return BuiltInHash.SHA256;
                case 48:
                    return BuiltInHash.SHA384;
                case 64:
                    return BuiltInHash.SHA512;
                default:
                    return null;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// A base implementation of <see cref="IDataHashAlgorithm"/>.
    /// </summary>
    public abstract class DataHashAlgorithm : HashAlgorithm, IDataHashAlgorithm
    {
        /// <inheritdoc/>
        public DataHashAlgorithm(IndividualUri identifier, int hashSize, string prefix, FormattingMethod formatting) : base(identifier, hashSize, prefix, formatting)
        {

        }

        /// <inheritdoc/>
        public abstract ValueTask<byte[]> ComputeHash(Stream input, IPersistentKey? key = null);

        /// <inheritdoc/>
        public abstract ValueTask<byte[]> ComputeHash(byte[] data, IPersistentKey? key = null);

        /// <inheritdoc/>
        public abstract ValueTask<byte[]> ComputeHash(byte[] data, int offset, int count, IPersistentKey? key = null);

        /// <inheritdoc/>
        public ValueTask<byte[]> ComputeHash(ArraySegment<byte> buffer, IPersistentKey? key = null)
        {
            return ComputeHash(buffer.Array, buffer.Offset, buffer.Count);
        }
    }

    /// <summary>
    /// A base implementation of <see cref="IFileHashAlgorithm"/>.
    /// </summary>
    public abstract class FileHashAlgorithm : HashAlgorithm, IFileHashAlgorithm
    {
        /// <inheritdoc/>
        public FileHashAlgorithm(IndividualUri identifier, int hashSize, string prefix, FormattingMethod formatting) : base(identifier, hashSize, prefix, formatting)
        {

        }

        /// <inheritdoc/>
        public abstract ValueTask<byte[]> ComputeHash(IFileInfo file);

        /// <inheritdoc/>
        public abstract ValueTask<byte[]> ComputeHash(IDirectoryInfo directory, bool contents);
    }

    /// <summary>
    /// A base implementation of <see cref="IObjectHashAlgorithm{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the accepted objects.</typeparam>
    public abstract class ObjectHashAlgorithm<T> : HashAlgorithm, IObjectHashAlgorithm<T>
    {
        /// <inheritdoc/>
        public ObjectHashAlgorithm(IndividualUri identifier, int hashSize, string prefix, FormattingMethod formatting) : base(identifier, hashSize, prefix, formatting)
        {

        }

        /// <inheritdoc/>
        public abstract ValueTask<byte[]> ComputeHash(T @object);
    }
}
