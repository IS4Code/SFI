using System;

namespace IS4.MultiArchiver.Vocabulary
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class UriAttribute : Attribute
    {
        public string Uri { get; }
        public Vocabularies Vocabulary { get; }
        public string LocalName { get; }

        public UriAttribute(string uri)
        {
            Uri = uri;
        }

        public UriAttribute(Vocabularies vocabulary, string localName = null)
        {
            Vocabulary = vocabulary;
            LocalName = localName;
        }
    }
}
