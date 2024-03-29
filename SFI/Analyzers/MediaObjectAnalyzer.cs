﻿using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System.Collections.Generic;
using System.Linq;

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
        /// Whether to omit assigning <see cref="Classes.MediaObject"/> to the node.
        /// </summary>
        protected bool SkipMediaObjectClass { get; set; }

        /// <inheritdoc cref="MediaObjectAnalyzer{T}.MediaObjectAnalyzer(IEnumerable{ClassUri}, ClassUri[])"/>
        public MediaObjectAnalyzer(IEnumerable<ClassUri> classes)
        {
            this.classes = classes;
        }

        /// <inheritdoc cref="MediaObjectAnalyzer{T}.MediaObjectAnalyzer(IEnumerable{ClassUri}, ClassUri[])"/>
        public MediaObjectAnalyzer(params ClassUri[] classes) : this((IEnumerable<ClassUri>)classes)
        {

        }

        /// <summary>
        /// Creates a new instance of the analyzer from a collection of classes applicable to the type.
        /// </summary>
        /// <param name="classes">The collection of classes.</param>
        /// <param name="additionalClasses">Additional classes appended to <paramref name="classes"/>.</param>
        public MediaObjectAnalyzer(IEnumerable<ClassUri> classes, params ClassUri[] additionalClasses) : this(classes.Concat(additionalClasses))
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

            if(!SkipMediaObjectClass)
            {
                node.SetClass(Classes.MediaObject);
            }

            foreach(var cls in classes)
            {
                node.SetClass(cls);
            }
        }
    }
}
