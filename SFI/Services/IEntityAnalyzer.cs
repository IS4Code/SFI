using IS4.SFI.Formats;
using IS4.SFI.Tools;
using System;
using System.Linq;
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
        /// True if <see cref="Node"/> was already initialized (i.e.
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
        /// Creates a new instance of the context.
        /// </summary>
        /// <param name="parent">The value of <see cref="Parent"/>.</param>
        /// <param name="node">The value of <see cref="Node"/>.</param>
        /// <param name="initialized">The value of <see cref="Initialized"/>.</param>
        /// <param name="nodeFactory">The value of <see cref="NodeFactory"/>.</param>
        /// <param name="matchContext">The value of <see cref="MatchContext"/>.</param>
        public AnalysisContext(ILinkedNodeFactory nodeFactory, ILinkedNode? parent = null, ILinkedNode? node = null, bool initialized = false, MatchContext matchContext = default)
        {
            Parent = parent;
            Node = node;
            Initialized = initialized;
            NodeFactory = nodeFactory;
            MatchContext = matchContext;
        }

        /// <summary>
        /// Creates a new context, keeping the <see cref="NodeFactory"/>
        /// and <see cref="MatchContext"/>.
        /// </summary>
        /// <param name="parent">The new value of <see cref="Parent"/>.</param>
        /// <returns>The updated context.</returns>
        public AnalysisContext WithParent(ILinkedNode? parent)
        {
            return new AnalysisContext(NodeFactory, parent, null, false, MatchContext);
        }

        /// <summary>
        /// Creates a new context, keeping the <see cref="NodeFactory"/>
        /// and <see cref="MatchContext"/>.
        /// The <see cref="Parent"/> property also preserved,
        /// unless <paramref name="node"/> is different from <see cref="Node"/>
        /// and both are non-null.
        /// </summary>
        /// <param name="node">The new value of <see cref="Node"/>.</param>
        /// <returns>The updated context.</returns>
        public AnalysisContext WithNode(ILinkedNode? node)
        {
            return new AnalysisContext(NodeFactory, Node != null && node != null && !Node.Equals(node) ? null : Parent, node, false, MatchContext);
        }

        /// <summary>
        /// Creates a new context, keeping the <see cref="Parent"/>,
        /// <see cref="Node"/>, and <see cref="NodeFactory"/>.
        /// </summary>
        /// <param name="matchContext">The new value of <see cref="MatchContext"/>.</param>
        /// <returns>The updated context.</returns>
        public AnalysisContext WithMatchContext(MatchContext matchContext)
        {
            return new AnalysisContext(NodeFactory, Parent, Node, false, matchContext);
        }

        /// <summary>
        /// Creates a new context, keeping the <see cref="Parent"/>,
        /// <see cref="Node"/>, and <see cref="NodeFactory"/>.
        /// </summary>
        /// <param name="matchContextTransform">
        /// A function which receives the old value of <see cref="MatchContext"/>
        /// and produces a new one to use.
        /// </param>
        /// <returns>The updated context.</returns>
        public AnalysisContext WithMatchContext(Func<MatchContext, MatchContext> matchContextTransform)
        {
            return new AnalysisContext(NodeFactory, Parent, Node, false, matchContextTransform(MatchContext));
        }

        /// <summary>
        /// Creates a new context, keeping all properties but setting
        /// <see cref="Initialized"/> to true.
        /// </summary>
        /// <returns>The updated context.</returns>
        public AnalysisContext AsInitialized()
        {
            return new AnalysisContext(NodeFactory, Parent, Node, true, MatchContext);
        }
    }

    /// <summary>
    /// Stores the result of an analysis.
    /// </summary>
    public struct AnalysisResult
    {
        /// <summary>
        /// The instance of <see cref="ILinkedNode"/> that was created or used for the
        /// analyzed entity. Could be null if the analysis was not
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

        private ILinkedNode InitNewNode(ILinkedNode node, AnalysisContext context)
        {
            if(!context.Initialized) InitNode(node, context);
            return node;
        }

        /// <summary>
        /// This method is called when a node is obtained for the entity,
        /// initializing its default properties (usually via
        /// <see cref="ILinkedNode.SetClass(Vocabulary.ClassUri)"/>), unless
        /// <see cref="AnalysisContext.Initialized"/> is set to true.
        /// The default implementation does nothing.
        /// </summary>
        /// <param name="node">The node to initialize.</param>
        /// <param name="context">The context to use.</param>
        protected virtual void InitNode(ILinkedNode node, AnalysisContext context)
        {

        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var typesId = String.Join("+", GetType().GetEntityAnalyzerTypes().Select(TextTools.GetIdentifierFromType).Distinct());
            return String.IsNullOrEmpty(typesId) ? base.ToString() : typesId;
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
