using IS4.MultiArchiver.Formats;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    public class DataObjectAnalyzer : EntityAnalyzer, IEntityAnalyzer<IDataObject>
    {
        public async ValueTask<AnalysisResult> Analyze(IDataObject dataObject, AnalysisContext context, IEntityAnalyzerProvider analyzers)
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

            if(!dataObject.Recognized && isBinary && DataTools.ExtractSignature(dataObject.ByteValue) is string magicText)
            {
                var signatureFormat = new ImprovisedSignatureFormat.Format(magicText);
                var formatObj = new BinaryFormatObject<ImprovisedSignatureFormat.Format>(dataObject, ImprovisedSignatureFormat.Instance, signatureFormat);
                var formatNode = (await analyzers.Analyze(formatObj, context.WithParent(node))).Node;
                if(formatNode != null)
                {
                    formatNode.Set(Properties.HasFormat, node);
                }
            }

            return new AnalysisResult(node);
        }

        class ImprovisedSignatureFormat : BinaryFileFormat<ImprovisedSignatureFormat.Format>
        {
            public static readonly ImprovisedSignatureFormat Instance = new ImprovisedSignatureFormat();

            private ImprovisedSignatureFormat() : base(0, null, null)
            {

            }


            public override string GetMediaType(Format value)
            {
                return DataTools.GetFakeMediaTypeFromSignature(value.Signature);
            }

            public override string GetExtension(Format value)
            {
                return value.Signature;
            }

            public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
            {
                throw new NotSupportedException();
            }

            public override ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<Format, TResult, TArgs> resultFactory, TArgs args)
            {
                throw new NotSupportedException();
            }

            public class Format
            {
                public string Signature { get; }

                public Format(string signature)
                {
                    Signature = signature;
                }
            }
        }
    }
}
