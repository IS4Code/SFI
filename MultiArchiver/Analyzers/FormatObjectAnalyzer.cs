using IS4.MultiArchiver.Formats;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using MorseCode.ITask;
using System;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    public class FormatObjectAnalyzer : EntityAnalyzer, IEntityAnalyzer<IFormatObject>, IResultFactory<AnalysisResult, (IFormatObject format, AnalysisContext context, IEntityAnalyzerProvider analyzer)>
    {
        public ValueTask<AnalysisResult> Analyze(IFormatObject format, AnalysisContext context, IEntityAnalyzerProvider analyzer)
        {
            return format.GetValue(this, (format, context, analyzer));
        }

        protected virtual async ValueTask<AnalysisResult> Analyze<T>(T value, IFormatObject format, AnalysisContext context, IEntityAnalyzerProvider analyzer) where T : class
        {
            var node = GetNode(format, context);

            node.SetAsBase();

            var result = await analyzer.Analyze(value, context.WithNode(node));
            node = result.Node ?? node;

            var type = format.MediaType?.ToLowerInvariant();
            if(type != null)
            {
                if(type.StartsWith("audio/", StringComparison.Ordinal))
                {
                    foreach(var cls in Common.AudioClasses)
                    {
                        node.SetClass(cls);
                    }
                }else if(type.StartsWith("video/", StringComparison.Ordinal))
                {
                    foreach(var cls in Common.VideoClasses)
                    {
                        node.SetClass(cls);
                    }
                }else if(type.StartsWith("image/", StringComparison.Ordinal))
                {
                    foreach(var cls in Common.ImageClasses)
                    {
                        node.SetClass(cls);
                    }
                }
                node.Set(Properties.EncodingFormat, Vocabularies.Urim, Uri.EscapeUriString(type));
            }
            
            var label = result.Label;
            if(format is IBinaryFormatObject binaryFormat)
            {
                label = label ?? DataTools.SizeSuffix(binaryFormat.Data.StreamFactory.Length, 2);
            }

            if(format.Format is IXmlDocumentFormat xmlFormat)
            {
                string pubId;
                Uri ns;
                if(format.Format is IXmlDocumentFormat<T> xmlFormat2)
                {
                    pubId = xmlFormat2.GetPublicId(value);
                    ns = xmlFormat2.GetNamespace(value);
                }else{
                    pubId = xmlFormat.GetPublicId(value);
                    ns = xmlFormat.GetNamespace(value);
                }

                if(pubId != null)
                {
                    node.Set(Properties.EncodingFormat, UriTools.PublicIdFormatter, pubId);
                }
            }

            if(format.Extension != null)
            {
                if(label != null)
                {
                    node.Set(Properties.PrefLabel, $"{format.Extension.ToUpperInvariant()} object ({label})", LanguageCode.En);
                }else{
                    node.Set(Properties.PrefLabel, $"{format.Extension.ToUpperInvariant()} object", LanguageCode.En);
                }
            }

            result.Node = node;
            return result;
        }

        async ITask<AnalysisResult> IResultFactory<AnalysisResult, (IFormatObject format, AnalysisContext context, IEntityAnalyzerProvider analyzer)>.Invoke<T>(T value, (IFormatObject format, AnalysisContext context, IEntityAnalyzerProvider analyzer) args)
        {
            var (format, context, analyzer) = args;
            return await Analyze(value, format, context, analyzer);
        }
    }
}
