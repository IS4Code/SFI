using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System.Collections.Generic;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of media objects of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of objects supported by this analyzer.</typeparam>
    public abstract class MediaObjectAnalyzer<T> : EntityAnalyzer<T> where T : class
    {
        readonly IEnumerable<ClassUri> classes;

        /// <summary>
        /// Creates a new instance of the analyzer from a collection of classes applicable to the type.
        /// </summary>
        /// <param name="classes">The collection of classes.</param>
        public MediaObjectAnalyzer(IEnumerable<ClassUri> classes)
        {
            this.classes = classes;
        }

        /// <summary>
        /// Creates a new instance of the analyzer from a collection of classes applicable to the type.
        /// </summary>
        /// <param name="classes">The collection of classes.</param>
        public MediaObjectAnalyzer(params ClassUri[] classes) : this((IEnumerable<ClassUri>)classes)
        {

        }

        /// <summary>
        /// Assigns the classes specified during construction of the analyzer to newly
        /// constructed nodes.
        /// </summary>
        /// <inheritdoc/>
        protected override void InitNode(ILinkedNode node, AnalysisContext context)
        {
            base.InitNode(node, context);

            node.SetClass(Classes.MediaObject);
            foreach(var cls in classes)
            {
                node.SetClass(cls);
            }
        }
    }
}
