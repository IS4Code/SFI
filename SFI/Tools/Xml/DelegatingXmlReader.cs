using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace IS4.SFI.Tools.Xml
{
    /// <summary>
    /// An implementation of <see cref="XmlReader"/>, <see cref="IXmlLineInfo"/>,
    /// and <see cref="IXmlNamespaceResolver"/> that delegates all calls
    /// to a series of other readers to specific categories of tasks.
    /// </summary>
    public abstract class DelegatingXmlReader : XmlReader, IXmlLineInfo, IXmlNamespaceResolver
    {
        /// <summary>
        /// <see langword="true"/> if <see cref="Close"/> has been called.
        /// </summary>
        protected bool Closed { get; private set; }

        /// <summary>
        /// The reader used to provide information about the current node
        /// or position in the document.
        /// </summary>
        protected abstract XmlReader? ScopeReader { get; }

        /// <summary>
        /// The reader used to query information about the current node,
        /// such as the attributes of the current element.
        /// </summary>
        protected abstract XmlReader? QueryReader { get; }

        /// <summary>
        /// The reader used for global information about the document.
        /// </summary>
        protected abstract XmlReader? GlobalReader { get; }

        /// <summary>
        /// The reader used to move in the XML data.
        /// </summary>
        protected abstract XmlReader? ActiveReader { get; }

        /// <summary>
        /// The reader used for inquiries about general capabilities.
        /// </summary>
        protected abstract XmlReader? PassiveReader { get; }

        private XmlReader ScopeReaderNotNull => ScopeReader ?? throw new NotSupportedException();
        private XmlReader QueryReaderNotNull => QueryReader ?? throw new NotSupportedException();
        private XmlReader GlobalReaderNotNull => GlobalReader ?? throw new NotSupportedException();
        private XmlReader ActiveReaderNotNull => ActiveReader ?? throw new NotSupportedException();
        private XmlReader PassiveReaderNotNull => PassiveReader ?? throw new NotSupportedException();

        /// <inheritdoc/>
        public override int AttributeCount => ScopeReaderNotNull.AttributeCount;

        /// <inheritdoc/>
        public override string BaseURI => ScopeReaderNotNull.BaseURI;

        /// <inheritdoc/>
        public override bool CanReadBinaryContent {
            get {
                if(PassiveReader is not XmlReader reader) return base.CanReadBinaryContent;
                return reader.CanReadBinaryContent;
            }
        }

        /// <inheritdoc/>
        public override bool CanReadValueChunk {
            get {
                if(PassiveReader is not XmlReader reader) return base.CanReadValueChunk;
                return reader.CanReadValueChunk;
            }
        }

        /// <inheritdoc/>
        public override bool CanResolveEntity {
            get {
                if(ScopeReader is not XmlReader reader) return base.CanResolveEntity;
                return reader.CanResolveEntity;
            }
        }

        /// <inheritdoc/>
        public override int Depth => ScopeReaderNotNull.Depth;

        /// <inheritdoc/>
        public override bool EOF => ScopeReaderNotNull.EOF;

        /// <inheritdoc/>
        public override bool HasAttributes {
            get {
                if(ScopeReader is not XmlReader reader) return base.HasAttributes;
                return reader.HasAttributes;
            }
        }

        /// <inheritdoc/>
        public override bool HasValue {
            get {
                if(ScopeReader is not XmlReader reader) return base.HasValue;
                return reader.HasValue;
            }
        }

        /// <inheritdoc/>
        public override bool IsDefault {
            get {
                if(ScopeReader is not XmlReader reader) return base.IsDefault;
                return reader.IsDefault;
            }
        }

        /// <inheritdoc/>
        public override bool IsEmptyElement => ScopeReaderNotNull.IsEmptyElement;

        /// <inheritdoc/>
        public override string LocalName => ScopeReaderNotNull.LocalName;

        /// <inheritdoc/>
        public override string Name {
            get {
                if(ScopeReader is not XmlReader reader) return base.Name;
                return reader.Name;
            }
        }

        /// <inheritdoc/>
        public override string NamespaceURI => ScopeReaderNotNull.NamespaceURI;

        /// <inheritdoc/>
        public override XmlNameTable NameTable => GlobalReaderNotNull.NameTable;

        /// <inheritdoc/>
        public override XmlNodeType NodeType => ScopeReaderNotNull.NodeType;

        /// <inheritdoc/>
        public override string Prefix => ScopeReaderNotNull.Prefix;

        /// <inheritdoc/>
        public override char QuoteChar {
            get {
                if(ScopeReader is not XmlReader reader) return base.QuoteChar;
                return reader.QuoteChar;
            }
        }

        /// <inheritdoc/>
        public override ReadState ReadState => Closed ? ReadState.Closed : ScopeReaderNotNull.ReadState;

        /// <inheritdoc/>
        public override IXmlSchemaInfo SchemaInfo {
            get {
                if(ScopeReader is not XmlReader reader) return base.SchemaInfo;
                return reader.SchemaInfo;
            }
        }

        /// <inheritdoc/>
        public override XmlReaderSettings Settings {
            get {
                if(GlobalReader is not XmlReader reader) return base.Settings;
                return reader.Settings;
            }
        }

        /// <inheritdoc/>
        public override string Value => ScopeReaderNotNull.Value;

        /// <inheritdoc/>
        public override Type ValueType {
            get {
                if(ScopeReader is not XmlReader reader) return base.ValueType;
                return reader.ValueType;
            }
        }

        /// <inheritdoc/>
        public override string XmlLang {
            get {
                if(ScopeReader is not XmlReader reader) return base.XmlLang;
                return reader.XmlLang;
            }
        }

        /// <inheritdoc/>
        public override XmlSpace XmlSpace {
            get {
                if(ScopeReader is not XmlReader reader) return base.XmlSpace;
                return reader.XmlSpace;
            }
        }

        /// <inheritdoc/>
        public override void Close()
        {
            base.Close();

            Closed = true;
        }

        /// <inheritdoc/>
        public override string GetAttribute(int i)
        {
            return QueryReaderNotNull.GetAttribute(i);
        }

        /// <inheritdoc/>
        public override string GetAttribute(string name)
        {
            return QueryReaderNotNull.GetAttribute(name);
        }

        /// <inheritdoc/>
        public override string GetAttribute(string name, string namespaceURI)
        {
            return QueryReaderNotNull.GetAttribute(name, namespaceURI);
        }

        /// <inheritdoc/>
        public override Task<string> GetValueAsync()
        {
            if(QueryReader is not XmlReader reader) return base.GetValueAsync();
            return reader.GetValueAsync();
        }

        /// <inheritdoc/>
        public override bool IsStartElement()
        {
            if(QueryReader is not XmlReader reader) return base.IsStartElement();
            return reader.IsStartElement();
        }

        /// <inheritdoc/>
        public override bool IsStartElement(string name)
        {
            if(QueryReader is not XmlReader reader) return base.IsStartElement(name);
            return reader.IsStartElement(name);
        }

        /// <inheritdoc/>
        public override bool IsStartElement(string localname, string ns)
        {
            if(QueryReader is not XmlReader reader) return base.IsStartElement(localname, ns);
            return reader.IsStartElement(localname, ns);
        }

        /// <inheritdoc/>
        public override string LookupNamespace(string prefix)
        {
            return QueryReaderNotNull.LookupNamespace(prefix);
        }

        /// <inheritdoc/>
        public override void MoveToAttribute(int i)
        {
            if(ActiveReader is not XmlReader reader)
            {
                base.MoveToAttribute(i);
                return;
            }
            reader.MoveToAttribute(i);
        }

        /// <inheritdoc/>
        public override bool MoveToAttribute(string name)
        {
            if(ActiveReader is not XmlReader reader)
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

        /// <inheritdoc/>
        public override bool MoveToAttribute(string name, string ns)
        {
            if(ActiveReader is not XmlReader reader)
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

        /// <inheritdoc/>
        public override XmlNodeType MoveToContent()
        {
            if(ActiveReader is not XmlReader reader) return base.MoveToContent();
            return reader.MoveToContent();
        }

        /// <inheritdoc/>
        public override Task<XmlNodeType> MoveToContentAsync()
        {
            if(ActiveReader is not XmlReader reader) return base.MoveToContentAsync();
            return reader.MoveToContentAsync();
        }

        /// <inheritdoc/>
        public override bool MoveToElement()
        {
            return ActiveReaderNotNull.MoveToElement();
        }

        /// <inheritdoc/>
        public override bool MoveToFirstAttribute()
        {
            return ActiveReaderNotNull.MoveToFirstAttribute();
        }

        /// <inheritdoc/>
        public override bool MoveToNextAttribute()
        {
            return ActiveReaderNotNull.MoveToNextAttribute();
        }

        /// <inheritdoc/>
        public override bool Read()
        {
            return ActiveReaderNotNull.Read();
        }

        /// <inheritdoc/>
        public override Task<bool> ReadAsync()
        {
            if(ActiveReader is not XmlReader reader) return base.ReadAsync();
            return reader.ReadAsync();
        }

        /// <inheritdoc/>
        public override bool ReadAttributeValue()
        {
            return ActiveReaderNotNull.ReadAttributeValue();
        }

        /// <inheritdoc/>
        public override void ResolveEntity()
        {
            QueryReaderNotNull.ResolveEntity();
        }

        /// <inheritdoc/>
        public override void Skip()
        {
            if(ActiveReader is not XmlReader reader)
            {
                base.Skip();
                return;
            }
            reader.Skip();
        }

        /// <inheritdoc/>
        public override Task SkipAsync()
        {
            if(ActiveReader is not XmlReader reader) return base.SkipAsync();
            return reader.SkipAsync();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if(ScopeReader is not XmlReader reader) return base.ToString();
            return reader.ToString();
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            if(GlobalReader is not XmlReader reader) return base.GetHashCode();
            return reader.GetHashCode();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                foreach(var inst in (new[] { ScopeReader, QueryReader, GlobalReader, PassiveReader }).Distinct(ReferenceEqualityComparer<XmlReader?>.Default))
                {
                    inst?.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public virtual int LineNumber {
            get {
                if(ScopeReader is not IXmlLineInfo info) return 0;
                return info.LineNumber;
            }
        }

        /// <inheritdoc/>
        public virtual int LinePosition {
            get {
                if(ScopeReader is not IXmlLineInfo info) return 0;
                return info.LinePosition;
            }
        }

        /// <inheritdoc/>
        public virtual bool HasLineInfo()
        {
            if(ScopeReader is not IXmlLineInfo info) return false;
            return info.HasLineInfo();
        }

        /// <inheritdoc/>
        public virtual IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
        {
            if(QueryReader is not IXmlNamespaceResolver resolver) throw new NotSupportedException();
            return resolver.GetNamespacesInScope(scope);
        }

        /// <inheritdoc/>
        public virtual string LookupPrefix(string namespaceName)
        {
            if(QueryReader is not IXmlNamespaceResolver resolver) throw new NotSupportedException();
            return resolver.LookupPrefix(namespaceName);
        }
    }
}
