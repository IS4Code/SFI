using System;

namespace IS4.SFI.Application
{
    /// <summary>
    /// Stores information about an externally loadable component.
    /// </summary>
    public class ComponentType
    {
        readonly ComponentInspector inspector;
        readonly Func<object> factory;

        /// <summary>
        /// The type of the component.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// <see langword="true"/> if an error occurred during the creation of the object.
        /// </summary>
        public bool Error { get; private set; }

        /// <summary>
        /// The instance of the component, if previously created.
        /// </summary>
        public object? Instance { get; private set; }

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="type">The value of <paramref name="type"/>.</param>
        /// <param name="factory">The factory function producing instances for the component.</param>
        /// <param name="inspector">The instance of <see cref="ComponentInspector"/> storing the components.</param>
        public ComponentType(Type type, Func<object> factory, ComponentInspector inspector)
        {
            this.inspector = inspector;
            this.factory = factory;
            Type = type;
        }

        /// <summary>
        /// Retrieves or creates a new instance of the component.
        /// </summary>
        /// <returns>The created instance.</returns>
        public object? GetInstance()
        {
            // Re-use instance if already created
            if(Instance == null && !Error)
            {
                try{
                    Instance = factory();
                }catch(Exception e) when(GlobalOptions.SuppressNonCriticalExceptions)
                {
                    inspector?.OutputLog?.WriteLine($"An exception occurred while creating an instance of type {Type} from assembly {Type.Assembly.GetName().Name}: {e}");
                    // Prevents attempting to create the instance next time
                    Error = true;
                }
            }
            return Instance;
        }
    }
}
