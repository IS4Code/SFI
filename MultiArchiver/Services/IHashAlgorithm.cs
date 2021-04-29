using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace IS4.MultiArchiver.Services
{
    public interface IHashAlgorithm : IUriFormatter<byte[]>
    {
        string Name { get; }
        int HashSize { get; }
        Individuals Identifier { get; }
    }

    public interface IDataHashAlgorithm : IHashAlgorithm
    {
        byte[] ComputeHash(Stream input);
        byte[] ComputeHash(byte[] buffer);
        byte[] ComputeHash(byte[] buffer, int offset, int count);
        byte[] ComputeHash(ArraySegment<byte> buffer);
    }

    public interface IFileHashAlgorithm : IHashAlgorithm
    {
        byte[] ComputeHash(IFileNodeInfo fileNode);
    }

    public abstract class HashAlgorithm : IHashAlgorithm
    {
        public string Name { get; }
        public int HashSize { get; }
        public Individuals Identifier { get; }

        readonly string prefix;
        readonly FormattingMethod formatting;

        public HashAlgorithm(Individuals identifier, int hashSize, string prefix, FormattingMethod formatting)
        {
            Identifier = identifier;
            HashSize = hashSize;
            this.prefix = prefix;
            this.formatting = formatting;
            Name = String.Concat(new Uri(prefix, UriKind.Absolute).AbsolutePath.Where(Char.IsLetterOrDigit));
        }

        public Uri FormatUri(byte[] data)
        {
            var sb = new StringBuilder(prefix.Length + data.Length * 2);
            sb.Append(prefix);
            switch(formatting)
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

        public enum FormattingMethod
        {
            Hex,
            Base32,
            Base64
        }

        public static void AddHash(ILinkedNode node, IHashAlgorithm algorithm, byte[] hash, ILinkedNodeFactory nodeFactory)
        {
            bool tooLong = hash.Length >= 1984;
            var hashNode = tooLong ? nodeFactory.NewGuidNode() : nodeFactory.Create(algorithm, hash);

            hashNode.SetClass(Classes.Digest);

            hashNode.Set(Properties.DigestAlgorithm, algorithm.Identifier);
            hashNode.Set(Properties.DigestValue, Convert.ToBase64String(hash), Datatypes.Base64Binary);

            if(tooLong)
            {
                hashNode.Set(Properties.Label, algorithm.FormatUri(Array.Empty<byte>()).AbsoluteUri + "\u2026 (URI too long)", "en");
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

        public abstract byte[] ComputeHash(Stream input);

        public abstract byte[] ComputeHash(byte[] data);

        public abstract byte[] ComputeHash(byte[] data, int offset, int count);

        public byte[] ComputeHash(ArraySegment<byte> buffer)
        {
            return ComputeHash(buffer.Array, buffer.Offset, buffer.Count);
        }
    }

    public abstract class FileHashAlgorithm : HashAlgorithm, IFileHashAlgorithm
    {
        public FileHashAlgorithm(Individuals identifier, int hashSize, string prefix, FormattingMethod formatting) : base(identifier, hashSize, prefix, formatting)
        {

        }

        public abstract byte[] ComputeHash(IFileNodeInfo fileNode);
    }
}
