using IS4.SFI.Application.Tools;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;

namespace IS4.SFI.Application.Plugins
{

    /// <summary>
    /// Stores information about an externally loadable component.
    /// </summary>
    public class ComponentType
    {
        /// <summary>
        /// The type of the component.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The instance of the component, or error.
        /// </summary>
        readonly Lazy<object?> instance;

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="type">The value of <paramref name="type"/>.</param>
        /// <param name="factory">The factory function producing instances for the component.</param>
        /// <param name="inspector">The instance of <see cref="ComponentInspector"/> storing the components.</param>
        public ComponentType(Type type, Func<object> factory, ComponentInspector inspector)
        {
            Type = type;

            Func<object?> innerFactory = () => {
                try{
                    return factory();
                }catch(Exception e) when(GlobalOptions.SuppressNonCriticalExceptions)
                {
                    inspector?.OutputLog?.LogError(e, $"An exception occurred while creating an instance of type {Type} from assembly {Type.Assembly.GetName().Name}.");
                    return null;
                }
            };

            instance = new(() => {
                var result = innerFactory();
                if(result != null && ConfigurationTools.GetConfigurableProperties(result).Any())
                {
                    // Value got created, but there are properties, so it should not be cached.
                    result = new InstanceFactory(result, innerFactory);
                }
                return result;
            }, false);
        }

        /// <summary>
        /// Retrieves or creates a new instance of the component.
        /// </summary>
        /// <returns>The created instance.</returns>
        public object? GetInstance()
        {
            // Re-use instance if already created
            var value = instance.Value;
            return value is InstanceFactory factory ? factory.Next() : value;
        }

        class InstanceFactory
        {
            object? firstResult;
            readonly Func<object?> factory;

            public InstanceFactory(object firstResult, Func<object?> factory)
            {
                this.firstResult = firstResult;
                this.factory = factory;
            }

            public object? Next()
            {
                if(Interlocked.Exchange(ref firstResult, null) is { } result)
                {
                    // First time obtaining the result
                    return result;
                }
                return factory();
            }
        }
    }
}
