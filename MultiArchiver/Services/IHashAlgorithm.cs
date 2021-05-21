using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace IS4.MultiArchiver.Services
{
    public enum FormattingMethod
    {
        Hex,
        Base32,
        Base64
    }

    public interface IHashAlgorithm : IUriFormatter<byte[]>
    {
        string Name { get; }
        int HashSize { get; }
        Individuals Identifier { get; }
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

    public abstract class HashAlgorithm : IHashAlgorithm
    {
        public string Name { get; }
        public int HashSize { get; }
        public Individuals Identifier { get; }
        public string Prefix { get; }
        public FormattingMethod FormattingMethod { get; }

        public HashAlgorithm(Individuals identifier, int hashSize, string prefix, FormattingMethod formatting)
        {
            Identifier = identifier;
            HashSize = hashSize;
            Prefix = prefix;
            FormattingMethod = formatting;
            Name = String.Concat(new Uri(prefix, UriKind.Absolute).AbsolutePath.Where(Char.IsLetterOrDigit));
        }

        public Uri FormatUri(byte[] data)
        {
            var sb = new StringBuilder(Prefix.Length + data.Length * 2);
            sb.Append(Prefix);
            switch(FormattingMethod)
            {
                case FormattingMethod.Hex:
                    foreach(byte b in data)
                    {
                        sb.Append(b.ToString("X2"));
                    }
                    break;
                case FormattingMethod.Base32:
                    Base32(data, sb);
                    break;
                case FormattingMethod.Base64:
                    Base64(data, sb);
                    break;
                default:
                    throw new InvalidOperationException();
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
                Base32(hashHash, sb);
                hashNode = nodeFactory.Create(Vocabularies.Ao, "hashed/" + algorithm.Name + "/" + sb);
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

        static void Base32(byte[] bytes, StringBuilder sb)
        {
            const string chars = "QAZ2WSX3EDC4RFV5TGB6YHN7UJM8K9LP";

            byte index;
            int hi = 5;
            int currentByte = 0;

            while(currentByte < bytes.Length)
            {
                if(hi > 8)
                {
                    index = (byte)(bytes[currentByte++] >> (hi - 5));
                    if(currentByte != bytes.Length)
                    {
                        index = (byte)(((byte)(bytes[currentByte] << (16 - hi)) >> 3) | index);
                    }
                    hi -= 3;
                }else if(hi == 8)
                { 
                    index = (byte)(bytes[currentByte++] >> 3);
                    hi -= 3; 
                }else{
                    index = (byte)((byte)(bytes[currentByte] << (8 - hi)) >> 3);
                    hi += 5;
                }
                sb.Append(chars[index]);
            }
        }
        
        static void Base64(byte[] bytes, StringBuilder sb)
        {
            string str = Convert.ToBase64String(bytes);

            int end = 0;
            for(end = str.Length; end > 0; end--)
            {
                if(str[end - 1] != '=')
                {
                    break;
                }
            }

            for(int i = 0; i < end; i++)
            {
                char c = str[i];

                switch (c) {
                    case '+':
                        sb.Append('-');
                        break;
                    case '/':
                        sb.Append('_');
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
        }
    }

    public abstract class DataHashAlgorithm : HashAlgorithm, IDataHashAlgorithm
    {
        public DataHashAlgorithm(Individuals identifier, int hashSize, string prefix, FormattingMethod formatting) : base(identifier, hashSize, prefix, formatting)
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
        public FileHashAlgorithm(Individuals identifier, int hashSize, string prefix, FormattingMethod formatting) : base(identifier, hashSize, prefix, formatting)
        {

        }

        public abstract byte[] ComputeHash(IFileInfo file);
        public abstract byte[] ComputeHash(IDirectoryInfo directory, bool contents);
    }
}
