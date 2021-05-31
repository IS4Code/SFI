using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace IS4.MultiArchiver.Tools.Xml
{
    public class InitialXmlReader : DelegatingXmlReader
    {
        static readonly XmlReader InitialPrototype = XmlReader.Create(new StringReader(""));

        bool started;

        protected override XmlReader GlobalReader { get; }

        protected override XmlReader ScopeReader => started ? GlobalReader : InitialPrototype;

        protected override XmlReader QueryReader => ScopeReader;

        protected override XmlReader ActiveReader {
            get {
                started = true;
                return GlobalReader;
            }
        }

        protected override XmlReader PassiveReader => GlobalReader;

        public InitialXmlReader(XmlReader reader)
        {
            GlobalReader = reader;
        }

        public override bool Read()
        {
            if(!started)
            {
                started = true;
                return true;
            }
            return GlobalReader.Read();
        }

        public override Task<bool> ReadAsync()
        {
            if(!started)
            {
                started = true;
                return Task.FromResult(true);
            }
            return GlobalReader.ReadAsync();
        }
    }
}
