using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace IS4.SFI.Application
{
    /// <summary>
    /// Provides an implementation of <see cref="ILogger"/> that prepends the name
    /// of the component to each message.
    /// </summary>
    public class ComponentLogger : ILogger
    {
        readonly AsyncLocal<AsyncState> logState = new();

        /// <summary>
        /// Stores a counter for every encountered entity type.
        /// </summary>
        readonly ConcurrentDictionary<Type, ObjectIDCounter> typeCounters = new();

        readonly TextWriter output;

        AsyncState LogState => logState.Value ??= new(this);

        /// <summary>
        /// The minimum level necessary to write messages to output.
        /// </summary>
        public LogLevel EnabledMinLevel { get; set; }

        /// <summary>
        /// Creates a new instance of the logger from a <see cref="TextWriter"/> instance.
        /// </summary>
        /// <param name="output">The writer that shall be used to write log messages.</param>
        public ComponentLogger(TextWriter output)
        {
            this.output = output;
        }

        /// <inheritdoc/>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            if(EnabledMinLevel >= LogLevel.None) return null;
            return LogState.BeginScope(state);
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= EnabledMinLevel;
        }

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if(IsEnabled(logLevel))
            {
                LogState.Log(logLevel, eventId, state, exception, formatter);
            }
        }

        class AsyncState
        {
            readonly LinkedList<Scope> scopes = new();

            readonly ComponentLogger logger;

            public AsyncState(ComponentLogger logger)
            {
                this.logger = logger;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                var message = formatter(state, exception);
                if(scopes.Last is { Value: var scope })
                {
                    logger.output?.WriteLine(scope + " " + message);
                }else{
                    logger.output?.WriteLine(message);
                }
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            {
                return new Scope<TState>(logger, scopes, state);
            }

            abstract class Scope : IDisposable
            {
                readonly LinkedList<Scope> scopes;
                readonly LinkedListNode<Scope> node;

                public Scope(LinkedList<Scope> scopes)
                {
                    this.scopes = scopes;
                    node = scopes.AddLast(this);
                }

                public void Dispose()
                {
                    scopes.Remove(node);
                }

                public abstract override string ToString();
            }

            class Scope<TState> : Scope where TState : notnull
            {
                readonly string key;

                public Scope(ComponentLogger logger, LinkedList<Scope> scopes, TState state) : base(scopes)
                {
                    var key = TextTools.GetIdentifierFromType<TState>();
                    var id = logger.typeCounters.GetOrAdd(typeof(TState), _ => new()).GetId(state);
                    this.key = id == 1 ? $"[{key}]" : $"[{key}#{id}]";
                }

                public override string ToString()
                {
                    return key;
                }
            }
        }

        class ObjectIDCounter
        {
            readonly ConditionalWeakTable<object, StrongBox<long>> counter = new();
            long count;

            public long GetId(object obj)
            {
                return counter.GetValue(obj, _ => new(Interlocked.Increment(ref count))).Value;
            }
        }
    }
}
