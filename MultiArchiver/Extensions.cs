using IS4.MultiArchiver.Services;
using Microsoft.CSharp.RuntimeBinder;
using VDS.RDF;

namespace IS4.MultiArchiver
{
    internal static class Extensions
    {
        public static void HandleTriple(this IRdfHandler handler, INode subj, INode pred, INode obj)
        {
            handler.HandleTriple(new Triple(subj, pred, obj));
        }

        public static IUriNode TryAnalyze(this IEntityAnalyzer analyzer, object value, IRdfHandler handler)
        {
            if(value == null) return null;
            try
            {
                return analyzer.Analyze((dynamic)value, handler);
            }catch(RuntimeBinderException)
            {
                return null;
            }
        }
    }
}
