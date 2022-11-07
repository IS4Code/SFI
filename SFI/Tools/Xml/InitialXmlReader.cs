using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace IS4.SFI.Tools.Xml
{
    /// <summary>
    /// An XML reader that simulates the initial state before using another reader for the
    /// rest of operations.
    /// </summary>
    public class InitialXmlReader : DelegatingXmlReader
    {
        /// <summary>
        /// The reader serving as the prototype when reading has not yet started.
        /// </summary>
        static readonly XmlReader InitialPrototype = XmlReader.Create(new StringReader(""));

        bool started;

        /// <inheritdoc/>
        protected override XmlReader GlobalReader { get; }

        /// <inheritdoc/>
        protected override XmlReader ScopeReader => started ? GlobalReader : InitialPrototype;

        /// <inheritdoc/>
        protected override XmlReader QueryReader => ScopeReader;

        /// <inheritdoc/>
        protected override XmlReader ActiveReader {
            get {
                started = true;
                return GlobalReader;
            }
        }

        /// <inheritdoc/>
        protected override XmlReader PassiveReader => GlobalReader;

        /// <summary>
        /// Creates a new instance of the reader.
        /// </summary>
        /// <param name="reader">The underlying reader to use after one call to <see cref="Read"/>.</param>
        public InitialXmlReader(XmlReader reader)
        {
            GlobalReader = reader;
        }

        /// <inheritdoc/>
        public override bool Read()
        {
            if(!started)
            {
                started = true;
                return true;
            }
            return GlobalReader.Read();
        }

        /// <inheritdoc/>
        public override Task<bool> ReadAsync()
        {
            if(!started)
            {
                started = true;
                return Task.FromResult(true);
            }
            return GlobalReader.ReadAsync();
        }
    }
}
