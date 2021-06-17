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

    public interface IHashAlgorithm : IUriFormatter<byte[]>
    {
        string Name { get; }
        int HashSize { get; }
        Individuals Identifier { get; }
        int? NumericIdentifier { get; }
        string Prefix { get; }
        FormattingMethod FormattingMethod { get; }
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
        public Individuals Identifier { get; }
        public string Prefix { get; }
        public FormattingMethod FormattingMethod { get; }
        public int? NumericIdentifier { get; }

        public HashAlgorithm(Individuals identifier, int? numericIdentifier, int hashSize, string prefix, FormattingMethod formatting)
        {
            Identifier = identifier;
            NumericIdentifier = numericIdentifier;
            HashSize = hashSize;
            Prefix = prefix;
            FormattingMethod = formatting;
            Name = String.Concat(new Uri(prefix, UriKind.Absolute).AbsolutePath.Where(Char.IsLetterOrDigit));
        }

        public Uri FormatUri(byte[] data)
        {
            var sb = new StringBuilder(Prefix.Length + data.Length * 2);
            sb.Append(Prefix);
            if(data.Length > 0)
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
                        switch(data.Length)
                        {
                            case sizeof(byte):
                                sb.Append(data[0]);
                                break;
                            case sizeof(ushort):
                                sb.Append(BitConverter.ToUInt16(data, 0));
                                break;
                            case sizeof(uint):
                                sb.Append(BitConverter.ToUInt32(data, 0));
                                break;
                            case sizeof(ulong):
                                sb.Append(BitConverter.ToUInt64(data, 0));
                                break;
                            default:
                                if(data[data.Length - 1] > SByte.MaxValue)
                                {
                                    Array.Resize(ref data, data.Length + 1);
                                }
                                sb.Append(new BigInteger(data).ToString());
                                break;
                        }
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            return new Uri(sb.ToString());
        }

        public static void AddHash(ILinkedNode node, IHashAlgorithm algorithm, byte[] hash, ILinkedNodeFactory nodeFactory)
        {
            if(hash == null) return;
            bool tooLong = hash.Length >= 1984;
            ILinkedNode hashNode;
            if(tooLong)
            {
                var hashHash = BuiltInHash.SHA256.ComputeHash(hash);
                var sb = new StringBuilder();
                DataTools.Base32(hashHash, sb);
                hashNode = nodeFactory.Create(Vocabularies.Ah, new Uri(algorithm.Prefix, UriKind.Absolute).AbsolutePath.TrimEnd(':').Replace(':', '-') + "/" + sb);
            }else{
                hashNode = nodeFactory.Create(algorithm, hash);
            }

            hashNode.SetClass(Classes.Digest);

            hashNode.Set(Properties.DigestAlgorithm, algorithm.Identifier);
            hashNode.Set(Properties.DigestValue, Convert.ToBase64String(hash), Datatypes.Base64Binary);

            if(tooLong)
            {
                hashNode.Set(Properties.AtPrefLabel, algorithm.FormatUri(Array.Empty<byte>()).AbsoluteUri + "\u2026 (URI too long)", "en");
            }

            node.Set(Properties.Digest, hashNode);
        }
    }

    public abstract class DataHashAlgorithm : HashAlgorithm, IDataHashAlgorithm
    {
        public DataHashAlgorithm(Individuals identifier, int? numericIdentifier, int hashSize, string prefix, FormattingMethod formatting) : base(identifier, numericIdentifier, hashSize, prefix, formatting)
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
        public FileHashAlgorithm(Individuals identifier, int? numericIdentifier, int hashSize, string prefix, FormattingMethod formatting) : base(identifier, numericIdentifier, hashSize, prefix, formatting)
        {

        }

        public abstract byte[] ComputeHash(IFileInfo file);
        public abstract byte[] ComputeHash(IDirectoryInfo directory, bool contents);
    }

    public abstract class ObjectHashAlgorithm<T> : HashAlgorithm, IObjectHashAlgorithm<T>
    {
        public ObjectHashAlgorithm(Individuals identifier, int? numericIdentifier, int hashSize, string prefix, FormattingMethod formatting) : base(identifier, numericIdentifier, hashSize, prefix, formatting)
        {

        }

        public abstract byte[] ComputeHash(T @object);
    }
}
