﻿using IS4.MultiArchiver.Formats;
using System;
using System.Text.RegularExpressions;
using System.Web;

namespace IS4.MultiArchiver.Services
{
    public interface IFormatObject : IIndividualUriFormatter<Uri>
    {
        string Extension { get; }
        string MediaType { get; }
        IFileFormat Format { get; }

        TResult GetValue<TResult, TArgs>(IResultFactory<TResult, TArgs> resultFactory, TArgs args);
    }

    public interface IFormatObject<out T> : IFormatObject
    {
        T Value { get; }
    }

    public interface IBinaryFormatObject : IFormatObject
    {
        IDataObject Data { get; }
    }

    public interface IBinaryFormatObject<out T> : IBinaryFormatObject, IFormatObject<T>
    {

    }

    public class FormatObject<T> : IFormatObject<T> where T : class
    {
        public IFileFormat Format { get; }
        public string Extension => Format is IFileFormat<T> fmt ? fmt.GetExtension(Value) : Format.GetExtension(Value);
        public string MediaType => Format is IFileFormat<T> fmt ? fmt.GetMediaType(Value) : Format.GetMediaType(Value);
        public T Value { get; }

        public FormatObject(IFileFormat format, T value)
        {
            Format = format;
            Value = value;
        }

        public override string ToString()
        {
            return $"Media object ({MediaType ?? Extension ?? Format?.ToString()})";
        }

        TResult IFormatObject.GetValue<TResult, TArgs>(IResultFactory<TResult, TArgs> resultFactory, TArgs args)
        {
            return resultFactory.Invoke<T>(Value, args);
        }

        static readonly Regex dataRegex = new Regex(@"^(.*?)(?:;[^=]*)?,", RegexOptions.Compiled);
        static readonly char[] splitChar = { '/' };

        public Uri this[Uri value] {
            get {
                if(value.Scheme.Equals("data", StringComparison.OrdinalIgnoreCase))
                {
                    var type = MediaType?.ToLowerInvariant();
                    if(type == null || type.IndexOf('/') == -1) return null;
                    var builder = new UriBuilder(value);
                    var match = dataRegex.Match(builder.Path);
                    if(!match.Success) throw new ArgumentException(null, nameof(value));
                    builder.Path = type + builder.Path.Substring(match.Groups[1].Length);
                    return builder.Uri;
                }else if(value.Scheme.Equals("ni", StringComparison.OrdinalIgnoreCase))
                {
                    var type = MediaType?.ToLowerInvariant();
                    if(type == null || type.IndexOf('/') == -1) return null;
                    var builder = new UriBuilder(value);
                    var parameters = HttpUtility.ParseQueryString(builder.Query);
                    parameters["ct"] = type;
                    builder.Query = parameters.ToString();
                    return builder.Uri;
                }
                if(String.IsNullOrEmpty(value.Authority)) throw new ArgumentException(null, nameof(value));
                var sub = Extension?.ToLowerInvariant();
                if(sub == null)
                {
                    sub = MediaType?.ToLowerInvariant();
                    if(sub == null || sub.IndexOf('/') == -1) return null;
                    sub = sub.Split(splitChar)[1];
                    if(sub.StartsWith("prs.") || sub.StartsWith("vnd."))
                    {
                        sub = sub.Substring(4);
                    }else if(sub.StartsWith("x-"))
                    {
                        sub = sub.Substring(2);
                    }
                    int plus = sub.IndexOf('+');
                    if(plus != -1)
                    {
                        sub = sub.Substring(0, plus);
                    }
                }
                return new Uri(value.AbsoluteUri + "/" + sub);
            }
        }
    }

    public sealed class BinaryFormatObject<T> : FormatObject<T>, IBinaryFormatObject<T> where T : class
    {
        public IDataObject Data { get; }

        public new IBinaryFileFormat Format => (IBinaryFileFormat)base.Format;

        public BinaryFormatObject(IDataObject data, IBinaryFileFormat format, T value) : base(format, value)
        {
            Data = data;
        }
    }
}
