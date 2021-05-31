using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace IS4.MultiArchiver.Tools.Xml
{
    public abstract class DelegatingXmlReader : XmlReader
    {
        protected abstract XmlReader ScopeReader { get; }
        protected abstract XmlReader QueryReader { get; }
        protected abstract XmlReader GlobalReader { get; }
        protected abstract XmlReader ActiveReader { get; }
        protected abstract XmlReader PassiveReader { get; }

        private XmlReader ScopeReaderNotNull => ScopeReader ?? throw new NotSupportedException();
        private XmlReader QueryReaderNotNull => QueryReader ?? throw new NotSupportedException();
        private XmlReader GlobalReaderNotNull => GlobalReader ?? throw new NotSupportedException();
        private XmlReader ActiveReaderNotNull => ActiveReader ?? throw new NotSupportedException();
        private XmlReader PassiveReaderNotNull => PassiveReader ?? throw new NotSupportedException();

        public DelegatingXmlReader()
        {

        }

        public override int AttributeCount => ScopeReaderNotNull.AttributeCount;

        public override string BaseURI => ScopeReaderNotNull.BaseURI;

        public override bool CanReadBinaryContent {
            get {
                if(!(PassiveReader is XmlReader reader)) return base.CanReadBinaryContent;
                return reader.CanReadBinaryContent;
            }
        }

        public override bool CanReadValueChunk {
            get {
                if(!(PassiveReader is XmlReader reader)) return base.CanReadValueChunk;
                return reader.CanReadValueChunk;
            }
        }

        public override bool CanResolveEntity {
            get {
                if(!(ScopeReader is XmlReader reader)) return base.CanResolveEntity;
                return reader.CanResolveEntity;
            }
        }

        public override int Depth => ScopeReaderNotNull.Depth;

        public override bool EOF => ScopeReaderNotNull.EOF;

        public override bool HasAttributes {
            get {
                if(!(ScopeReader is XmlReader reader)) return base.HasAttributes;
                return reader.HasAttributes;
            }
        }

        public override bool HasValue {
            get {
                if(!(ScopeReader is XmlReader reader)) return base.HasValue;
                return reader.HasValue;
            }
        }

        public override bool IsDefault {
            get {
                if(!(ScopeReader is XmlReader reader)) return base.IsDefault;
                return reader.IsDefault;
            }
        }

        public override bool IsEmptyElement => ScopeReaderNotNull.IsEmptyElement;

        public override string LocalName => ScopeReaderNotNull.LocalName;

        public override string Name {
            get {
                if(!(ScopeReader is XmlReader reader)) return base.Name;
                return reader.Name;
            }
        }

        public override string NamespaceURI => ScopeReaderNotNull.NamespaceURI;

        public override XmlNameTable NameTable => GlobalReaderNotNull.NameTable;

        public override XmlNodeType NodeType => ScopeReaderNotNull.NodeType;

        public override string Prefix => ScopeReaderNotNull.Prefix;

        public override char QuoteChar {
            get {
                if(!(GlobalReader is XmlReader reader)) return base.QuoteChar;
                return reader.QuoteChar;
            }
        }

        public override ReadState ReadState => ScopeReaderNotNull.ReadState;

        public override IXmlSchemaInfo SchemaInfo {
            get {
                if(!(ScopeReader is XmlReader reader)) return base.SchemaInfo;
                return reader.SchemaInfo;
            }
        }

        public override XmlReaderSettings Settings {
            get {
                if(!(GlobalReader is XmlReader reader)) return base.Settings;
                return reader.Settings;
            }
        }

        public override string Value => ScopeReaderNotNull.Value;

        public override Type ValueType {
            get {
                if(!(ScopeReader is XmlReader reader)) return base.ValueType;
                return reader.ValueType;
            }
        }

        public override string XmlLang {
            get {
                if(!(ScopeReader is XmlReader reader)) return base.XmlLang;
                return reader.XmlLang;
            }
        }

        public override XmlSpace XmlSpace {
            get {
                if(!(ScopeReader is XmlReader reader)) return base.XmlSpace;
                return reader.XmlSpace;
            }
        }

        public override string GetAttribute(int i)
        {
            return QueryReaderNotNull.GetAttribute(i);
        }

        public override string GetAttribute(string name)
        {
            return QueryReaderNotNull.GetAttribute(name);
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            return QueryReaderNotNull.GetAttribute(name, namespaceURI);
        }

        public override Task<string> GetValueAsync()
        {
            if(!(QueryReader is XmlReader reader)) return base.GetValueAsync();
            return reader.GetValueAsync();
        }

        public override bool IsStartElement()
        {
            if(!(QueryReader is XmlReader reader)) return base.IsStartElement();
            return reader.IsStartElement();
        }

        public override bool IsStartElement(string name)
        {
            if(!(QueryReader is XmlReader reader)) return base.IsStartElement(name);
            return reader.IsStartElement(name);
        }

        public override bool IsStartElement(string localname, string ns)
        {
            if(!(QueryReader is XmlReader reader)) return base.IsStartElement(localname, ns);
            return reader.IsStartElement(localname, ns);
        }

        public override string LookupNamespace(string prefix)
        {
            return QueryReaderNotNull.LookupNamespace(prefix);
        }

        public override void MoveToAttribute(int i)
        {
            if(!(ActiveReader is XmlReader reader))
            {
                base.MoveToAttribute(i);
                return;
            }
            reader.MoveToAttribute(i);
        }

        public override bool MoveToAttribute(string name)
        {
            if(!(ActiveReader is XmlReader reader))
            {
                if(!MoveToElement()) return false;
                while(MoveToNextAttribute())
                {
                    if(Name == name)
                    {
                        return true;
                    }
                }
                return false;
            }
            return reader.MoveToAttribute(name);
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            if(!(ActiveReader is XmlReader reader))
            {
                if(!MoveToElement()) return false;
                while(MoveToNextAttribute())
                {
                    if(LocalName == name && NamespaceURI == ns)
                    {
                        return true;
                    }
                }
                return false;
            }
            return reader.MoveToAttribute(name, ns);
        }

        public override XmlNodeType MoveToContent()
        {
            if(!(ActiveReader is XmlReader reader)) return base.MoveToContent();
            return reader.MoveToContent();
        }

        public override Task<XmlNodeType> MoveToContentAsync()
        {
            if(!(ActiveReader is XmlReader reader)) return base.MoveToContentAsync();
            return reader.MoveToContentAsync();
        }

        public override bool MoveToElement()
        {
            return ActiveReaderNotNull.MoveToElement();
        }

        public override bool MoveToFirstAttribute()
        {
            return ActiveReaderNotNull.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            return ActiveReaderNotNull.MoveToNextAttribute();
        }

        public override bool Read()
        {
            return ActiveReaderNotNull.Read();
        }

        public override Task<bool> ReadAsync()
        {
            if(!(ActiveReader is XmlReader reader)) return base.ReadAsync();
            return reader.ReadAsync();
        }

        public override bool ReadAttributeValue()
        {
            return ActiveReaderNotNull.ReadAttributeValue();
        }

        public override void ResolveEntity()
        {
            QueryReaderNotNull.ResolveEntity();
        }

        public override void Skip()
        {
            if(!(ActiveReader is XmlReader reader))
            {
                base.Skip();
                return;
            }
            reader.Skip();
        }

        public override Task SkipAsync()
        {
            if(!(ActiveReader is XmlReader reader)) return base.SkipAsync();
            return reader.SkipAsync();
        }

        public override string ToString()
        {
            if(!(ScopeReader is XmlReader reader)) return base.ToString();
            return reader.ToString();
        }

        public override int GetHashCode()
        {
            if(!(GlobalReader is XmlReader reader)) return base.GetHashCode();
            return reader.GetHashCode();
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                foreach(var inst in (new[] { ScopeReader, QueryReader, GlobalReader, PassiveReader }).Distinct(ReferenceEqualityComparer<XmlReader>.Default))
                {
                    inst?.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        class ReferenceEqualityComparer<T> : EqualityComparer<T> where T : class
        {
            public new static readonly IEqualityComparer<T> Default = new ReferenceEqualityComparer<T>();
            
            public override bool Equals(T x, T y)
            {
                return Object.ReferenceEquals(x, y);
            }

            public override int GetHashCode(T obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}
