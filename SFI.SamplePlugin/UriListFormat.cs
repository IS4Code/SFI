using IS4.SFI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// The <c>text/uri-list</c> format, containing a list of <see cref="Uri"/> instances.
    /// </summary>
    public class UriListFormat : BinaryFileFormat<IReadOnlyList<Uri>>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public UriListFormat() : base(246, "text/uri-list", "uris")
        {

        }

        /// <inheritdoc/>
        public override bool CheckHeader(ReadOnlySpan<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
        {
            if(isBinary) return false;
            using var reader = new StringReader(Encoding.UTF8.GetString(header));
            while(reader.ReadLine() is string line)
            {
                if(Uri.TryCreate(line, UriKind.Absolute, out _))
                {
                    return true;
                }
            }
            return false;
        }

        /// <inheritdoc/>
        public override async ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IReadOnlyList<Uri>, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            using var reader = new StreamReader(stream);
            var list = new List<Uri>();
            while(await reader.ReadLineAsync() is string line)
            {
                if(line.StartsWith("#")) continue;
                list.Add(new Uri(line, UriKind.Absolute));
            }
            return await resultFactory(list, args);
        }
    }
}
