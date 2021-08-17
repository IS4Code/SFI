using System;

namespace IS4.MultiArchiver.Vocabulary
{
    public static class Graphs
    {
        [Uri(Vocabularies.A)]
        public static readonly GraphUri Meta;

        static Graphs()
        {
            typeof(Graphs).InitializeUris();
        }
    }
}
