using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using VDS.RDF;
using VDS.RDF.JsonLd.Syntax;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace IS4.SFI
{
    /// <summary>
    /// A custom RDF handler that writes JSON-LD output to a file.
    /// </summary>
    public class JsonLdHandler : BaseRdfHandler
    {
        readonly TextWriter output;
        readonly INamespaceMapper namespaceMapper;
        readonly IUriFormatter? uriFormatter;
        readonly IEqualityComparer<Uri> uriComparer;
        readonly ILiteralNode typePredicate;

        readonly JsonLdWriterOptions options;

        /// <inheritdoc cref="JsonLdWriterOptions.UseNativeTypes"/>
        public bool UseNativeTypes { get => options.UseNativeTypes; set => options.UseNativeTypes = value; }

        /// <inheritdoc cref="JsonLdWriterOptions.UseRdfType"/>
        public bool UseRdfType { get => options.UseRdfType; set => options.UseRdfType = value; }

        /// <inheritdoc cref="JsonLdWriterOptions.ProcessingMode"/>
        public JsonLdProcessingMode ProcessingMode { get => options.ProcessingMode; set => options.ProcessingMode = value; }

        /// <inheritdoc cref="JsonLdWriterOptions.JsonFormatting"/>
        public Newtonsoft.Json.Formatting JsonFormatting { get => options.JsonFormatting; set => options.JsonFormatting = value; }

        readonly Stack<State> stateStack = new();
        readonly State initialState;
        State state;

        /// <summary>
        /// Number of space characters to indent with.
        /// </summary>
        public int IndentSize {
            get => __.Length;
            set => __ = new string(' ', value);
        }

        string __ = "  ";

        /// <summary>
        /// Create a new JSON-LD handler with default formatting options.
        /// </summary>
        /// <param name="output">The text output to format to.</param>
        /// <param name="namespaceMapper">The namespace mapper to register namespaces to.</param>
        /// <param name="options">The options to use when writing JSON-LD.</param>
        /// <param name="uriFormatter">The formatter to convert <see cref="Uri"/> instances to strings.</param>
        /// <param name="uriComparer">The comparer of <see cref="Uri"/> instances.</param>
        public JsonLdHandler(TextWriter output, INamespaceMapper namespaceMapper, JsonLdWriterOptions? options = null, IUriFormatter? uriFormatter = null, IEqualityComparer<Uri>? uriComparer = null)
        {
            if(NodeFactory is NodeFactory nodeFactory)
            {
                nodeFactory.ValidateLanguageSpecifiers = false;
            }

            this.output = output;
            this.namespaceMapper = namespaceMapper;
            this.options = options ?? new()
            {
                UseNativeTypes = true
            };
            this.uriFormatter = uriFormatter;
            this.uriComparer = uriComparer ?? new UriComparer();

            typePredicate = CreateLiteralNode("@type");
            initialState = state = new();
        }

        /// <inheritdoc/>
        public override bool AcceptsAll => true;

        /// <inheritdoc/>
        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            if(namespaceMapper.HasNamespace(prefix))
            {
                var original = namespaceMapper.GetNamespaceUri(prefix);
                if(uriComparer.Equals(namespaceUri, original))
                {
                    // Already defined
                    return true;
                }
                var e = stateStack.GetEnumerator();
                if(!FindOrDefineContext(prefix, namespaceUri, state, ref e, out var replacing))
                {
                    // Not possible to redefine up to this context, nest
                    CloseValue();
                    Nest();
                    // Define it here
                    state.Context.Add(prefix, namespaceUri);
                    if(replacing != null)
                    {
                        state.LeavingContext.Add(prefix, replacing);
                    }
                }
                namespaceMapper.RemoveNamespace(prefix);
            }else{
                initialState.Context[prefix] = namespaceUri;
            }
            namespaceMapper.AddNamespace(prefix, namespaceUri);
            return true;
        }

        /// <inheritdoc/>
        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            // not really worth it
            return true;
        }

        /// <inheritdoc/>
        protected override void StartRdfInternal()
        {
            base.StartRdfInternal();
            namespaceMapper.Clear();
            WriteLine("{");
        }

        /// <inheritdoc/>
        protected override void EndRdfInternal(bool ok)
        {
            base.EndRdfInternal(ok);
            while(stateStack.Count > 0)
            {
                CloseObject();
                Write("}");
                PopState();
            }
            CloseObject();
            WriteLine("}");
            Flush();
        }

        void CloseValue()
        {
            if(state.LastObject != null)
            {
                if(state.InArray)
                {
                    WriteLine($"{__}{__}{FormatComplexNode(state.LastObject)}");
                    Write($"{__}]");
                    state.InArray = false;
                }else{
                    Write(FormatComplexNode(state.LastObject));
                }
            }else if(state.InArray)
            {
                Write("]");
                state.InArray = false;
            }
        }

        void CloseObject()
        {
            CloseValue();
            // Write out context
            if(state.Context.Count > 0)
            {
                WriteLine(",");
                WriteLine($"{__}\"@context\": {{");
                var first = true;
                foreach(var def in state.Context)
                {
                    if(first)
                    {
                        first = false;
                    }else{
                        WriteLine(",");
                    }
                    Write($"{__}{__}{FormatString(def.Key)}: {FormatString(GetUriString(def.Value))}");
                }
                WriteLine();
                WriteLine($"{__}}}");
            }else{
                WriteLine();
            }
        }

        void Nest()
        {
            // Enter nested object
            Write($"{__}\"@nest\": ");

            // Preserve original state
            var fork = state.Fork();
            state.LastPredicate = null;
            state.LastObject = null;
            stateStack.Push(state);
            fork.Indent += __;
            state = fork;
            WriteLine("{");
        }

        bool BeforeAddKey(ref string key)
        {
            if(state.Keys.Add(key))
            {
                return false;
            }

            Nest();
            state.Keys.Add(key);

            return true;
        }

        State PopState()
        {
            // Restore things changed from upper contexts
            foreach(var leaving in state.LeavingContext)
            {
                namespaceMapper.RemoveNamespace(leaving.Key);
                namespaceMapper.AddNamespace(leaving.Key, leaving.Value);
            }
            return state = stateStack.Pop();
        }

        /// <inheritdoc/>
        protected override bool HandleTripleInternal(Triple t)
        {
            if(
                !UseRdfType &&
                t.Predicate is IUriNode { Uri: { AbsoluteUri: RdfSpecsHelper.RdfType } } &&
                t.Object is IUriNode uriTypeObject)
            {
                // can be shortened to @type
                t = new Triple(
                    t.Subject,
                    typePredicate,
                    CreateLiteralNode(ResolveUri(uriTypeObject.Uri))
                );
            }
            if(state.LastSubject == null)
            {
                // Begin of document
                WriteLine($"{__}\"@id\": {FormatSimpleNode(t.Subject)},");
                Write($"{__}{FormatSimpleNode(t.Predicate)}: ");
            }else{
                if(t.Subject.Equals(state.LastSubject))
                {
                    // Continuing the object
                    if(t.Predicate.Equals(state.LastPredicate))
                    {
                        // Continuing the value
                        if(t.Object.Equals(state.LastObject))
                        {
                            // Duplicate triple; ignore
                        }else if(state.LastObject != null)
                        {
                            // Output as array
                            if(!state.InArray)
                            {
                                state.InArray = true;
                                WriteLine("[");
                            }
                            if(state.InArray)
                            {
                                Write(__);
                            }
                            WriteLine($"{__}{FormatComplexNode(state.LastObject)},");
                        }
                    }else{
                        if(state.LastObject != null)
                        {
                            // Close the previous value
                            CloseValue();
                            WriteLine(",");
                        }
                        var predicateKey = FormatSimpleNode(t.Predicate);
                        BeforeAddKey(ref predicateKey);
                        Write($"{__}{predicateKey}: ");
                    }
                }else{
                    // A different subject
                    if(!t.Subject.Equals(state.LastObject))
                    {
                        if(stateStack.Any(s => t.Subject.Equals(s.LastSubject)))
                        {
                            // We can return to a lower state
                            CloseObject();
                            Write("}");
                            while(!t.Subject.Equals(PopState().LastSubject))
                            {
                                // Close intermediate states
                                CloseObject();
                                Write("}");
                            }
                            // Start again
                            WriteLine(",");
                            // This time with t.Subject.Equals(state.LastSubject)
                            return HandleTripleInternal(t);
                        }else{
                            // Close the previous value
                            CloseValue();
                            WriteLine(",");
                            var includedKey = "\"@included\"";
                            BeforeAddKey(ref includedKey);
                            Write($"{__}{includedKey}: ");
                        }
                    }else{
                        // Could be merged with the last property
                    }

                    var indent = state.Indent + __;
                    // Already written
                    state.LastPredicate = null;
                    state.LastObject = null;
                    // Open a new state
                    stateStack.Push(state);
                    state = new();
                    state.Indent = indent;

                    WriteLine("{");
                    WriteLine($"{__}\"@id\": {FormatSimpleNode(t.Subject)},");
                    Write($"{__}{FormatSimpleNode(t.Predicate)}: ");
                }
            }
            state.LastSubject = t.Subject;
            state.LastPredicate = t.Predicate;
            state.LastObject = t.Object;
            return true;
        }

        /// <inheritdoc/>
        protected override bool HandleQuadInternal(Triple t, IRefNode graph)
        {
            throw new NotSupportedException();
        }

        string GetUriString(Uri uri)
        {
            return uriFormatter?.FormatUri(uri) ?? uri.AbsoluteUri;
        }

        string ResolveUri(Uri uri)
        {
            var uriString = GetUriString(uri);
            if(namespaceMapper.ReduceToQName(uriString, out var qname))
            {
                return qname;
            }
            return uriString;
        }

        bool FindOrDefineContext<TEnum>(string prefix, Uri ns, State current, ref TEnum enumerator, out Uri? replacingNs) where TEnum : IEnumerator<State>
        {
            if(current.Context.TryGetValue(prefix, out replacingNs))
            {
                // Either already defined or conflicting; return back
                return uriComparer.Equals(ns, replacingNs);
            }
            if(enumerator.MoveNext())
            {
                // Try to push definition lower
                if(FindOrDefineContext(prefix, ns, enumerator.Current, ref enumerator, out replacingNs))
                {
                    // Was found or defined there, all is good
                    return true;
                }
            }
            // Define it here
            current.Context.Add(prefix, ns);
            if(replacingNs != null)
            {
                current.LeavingContext.Add(prefix, replacingNs);
            }
            return true;
        }

        (string prefix, string ns, string name) DecodeQName(string qname, string originalUri)
        {
            int i = -1;
            while((i = qname.IndexOf(':', i + 1)) != -1)
            {
                // Possible prefix here
                var prefix = qname.Substring(0, i);
                if(namespaceMapper.GetNamespaceUri(prefix) is Uri nsUri)
                {
                    // Existing prefix
                    var ns = nsUri.AbsoluteUri;
                    if(originalUri.StartsWith(ns, StringComparison.Ordinal))
                    {
                        // Actual prefix
                        int remaining = qname.Length - i - 1;
                        if(originalUri.Length == ns.Length + remaining)
                        {
                            // The length matches
                            var name = qname.Substring(i + 1);
                            if(originalUri.EndsWith(name, StringComparison.Ordinal))
                            {
                                // Successfully decoded
                                return (prefix, ns, name);
                            }
                        }
                    }
                }
            }
            throw new InvalidOperationException($"{nameof(namespaceMapper.ReduceToQName)} produced invalid QName.");
        }

        string FormatUri(Uri uri)
        {
            return FormatString(ResolveUri(uri));
        }

        string FormatSimpleNode(INode node)
        {
            switch(node)
            {
                case IUriNode uriNode:
                    return FormatUri(uriNode.Uri);
                case IBlankNode blankNode:
                    return FormatString("_:" + blankNode.InternalID);
                case ILiteralNode literalNode when literalNode.DataType is null or { AbsoluteUri: XmlSpecsHelper.XmlSchemaDataTypeString } && String.IsNullOrEmpty(literalNode.Language):
                    return FormatString(literalNode.Value);
                default:
                    throw new NotSupportedException($"The node type {node.NodeType} is not supported in this position.");
            }
        }

        static readonly JsonLoadSettings jsonLoadSettings = new()
        {
            CommentHandling = CommentHandling.Ignore,
            DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error,
            LineInfoHandling = LineInfoHandling.Ignore
        };

        string FormatComplexNode(INode node)
        {
            switch(node)
            {
                case IUriNode uriNode:
                    return $"{{\"@id\": {FormatUri(uriNode.Uri)}}}";
                case IBlankNode blankNode:
                    return $"{{\"@id\": {FormatString("_:" + blankNode.InternalID)}}}";
                case ILiteralNode literalNode:
                    var rawValue = literalNode.Value;
                    var value = FormatString(rawValue);
                    if(!String.IsNullOrEmpty(literalNode.Language))
                    {
                        return $"{{\"@value\": {value}, \"@language\": {FormatString(literalNode.Language)}}}";
                    }else if(literalNode.DataType is not (null or { AbsoluteUri: XmlSpecsHelper.XmlSchemaDataTypeString }))
                    {
                        if(UseNativeTypes)
                        {
                            try{
                                switch(literalNode.DataType.AbsoluteUri)
                                {
                                    case XmlSpecsHelper.XmlSchemaDataTypeBoolean:
                                        var boolValue = XmlConvert.ToBoolean(rawValue);
                                        if(XmlConvert.ToString(boolValue) == rawValue)
                                        {
                                            return JsonConvert.ToString(boolValue);
                                        }
                                        break;
                                    case XmlSpecsHelper.XmlSchemaDataTypeInteger:
                                        var intValue = XmlConvert.ToInt32(rawValue);
                                        if(XmlConvert.ToString(intValue) == rawValue)
                                        {
                                            return JsonConvert.ToString(intValue);
                                        }
                                        break;
                                    case XmlSpecsHelper.XmlSchemaDataTypeDouble:
                                        var doubleValue = XmlConvert.ToDouble(rawValue);
                                        if(XmlConvert.ToString(doubleValue) == rawValue)
                                        {
                                            return JsonConvert.ToString(doubleValue);
                                        }
                                        break;
                                    case RdfSpecsHelper.RdfJson when ProcessingMode != JsonLdProcessingMode.JsonLd10:
                                        // Ignore for now; Json.NET does not understand what valid JSON means
                                        break;
                                }
                            }catch(FormatException)
                            {

                            }catch(ArithmeticException)
                            {

                            }catch(JsonException)
                            {

                            }
                        }
                        return $"{{\"@value\": {value}, \"@type\": {FormatUri(literalNode.DataType)}}}";
                    }
                    return value;
                default:
                    throw new NotSupportedException($"The node type {node.NodeType} is not supported in this position.");
            }
        }

        string FormatString(string str)
        {
            return JsonConvert.ToString(str);
        }

        string linePart = "";
        void Write(string str)
        {
            linePart += str;
        }

        void WriteLine(string? str = null)
        {
            output.WriteLine(linePart + str);
            linePart = state.Indent;
        }

        void Flush()
        {
            output.Write(linePart);
            output.Flush();
            linePart = "";
        }

        class State
        {
            public INode? LastSubject;
            public INode? LastPredicate;
            public INode? LastObject;
            public bool InArray;
            public string Indent = "";
            public HashSet<string> Keys = new(StringComparer.Ordinal);
            public Dictionary<string, Uri> Context = new(StringComparer.Ordinal);
            public Dictionary<string, Uri> LeavingContext = new(StringComparer.Ordinal);

            public State Fork()
            {
                var fork = (State)MemberwiseClone();
                fork.Keys = new(StringComparer.Ordinal);
                fork.Context = new(StringComparer.Ordinal);
                return fork;
            }
        }
    }
}
