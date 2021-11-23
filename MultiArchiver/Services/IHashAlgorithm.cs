using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace IS4.MultiArchiver.Services
{
    public enum FormattingMethod
    {
        Hex,
        Base32,
        Base58,
        Base64,
        Decimal
    }

    public interface IHashAlgorithm : IIndividualUriFormatter<ArraySegment<byte>>
    {
        string Name { get; }
        int HashSize { get; }
        IndividualUri Identifier { get; }
        int? NumericIdentifier { get; }
        string Prefix { get; }
        FormattingMethod FormattingMethod { get; }
        string NiName { get; }
    }

    public interface IDataHashAlgorithm : IHashAlgorithm
    {
        byte[] ComputeHash(Stream input, IPersistentKey key = null);
        byte[] ComputeHash(byte[] buffer, IPersistentKey key = null);
        byte[] ComputeHash(byte[] buffer, int offset, int count, IPersistentKey key = null);
        byte[] ComputeHash(ArraySegment<byte> buffer, IPersistentKey key = null);
    }

    public interface IFileHashAlgorithm : IHashAlgorithm
    {
        byte[] ComputeHash(IFileInfo file);
        byte[] ComputeHash(IDirectoryInfo directory, bool contents);
    }

    public interface IObjectHashAlgorithm<T> : IHashAlgorithm
    {
        byte[] ComputeHash(T @object);
    }

    public abstract class HashAlgorithm : IHashAlgorithm
    {
        public string Name { get; }
        public int HashSize { get; }
        public IndividualUri Identifier { get; }
        public string Prefix { get; }
        public FormattingMethod FormattingMethod { get; }
        public virtual int? NumericIdentifier { get; }
        public virtual string NiName { get; }

        public HashAlgorithm(IndividualUri identifier, int hashSize, string prefix, FormattingMethod formatting)
        {
            Identifier = identifier;
            HashSize = hashSize;
            Prefix = prefix;
            FormattingMethod = formatting;
            Name = String.Concat(new Uri(prefix, UriKind.Absolute).AbsolutePath.Where(Char.IsLetterOrDigit));
        }

        public Uri this[ArraySegment<byte> data] {
            get {
                var sb = new StringBuilder(Prefix.Length + data.Count * 2);
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
                            DataTools.Base64(data, sb);
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

        public static void AddHash(ILinkedNode node, IHashAlgorithm algorithm, byte[] hash, ILinkedNodeFactory nodeFactory)
        {
            AddHash(node, algorithm, new ArraySegment<byte>(hash), nodeFactory);
        }

        public static void AddHash(ILinkedNode node, IHashAlgorithm algorithm, ArraySegment<byte> hash, ILinkedNodeFactory nodeFactory)
        {
            if(algorithm == null || hash == null) return;

            var hashNode = nodeFactory.Create(algorithm, hash);

            hashNode.SetClass(Classes.Digest);

            hashNode.Set(Properties.DigestAlgorithm, algorithm.Identifier);
            hashNode.Set(Properties.DigestValue, hash.ToBase64String(), Datatypes.Base64Binary);

            node.Set(Properties.Digest, hashNode);
        }

        public static BuiltInHash FromLength(int length)
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
    }

    public abstract class DataHashAlgorithm : HashAlgorithm, IDataHashAlgorithm
    {
        public DataHashAlgorithm(IndividualUri identifier, int hashSize, string prefix, FormattingMethod formatting) : base(identifier, hashSize, prefix, formatting)
        {

        }

        public abstract byte[] ComputeHash(Stream input, IPersistentKey key = null);

        public abstract byte[] ComputeHash(byte[] data, IPersistentKey key = null);

        public abstract byte[] ComputeHash(byte[] data, int offset, int count, IPersistentKey key = null);

        public byte[] ComputeHash(ArraySegment<byte> buffer, IPersistentKey key = null)
        {
            return ComputeHash(buffer.Array, buffer.Offset, buffer.Count);
        }
    }

    public abstract class FileHashAlgorithm : HashAlgorithm, IFileHashAlgorithm
    {
        public FileHashAlgorithm(IndividualUri identifier, int hashSize, string prefix, FormattingMethod formatting) : base(identifier, hashSize, prefix, formatting)
        {

        }

        public abstract byte[] ComputeHash(IFileInfo file);
        public abstract byte[] ComputeHash(IDirectoryInfo directory, bool contents);
    }

    public abstract class ObjectHashAlgorithm<T> : HashAlgorithm, IObjectHashAlgorithm<T>
    {
        public ObjectHashAlgorithm(IndividualUri identifier, int hashSize, string prefix, FormattingMethod formatting) : base(identifier, hashSize, prefix, formatting)
        {

        }

        public abstract byte[] ComputeHash(T @object);
    }
}
