using IS4.MultiArchiver.Services;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    public class JsonFormat : BinaryFileFormat<JsonDocument>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public JsonFormat() : base(8, "application/json", "json")
        {

        }

        public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            var reader = new Utf8JsonReader(header, false, default);
            try{
                return reader.Read();
            }catch(JsonException)
            {
                return false;
            }
        }

        public async override ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, IS4.MultiArchiver.ResultFactory<JsonDocument, TResult, TArgs> resultFactory, TArgs args)
        {
            return await resultFactory(await JsonDocument.ParseAsync(stream), args);
        }
    }
}
