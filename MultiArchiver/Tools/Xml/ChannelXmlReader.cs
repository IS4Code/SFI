using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml;

namespace IS4.MultiArchiver.Tools.Xml
{
    public class ChannelXmlReader : DelegatingXmlReader, IEnumerator<XmlReaderState>
    {
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

        public ChannelXmlReader(ChannelReader<XmlReaderState> channel, XmlReader reader = null)
        {
            this.channel = channel ?? throw new ArgumentNullException(nameof(channel));
            globalReader = reader;
        }

        public static ChannelXmlReader Create(XmlReader reader, out ChannelWriter<XmlReaderState> writer, int? capacity = null)
        {
            var ch = capacity is int i ? Channel.CreateBounded<XmlReaderState>(new BoundedChannelOptions(i)
            {
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
                    var task = channel.ReadAsync();
                    if(task.IsCompletedSuccessfully)
                    {
                        currentState = task.Result;
                    }else{
                        currentState = task.AsTask().Result;
                    }
                }
                return (currentState?.NodeType ?? 0) != 0;
            }catch(AggregateException e) when(e.InnerExceptions.Count == 1 && e.InnerException is ChannelClosedException)
            {
                return false;
            }catch(ChannelClosedException)
            {
                return false;
            }
        }

        public override async Task<bool> ReadAsync()
        {
            try{
                if(!channel.TryRead(out currentState))
                {
                    currentState = await channel.ReadAsync();
                }
                return (currentState?.NodeType ?? 0) != 0;
            }catch(AggregateException e) when(e.InnerExceptions.Count == 1 && e.InnerException is ChannelClosedException)
            {
                return false;
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
