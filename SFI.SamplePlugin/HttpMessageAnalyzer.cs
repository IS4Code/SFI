using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using System;
using System.Net.Http.Headers;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of HTTP messages, as instances of <typeparamref name="TMessage"/>.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class HttpMessageAnalyzer<TMessage> : MediaObjectAnalyzer<TMessage> where TMessage : class, IDisposable
    {
        /// <inheritdoc cref="MediaObjectAnalyzer{T}.MediaObjectAnalyzer(Vocabulary.ClassUri[])"/>
        public HttpMessageAnalyzer(params ClassUri[] classes) : base(classes)
        {

        }

        /// <summary>
        /// Describes a collection of HTTP headers and assigns it to a node.
        /// </summary>
        /// <param name="node">The linked node to link the headers to.</param>
        /// <param name="nodeFactory">The <see cref="ILinkedNodeFactory"/> instance to use.</param>
        /// <param name="headers">The collection of HTTP headers.</param>
        protected void SetHttpHeaders(ILinkedNode node, ILinkedNodeFactory nodeFactory, HttpHeaders headers)
        {
            var headersRoot = node[Properties.HttpHeaders.Term];
            ILinkedNode? lastNode = null;
            int i = 0;
            foreach(var (field, values) in headers)
            {
                foreach(var value in values)
                {
                    var listNode = nodeFactory.CreateBlank();
                    if(lastNode == null)
                    {
                        node.Set(Properties.HttpHeaders, listNode);
                    }else{
                        lastNode.Set(Properties.Rest, listNode);
                    }
                
                    var headerNode = headersRoot[i.ToString()];
                    headerNode.SetClass(Classes.HttpMessageHeader);
                    headerNode.Set(Properties.HttpFieldName, field);
                    headerNode.Set(Properties.HttpHeaderName, Vocabularies.Httph, field.ToLowerInvariant());
                    headerNode.Set(Properties.HttpFieldValue, value);

                    node.Set(Properties.MessageHeader, headerNode);

                    listNode.Set(Properties.First, headerNode);

                    lastNode = listNode;
                    i++;
                }
            }
            if(lastNode != null)
            {
                lastNode.Set(Properties.Rest, Individuals.Nil);
            }
        }
    }
}
