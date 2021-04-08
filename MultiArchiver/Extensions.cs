using IS4.MultiArchiver.Services;
using Microsoft.CSharp.RuntimeBinder;
using VDS.RDF;

namespace IS4.MultiArchiver
{
    internal static class Extensions
    {
        public static void HandleTriple(this IRdfHandler handler, VDS.RDF.INode subj, VDS.RDF.INode pred, VDS.RDF.INode obj)
        {
            handler.HandleTriple(new Triple(subj, pred, obj));
        }

        public static ILinkedNode TryAnalyze(this ILinkedNodeFactory analyzer, object value)
        {
            if(value == null) return null;
            try
            {
                return analyzer.Create((dynamic)value);
            }catch(RuntimeBinderException)
            {
                return null;
            }
        }
    }
}
