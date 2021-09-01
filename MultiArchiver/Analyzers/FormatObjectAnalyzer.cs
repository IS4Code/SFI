using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;

namespace IS4.MultiArchiver.Analyzers
{
    public class FormatObjectAnalyzer : EntityAnalyzerBase, IEntityAnalyzer<IFormatObject>, IResultFactory<AnalysisResult, (IFormatObject format, AnalysisContext context, IEntityAnalyzerProvider analyzer)>
    {
        public AnalysisResult Analyze(IFormatObject format, AnalysisContext context, IEntityAnalyzerProvider analyzer)
        {
            return format.GetValue(this, (format, context, analyzer));
        }

        protected virtual AnalysisResult Analyze<T>(T value, IFormatObject format, AnalysisContext context, IEntityAnalyzerProvider analyzer) where T : class
        {
            var node = GetNode(format, context);

            var result = analyzer.Analyze(value, context.WithNode(node));
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
            if(context.MatchContext.Stream is Stream stream)
            {
                label = label ?? DataTools.SizeSuffix(stream.Length, 2);
            }else if(context.MatchContext.GetService<IStreamFactory>() is IStreamFactory streamFactory)
            {
                label = label ?? DataTools.SizeSuffix(streamFactory.Length, 2);
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

        AnalysisResult IResultFactory<AnalysisResult, (IFormatObject format, AnalysisContext context, IEntityAnalyzerProvider analyzer)>.Invoke<T>(T value, (IFormatObject format, AnalysisContext context, IEntityAnalyzerProvider analyzer) args)
        {
            var (format, context, analyzer) = args;
            return Analyze(value, format, context, analyzer);
        }
    }
}
