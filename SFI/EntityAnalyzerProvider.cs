using IS4.SFI.Services;
using IS4.SFI.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace IS4.SFI
{
    /// <summary>
    /// This class implements <see cref="IEntityAnalyzers"/>, storing a collection of
    /// type-based analyzers in <see cref="Analyzers"/>.
    /// </summary>
    public class EntityAnalyzerProvider : IEntityAnalyzers
    {
        /// <summary>
        /// A collection of analyzers, each implementing <see cref="IEntityAnalyzer{T}"/>.
        /// </summary>
        [ComponentCollection("analyzer", typeof(IEntityAnalyzer<>))]
        public ICollection<object> Analyzers { get; } = new SortedSet<object>(EntityAnalyzerComparer.Instance);

        /// <summary>
        /// A collection of instances of <see cref="IContainerAnalyzerProvider"/> to use to
        /// process the hierarchy.
        /// </summary>
        [ComponentCollection("container-format")]
        public ICollection<IContainerAnalyzerProvider> ContainerProviders { get; } = new List<IContainerAnalyzerProvider>();

        /// <summary>
        /// An instance of <see cref="TextWriter"/> to use for logging.
        /// </summary>
        public TextWriter OutputLog { get; set; } = Console.Error;

        /// <summary>
        /// Called when a new entity is or was analyzed.
        /// </summary>
        public event Func<ValueTask>? Updated;

        /// <summary>
        /// Invokes the <see cref="Updated"/> event.
        /// </summary>
        private ValueTask Update()
        {
            return (Updated?.Invoke()).GetValueOrDefault();
        }

        /// <summary>
        /// Stores a counter for every encountered entity type identifier.
        /// </summary>
        readonly ConcurrentDictionary<string, StrongBox<long>> typeCounters = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Stores if a 
        /// </summary>
        readonly ConcurrentDictionary<string, bool> noAnalyzerWarned = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Traverses the <see cref="Analyzers"/> and picks the first to implement
        /// <see cref="IEntityAnalyzer{T}"/> of <typeparamref name="T"/> to analyze
        /// <paramref name="entity"/>.
        /// </summary>
        private async ValueTask<AnalysisResult> Analyze<T>(T entity, AnalysisContext context, IEntityAnalyzers analyzers) where T : class
        {
            var nameKey = DataTools.GetIdentifierFromType<T>();
            var id = Interlocked.Increment(ref typeCounters.GetOrAdd(nameKey, _ => new StrongBox<long>(0)).Value);
            
            var nameOrdinal = nameKey + "#" + id;
            var nameFriendly = DataTools.GetUserFriendlyName(entity);

            bool any = false;
            foreach(var analyzer in Analyzers.OfType<IEntityAnalyzer<T>>())
            {
                any = true;
                OutputLog.WriteLine($"[{nameOrdinal}] Analyzing: {nameFriendly}...");
                await Update();
                try{
                    var result = await analyzer.Analyze(entity, context, analyzers);
                    if(result.Node != null)
                    {
                        if(!String.IsNullOrEmpty(result.Label))
                        {
                            OutputLog.WriteLine($"[{nameOrdinal}] OK! {result.Label}");
                        }else{
                            OutputLog.WriteLine($"[{nameOrdinal}] OK!");
                        }
                        return result;
                    }
                    OutputLog.WriteLine($"[{nameOrdinal}] No result!");
                }catch(InternalApplicationException e)
                {
                    ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                    throw;
                }catch(Exception e) when(GlobalOptions.SuppressNonCriticalExceptions)
                {
                    OutputLog.WriteLine($"[{nameOrdinal}] Error!");
                    OutputLog.WriteLine(e);
                    return default;
                }finally{
                    await Update();
                }
            }
            if(any || !noAnalyzerWarned.ContainsKey(nameKey))
            {
                OutputLog.WriteLine($"[{nameOrdinal}] No analyzer for {nameFriendly}.");
                noAnalyzerWarned[nameKey] = true;
                await Update();
            }
            return default;
        }

        /// <summary>
        /// Selects the <see cref="ContainerProviders"/> based on the result of
        /// <see cref="IContainerAnalyzerProvider.MatchRoot{TRoot}(TRoot, AnalysisContext)"/>
        /// of the particular <paramref name="root"/>.
        /// </summary>
        private ContainerNode<object, IContainerNode>? MatchRoot<TRoot>(TRoot root, AnalysisContext context, IEntityAnalyzers analyzers, IReadOnlyCollection<IContainerAnalyzerProvider>? blocked) where TRoot : class
        {
            List<ContainerAnalysisInfo>? analyzerList = null;
            foreach(var containerProvider in ContainerProviders)
            {
                if(blocked == null || !blocked.Contains(containerProvider))
                {
                    var analyzer = containerProvider.MatchRoot(root, context);
                    if(analyzer != null)
                    {
                        // If the provider is not blocked and the root matches, add it to the collection
                        if(analyzerList == null)
                        {
                            analyzerList = new List<ContainerAnalysisInfo>();
                        }
                        analyzerList.Add(new ContainerAnalysisInfo(analyzer, containerProvider));
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
        public async ValueTask<AnalysisResult> Analyze<T>(T entity, AnalysisContext context) where T : class
        {
            if(entity == null) return default;

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
        class ContainerNode<TValue, TParent> : IEntityAnalyzers, IContainerNode<TValue, TParent> where TValue : class where TParent : IContainerNode
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

            public async ValueTask<AnalysisResult> Analyze<TEntity>(TEntity entity, AnalysisContext context) where TEntity : class
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
        struct ContainerAnalysisInfo
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

        class EntityAnalyzerComparer : TypeInheritanceComparer<object>
        {
            public static new readonly IComparer<object> Instance = new EntityAnalyzerComparer();

            private EntityAnalyzerComparer()
            {

            }

            static readonly Type analyzerType = typeof(IEntityAnalyzer<>);

            /// <summary>
            /// Retrieves all implemented interfaces of <paramref name="initial"/> that
            /// are generic instantiations of <see cref="IEntityAnalyzer{T}"/>.
            /// </summary>
            /// <inheritdoc/>
            protected override IEnumerable<Type> SelectType(Type initial)
            {
                foreach(var i in initial.GetInterfaces())
                {
                    if(i.IsGenericType && i.GetGenericTypeDefinition().Equals(analyzerType))
                    {
                        yield return i;
                    }
                }
            }
        }
    }
}
