using IS4.MultiArchiver.Formats;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using MorseCode.ITask;
using System;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    /// <summary>
    /// An analyzer of format objects as instances of <see cref="IFormatObject"/>.
    /// </summary>
    public class FormatObjectAnalyzer : EntityAnalyzer, IEntityAnalyzer<IFormatObject>, IResultFactory<AnalysisResult, (IFormatObject format, AnalysisContext context, IEntityAnalyzers analyzer)>
    {
        public ValueTask<AnalysisResult> Analyze(IFormatObject format, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            return format.GetValue(this, (format, context, analyzers));
        }

        /// <summary>
        /// Analyzes <paramref name="format"/> and the value obtained from it.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="value"/>.</typeparam>
        /// <param name="value">The value extracted from <paramref name="format"/>.</param>
        /// <param name="format">The format object to analyze.</param>
        /// <inheritdoc cref="Analyze(IFormatObject, AnalysisContext, IEntityAnalyzers)"/>
        protected virtual async ValueTask<AnalysisResult> Analyze<T>(T value, IFormatObject format, AnalysisContext context, IEntityAnalyzers analyzers) where T : class
        {
            var node = GetNode(format, context);

            // The media object node should be used as base in Turtle.
            node.SetAsBase();

            var result = await analyzers.Analyze(value, context.WithNode(node));
            node = result.Node ?? node;

            var type = format.MediaType?.ToLowerInvariant();
            if(type != null)
            {
                node.SetClass(Classes.MediaObject);
                // Some classes are set automatically based on the media type
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
                    // The PUBLIC identifier can also specify the format of the document
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

        async ITask<AnalysisResult> IResultFactory<AnalysisResult, (IFormatObject format, AnalysisContext context, IEntityAnalyzers analyzer)>.Invoke<T>(T value, (IFormatObject format, AnalysisContext context, IEntityAnalyzers analyzer) args)
        {
            var (format, context, analyzer) = args;
            return await Analyze(value, format, context, analyzer);
        }
    }
}
