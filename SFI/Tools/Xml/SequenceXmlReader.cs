using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IS4.SFI.Tools.Xml
{
    /// <summary>
    /// Provides an implementation of <see cref="XmlReader"/>
    /// that joins together a sequence of inner <see cref="XmlReader"/>
    /// instances.
    /// </summary>
    public class SequenceXmlReader : DelegatingXmlReader
    {
        bool started;
        readonly IEnumerator<XmlReader> enumerator;

        XmlReader Current => enumerator.Current;

        /// <inheritdoc/>
        protected override XmlReader? ScopeReader => Current;

        /// <inheritdoc/>
        protected override XmlReader? QueryReader => Current;

        /// <inheritdoc/>
        protected override XmlReader? GlobalReader => Current;

        /// <inheritdoc/>
        protected override XmlReader? ActiveReader => Current;

        /// <inheritdoc/>
        protected override XmlReader? PassiveReader => Current;

        /// <summary>
        /// Creates a new reader from a sequence of readers.
        /// </summary>
        /// <param name="sequence">The inner sequence to read from.</param>
        /// <exception cref="ArgumentException"><paramref name="sequence"/> is empty.</exception>
        public SequenceXmlReader(IEnumerable<XmlReader> sequence)
        {
            enumerator = sequence.GetEnumerator();
        }
        
        /// <inheritdoc/>
        public override bool Read()
        {
            if(!started)
            {
                // First move the enumerator
                var result = enumerator.MoveNext();
                started = true;
                return result;
            }
            if(!Current.Read() && !enumerator.MoveNext())
            {
                // Nothing after the current element
                return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public async override Task<bool> ReadAsync()
        {
            if(!started)
            {
                // First move the enumerator
                var result = enumerator.MoveNext();
                started = true;
                return result;
            }
            if(!await Current.ReadAsync() && !enumerator.MoveNext())
            {
                // Nothing after the current element
                return false;
            }
            return true;
        }
    }
}
