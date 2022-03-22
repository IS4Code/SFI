using System;
using System.Collections.Generic;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace IS4.MultiArchiver.Extensions
{
    public class TurtleHandler<TFormatter> : BaseRdfHandler where TFormatter : INodeFormatter, ITripleFormatter, IUriFormatter, INamespaceFormatter, IBaseUriFormatter
    {
        readonly TextWriter output;
        readonly TFormatter formatter;
        readonly INamespaceMapper namespaceMapper;
        readonly IEqualityComparer<Uri> uriComparer;

        string lastSubject;
        Uri lastBase;

        public TurtleHandler(TextWriter output, TFormatter formatter, INamespaceMapper namespaceMapper, IEqualityComparer<Uri> uriComparer = null)
        {
            this.output = output;
            this.formatter = formatter;
            this.namespaceMapper = namespaceMapper;
            this.uriComparer = uriComparer ?? new UriComparer();
        }

        public override bool AcceptsAll => true;

        private void EndSubject(string test)
        {
            if(lastSubject != test && lastSubject != null)
            {
                if(lastSubject != "@base" && lastSubject != "@prefix")
                {
                    output.WriteLine(" .");
                }
                output.WriteLine();
            }
            lastSubject = test;
        }

        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            EndSubject("@prefix");
            output.WriteLine(formatter.FormatNamespace(prefix, ResolveUri(namespaceUri)));
            namespaceMapper.AddNamespace(prefix, namespaceUri);
            return true;
        }

        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            if(lastBase == null || !uriComparer.Equals(baseUri, lastBase))
            {
                EndSubject("@base");
                output.WriteLine(formatter.FormatBaseUri(ResolveUri(baseUri)));
                lastBase = baseUri;
            }
            return true;
        }

        protected override void EndRdfInternal(bool ok)
        {
            base.EndRdfInternal(ok);
            EndSubject(null);
            output.Flush();
        }

        private Uri ResolveUri(Uri uri)
        {
            if(lastBase != null)
            {
                var relative = lastBase.MakeRelativeUri(uri);
                if(!relative.IsAbsoluteUri && uriComparer.Equals(uri, new Uri(lastBase, relative)))
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
                    output.Write($"{subj} a {obj}");
                }else{
                    output.WriteLine(subj);
                    output.Write($"  {pred} {obj}");
                }
                lastSubject = subj;
            }else{
                output.WriteLine(" ;");
                output.Write($"  {pred} {obj}");
            }
            return true;
        }

        class FakeUri : Uri
        {
            readonly string stringValue;

            public FakeUri(string realValue, string stringValue) : base(realValue)
            {
                this.stringValue = stringValue;
            }

            public override string ToString()
            {
                return stringValue;
            }
        }
    }
}
