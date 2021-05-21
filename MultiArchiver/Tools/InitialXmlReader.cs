using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace IS4.MultiArchiver.Tools
{
    public class InitialXmlReader : XmlReader
    {
        static readonly XmlReader InitialPrototype = XmlReader.Create(new StringReader(""));

        bool started;
        readonly XmlReader reader;

        XmlReader Reader => started ? reader : InitialPrototype;

        public InitialXmlReader(XmlReader reader)
        {
            this.reader = reader;
        }

        public override int AttributeCount => Reader.AttributeCount;

        public override string BaseURI => Reader.BaseURI;

        public override int Depth => Reader.Depth;

        public override bool EOF => Reader.EOF;

        public override bool IsEmptyElement => Reader.IsEmptyElement;

        public override string LocalName => Reader.LocalName;

        public override string NamespaceURI => Reader.NamespaceURI;

        public override XmlNameTable NameTable => reader.NameTable;

        public override XmlNodeType NodeType => Reader.NodeType;

        public override string Prefix => Reader.Prefix;

        public override ReadState ReadState => Reader.ReadState;

        public override string Value => Reader.Value;

        public override bool CanReadBinaryContent => reader.CanReadBinaryContent;

        public override bool CanReadValueChunk => reader.CanReadValueChunk;

        public override bool CanResolveEntity => reader.CanResolveEntity;

        public override bool HasAttributes => Reader.HasAttributes;

        public override bool HasValue => Reader.HasValue;

        public override bool IsDefault => Reader.IsDefault;

        public override IXmlSchemaInfo SchemaInfo => Reader.SchemaInfo;

        public override string Name => Reader.Name;

        public override char QuoteChar => reader.QuoteChar;

        public override XmlReaderSettings Settings => reader.Settings;

        public override Type ValueType => Reader.ValueType;

        public override string XmlLang => Reader.XmlLang;

        public override XmlSpace XmlSpace => Reader.XmlSpace;

        public override bool IsStartElement()
        {
            return Reader.IsStartElement();
        }

        public override bool IsStartElement(string localname, string ns)
        {
            return Reader.IsStartElement(localname, ns);
        }

        public override Task<string> GetValueAsync()
        {
            return Reader.GetValueAsync();
        }

        public override bool IsStartElement(string name)
        {
            return Reader.IsStartElement(name);
        }

        public override string GetAttribute(int i)
        {
            return Reader.GetAttribute(i);
        }

        public override string GetAttribute(string name)
        {
            return Reader.GetAttribute(name);
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            return Reader.GetAttribute(name, namespaceURI);
        }

        public override string LookupNamespace(string prefix)
        {
            return reader.LookupNamespace(prefix);
        }

        public override bool MoveToAttribute(string name)
        {
            started = true;
            return reader.MoveToAttribute(name);
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            started = true;
            return reader.MoveToAttribute(name, ns);
        }

        public override bool MoveToElement()
        {
            started = true;
            return reader.MoveToElement();
        }

        public override bool MoveToFirstAttribute()
        {
            started = true;
            return reader.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            started = true;
            return reader.MoveToNextAttribute();
        }

        public override void MoveToAttribute(int i)
        {
            started = true;
            reader.MoveToAttribute(i);
        }

        public override XmlNodeType MoveToContent()
        {
            started = true;
            return reader.MoveToContent();
        }

        public override Task<XmlNodeType> MoveToContentAsync()
        {
            started = true;
            return reader.MoveToContentAsync();
        }

        public override bool Read()
        {
            if(!started)
            {
                started = true;
                return true;
            }
            return reader.Read();
        }

        public override Task<bool> ReadAsync()
        {
            if(!started)
            {
                started = true;
                return Task.FromResult(true);
            }
            return reader.ReadAsync();
        }

        public override bool ReadAttributeValue()
        {
            return started ? reader.ReadAttributeValue() : false;
        }

        public override void ResolveEntity()
        {
            reader.ResolveEntity();
        }

        public override string ToString()
        {
            return Reader.ToString();
        }

        public override int GetHashCode()
        {
            return reader.GetHashCode();
        }
    }
}
