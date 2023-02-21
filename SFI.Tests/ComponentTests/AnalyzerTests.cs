using IS4.SFI.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace IS4.SFI.Tests
{
    /// <summary>
    /// The base class for analyzer tests.
    /// </summary>
    public abstract class AnalyzerTests : AnalyzedObjectCollection
    {
        /// <summary>
        /// The local instance of <see cref="UriCache"/>.
        /// </summary>
        protected UriCache Cache { get; } = new UriCache();

        /// <summary>
        /// The local instance of <see cref="ILinkedNode"/> and <see cref="ILinkedNodeFactory"/>.
        /// </summary>
        protected StorageLinkedNode Node { get; }

        /// <summary>
        /// The context to use when calling <see cref="IEntityAnalyzer{T}.Analyze(T, AnalysisContext, IEntityAnalyzers)"/>,
        /// storing <see cref="Node"/>.
        /// </summary>
        protected AnalysisContext Context { get; }

        /// <summary>
        /// Creates a new instance of the class and initialized its members.
        /// </summary>
        public AnalyzerTests()
        {
            var uri = new EquatableUri("x.base:", UriKind.Absolute);
            Node = new StorageLinkedNode(uri, uri, Cache);
            Context = AnalysisContext.Create(Node, Node);
        }

        /// <summary>
        /// Returns the single instance of type <typeparamref name="T"/>
        /// that was produced during previous analysis.
        /// </summary>
        /// <typeparam name="T">The exact type to look for.</typeparam>
        /// <param name="context">The variable to receive the additional stored context.</param>
        /// <returns>The resulting entity.</returns>
        protected T GetOutput<T>(out AnalysisContext context)
        {
            var result = Analyzed[typeof(T)].Single();
            context = result.context;
            return (T)result.entity;
        }

        /// <summary>
        /// Performs cleanup on the local properties.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            Analyzed.Clear();
            Node.Properties.Clear();
        }
    }
}
