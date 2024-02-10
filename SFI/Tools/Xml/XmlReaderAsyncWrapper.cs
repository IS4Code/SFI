using System;
using System.Threading.Tasks;
using System.Xml;

namespace IS4.SFI.Tools.Xml
{
    /// <summary>
    /// A wrapper of <see cref="XmlReader"/> that implements
    /// <see cref="XmlReader.ReadAsync"/> and <see cref="XmlReader.GetValueAsync"/>
    /// using their synchronous equivalents, if they are not supported.
    /// </summary>
    public sealed class XmlReaderAsyncWrapper : DelegatingXmlReader
    {
        readonly XmlReader innerReader;
        bool tryAsync = true;

        /// <inheritdoc/>
        protected override XmlReader? ScopeReader => innerReader;

        /// <inheritdoc/>
        protected override XmlReader? QueryReader => innerReader;

        /// <inheritdoc/>
        protected override XmlReader? GlobalReader => innerReader;

        /// <inheritdoc/>
        protected override XmlReader? ActiveReader => innerReader;

        /// <inheritdoc/>
        protected override XmlReader? PassiveReader => innerReader;

        /// <summary>
        /// Creates a new instance of the reader.
        /// </summary>
        /// <param name="innerReader">The underlying <see cref="XmlReader"/> instance to wrap.</param>
        /// <param name="tryAsync">Whether to the asynchronous methods the first time to see if they are supported.</param>
        public XmlReaderAsyncWrapper(XmlReader innerReader, bool tryAsync = true)
        {
            this.innerReader = innerReader;
            this.tryAsync = tryAsync;
        }

        /// <inheritdoc/>
        public async override Task<bool> ReadAsync()
        {
            if(tryAsync)
            {
                try{
                    return await base.ReadAsync();
                }catch(NotImplementedException)
                {
                    tryAsync = false;
                }catch(NotSupportedException)
                {
                    tryAsync = false;
                }
            }
            return Read();
        }

        /// <inheritdoc/>
        public async override Task<string> GetValueAsync()
        {
            if(tryAsync)
            {
                try{
                    return await base.GetValueAsync();
                }catch(NotImplementedException)
                {
                    tryAsync = false;
                }catch(NotSupportedException)
                {
                    tryAsync = false;
                }
            }
            return Value;
        }
    }
}
