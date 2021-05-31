using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace IS4.MultiArchiver.Tools.Xml
{
    public sealed class XmlReaderState : XmlReader, IXmlSchemaInfo, IXmlLineInfo
    {
        public override int AttributeCount { get; }

        public override string BaseURI { get; }

        public override int Depth { get; }

        public override bool EOF { get; }

        public override bool IsEmptyElement { get; }

        public override string LocalName { get; }

        public override string NamespaceURI { get; }

        public override XmlNodeType NodeType { get; }

        public override string Prefix { get; }

        public override ReadState ReadState { get; }

        public override string Value { get; }

        public override bool CanReadBinaryContent => false;

        public override bool CanReadValueChunk => false;

        public override bool CanResolveEntity => false;

        public override bool HasAttributes { get; }

        public override bool HasValue { get; }

        public override bool IsDefault { get; }

        public override string Name { get; }

        public override Type ValueType { get; }

        public override string XmlLang { get; }

        public override XmlSpace XmlSpace { get; }

        public override XmlNameTable NameTable { get; }

        public override IXmlSchemaInfo SchemaInfo => this;

        public bool IsNil { get; }

        public XmlSchemaSimpleType MemberType { get; }

        public XmlSchemaAttribute SchemaAttribute { get; }

        public XmlSchemaElement SchemaElement { get; }

        public XmlSchemaType SchemaType { get; }

        public XmlSchemaValidity Validity { get; }

        public IReadOnlyList<XmlReaderState> Attributes { get; }

        public IReadOnlyList<XmlReaderState> AttributeContents { get; }

        public IReadOnlyDictionary<string, string> NamespaceMap { get; }

        public int LineNumber { get; }

        public int LinePosition { get; }

        public bool HasLineInfo { get; }

        public XmlReaderState(XmlReader reader, IReadOnlyDictionary<string, string> namespaceMap = null) : this(reader, namespaceMap, null)
        {

        }

        private XmlReaderState(XmlReader reader, IReadOnlyDictionary<string, string> namespaceMap, IReadOnlyList<XmlReaderState> parentAttributes)
        {
            AttributeCount = reader.AttributeCount;
            BaseURI = reader.BaseURI;
            Depth = reader.Depth;
            EOF = reader.EOF;
            IsEmptyElement = reader.IsEmptyElement;
            LocalName = reader.LocalName;
            NamespaceURI = reader.NamespaceURI;
            NodeType = reader.NodeType;
            Prefix = reader.Prefix;
            ReadState = reader.ReadState;
            Value = reader.Value;
            HasAttributes = reader.HasAttributes;
            HasValue = reader.HasValue;
            IsDefault = reader.IsDefault;
            Name = reader.Name;
            ValueType = reader.ValueType;
            XmlLang = reader.XmlLang;
            XmlSpace = reader.XmlSpace;
            NameTable = reader.NameTable;

            if(reader.SchemaInfo is IXmlSchemaInfo schemaInfo)
            {
                IsNil = schemaInfo.IsNil;
                MemberType = schemaInfo.MemberType;
                SchemaAttribute = schemaInfo.SchemaAttribute;
                SchemaElement = schemaInfo.SchemaElement;
                SchemaType = schemaInfo.SchemaType;
                Validity = schemaInfo.Validity;
            }

            if(reader is IXmlLineInfo lineInfo)
            {
                HasLineInfo = lineInfo.HasLineInfo();
                LineNumber = lineInfo.LineNumber;
                LinePosition = lineInfo.LinePosition;
            }

            if(parentAttributes == null)
            {
                ImmutableDictionary<string, string>.Builder nsBuilder;
                switch(namespaceMap)
                {
                    case null:
                        nsBuilder = ImmutableDictionary.CreateBuilder<string, string>();
                        break;
                    case ImmutableDictionary<string, string> imDict:
                        nsBuilder = imDict.ToBuilder();
                        break;
                    case ImmutableDictionary<string, string>.Builder imBuilder:
                        nsBuilder = imBuilder.ToImmutable().ToBuilder();
                        break;
                    default:
                        nsBuilder = namespaceMap.ToImmutableDictionary().ToBuilder();
                        break;
                }

                var attrs = new List<XmlReaderState>();
                if(HasAttributes)
                {
                    while(reader.MoveToNextAttribute())
                    {
                        attrs.Add(new XmlReaderState(reader, nsBuilder, attrs));
                        if(reader.Prefix == "xmlns")
                        {
                            nsBuilder[reader.LocalName] = reader.Value;
                        }
                    }
                    if(!reader.MoveToElement())
                    {
                        throw new InvalidOperationException();
                    }
                }
                Attributes = attrs;

                NamespaceMap = nsBuilder.ToImmutable();
                AttributeContents = Array.Empty<XmlReaderState>();
            }else{
                Attributes = parentAttributes;

                if(NodeType == XmlNodeType.Attribute)
                {
                    var cnts = new List<XmlReaderState>();

                    while(reader.ReadAttributeValue())
                    {
                        cnts.Add(new XmlReaderState(reader, namespaceMap, parentAttributes));
                    }

                    AttributeContents = cnts;
                }else{
                    AttributeContents = Array.Empty<XmlReaderState>();
                }

                NamespaceMap = namespaceMap;
            }
        }

        public override string GetAttribute(int i)
        {
            if(i < 0 || i >= Attributes.Count) throw new ArgumentOutOfRangeException(nameof(i));
            return Attributes[i].Value;
        }

        public override string GetAttribute(string name)
        {
            return Attributes.FirstOrDefault(a => a.Name == name)?.Value;
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            return Attributes.FirstOrDefault(a => a.LocalName == name && a.NamespaceURI == namespaceURI)?.Value;
        }

        public override string LookupNamespace(string prefix)
        {
            return NamespaceMap.TryGetValue(prefix, out var value) ? value : null;
        }

        public override bool MoveToAttribute(string name)
        {
            throw new NotSupportedException();
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            throw new NotSupportedException();
        }

        public override bool MoveToElement()
        {
            throw new NotSupportedException();
        }

        public override bool MoveToFirstAttribute()
        {
            throw new NotSupportedException();
        }

        public override bool MoveToNextAttribute()
        {
            throw new NotSupportedException();
        }

        public override bool Read()
        {
            throw new NotSupportedException();
        }

        public override bool ReadAttributeValue()
        {
            throw new NotSupportedException();
        }

        public override void ResolveEntity()
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<XmlReaderState> ReadFrom(XmlReader reader)
        {
            var namespaceFrames = new Stack<IReadOnlyDictionary<string, string>>();
            int depth = -1;
            IReadOnlyDictionary<string, string> namespaceMap = null;

            while(reader.Read())
            {
                if(reader.Depth > depth)
                {
                    namespaceFrames.Push(namespaceMap);
                }else if(reader.Depth <= depth)
                {
                    namespaceMap = namespaceFrames.Peek();
                }
                depth = reader.Depth;

                var state = new XmlReaderState(reader, namespaceMap);
                yield return state;

                namespaceMap = state.NamespaceMap;
            }
        }

        bool IXmlLineInfo.HasLineInfo()
        {
            return HasLineInfo;
        }
    }
}
