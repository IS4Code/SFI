using IS4.SFI.Formats;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using MorseCode.ITask;
using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of format objects as instances of <see cref="IFormatObject"/>.
    /// </summary>
    public class FormatObjectAnalyzer : EntityAnalyzer<IFormatObject>, IResultFactory<AnalysisResult, (IFormatObject format, AnalysisContext context, IEntityAnalyzers analyzer)>
    {
        /// <summary>
        /// Stores the number of digits used for <see cref="TextTools.SizeSuffix(long, int)"/>
        /// when creating the label.
        /// </summary>
        public int LabelSizeSuffixDigits { get; set; } = 2;

        /// <inheritdoc/>
        public override ValueTask<AnalysisResult> Analyze(IFormatObject format, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            return format.GetValue(this, (format, context, analyzers));
        }

        /// <summary>
        /// Analyzes <paramref name="format"/> and the value obtained from it.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="value"/>.</typeparam>
        /// <param name="value">The value extracted from <paramref name="format"/>.</param>
        /// <param name="format">The format object to analyze.</param>
        /// <param name="context"><inheritdoc cref="Analyze(IFormatObject, AnalysisContext, IEntityAnalyzers)" path="/param[@name='context']"/></param>
        /// <param name="analyzers"><inheritdoc cref="Analyze(IFormatObject, AnalysisContext, IEntityAnalyzers)" path="/param[@name='analyzers']"/></param>
        protected virtual async ValueTask<AnalysisResult> Analyze<T>(T value, IFormatObject format, AnalysisContext context, IEntityAnalyzers analyzers) where T : class
        {
            var node = GetNode(format, context);

            // The media object node should be used as base in Turtle.
            node.SetAsBase();

            var result = await analyzers.Analyze(value, context.WithNode(node));
            node = result.Node ?? node;

            var type = format.MediaType;
            if(type != null)
            {
                node.SetClass(Classes.MediaObject);
                // Some classes are set automatically based on the media type
                if(type.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
                {
                    foreach(var cls in Common.AudioClasses)
                    {
                        node.SetClass(cls);
                    }
                }else if(type.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
                {
                    foreach(var cls in Common.VideoClasses)
                    {
                        node.SetClass(cls);
                    }
                }else if(type.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    foreach(var cls in Common.ImageClasses)
                    {
                        node.SetClass(cls);
                    }
                }
                var typeObject = ImmutableContentType.GetCached(type);
                var typeNode = (await analyzers.Analyze<ContentType>(typeObject, context.WithParent(node))).Node;
                if(typeNode != null)
                {
                    node.Set(Properties.EncodingFormat, typeNode);
                }
            }
            
            var label = result.Label;
            if(format is IBinaryFormatObject binaryFormat)
            {
                label ??= TextTools.SizeSuffix(binaryFormat.Data.StreamFactory.Length, LabelSizeSuffixDigits);
            }

            if(format.Format is IXmlDocumentFormat xmlFormat)
            {
                string? pubId;
                Uri? ns;
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
                    label = $"{format.Extension.ToUpperInvariant()} object ({label})";
                }else{
                    label = $"{format.Extension.ToUpperInvariant()} object";
                }
                result.Label = label;
                node.Set(Properties.PrefLabel, label, LanguageCode.En);
            }else{
                result.Label = null;
            }

            result.Node = node;
            return result;
        }

        async ITask<AnalysisResult> IResultFactory<AnalysisResult, (IFormatObject format, AnalysisContext context, IEntityAnalyzers analyzer)>.Invoke<T>(T? value, (IFormatObject format, AnalysisContext context, IEntityAnalyzers analyzer) args) where T : class
        {
            if(value == null) return default;
            var (format, context, analyzer) = args;
            return await Analyze(value, format, context, analyzer);
        }
    }
}
