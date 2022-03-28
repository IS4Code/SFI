using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace IS4.MultiArchiver
{
    public class EntityAnalyzerProvider : IEntityAnalyzerProvider
    {
        public ICollection<object> Analyzers { get; } = new SortedSet<object>(EntityAnalyzerComparer.Instance);

        public ICollection<IContainerAnalyzerProvider> ContainerProviders { get; } = new List<IContainerAnalyzerProvider>();

        public TextWriter OutputLog { get; set; } = Console.Error;

        private async ValueTask<AnalysisResult> Analyze<T>(T entity, AnalysisContext context, IEntityAnalyzerProvider analyzers) where T : class
        {
            var entityName = DataTools.GetUserFriendlyName<T>(entity);

            foreach(var analyzer in Analyzers.OfType<IEntityAnalyzer<T>>())
            {
                try{
                    OutputLog.WriteLine("Analyzing " + entityName);
                    var result = await analyzer.Analyze(entity, context, analyzers);
                    if(result.Node != null)
                    {
                        OutputLog.WriteLine("Finished " + entityName);
                        return result;
                    }
                }catch(InternalArchiverException e)
                {
                    ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                    throw;
                }catch(Exception e)
                {
                    OutputLog.WriteLine("Error in analyzer " + analyzer.GetType().Name);
                    OutputLog.WriteLine(e);
                    return default;
                }
            }
            OutputLog.WriteLine("No analyzer for entity " + entityName);
            return default;
        }

        private ContainerNode<object, IContainerNode> MatchRoot<TRoot>(TRoot root, AnalysisContext context, IEntityAnalyzerProvider analyzers, IReadOnlyCollection<IContainerAnalyzerProvider> blocked) where TRoot : class
        {
            List<ContainerAnalysisInfo> analyzerList = null;
            foreach(var containerProvider in ContainerProviders)
            {
                if(blocked == null || !blocked.Contains(containerProvider))
                {
                    var analyzer = containerProvider.MatchRoot(root, context);
                    if(analyzer != null)
                    {
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
                return new ContainerNode<object, IContainerNode>(null, null, analyzerList, this, analyzers);
            }
            return null;
        }

        public async ValueTask<AnalysisResult> Analyze<T>(T entity, AnalysisContext context) where T : class
        {
            if(entity == null) return default;

            var wrapper = MatchRoot(entity, context, this, null);
            if(wrapper != null)
            {
                return await wrapper.Analyze(entity, context);
            }

            return await Analyze(entity, context, this);
        }

        class ContainerNode<TValue, TParent> : IEntityAnalyzerProvider, IContainerNode<TValue, TParent> where TValue : class where TParent : IContainerNode
        {
            public TParent ParentNode { get; }
            public TValue Value { get; }
            readonly EntityAnalyzerProvider baseProvider;
            readonly IEntityAnalyzerProvider analyzers;
            readonly IEnumerable<ContainerAnalysisInfo> activeAnalyzers;

            public ContainerNode(TParent parentNode, TValue value, IEnumerable<ContainerAnalysisInfo> activeAnalyzers, EntityAnalyzerProvider baseProvider, IEntityAnalyzerProvider analyzers)
            {
                ParentNode = parentNode;
                Value = value;
                this.activeAnalyzers = activeAnalyzers;
                this.baseProvider = baseProvider;
                this.analyzers = analyzers;
            }

            IContainerNode IContainerNode.ParentNode => ParentNode;

            object IContainerNode.Value => Value;

            public async ValueTask<AnalysisResult> Analyze<TEntity>(TEntity entity, AnalysisContext context) where TEntity : class
            {
                List<ContainerAnalysisInfo> followedAnalyzers = null;
                List<IContainerAnalyzerProvider> blocked = null;

                var enumerator = activeAnalyzers.GetEnumerator();

                async ValueTask<AnalysisResult> GetResult(ContainerBehaviour behaviour)
                {
                    if((behaviour & ContainerBehaviour.FollowChildren) != 0)
                    {
                        (followedAnalyzers ?? (followedAnalyzers = new List<ContainerAnalysisInfo>()))
                            .Add(enumerator.Current);
                    }
                    if((behaviour & ContainerBehaviour.BlockOther) != 0)
                    {
                        (blocked ?? (blocked = new List<IContainerAnalyzerProvider>()))
                            .Add(enumerator.Current.Provider);
                    }
                    var path = Value != null ? this : null;
                    if(enumerator.MoveNext())
                    {
                        return await enumerator.Current.Analyzer.Analyze(path, entity, context, GetResult, analyzers);
                    }
                    if(followedAnalyzers == null && blocked == null && Value != null)
                    {
                        return await analyzers.Analyze(entity, context);
                    }
                    var innerAnalyzer = analyzers;
                    if(Value != null)
                    {
                        innerAnalyzer = new ContainerNode<TEntity, IContainerNode<TValue, TParent>>(path, entity, followedAnalyzers, baseProvider, innerAnalyzer);
                        innerAnalyzer = baseProvider.MatchRoot(entity, context, innerAnalyzer, blocked) ?? innerAnalyzer;
                    }else{
                        innerAnalyzer = new ContainerNode<TEntity, IContainerNode>(path, entity, followedAnalyzers, baseProvider, innerAnalyzer);
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

        struct ContainerAnalysisInfo
        {
            public IContainerAnalyzer Analyzer { get; }
            public IContainerAnalyzerProvider Provider { get; }

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

            protected override IEnumerable<Type> SelectType(Type t)
            {
                foreach(var i in t.GetInterfaces())
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
