using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace IS4.SFI.Tools.Xml
{
    /// <summary>
    /// Stores the immediate state from an XML reader.
    /// </summary>
    public sealed class XmlReaderState : XmlReader, IXmlSchemaInfo, IXmlLineInfo
    {
        /// <inheritdoc/>
        public override int AttributeCount { get; }

        /// <inheritdoc/>
        public override string BaseURI { get; }

        /// <inheritdoc/>
        public override int Depth { get; }

        /// <inheritdoc/>
        public override bool EOF { get; }

        /// <inheritdoc/>
        public override bool IsEmptyElement { get; }

        /// <inheritdoc/>
        public override string LocalName { get; }

        /// <inheritdoc/>
        public override string NamespaceURI { get; }

        /// <inheritdoc/>
        public override XmlNodeType NodeType { get; }

        /// <inheritdoc/>
        public override string Prefix { get; }

        /// <inheritdoc/>
        public override ReadState ReadState { get; }

        /// <inheritdoc/>
        public override string Value { get; }

        /// <inheritdoc/>
        public override bool CanReadBinaryContent => false;

        /// <inheritdoc/>
        public override bool CanReadValueChunk => false;

        /// <inheritdoc/>
        public override bool CanResolveEntity => false;

        /// <inheritdoc/>
        public override bool HasAttributes { get; }

        /// <inheritdoc/>
        public override bool HasValue { get; }

        /// <inheritdoc/>
        public override bool IsDefault { get; }

        /// <inheritdoc/>
        public override string Name { get; }

        /// <inheritdoc/>
        public override Type ValueType { get; }

        /// <inheritdoc/>
        public override string XmlLang { get; }

        /// <inheritdoc/>
        public override XmlSpace XmlSpace { get; }

        /// <inheritdoc/>
        public override XmlNameTable NameTable { get; }

        /// <inheritdoc/>
        public override IXmlSchemaInfo SchemaInfo => this;

        /// <inheritdoc/>
        public bool IsNil { get; }

        /// <inheritdoc/>
        public XmlSchemaSimpleType? MemberType { get; }

        /// <inheritdoc/>
        public XmlSchemaAttribute? SchemaAttribute { get; }

        /// <inheritdoc/>
        public XmlSchemaElement? SchemaElement { get; }

        /// <inheritdoc/>
        public XmlSchemaType? SchemaType { get; }

        /// <inheritdoc/>
        public XmlSchemaValidity Validity { get; }

        /// <summary>
        /// The collection of instances of <see cref="XmlReaderState"/>
        /// for the attributes of the current element or the element
        /// storing this attribute.
        /// </summary>
        public IReadOnlyList<XmlReaderState> Attributes { get; }

        /// <summary>
        /// The collection of contents of the current attribute,
        /// as instances of <see cref="XmlReaderState"/>.
        /// </summary>
        public IReadOnlyList<XmlReaderState> AttributeContents { get; }

        /// <summary>
        /// The map of namespace prefix to value for namespaces valid in the current position.
        /// </summary>
        public IReadOnlyDictionary<string, string> NamespaceMap { get; }

        /// <inheritdoc/>
        public int LineNumber { get; }

        /// <inheritdoc/>
        public int LinePosition { get; }

        /// <summary>
        /// <see langword="true"/> if the current state provides <see cref="LineNumber"/>
        /// and <see cref="LinePosition"/>.
        /// </summary>
        public bool HasLineInfo { get; }

        /// <summary>
        /// Creates a new state from the current node in an XML reader.
        /// </summary>
        /// <param name="reader">The reader to capture the state from.</param>
        /// <param name="namespaceMap">The map of namespace prefix to value for namespaces valid in the current position.</param>
        public XmlReaderState(XmlReader reader, IReadOnlyDictionary<string, string>? namespaceMap = null) : this(reader, namespaceMap, null)
        {

        }

        private XmlReaderState(XmlReader reader, IReadOnlyDictionary<string, string>? namespaceMap, IReadOnlyList<XmlReaderState>? parentAttributes)
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
                // This is not an attribute node
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
                // This is an attribute node among parentAttributes
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

                NamespaceMap = namespaceMap ?? new Dictionary<string, string>();
            }
        }

        /// <inheritdoc/>
        public override string GetAttribute(int i)
        {
            if(i < 0 || i >= Attributes.Count) throw new ArgumentOutOfRangeException(nameof(i));
            return Attributes[i].Value;
        }

        /// <inheritdoc/>
        public override string? GetAttribute(string name)
        {
            return Attributes.FirstOrDefault(a => a.Name == name)?.Value;
        }

        /// <inheritdoc/>
        public override string? GetAttribute(string name, string namespaceURI)
        {
            return Attributes.FirstOrDefault(a => a.LocalName == name && a.NamespaceURI == namespaceURI)?.Value;
        }

        /// <inheritdoc/>
        public override string? LookupNamespace(string prefix)
        {
            return NamespaceMap.TryGetValue(prefix, out var value) ? value : null;
        }

        /// <inheritdoc/>
        public override Task<string> GetValueAsync()
        {
            return Task.FromResult(Value);
        }

        /// <inheritdoc/>
        public override bool MoveToAttribute(string name)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override bool MoveToAttribute(string name, string ns)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override bool MoveToElement()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override bool MoveToFirstAttribute()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override bool MoveToNextAttribute()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override bool Read()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override bool ReadAttributeValue()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void ResolveEntity()
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Produces a collection of instances of <see cref="XmlReaderState"/>
        /// from all the nodes returned by <paramref name="reader"/>,
        /// by enumerating them using <see cref="XmlReader.Read"/>.
        /// </summary>
        /// <param name="reader">The reader to capture the states from.</param>
        /// <returns>The collection of states representing each point of the reading.</returns>
        public static IEnumerable<XmlReaderState> ReadFrom(XmlReader reader)
        {
            var namespaceFrames = new Stack<IReadOnlyDictionary<string, string>?>();
            int depth = -1;
            IReadOnlyDictionary<string, string>? namespaceMap = null;

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

            yield return new XmlReaderState(reader, null);
        }

        bool IXmlLineInfo.HasLineInfo()
        {
            return HasLineInfo;
        }
    }
}
