using VDS.RDF;

namespace IS4.MultiArchiver
{
    internal static class Extensions
    {
        public static void HandleTriple(this IRdfHandler handler, INode subj, INode pred, INode obj)
        {
            handler.HandleTriple(new Triple(subj, pred, obj));
        }
    }
}
