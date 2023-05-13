using IS4.SFI.Formats;
using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace IS4.SFI.Services
{
    /// <summary>
    /// Supports analysis of arbitrary entities. It usually stores
    /// a collection of <see cref="IEntityAnalyzer{T}"/> to query
    /// with the concrete type of the entity.
    /// </summary>
    public interface IEntityAnalyzers
    {
        /// <summary>
        /// Analyzes <paramref name="entity"/> of an arbitrary type.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="entity"/>.</typeparam>
        /// <param name="entity">The entity to analyze.</param>
        /// <param name="context">An instance of <see cref="AnalysisContext"/> to provide additional parameters.</param>
        /// <returns>
        /// The result of the analysis, or the default value of <see cref="AnalysisResult"/>
        /// if it was not successful.
        /// </returns>
        ValueTask<AnalysisResult> Analyze<T>(T entity, AnalysisContext context) where T : class;
    }

    /// <summary>
    /// Supports analysis of entities of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The supported type of the entity.</typeparam>
    public interface IEntityAnalyzer<in T> where T : class
    {
        /// <summary>
        /// Analyzes <paramref name="entity"/>.
        /// </summary>
        /// <param name="entity">The entity to analyze.</param>
        /// <param name="context">An instance of <see cref="AnalysisContext"/> to provide additional parameters.</param>
        /// <param name="analyzers">The collection of analyzers to use for inner entities.</param>
        /// <returns>
        /// The result of the analysis, or the default value of <see cref="AnalysisResult"/>
        /// if it was not successful.
        /// </returns>
        ValueTask<AnalysisResult> Analyze(T entity, AnalysisContext context, IEntityAnalyzers analyzers);
    }

    /// <summary>
    /// Additional state used during analysis.
    /// </summary>
    public struct AnalysisContext
    {
        /// <summary>
        /// The parent node logically containing the analyzed entity.
        /// </summary>
        public ILinkedNode? Parent { get; }

        /// <summary>
        /// The node logically corresponding to the analyzed entity.
        /// </summary>
        public ILinkedNode? Node { get; }

        /// <summary>
        /// The linking property of the newly created node to its parent,
        /// used as a subject.
        /// </summary>
        public PropertyUri? Link { get; }

        /// <summary>
        /// <see langword="true"/> if <see cref="Node"/> was already initialized (i.e.
        /// by calling <see cref="ILinkedNode.SetClass(Vocabulary.ClassUri)"/>)
        /// and it is not necessary to initialize it again.
        /// </summary>
        public bool Initialized { get; }

        /// <summary>
        /// The factory that is used to produce instances of <see cref="ILinkedNode"/>.
        /// </summary>
        public ILinkedNodeFactory NodeFactory { get; }

        /// <summary>
        /// Additional context used when matching formats.
        /// </summary>
        public MatchContext MatchContext { get; }

        /// <summary>
        /// The nesting depth of the analysis.
        /// </summary>
        public int Depth { get; }

        /// <summary>
        /// Creates a new instance of the context.
        /// </summary>
        /// <param name="parent">The value of <see cref="Parent"/>.</param>
        /// <param name="node">The value of <see cref="Node"/>.</param>
        /// <param name="link">The value of <see cref="Link"/>.</param>
        /// <param name="initialized">The value of <see cref="Initialized"/>.</param>
        /// <param name="nodeFactory">The value of <see cref="NodeFactory"/>.</param>
        /// <param name="matchContext">The value of <see cref="MatchContext"/>.</param>
        /// <param name="depth">The value of <see cref="Depth"/>.</param>
        private AnalysisContext(ILinkedNodeFactory nodeFactory, ILinkedNode? parent, ILinkedNode? node, PropertyUri? link, bool initialized, MatchContext matchContext, int depth)
        {
            Parent = parent;
            Node = node;
            Link = link;
            Initialized = initialized;
            NodeFactory = nodeFactory;
            MatchContext = matchContext;
            Depth = depth;
        }

        /// <summary>
        /// Creates a new instance of <see cref="AnalysisContext"/> starting from a particular
        /// node and an instance of <see cref="ILinkedNodeFactory"/>.
        /// </summary>
        /// <param name="node">The value of <see cref="Node"/>.</param>
        /// <param name="nodeFactory">The value of <see cref="NodeFactory"/>.</param>
        /// <returns>A new instance with the specified objects.</returns>
        public static AnalysisContext Create(ILinkedNode? node, ILinkedNodeFactory nodeFactory)
        {
            return new AnalysisContext(nodeFactory, null, node, null, false, default, 0);
        }

        /// <summary>
        /// Creates a new context, with a new value of <see cref="Parent"/>.
        /// </summary>
        /// <param name="parent">The new value of <see cref="Parent"/>.</param>
        /// <returns>The updated context.</returns>
        public AnalysisContext WithParent(ILinkedNode? parent)
        {
            return new AnalysisContext(NodeFactory, parent, null, null, false, MatchContext, unchecked(Depth + 1));
        }

        /// <summary>
        /// Creates a new context, with a new value of <see cref="Parent"/>
        /// and <see cref="Link"/>.
        /// </summary>
        /// <param name="parent">The new value of <see cref="Parent"/>.</param>
        /// <param name="link">The new value of <see cref="Link"/>.</param>
        /// <returns>The updated context.</returns>
        public AnalysisContext WithParentLink(ILinkedNode? parent, PropertyUri link)
        {
            return new AnalysisContext(NodeFactory, parent, null, link, false, MatchContext, unchecked(Depth + 1));
        }

        /// <summary>
        /// Creates a new context, keeping the <see cref="NodeFactory"/>
        /// and <see cref="MatchContext"/>.
        /// The <see cref="Parent"/>, <see cref="Link"/>, and <see cref="Initialized"/> properties also preserved,
        /// unless <paramref name="node"/> is different from <see cref="Node"/>
        /// and both are non-<see langword="null"/>.
        /// </summary>
        /// <param name="node">The new value of <see cref="Node"/>.</param>
        /// <returns>The updated context.</returns>
        public AnalysisContext WithNode(ILinkedNode? node)
        {
            bool reset = Node != null && node != null && !Node.Equals(node);
            return new AnalysisContext(NodeFactory, reset ? null : Parent, node, reset ? null : Link, reset ? false : Initialized, MatchContext, Depth);
        }

        /// <summary>
        /// Creates a new context, updating the value of <see cref="MatchContext"/>.
        /// </summary>
        /// <param name="matchContext">The new value of <see cref="MatchContext"/>.</param>
        /// <returns>The updated context.</returns>
        public AnalysisContext WithMatchContext(MatchContext matchContext)
        {
            return new AnalysisContext(NodeFactory, Parent, Node, Link, Initialized, matchContext, Depth);
        }

        /// <summary>
        /// Creates a new context, updating the value of <see cref="MatchContext"/>.
        /// </summary>
        /// <param name="matchContextTransform">
        /// A function which receives the old value of <see cref="MatchContext"/>
        /// and produces a new one to use.
        /// </param>
        /// <returns>The updated context.</returns>
        public AnalysisContext WithMatchContext(Func<MatchContext, MatchContext> matchContextTransform)
        {
            return new AnalysisContext(NodeFactory, Parent, Node, Link, Initialized, matchContextTransform(MatchContext), Depth);
        }

        /// <summary>
        /// Creates a new context, keeping all properties but setting
        /// <see cref="Initialized"/> to <see langword="true"/>.
        /// </summary>
        /// <returns>The updated context.</returns>
        public AnalysisContext AsInitialized()
        {
            return new AnalysisContext(NodeFactory, Parent, Node, null, true, MatchContext, Depth);
        }
    }

    /// <summary>
    /// Stores the result of an analysis.
    /// </summary>
    public struct AnalysisResult
    {
        /// <summary>
        /// The instance of <see cref="ILinkedNode"/> that was created or used for the
        /// analyzed entity. Could be <see langword="null"/> if the analysis was not
        /// successful.
        /// </summary>
        public ILinkedNode? Node { get; set; }

        /// <summary>
        /// A human-readable label storing additional information about the entity.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Stores an exception that might have occurred during the analysis,
        /// possibly as an <see cref="AggregateException"/> if there were
        /// multiple exceptions.
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// Creates a new instance of the result.
        /// </summary>
        /// <param name="node">The value of <see cref="Node"/>.</param>
        /// <param name="label">The value of <see cref="Label"/>.</param>
        /// <param name="exception">The value of <see cref="Exception"/>.</param>
        public AnalysisResult(ILinkedNode? node, string? label = null, Exception? exception = null)
        {
            Node = node;
            Label = label;
            Exception = exception;
        }
    }

    /// <summary>
    /// A base implementation of an analyzer, containing useful
    /// functions for obtaining and initializing the node.
    /// It is still necessary to implement the concrete
    /// <see cref="IEntityAnalyzer{T}"/> type.
    /// </summary>
    public abstract class EntityAnalyzer : IHasFileOutput
    {
        /// <inheritdoc/>
        public event OutputFileDelegate? OutputFile;

        static readonly OutputFileDelegate defaultOutputFile = delegate { return default(ValueTask); };

        /// <summary>
        /// Retrieves the <see cref="OutputFile"/> event caller.
        /// </summary>
        protected OutputFileDelegate OnOutputFile => OutputFile ?? defaultOutputFile;

        /// <summary>
        /// Creates a new instance of the analyzer.
        /// </summary>
        public EntityAnalyzer()
        {

        }

        /// <summary>
        /// Obtains or creates a new node from <see cref="AnalysisContext"/>.
        /// </summary>
        /// <param name="context">The context to use.</param>
        /// <returns>
        /// The value of <see cref="AnalysisContext.Node"/>,
        /// or the result of <see cref="LinkedNodeFactoryExtensions.CreateUnique(ILinkedNodeFactory)"/>.
        /// </returns>
        protected ILinkedNode GetNode(AnalysisContext context)
        {
            return
                InitNewNode(context.Node ?? context.NodeFactory.CreateUnique(), context);
        }

        /// <param name="subName">The name of the node relative to its parent.</param>
        /// <returns>
        /// The value of <see cref="AnalysisContext.Node"/>, the result of indexing
        /// <see cref="AnalysisContext.Parent"/> with <paramref name="subName"/>,
        /// or the result of <see cref="LinkedNodeFactoryExtensions.CreateUnique(ILinkedNodeFactory)"/>.
        /// </returns>
        /// <inheritdoc cref="GetNode(AnalysisContext)"/>
        /// <param name="context"><inheritdoc cref="GetNode(AnalysisContext)" path="/param[@name='context']"/></param>
        protected ILinkedNode GetNode(string subName, AnalysisContext context)
        {
            return
                InitNewNode(context.Node ?? context.Parent?[subName] ?? context.NodeFactory.CreateUnique(), context);
        }

        /// <param name="formatter">The formatter to use to produce the node from its parent.</param>
        /// <returns>
        /// The value of <see cref="AnalysisContext.Node"/>, the result of indexing
        /// <see cref="AnalysisContext.Parent"/> with <paramref name="formatter"/>,
        /// or the result of <see cref="LinkedNodeFactoryExtensions.CreateUnique(ILinkedNodeFactory)"/>.
        /// </returns>
        /// <inheritdoc cref="GetNode(AnalysisContext)"/>
        /// <param name="context"><inheritdoc cref="GetNode(AnalysisContext)" path="/param[@name='context']"/></param>
        protected ILinkedNode GetNode(IIndividualUriFormatter<Uri> formatter, AnalysisContext context)
        {
            return
                InitNewNode(context.Node ?? context.Parent?[formatter] ?? context.NodeFactory.CreateUnique(), context);
        }

        /// <summary>
        /// This method should be called to initialize a newly constructed node
        /// when such a node is not obtained from <see cref="GetNode(AnalysisContext)"/>
        /// or its overloads. The method calls <see cref="InitNode(ILinkedNode, AnalysisContext)"/>
        /// if <see cref="AnalysisContext.Initialized"/> is not <see langword="true"/>.
        /// </summary>
        /// <param name="node">The node to initialize.</param>
        /// <param name="context">The context to use.</param>
        protected ILinkedNode InitNewNode(ILinkedNode node, AnalysisContext context)
        {
            if(!context.Initialized) InitNode(node, context);
            return node;
        }

        /// <summary>
        /// This method is called when a node is obtained for the entity,
        /// initializing its default properties (usually via
        /// <see cref="ILinkedNode.SetClass(Vocabulary.ClassUri)"/>), unless
        /// <see cref="AnalysisContext.Initialized"/> is set to <see langword="true"/>.
        /// The default implementation links <paramref name="node"/> to
        /// <see cref="AnalysisContext.Parent"/> using <see cref="AnalysisContext.Link"/>.
        /// </summary>
        /// <param name="node">The node to initialize.</param>
        /// <param name="context">The context to use.</param>
        protected virtual void InitNode(ILinkedNode node, AnalysisContext context)
        {
            if(context.Link is PropertyUri link)
            {
                context.Parent?.Set(link, node);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var typesId = String.Join("+", GetType().GetEntityAnalyzerTypes().Select(TextTools.GetIdentifierFromType).Distinct());
            return String.IsNullOrEmpty(typesId) ? base.ToString() : typesId;
        }

        /// <summary>
        /// Ensures that an object has a valid value.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="value">The value of the object.</param>
        /// <param name="result">The variable that receives the value of the object.</param>
        /// <returns><see langword="true"/> if the value is valid.</returns>
        protected static bool IsDefined<T>(T? value, out T result) where T : struct
        {
            result = value.GetValueOrDefault();
            return value != null;
        }

        /// <inheritdoc cref="IsDefined{T}(T?, out T)"/>
        protected static bool IsDefined<T>(T? value, [MaybeNullWhen(false)] out T result) where T : class
        {
            result = value!;
            return value != null;
        }

        /// <inheritdoc cref="IsDefined{T}(T?, out T)"/>
        protected static bool IsDefined(string? value, [MaybeNullWhen(false)] out string result)
        {
            result = value!;
            return !String.IsNullOrWhiteSpace(value);
        }

        /// <inheritdoc cref="IsDefined{T}(T?, out T)"/>
        protected static bool IsDefined(int value, out int result)
        {
            result = value;
            return value > 0;
        }

        /// <inheritdoc cref="IsDefined{T}(T?, out T)"/>
        protected static bool IsDefined(long value, out long result)
        {
            result = value;
            return value > 0;
        }

        /// <inheritdoc cref="IsDefined{T}(T?, out T)"/>
        protected static bool IsDefined(float value, out float result)
        {
            result = value;
            return value > 0;
        }

        /// <inheritdoc cref="IsDefined{T}(T?, out T)"/>
        protected static bool IsDefined(double value, out double result)
        {
            result = value;
            return value > 0;
        }

        /// <inheritdoc cref="IsDefined{T}(T?, out T)"/>
        protected static bool IsDefined(DateTime value, out DateTime result)
        {
            result = value;
            try{
                if(value.ToBinary() == 0) return false;
                return value.ToFileTimeUtc() != 0 && value.ToFileTime() != 0;
            }catch{
                return false;
            }
        }
    }

    /// <summary>
    /// An implementation of <see cref="IEntityAnalyzer{T}"/> where
    /// <typeparamref name="T"/> is the primary analyzable type.
    /// </summary>
    /// <typeparam name="T">The primary type of entities accepted by this analyzer.</typeparam>
    public abstract class EntityAnalyzer<T> : EntityAnalyzer, IEntityAnalyzer<T> where T : class
    {
        /// <inheritdoc/>
        public abstract ValueTask<AnalysisResult> Analyze(T entity, AnalysisContext context, IEntityAnalyzers analyzers);

        /// <inheritdoc/>
        public override string ToString()
        {
            return TextTools.GetIdentifierFromType<T>() ?? base.ToString();
        }
    }
}
