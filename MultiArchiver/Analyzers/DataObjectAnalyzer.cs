using IS4.MultiArchiver.Formats;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    /// <summary>
    /// An analyzer describing instances of <see cref="IDataObject"/>.
    /// </summary>
    public class DataObjectAnalyzer : EntityAnalyzer, IEntityAnalyzer<IDataObject>
    {
        public async ValueTask<AnalysisResult> Analyze(IDataObject dataObject, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);

            var isBinary = dataObject.IsBinary;

            if(!isBinary)
            {
                node.Set(Properties.CharacterEncoding, dataObject.Charset);
            }

            if(dataObject.IsComplete)
            {
                if(isBinary)
                {
                    node.Set(Properties.Bytes, dataObject.ByteValue.ToBase64String(), Datatypes.Base64Binary);
                }else if(dataObject.StringValue != null)
                {
                    node.Set(Properties.Chars, DataTools.ReplaceControlCharacters(dataObject.StringValue, dataObject.Encoding), Datatypes.String);
                }
            }else if(!isBinary && dataObject.StringValue != null && dataObject.Encoding != null && DataTools.ExtractFirstLine(dataObject.StringValue) is string firstLine)
            {
                var firstLineNode = node["#line=,1"];
                if(firstLineNode != null)
                {
                    firstLineNode.Set(Properties.Value, firstLine);
                    node.Set(Properties.HasPart, firstLineNode);
                }
            }

            var sizeSuffix = DataTools.SizeSuffix(dataObject.ActualLength, 2);

            var label = $"{(isBinary ? "binary data" : "text")} ({sizeSuffix})";

            node.Set(Properties.PrefLabel, label, LanguageCode.En);

            node.SetClass(isBinary ? Classes.ContentAsBase64 : Classes.ContentAsText);
            node.Set(Properties.Extent, dataObject.ActualLength, Datatypes.Byte);
            
            foreach(var (algorithm, value) in dataObject.Hashes)
            {
                HashAlgorithm.AddHash(node, algorithm, value, context.NodeFactory);
            }

            if(!dataObject.Recognized)
            {
                // Prepare an improvised format if no other format was recognized
                ImprovisedFormat.Format improvisedFormat = null;

                if(isBinary && DataTools.ExtractSignature(dataObject.ByteValue) is string magicText)
                {
                    improvisedFormat = new SignatureFormat(magicText);
                }

                if(!isBinary && dataObject.StringValue != null && DataTools.ExtractInterpreter(dataObject.StringValue) is string interpreter)
                {
                    improvisedFormat = new InterpreterFormat(interpreter);
                }

                if(improvisedFormat != null)
                {
                    var formatObj = new BinaryFormatObject<ImprovisedFormat.Format>(dataObject, ImprovisedFormat.Instance, improvisedFormat);
                    var formatNode = (await analyzers.Analyze(formatObj, context.WithParent(node))).Node;
                    if(formatNode != null)
                    {
                        node.Set(Properties.HasFormat, formatNode);
                    }
                }
            }

            if(node.Match(out var properties))
            {
                await OnOutputFile(label, dataObject.IsBinary, properties, async stream => {
                    using(var input = dataObject.StreamFactory.Open())
                    {
                        await input.CopyToAsync(stream);
                    }
                });
            }

            return new AnalysisResult(node);
        }

        /// <summary>
        /// This improvised format is used for binary files when the signature can be extracted
        /// via <see cref="DataTools.ExtractSignature(ArraySegment{byte})"/>.
        /// </summary>
        class SignatureFormat : ImprovisedFormat.Format
        {
            /// <summary>
            /// The extension is the signature of the file.
            /// </summary>
            public override string Extension { get; }

            /// <summary>
            /// The media type is produced by <see cref="DataTools.GetFakeMediaTypeFromSignature(string)"/>.
            /// </summary>
            public override string MediaType => DataTools.GetFakeMediaTypeFromSignature(Extension);

            public SignatureFormat(string signature)
            {
                Extension = signature;
            }
        }

        /// <summary>
        /// This improvised format is used for text files when the interpreter command can be extracted
        /// via <see cref="DataTools.ExtractInterpreter(string)"/>.
        /// </summary>
        class InterpreterFormat : ImprovisedFormat.Format
        {
            /// <summary>
            /// The extension is the interpreter command.
            /// </summary>
            public override string Extension { get; }

            /// <summary>
            /// The media type is produced by <see cref="DataTools.GetFakeMediaTypeFromInterpreter(string)"/>.
            /// </summary>
            public override string MediaType => DataTools.GetFakeMediaTypeFromInterpreter(Extension);

            public InterpreterFormat(string interpreter)
            {
                Extension = interpreter;
            }
        }

        /// <summary>
        /// An improvised format is created when there are no other formats detectable from the input.
        /// Its properties are implied based on the data itself and serve to link data likely in the same format
        /// even when the format is unknown.
        /// </summary>
        class ImprovisedFormat : BinaryFileFormat<ImprovisedFormat.Format>
        {
            public static readonly ImprovisedFormat Instance = new ImprovisedFormat();

            private ImprovisedFormat() : base(0, null, null)
            {

            }


            public override string GetMediaType(Format value)
            {
                return value.MediaType;
            }

            public override string GetExtension(Format value)
            {
                return value.Extension;
            }

            public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
            {
                throw new NotSupportedException();
            }

            public async override ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<Format, TResult, TArgs> resultFactory, TArgs args)
            {
                throw new NotSupportedException();
            }

            public abstract class Format
            {
                public abstract string Extension { get; }

                public abstract string MediaType { get; }
            }
        }
    }
}
