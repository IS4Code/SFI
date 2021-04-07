using System;

namespace IS4.MultiArchiver.Vocabulary
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class UriAttribute : Attribute
    {
        public string Vocabulary { get; }
        public string LocalName { get; }

        public UriAttribute(string vocabulary, string localName = null)
        {
            Vocabulary = vocabulary;
            LocalName = localName;
        }
    }
}
