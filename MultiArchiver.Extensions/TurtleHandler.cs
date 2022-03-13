using System;
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

        string lastSubject;

        public TurtleHandler(TextWriter output, TFormatter formatter, INamespaceMapper namespaceMapper)
        {
            this.output = output;
            this.formatter = formatter;
            this.namespaceMapper = namespaceMapper;
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
            output.WriteLine(formatter.FormatNamespace(prefix, namespaceUri));
            namespaceMapper.AddNamespace(prefix, namespaceUri);
            return true;
        }

        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            EndSubject("@base");
            output.WriteLine(formatter.FormatBaseUri(baseUri));
            return true;
        }

        protected override void EndRdfInternal(bool ok)
        {
            base.EndRdfInternal(ok);
            EndSubject(null);
            output.Flush();
        }

        protected override bool HandleTripleInternal(Triple t)
        {
            var subj = formatter.Format(t.Subject, TripleSegment.Subject).Trim();
            var pred = formatter.Format(t.Predicate, TripleSegment.Predicate).Trim();
            var obj = formatter.Format(t.Object, TripleSegment.Object).Trim();
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
    }
}
