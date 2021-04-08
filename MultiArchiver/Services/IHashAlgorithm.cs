using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;
using System.Text;

namespace IS4.MultiArchiver.Services
{
    public interface IHashAlgorithm : IUriFormatter<byte[]>
    {
        Individuals Identifier { get; }
        byte[] ComputeHash(Stream input);
    }

    public abstract class HashAlgorithm : IHashAlgorithm
    {
        public Individuals Identifier { get; }

        readonly string prefix;
        readonly FormattingMethod formatting;

        public HashAlgorithm(Individuals identifier, string prefix, FormattingMethod formatting)
        {
            Identifier = identifier;
            this.prefix = prefix;
            this.formatting = formatting;
        }

        public abstract byte[] ComputeHash(Stream input);

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
}
