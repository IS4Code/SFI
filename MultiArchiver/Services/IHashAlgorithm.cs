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
                    sb.Append(Convert.ToBase64String(data).TrimEnd('='));
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
