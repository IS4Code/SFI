using IS4.SFI.Services;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the JSON document format, creating instances of <see cref="JsonDocument"/>.
    /// </summary>
    public class JsonFormat : BinaryFileFormat<JsonDocument>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public JsonFormat() : base(8, "application/json", "json")
        {

        }

        /// <inheritdoc/>
        public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
        {
            var reader = new Utf8JsonReader(header, false, default);
            try{
                return reader.Read();
            }catch(JsonException)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, IS4.SFI.ResultFactory<JsonDocument, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            return await resultFactory(await JsonDocument.ParseAsync(stream), args);
        }
    }
}
