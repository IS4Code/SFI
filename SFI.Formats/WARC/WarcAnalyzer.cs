﻿using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Toimik.WarcProtocol;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// Analyzes Web ARChive records, as instances of <see cref="IAsyncEnumerable{T}"/>
    /// of <see cref="Record"/>.
    /// </summary>
    [Description("Analyzes Web ARChive records.")]
    public class WarcAnalyzer : MediaObjectAnalyzer<IAsyncEnumerable<Record>>
    {
        /// <inheritdoc/>
        public override async ValueTask<AnalysisResult> Analyze(IAsyncEnumerable<Record> records, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            string? label = null;
            var requests = new Dictionary<string, ILinkedNode>();
            var responses = new Dictionary<string, ILinkedNode>();
            var requestsOf = new Dictionary<string, HashSet<ILinkedNode>>();
            var responsesOf = new Dictionary<string, HashSet<ILinkedNode>>();
            await foreach(var record in records)
            {
                var id = record.Id.OriginalString;
                var itemNode = node[id];
                if(itemNode != null)
                {
                    switch(record)
                    {
                        case WarcinfoRecord info:
                            label = info.Filename;
                            break;
                        case RequestRecord request:
                            if(request.ContentType.StartsWith("application/http", StringComparison.OrdinalIgnoreCase))
                            {
                                LinkRequestResponse(
                                    itemNode,
                                    id, request.ConcurrentTos,
                                    requests, responses, requestsOf, responsesOf,
                                    (a, b) => b.Set(Properties.InReplyTo, a)
                                );

                                var (line, fields) = ParseHttp(request.RecordBlock);
                                itemNode.SetClass(Classes.HttpRequest);
                                itemNode.SetClass(Classes.Message);
                                itemNode.Set(Properties.MessageId, id);
                                itemNode.Set(Properties.Subject, UriFormatter.Instance, request.TargetUri);
                                itemNode.Set(Properties.MimeType, request.ContentType);
                                if(line != null)
                                {
                                    DescribeHttpRequest(itemNode, line, fields);
                                    DescribeHttpFields(itemNode, context.NodeFactory, fields);
                                }

                                LinkRequestResponse(
                                    itemNode,
                                    id, request.ConcurrentTos,
                                    requests, responses, requestsOf, responsesOf,
                                    (a, b) => a.Set(Properties.HttpResponse, b)
                                );

                                await DescribePayload(itemNode, request.Payload, request.PayloadDigest, context, analyzers);
                            }
                            break;
                        case ResponseRecord response:
                            if(response.ContentType.StartsWith("application/http", StringComparison.OrdinalIgnoreCase))
                            {
                                LinkRequestResponse(
                                    itemNode,
                                    id, response.ConcurrentTos,
                                    responses, requests, responsesOf, requestsOf,
                                    (a, b) => b.Set(Properties.HttpResponse, a)
                                );

                                var (line, fields) = ParseHttp(response.RecordBlock);
                                itemNode.SetClass(Classes.HttpResponse);
                                itemNode.SetClass(Classes.Message);
                                itemNode.Set(Properties.MessageId, id);
                                itemNode.Set(Properties.Subject, UriFormatter.Instance, response.TargetUri);
                                itemNode.Set(Properties.MimeType, response.ContentType);
                                if(line != null)
                                {
                                    DescribeHttpResponse(itemNode, line, fields);
                                    DescribeHttpFields(itemNode, context.NodeFactory, fields);
                                }

                                LinkRequestResponse(
                                    itemNode,
                                    id, response.ConcurrentTos,
                                    responses, requests, responsesOf, requestsOf,
                                    (a, b) => a.Set(Properties.InReplyTo, b)
                                );

                                await DescribePayload(itemNode, response.Payload, response.PayloadDigest, context, analyzers);
                            }
                            break;
                    }
                    itemNode.Set(Properties.Date, record.Date);
                    node.Set(Properties.HasPart, itemNode);
                }
            }
            return new AnalysisResult(node, label: label);
        }

        void LinkRequestResponse(ILinkedNode node, string id, IEnumerable<Uri> concurrent, Dictionary<string, ILinkedNode> primary, Dictionary<string, ILinkedNode> target, Dictionary<string, HashSet<ILinkedNode>> inversePrimary, Dictionary<string, HashSet<ILinkedNode>> inverseTarget, Action<ILinkedNode, ILinkedNode> setter)
        {
            primary[id] = node;
            foreach(var uri in concurrent)
            {
                if(target.TryGetValue(uri.OriginalString, out var concurrentItem))
                {
                    setter(node, concurrentItem);
                    continue;
                }
                if(!inversePrimary.TryGetValue(uri.OriginalString, out var set))
                {
                    set = inversePrimary[uri.OriginalString] = new(ReferenceEqualityComparer<ILinkedNode>.Default);
                }
                set.Add(node);
            }
            if(inverseTarget.TryGetValue(id, out var targetSet))
            {
                foreach(var targetNode in targetSet)
                {
                    setter(node, targetNode);
                }
            }
        }

        static readonly Regex fieldRegex = new(@"^([^:]*):\s*(.*)\s*$", RegexOptions.Compiled);

        (string? line, IEnumerator<KeyValuePair<string, string>> fields) ParseHttp(string record)
        {
            var reader = new LineReader(new StringReader(record));
            var line = reader.ReadLine();
            return (line, Inner());

            IEnumerator<KeyValuePair<string, string>> Inner()
            {
                string? line;
                while((line = reader.ReadLine()) != null)
                {
                    if(fieldRegex.Match(line) is { Success: true } match)
                    {
                        yield return new(match.Groups[1].Value, match.Groups[2].Value);
                    }
                }
            }
        }

        static readonly char[] whitespace = { ' ', '\t', '\v', '\r', '\f' };

        void DescribeHttpRequest(ILinkedNode node, string line, IEnumerator<KeyValuePair<string, string>> fields)
        {
            var split = line.Split(whitespace, 3, StringSplitOptions.RemoveEmptyEntries);
            if(split.Length == 3)
            {
                var method = split[0].ToUpperInvariant();
                node.Set(Properties.HttpMethodName, method);
                node.Set(Properties.HttpMethod, Vocabularies.Httpm, method);

                var target = split[1];
                if(Uri.IsWellFormedUriString(target, UriKind.Absolute))
                {
                    node.Set(Properties.HttpAbsoluteUri, target, Datatypes.AnyUri);
                }else if(target.StartsWith("/"))
                {
                    node.Set(Properties.HttpAbsolutePath, target, Datatypes.AnyUri);
                }else if(target.IndexOf("/", StringComparison.Ordinal) == -1 && Uri.IsWellFormedUriString("http://" + target, UriKind.Absolute))
                {
                    node.Set(Properties.HttpAuthority, target);
                }

                SetVersion(node, split[2]);
            }
        }

        void DescribeHttpResponse(ILinkedNode node, string line, IEnumerator<KeyValuePair<string, string>> fields)
        {
            var split = line.Split(whitespace, 3, StringSplitOptions.RemoveEmptyEntries);
            if(split.Length >= 2)
            {
                SetVersion(node, split[0]);
                if(Enum.TryParse<HttpStatusCode>(split[1], out var code))
                {
                    node.Set(Properties.HttpStatusCodeValue, (int)code);
                    var codeString = code.ToString();
                    if(!Int32.TryParse(codeString, out _))
                    {
                        node.Set(Properties.HttpStatusCode, Vocabularies.Httpsc, codeString);
                    }
                }
                if(split.Length >= 3)
                {
                    node.Set(Properties.HttpReasonPhrase, split[2]);
                }
            }
        }

        async ValueTask DescribePayload(ILinkedNode node, byte[] payload, string? digest, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            if(payload.Length > 0 || digest != null)
            {
                await analyzers.Analyze(payload, context.WithParentLink(node, Properties.HttpBody));
            }
        }

        static readonly char[] slash = { '/' };

        void SetVersion(ILinkedNode node, string version)
        {
            var split = version.Split(slash, 2);
            if(split.Length == 2)
            {
                node.Set(Properties.HttpVersion, split[1]);
            }
        }

        void DescribeHttpFields(ILinkedNode node, ILinkedNodeFactory nodeFactory, IEnumerator<KeyValuePair<string, string>> fields)
        {
            var headers = node[Properties.HttpHeaders.Term];
            ILinkedNode? lastNode = null;
            int i = 0;
            while(fields.MoveNext())
            {
                var (field, value) = fields.Current;
                
                var listNode = nodeFactory.CreateBlank();
                if(lastNode == null)
                {
                    node.Set(Properties.HttpHeaders, listNode);
                }else{
                    lastNode.Set(Properties.Rest, listNode);
                }
                
                var headerNode = headers[i.ToString()];
                headerNode.SetClass(Classes.HttpMessageHeader);
                headerNode.Set(Properties.HttpFieldName, field);
                headerNode.Set(Properties.HttpHeaderName, Vocabularies.Httph, field.ToLowerInvariant());
                headerNode.Set(Properties.HttpFieldValue, value);

                node.Set(Properties.MessageHeader, headerNode);

                listNode.Set(Properties.First, headerNode);

                lastNode = listNode;
                i++;
            }
            if(lastNode != null)
            {
                lastNode.Set(Properties.Rest, Individuals.Nil);
            }
        }

        static readonly Regex foldWhitespace = new("^[ \t]+", RegexOptions.Compiled);

        class LineReader
        {
            readonly TextReader reader;
            string? lineBuffer;

            public LineReader(TextReader reader)
            {
                this.reader = reader;
            }

            public string? ReadLine()
            {
                var line = lineBuffer ?? reader.ReadLine();
                if(line == null)
                {
                    return null;
                }
                lineBuffer = null;
                StringBuilder? sb = null;
                while(true)
                {
                    var line2 = reader.ReadLine();
                    if(line2 == null)
                    {
                        return line ?? sb?.ToString();
                    }
                    if(foldWhitespace.Match(line2) is { Success: true } match)
                    {
                        if(sb == null)
                        {
                            sb = new StringBuilder();
                            sb.Append(line);
                            line = null;
                        }
                        sb.Append(' ');
                        sb.Append(line2.Substring(match.Length));
                    }
                    lineBuffer = line2;
                    return line ?? sb?.ToString();
                }
            }
        }
    }
}
