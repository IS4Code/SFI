using IS4.SFI.Services;
using IS4.SFI.Tools;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace IS4.SFI
{
    /// <summary>
    /// This class implements <see cref="IEntityAnalyzers"/>, storing a collection of
    /// type-based analyzers in <see cref="Analyzers"/>.
    /// </summary>
    public class EntityAnalyzerProvider : IEntityAnalyzers
    {
        readonly SortedMultiTree<object, Type> analyzers = new(new ObjectAnalyzer(), typeof(object), obj => obj.GetType().GetEntityAnalyzerTypes(), ObjectInheritanceComparer.Instance, TypeInheritanceComparer.Instance);

        /// <summary>
        /// A collection of analyzers, each implementing <see cref="IEntityAnalyzer{T}"/>.
        /// </summary>
        [ComponentCollection("analyzer", typeof(IEntityAnalyzer<>))]
        public ICollection<object> Analyzers => analyzers;

        /// <summary>
        /// A collection of instances of <see cref="IContainerAnalyzerProvider"/> to use to
        /// process the hierarchy.
        /// </summary>
        [ComponentCollection("container-format")]
        public ICollection<IContainerAnalyzerProvider> ContainerProviders { get; } = new List<IContainerAnalyzerProvider>();

        /// <summary>
        /// An instance of <see cref="ILogger"/> to use for logging.
        /// </summary>
        public ILogger? OutputLog { get; set; }

        /// <summary>
        /// Called when a new entity is or was analyzed.
        /// </summary>
        public event Func<ValueTask>? Updated;

        IdentityStore<IIdentityKey, ConcurrentDictionary<Type, Task<AnalysisResult>>>? cachedResults;

        /// <summary>
        /// Whether to cache results for analyzed entities and reuse them later.
        /// </summary>
        public bool CacheResults {
            get {
                return cachedResults != null;
            }
            set {
                cachedResults = value ? (cachedResults ?? new(_ => new())) : null;
            }
        }

        /// <summary>
        /// Invokes the <see cref="Updated"/> event.
        /// </summary>
        private ValueTask Update()
        {
            return (Updated?.Invoke()).GetValueOrDefault();
        }

        /// <summary>
        /// Stores whether a warning has been already issued for an analyzer missing for a particular type. 
        /// </summary>
        readonly ConcurrentDictionary<Type, bool> noAnalyzerWarned = new();

        /// <summary>
        /// Traverses the <see cref="Analyzers"/> and picks the first to implement
        /// <see cref="IEntityAnalyzer{T}"/> of <typeparamref name="T"/> to analyze
        /// <paramref name="entity"/>.
        /// </summary>
        private async ValueTask<AnalysisResult> Analyze<T>(T entity, AnalysisContext context, IEntityAnalyzers analyzers) where T : notnull
        {
            var log = TypeInfo<T>.IsValueType ? null : OutputLog;
            using var scope1 = log?.BeginScope(entity);
            var nameFriendly = TextTools.GetUserFriendlyName(entity);
            bool any = false;
            foreach(var analyzer in FindAnalyzers<T>())
            {
                if(analyzer is not ObjectAnalyzer)
                {
                    any = true;
                }
                log?.LogInformation($"Analyzing: {nameFriendly}...");
                await Update();
                try{
                    var result = await analyzer.Analyze(entity, context, analyzers);
                    if(result.Node != null)
                    {
                        if(!String.IsNullOrEmpty(result.Label))
                        {
                            log?.LogInformation($"OK! {result.Label}");
                        }else{
                            log?.LogInformation($"OK!");
                        }
#if DEBUG
                        if(context.Linked is StrongBox<bool> linked && !linked.Value)
                        {
                            throw new InvalidOperationException($"The analyzer {analyzer} returned success but did not initialize the node.");
                        }
#endif
                        return result;
                    }
                    if(analyzer is not ObjectAnalyzer)
                    {
                        using var scope2 = OutputLog?.BeginScope(analyzer);
                        OutputLog?.LogWarning($"No result from {TextTools.GetUserFriendlyName(analyzer)}!");
                    }
                }catch(InternalApplicationException)
                {
                    throw;
                }catch(Exception exception) when(GlobalOptions.SuppressNonCriticalExceptions)
                {
                    using(var scope2 = OutputLog?.BeginScope(analyzer))
                    {
                        OutputLog?.LogError(exception, $"Error from {TextTools.GetUserFriendlyName(analyzer)}!");
                    }
                    if(entity is not Exception && (context.Node != null || context.Link != null))
                    {
                        // Non-recursive and some way to track the path to parent
                        await Analyze(exception, context);
                    }
                    continue;
                }finally{
                    await Update();
                }
            }
            Type type;
            if(!any && !noAnalyzerWarned.ContainsKey(type = typeof(T)))
            {
                using var scope2 = TypeInfo<T>.IsValueType ? OutputLog?.BeginScope(entity) : null;
                OutputLog?.LogWarning($"No analyzer for {nameFriendly}.");
                noAnalyzerWarned[type] = true;
                await Update();
            }
            return default;
        }

        private IEnumerable<IEntityAnalyzer<T>> FindAnalyzers<T>() where T : notnull
        {
            return analyzers.Find(typeof(T)).OfType<IEntityAnalyzer<T>>();
        }

        /// <summary>
        /// Selects the <see cref="ContainerProviders"/> based on the result of
        /// <see cref="IContainerAnalyzerProvider.MatchRoot{TRoot}(TRoot, AnalysisContext)"/>
        /// of the particular <paramref name="root"/>.
        /// </summary>
        private ContainerNode<object, IContainerNode>? MatchRoot<TRoot>(TRoot root, AnalysisContext context, IEntityAnalyzers analyzers, IReadOnlyCollection<IContainerAnalyzerProvider>? blocked) where TRoot : notnull
        {
            using var scope1 = OutputLog?.BeginScope(root);
            List<ContainerAnalysisInfo>? analyzerList = null;
            foreach(var containerProvider in ContainerProviders)
            {
                if(blocked == null || !blocked.Contains(containerProvider))
                {
                    try{
                        var analyzer = containerProvider.MatchRoot(root, context);
                        if(analyzer != null)
                        {
                            // If the provider is not blocked and the root matches, add it to the collection
                            analyzerList ??= new List<ContainerAnalysisInfo>();
                            analyzerList.Add(new ContainerAnalysisInfo(analyzer, containerProvider));
                        }
                    }catch(InternalApplicationException)
                    {
                        throw;
                    }catch(Exception e) when(GlobalOptions.SuppressNonCriticalExceptions)
                    {
                        using var scope2 = OutputLog?.BeginScope(containerProvider);
                        OutputLog?.LogError(e, $"Error from {TextTools.GetUserFriendlyName(containerProvider)}!");
                    }
                }
            }
            if(analyzerList != null)
            {
                // Return a new analyzer collection representing the matching providers
                return new ContainerNode<object, IContainerNode>(null, null, analyzerList, this, analyzers);
            }
            return null;
        }

        /// <inheritdoc/>
        public ValueTask<AnalysisResult> Analyze<T>(T? entity, AnalysisContext context) where T : notnull
        {
            if(entity == null) return default;

            var cached = cachedResults;
            if(cached != null && entity is IIdentityKey key)
            {
                return new(cached[key].AddOrUpdate(typeof(T), _ => {
                    return AnalyzeInner(entity, context).AsTask();
                }, (t, old) => {
                    var log = TypeInfo<T>.IsValueType ? null : OutputLog;
                    using var scope = log?.BeginScope(entity);
                    log?.LogInformation($"Reusing cached {TextTools.GetUserFriendlyName(entity)}.");
                    return old;
                }));
            }

            return AnalyzeInner(entity, context);
        }

        static class TypeInfo<T>
        {
            public static readonly bool IsValueType = typeof(T).IsValueType;
        }

        async ValueTask<AnalysisResult> AnalyzeInner<T>(T entity, AnalysisContext context) where T : notnull
        {
            var wrapper = MatchRoot(entity, context, this, null);
            if(wrapper != null)
            {
                // The root was matched by one container formats, delegate the call to it
                return await wrapper.Analyze(entity, context);
            }

            return await Analyze(entity, context, this);
        }

        /// <summary>
        /// Provides an implementation of <see cref="IEntityAnalyzers"/>
        /// and <see cref="IContainerNode{TValue, TParent}"/> that is used
        /// to describe and analyze a node in a container hierarchy.
        /// </summary>
        /// <typeparam name="TValue">The type of <see cref="Value"/>.</typeparam>
        /// <typeparam name="TParent">The type of <see cref="ParentNode"/>.</typeparam>
        class ContainerNode<TValue, TParent> : IEntityAnalyzers, IContainerNode<TValue, TParent> where TValue : notnull where TParent : IContainerNode
        {
            public TParent? ParentNode { get; }
            public TValue? Value { get; }
            readonly EntityAnalyzerProvider baseProvider;
            readonly IEntityAnalyzers analyzers;
            readonly IEnumerable<ContainerAnalysisInfo> activeAnalyzers;

            /// <summary>
            /// Creates a new instance of the node.
            /// </summary>
            /// <param name="parentNode">The value of <see cref="ParentNode"/>.</param>
            /// <param name="value">The value of <see cref="Value"/>.</param>
            /// <param name="activeAnalyzers">The collection of analyzers that should be used to process the node.</param>
            /// <param name="baseProvider">The instance of <see cref="EntityAnalyzerProvider"/> that initiated the analysis.</param>
            /// <param name="analyzers">The base collection of analyzers.</param>
            public ContainerNode(TParent? parentNode, TValue? value, IEnumerable<ContainerAnalysisInfo>? activeAnalyzers, EntityAnalyzerProvider baseProvider, IEntityAnalyzers analyzers)
            {
                ParentNode = parentNode;
                Value = value;
                this.activeAnalyzers = activeAnalyzers ?? Array.Empty<ContainerAnalysisInfo>();
                this.baseProvider = baseProvider;
                this.analyzers = analyzers;
            }

            IContainerNode? IContainerNode.ParentNode => ParentNode;

            object? IContainerNode.Value => Value;

            public async ValueTask<AnalysisResult> Analyze<TEntity>(TEntity entity, AnalysisContext context) where TEntity : notnull
            {
                List<ContainerAnalysisInfo>? followedAnalyzers = null;
                List<IContainerAnalyzerProvider>? blocked = null;

                // Start traversing the collection of active analyzers
                var enumerator = activeAnalyzers.GetEnumerator();

                // The implementation of AnalyzeInner
                async ValueTask<AnalysisResult> GetResult(ContainerBehaviour behaviour)
                {
                    if((behaviour & ContainerBehaviour.FollowChildren) != 0)
                    {
                        // The current analyzer should be followed into children
                        (followedAnalyzers ??= new List<ContainerAnalysisInfo>())
                            .Add(enumerator.Current);
                    }
                    if((behaviour & ContainerBehaviour.BlockOther) != 0)
                    {
                        // The current analyzer's provider is blocked for root matches
                        (blocked ??= new List<IContainerAnalyzerProvider>())
                            .Add(enumerator.Current.Provider);
                    }
                    // The parent is null only for the root of the hierarchy
                    var parent = Value != null ? this : null;
                    if(enumerator.MoveNext())
                    {
                        // There is a next analyzer; use it
                        return await enumerator.Current.Analyzer.Analyze(parent, entity, context, GetResult, analyzers);
                    }
                    // There are no more analyzers
                    if(followedAnalyzers == null && blocked == null && Value != null)
                    {
                        // No special handling required, just use the stored IEntityAnalyzers
                        return await analyzers.Analyze(entity, context);
                    }
                    var innerAnalyzer = analyzers;
                    if(parent != null)
                    {
                        // Analyzing a new node
                        innerAnalyzer = new ContainerNode<TEntity, IContainerNode<TValue, TParent>>(parent, entity, followedAnalyzers, baseProvider, innerAnalyzer);
                        // Try matching the node as a root for other analyzers
                        var wrapper = baseProvider.MatchRoot(entity, context, innerAnalyzer, blocked);
                        if(wrapper != null)
                        {
                            return await wrapper.Analyze(entity, context);
                        }
                    }else{
                        // Nested nodes will use this analyzer
                        innerAnalyzer = new ContainerNode<TEntity, IContainerNode>(parent, entity, followedAnalyzers, baseProvider, innerAnalyzer);
                    }
                    return await baseProvider.Analyze(entity, context, innerAnalyzer);
                }

                return await GetResult(0);
            }

            public override string ToString()
            {
                if(ParentNode == null) return $"[{Value}]";
                return $"{ParentNode}.[{Value}]";
            }
        }

        /// <summary>
        /// Stores information about an instance of <see cref="IContainerAnalyzer"/>.
        /// </summary>
        readonly struct ContainerAnalysisInfo
        {
            /// <summary>
            /// The stored analyzer.
            /// </summary>
            public IContainerAnalyzer Analyzer { get; }

            /// <summary>
            /// The provider that was used to obtain <see cref="Analyzer"/>.
            /// </summary>
            public IContainerAnalyzerProvider Provider { get; }

            /// <summary>
            /// Creates a new instance of the type.
            /// </summary>
            /// <param name="analyzer">The value of <see cref="Analyzer"/>.</param>
            /// <param name="provider">The value of <see cref="Provider"/>.</param>
            public ContainerAnalysisInfo(IContainerAnalyzer analyzer, IContainerAnalyzerProvider provider)
            {
                Analyzer = analyzer;
                Provider = provider;
            }
        }

        /// <summary>
        /// The analyzer for arbitrary objects, i.e. those that are
        /// not accepted by any other analyzer.
        /// </summary>
        class ObjectAnalyzer : EntityAnalyzer<object>
        {
            /// <summary>
            /// Whether to accept all values given to the analyzer.
            /// </summary>
            [Description("Whether to accept all values given to the analyzer.")]
            public bool AcceptEverything { get; set; }

            public async override ValueTask<AnalysisResult> Analyze(object entity, AnalysisContext context, IEntityAnalyzers analyzers)
            {
                if(AcceptEverything)
                {
                    var node = GetNode(context);
                    var label = TextTools.GetUserFriendlyName(entity);
                    return new(node, label);
                }
                return default;
            }
        }

        class ObjectInheritanceComparer : IComparer<object>
        {
            public static readonly IComparer<object> Instance = new ObjectInheritanceComparer();

            private ObjectInheritanceComparer()
            {

            }

            public int Compare(object ox, object oy)
            {
                var x = ox.GetType();
                var y = oy.GetType();
                if(x.IsAssignableFrom(y))
                {
                    if(y.IsAssignableFrom(x)) return 0;
                    return -1;
                }
                if(y.IsAssignableFrom(x)) return 1;
                return 0;
            }
        }

        class TypeInheritanceComparer : IComparer<Type>
        {
            public static readonly IComparer<Type> Instance = new TypeInheritanceComparer();

            static readonly Type streamFactoryType = typeof(IStreamFactory);
            static readonly Type fileInfoType = typeof(IFileInfo);

            private TypeInheritanceComparer()
            {

            }

            public int Compare(Type x, Type y)
            {
                if(
                    (streamFactoryType.Equals(x) && fileInfoType.IsAssignableFrom(y)) ||
                    (fileInfoType.IsAssignableFrom(x) && streamFactoryType.Equals(y)))
                {
                    // Make IFileInfo not reachable from IStreamFactory
                    return 0;
                }

                if(x.IsAssignableFrom(y))
                {
                    if(y.IsAssignableFrom(x)) return 0;
                    return -1;
                }
                if(y.IsAssignableFrom(x)) return 1;
                return 0;
            }
        }
    }
}
