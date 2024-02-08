using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    using static Individuals.Errors;

    /// <summary>
    /// An analyzer of instances of <see cref="Exception"/> that may arise during analysis of other entities.
    /// </summary>
    [Description("An analyzer of exceptions that may arise during analysis of other entities.")]
    public sealed class ExceptionAnalyzer : EntityAnalyzer<Exception>
    {
        /// <summary>
        /// Whether to ignore source code position obtained from the stack trace.
        /// </summary>
        [Description("Whether to ignore source code position obtained from the stack trace.")]
        public bool IgnoreSourcePosition { get; set; }

        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public ExceptionAnalyzer()
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(Exception exception, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);

            var language = new LanguageCode(CultureInfo.CurrentUICulture);
            AnalyzeException(exception, node, language);

            return new(node);
        }

        void AnalyzeException(Exception exception, ILinkedNode node, LanguageCode language)
        {
            if(exception is AggregateException aggr)
            {
                foreach(var inner in aggr.InnerExceptions)
                {
                    AnalyzeException(inner, node, language);
                }
                return;
            }

            node.Set(Properties.ErrorValue, ClrNamespaceUriFormatter.Instance, exception.GetType());

            node.Set(Properties.ErrorDescription, exception.Message, language);

            if(GetErrorCodeAndDetails(exception) is ({ } code, var details))
            {
                node.Set(Properties.ErrorCode, code);
                node.TrySet(Properties.ErrorValue, details);
            }

            var trace = new StackTrace(exception, true);
            if(HasComponentStackFrame(trace, out var frame, out var analyzerType))
            {
                node.Set(Properties.ErrorModule, ClrNamespaceUriFormatter.Instance, analyzerType);
                if(!IgnoreSourcePosition)
                {
                    if(IsDefined(frame.GetFileLineNumber(), out var line))
                    {
                        node.Set(Properties.ErrorLineNumber, line);
                    }
                    if(IsDefined(frame.GetFileColumnNumber(), out var column))
                    {
                        node.Set(Properties.ErrorColumnNumber, column);
                    }
                }
            }else if(exception.TargetSite is { DeclaringType: { } declaringType })
            {
                node.Set(Properties.ErrorModule, ClrNamespaceUriFormatter.Instance, declaringType);
            }
        }

        static bool HasComponentStackFrame(StackTrace trace, [MaybeNullWhen(false)] out StackFrame frame, [MaybeNullWhen(false)] out Type analyzerType)
        {
            if(trace.FrameCount == 0)
            {
                frame = null;
                analyzerType = null;
                return false;
            }
            // Look for an entity analyzer
            for(int i = 0; i < trace.FrameCount; i++)
            {
                frame = trace.GetFrame(i);

                if(frame.GetMethod() is { DeclaringType: { } declaringType })
                {
                    if(IsInEntityAnalyzer(declaringType, out analyzerType))
                    {
                        return true;
                    }
                }
            }
            // Look for any method information
            for(int i = 0; i < trace.FrameCount; i++)
            {
                frame = trace.GetFrame(i);

                if(frame.GetMethod() is { DeclaringType: { } declaringType })
                {
                    analyzerType = declaringType;
                    return true;
                }
            }
            frame = null;
            analyzerType = null;
            return false;
        }

        static bool IsInEntityAnalyzer(Type type, [MaybeNullWhen(false)] out Type declaringType)
        {
            if(type.IsEntityAnalyzerType())
            {
                declaringType = type;
                return true;
            }
            if(type.DeclaringType is { } containingType)
            {
                return IsInEntityAnalyzer(containingType, out declaringType);
            }
            declaringType = null;
            return false;
        }

        static readonly HashSet<Type> UnknownExceptionTypes = new()
        {
            typeof(Exception), typeof(SystemException), typeof(ApplicationException)
        };

        (IndividualUri?, object?) GetErrorCodeAndDetails(Exception exception)
        {
            return exception switch
            {
                DivideByZeroException => (DivisionByZero, null),
                OverflowException => (NumericOperationOverflowOrUnderflow, null),
                NotFiniteNumberException notFinite when Double.IsNaN(notFinite.OffendingNumber) => (NaNSuppliedAsFloatOrDoubleValue, null),
                NotFiniteNumberException notFinite => (NumericOperationOverflowOrUnderflow, notFinite.OffendingNumber),
                InvalidCastException => (InvalidValueForCastOrConstructor, null),
                FormatException => (InvalidLexicalValue, null),
                InvalidTimeZoneException => (InvalidTimezoneValue, null),
                IndexOutOfRangeException => (ArrayIndexOutOfBounds, null),
                ArgumentException argument => (InvalidArgumentType, argument.ParamName),
                NullReferenceException => (ErrorRetrievingResource, null),
                _ when UnknownExceptionTypes.Contains(exception.GetType()) => (UnidentifiedError, null),
                _ => default
            };
        }
    }
}
