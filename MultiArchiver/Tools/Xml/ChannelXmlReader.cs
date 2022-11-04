using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml;

namespace IS4.MultiArchiver.Tools.Xml
{
    /// <summary>
    /// Provides support for reading XML data from a channel of
    /// snapshots as instances of <see cref="XmlReaderState"/>.
    /// </summary>
    public class ChannelXmlReader : DelegatingXmlReader, IEnumerator<XmlReaderState>
    {
        /// <summary>
        /// The reader serving as the prototype when reading has not yet started.
        /// </summary>
        static readonly XmlReader InitialPrototype = XmlReader.Create(new StringReader(""));

        readonly XmlReader globalReader;
        readonly ChannelReader<XmlReaderState> channel;
        XmlReaderState currentState;
        IEnumerator<XmlReaderState> attributeEnumerator;
        IEnumerator<XmlReaderState> attributeValueEnumerator;

        protected override XmlReader GlobalReader => globalReader ?? ScopeReader;
        protected override XmlReader ScopeReader => attributeValueEnumerator?.Current ?? attributeEnumerator?.Current ?? currentState ?? InitialPrototype;
        protected override XmlReader QueryReader => ScopeReader;
        protected override XmlReader ActiveReader => null;
        protected override XmlReader PassiveReader => ScopeReader;

        /// <summary>
        /// Creates a new instance of the XML reader from the given channel reader.
        /// </summary>
        /// <param name="channel">The channel to read the XML states from.</param>
        /// <param name="reader">The reader to use for properties about the document.</param>
        public ChannelXmlReader(ChannelReader<XmlReaderState> channel, XmlReader reader = null)
        {
            this.channel = channel ?? throw new ArgumentNullException(nameof(channel));
            globalReader = reader;
        }

        /// <summary>
        /// Creates a new channel and retrieve its XML reader and writer.
        /// </summary>
        /// <param name="reader">The reader to use for properties about the document.</param>
        /// <param name="writer">The variable to receive the writer for the created channel.</param>
        /// <param name="capacity">The capacity of the channel, if it should be bounded.</param>
        /// <returns>The XML reader using the created channel.</returns>
        /// <remarks>
        /// The channel is created with the following settings:
        /// <list type="bullet">
        /// <item>
        ///     <term><see cref="ChannelOptions.AllowSynchronousContinuations"/></term>
        ///     <description>true</description>
        /// </item>
        /// <item>
        ///     <term><see cref="ChannelOptions.SingleReader"/></term>
        ///     <description>true</description>
        /// </item>
        /// <item>
        ///     <term><see cref="ChannelOptions.SingleWriter"/></term>
        ///     <description>true</description>
        /// </item>
        /// <item>
        ///     <term><see cref="BoundedChannelOptions.FullMode"/> (if <paramref name="capacity"/> is provided)</term>
        ///     <description><see cref="BoundedChannelFullMode.Wait"/></description>
        /// </item>
        /// </list>
        /// </remarks>
        public static ChannelXmlReader Create(XmlReader reader, out ChannelWriter<XmlReaderState> writer, int? capacity = null)
        {
            var ch = capacity is int i ? Channel.CreateBounded<XmlReaderState>(new BoundedChannelOptions(i)
            {
                FullMode = BoundedChannelFullMode.Wait,
                AllowSynchronousContinuations = true,
                SingleReader = true,
                SingleWriter = true
            }) : Channel.CreateUnbounded<XmlReaderState>(new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = true,
                SingleReader = true,
                SingleWriter = true
            });
            writer = ch.Writer;
            return new ChannelXmlReader(ch.Reader, reader);
        }

        public override bool Read()
        {
            try{
                if(!channel.TryRead(out currentState))
                {
                    var valueTask = channel.ReadAsync();
                    if(valueTask.IsCompletedSuccessfully)
                    {
                        currentState = valueTask.Result;
                    }else if(valueTask.IsFaulted)
                    {
                        var task = valueTask.AsTask();
                        if(task.Exception.InnerExceptions.Count == 1)
                        {
                            if(task.Exception.InnerException is ChannelClosedException)
                            {
                                return false;
                            }
                            ExceptionDispatchInfo.Capture(task.Exception.InnerException).Throw();
                        }
                        throw task.Exception;
                    }else{
                        currentState = valueTask.AsTask().Result;
                    }
                }
                return (currentState?.NodeType ?? 0) != 0;
            }catch(ChannelClosedException)
            {
                return false;
            }catch(AggregateException agg) when(agg.InnerExceptions.Count == 1)
            {
                if(!(agg.InnerException is ChannelClosedException))
                {
                    ExceptionDispatchInfo.Capture(agg.InnerException).Throw();
                    throw;
                }
                return false;
            }
        }

        public async override Task<bool> ReadAsync()
        {
            try{
                if(!channel.TryRead(out currentState))
                {
                    currentState = await channel.ReadAsync();
                }
                return (currentState?.NodeType ?? 0) != 0;
            }catch(ChannelClosedException)
            {
                return false;
            }
        }

        public override bool MoveToFirstAttribute()
        {
            attributeEnumerator = currentState.Attributes.GetEnumerator();
            return MoveToNextAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            if(attributeEnumerator == null) return MoveToFirstAttribute();
            if(attributeEnumerator.MoveNext())
            {
                return true;
            }
            attributeEnumerator.Dispose();
            attributeEnumerator = null;
            return false;
        }

        public override bool ReadAttributeValue()
        {
            if(attributeEnumerator == null) return false;
            if(attributeValueEnumerator == null) attributeValueEnumerator = attributeEnumerator.Current.AttributeContents.GetEnumerator();
            if(attributeValueEnumerator.MoveNext())
            {
                return true;
            }
            attributeValueEnumerator.Dispose();
            attributeValueEnumerator = null;
            return false;
        }

        public override bool MoveToElement()
        {
            if(attributeEnumerator != null)
            {
                attributeEnumerator.Dispose();
                attributeEnumerator = null;
                attributeValueEnumerator?.Dispose();
                attributeValueEnumerator = null;
                return true;
            }
            return false;
        }

        XmlReaderState IEnumerator<XmlReaderState>.Current => currentState ?? throw new InvalidOperationException();

        object IEnumerator.Current => ((IEnumerator<XmlReaderState>)this).Current;

        bool IEnumerator.MoveNext()
        {
            return Read();
        }

        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }
    }
}
