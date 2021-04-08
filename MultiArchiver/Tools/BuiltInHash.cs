using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace IS4.MultiArchiver.Tools
{
    public class BuiltInHash : IHashAlgorithm
    {
        public Individuals Identifier { get; }

        readonly string prefix;
        readonly ThreadLocal<HashAlgorithm> algorithm;

        public BuiltInHash(Func<HashAlgorithm> factory, Individuals identifier, string prefix)
        {
            Identifier = identifier;
            algorithm = new ThreadLocal<HashAlgorithm>(factory);
            this.prefix = prefix;
        }

        public byte[] ComputeHash(Stream input)
        {
            return algorithm.Value.ComputeHash(input);
        }

        public Uri FormatUri(byte[] data)
        {
            var sb = new StringBuilder(prefix.Length + data.Length * 2);
            sb.Append(prefix);
            foreach(byte b in data)
            {
                sb.Append(b.ToString("X2"));
            }
            return new Uri(sb.ToString());
        }
    }
}
