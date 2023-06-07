using Microsoft.Extensions.Logging;
using System;

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
        readonly Lazy<(bool success, object result)> instance;

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="type">The value of <paramref name="type"/>.</param>
        /// <param name="factory">The factory function producing instances for the component.</param>
        /// <param name="inspector">The instance of <see cref="ComponentInspector"/> storing the components.</param>
        public ComponentType(Type type, Func<object> factory, ComponentInspector inspector)
        {
            Type = type;

            instance = new(() => {
                try{
                    return (true, factory());
                }catch(Exception e) when(GlobalOptions.SuppressNonCriticalExceptions)
                {
                    inspector?.OutputLog?.LogError(e, $"An exception occurred while creating an instance of type {Type} from assembly {Type.Assembly.GetName().Name}.");
                    return (false, e);
                }
            }, false);
        }

        /// <summary>
        /// Retrieves or creates a new instance of the component.
        /// </summary>
        /// <returns>The created instance.</returns>
        public object? GetInstance()
        {
            // Re-use instance if already created
            var (success, value) = instance.Value;
            return success ? value : null;
        }
    }
}
