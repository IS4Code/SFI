using System;
using System.Collections.Generic;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace IS4.SFI
{
    /// <summary>
    /// A custom RDF handler that writes Turtle output to a file.
    /// </summary>
    /// <typeparam name="TFormatter">
    /// The Turtle formatter used for nodes, usually <see cref="TurtleFormatter"/>.
    /// </typeparam>
    public class TurtleHandler<TFormatter> : BaseRdfHandler where TFormatter : INodeFormatter, ITripleFormatter, IUriFormatter, INamespaceFormatter, IBaseUriFormatter
    {
        readonly TextWriter output;
        readonly TFormatter formatter;
        readonly INamespaceMapper namespaceMapper;
        readonly IEqualityComparer<Uri> uriComparer;

        string? lastSubject;
        Uri? lastBase;

        /// <summary>
        /// Creates a new instance of the Turtle handler.
        /// </summary>
        /// <param name="output">The output writer to write Turtle to.</param>
        /// <param name="formatter">The formatter of Turtle nodes.</param>
        /// <param name="namespaceMapper">The namespace mapper to register namespaces to.</param>
        /// <param name="uriComparer">The comparer of <see cref="Uri"/> instances.</param>
        public TurtleHandler(TextWriter output, TFormatter formatter, INamespaceMapper namespaceMapper, IEqualityComparer<Uri>? uriComparer = null)
        {
            if(NodeFactory is NodeFactory nodeFactory)
            {
                nodeFactory.ValidateLanguageSpecifiers = false;
            }

            this.output = output;
            this.formatter = formatter;
            this.namespaceMapper = namespaceMapper;
            this.uriComparer = uriComparer ?? new UriComparer();
        }

        /// <inheritdoc/>
        public override bool AcceptsAll => true;

        private void EndSubject(string? test)
        {
            if(lastSubject != test && lastSubject != null)
            {
                if(lastSubject != "@base" && lastSubject != "@prefix")
                {
                    WriteLine(" .");
                }
                WriteLine();
            }
            lastSubject = test;
        }

        /// <inheritdoc/>
        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            EndSubject("@prefix");
            WriteLine(formatter.FormatNamespace(prefix, ResolveUri(namespaceUri)));
            namespaceMapper.AddNamespace(prefix, namespaceUri);
            return true;
        }

        /// <inheritdoc/>
        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            if(lastBase == null || !uriComparer.Equals(baseUri, lastBase))
            {
                EndSubject("@base");
                WriteLine(formatter.FormatBaseUri(ResolveUri(baseUri)));
                lastBase = baseUri;
            }
            return true;
        }

        /// <inheritdoc/>
        protected override void EndRdfInternal(bool ok)
        {
            base.EndRdfInternal(ok);
            EndSubject(null);
            Flush();
        }

        private Uri ResolveUri(Uri uri)
        {
            if(lastBase != null)
            {
                var relative = lastBase.MakeRelativeUri(uri);
                if(!relative.IsAbsoluteUri && uriComparer.Equals(uri, new Uri(lastBase, relative)) && relative.OriginalString.Length < uri.AbsoluteUri.Length)
                {
                    return new FakeUri(uri.AbsoluteUri, relative.OriginalString);
                }
            }
            return uri;
        }

        private INode ResolveNode(INode node)
        {
            if(lastBase != null && node is IUriNode uriNode)
            {
                return CreateUriNode(ResolveUri(uriNode.Uri));
            }
            return node;
        }

        /// <inheritdoc/>
        protected override bool HandleTripleInternal(Triple t)
        {
            var subj = formatter.Format(ResolveNode(t.Subject), TripleSegment.Subject).Trim();
            var pred = formatter.Format(ResolveNode(t.Predicate), TripleSegment.Predicate).Trim();
            var obj = formatter.Format(ResolveNode(t.Object), TripleSegment.Object).Trim();
            if(lastSubject != subj)
            {
                EndSubject(null);
                if(pred == "a")
                {
                    Write($"{subj} a {obj}");
                }else{
                    WriteLine(subj);
                    Write($"  {pred} {obj}");
                }
                lastSubject = subj;
            }else{
                WriteLine(" ;");
                Write($"  {pred} {obj}");
            }
            return true;
        }

        /// <inheritdoc/>
        protected override bool HandleQuadInternal(Triple t, IRefNode graph)
        {
            throw new NotSupportedException();
        }

        string linePart = "";
        void Write(string str)
        {
            linePart += str;
        }

        void WriteLine(string? str = null)
        {
            output.WriteLine(linePart + str);
            linePart = "";
        }

        void Flush()
        {
            output.Write(linePart);
            output.Flush();
            linePart = "";
        }

        /// <summary>
        /// A fake <see cref="Uri"/> that has a custom <see cref="ToString"/> value,
        /// to give to the <typeparamref name="TFormatter"/>.
        /// </summary>
        class FakeUri : Uri
        {
            readonly string stringValue;

            public FakeUri(string realValue, string stringValue) : base(realValue)
            {
                this.stringValue = stringValue;
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                return stringValue;
            }
        }
    }
}
