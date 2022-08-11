using IS4.MultiArchiver.Tools;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// Stores additional parameters relevant when matching formats.
    /// The parameters are stored as services implementing specific
    /// interfaces, retrievable via <see cref="GetService{T}"/>.
    /// </summary>
    public struct MatchContext
    {
        readonly ImmutableDictionary<Type, object> serviceMap;

        private MatchContext(ImmutableDictionary<Type, object> serviceMap, Type serviceType, object services)
        {
            if(serviceMap == null)
            {
                if(services == null)
                {
                    this.serviceMap = null;
                }else{
                    this.serviceMap = ImmutableDictionary.CreateRange(GetServices(serviceType, services));
                }
            }else{
                if(services == null)
                {
                    this.serviceMap = serviceMap;
                }else{
                    var builder = serviceMap.ToBuilder();
                    foreach(var (key, value) in GetServices(serviceType, services))
                    {
                        builder[key] = value;
                    }
                    this.serviceMap = builder.ToImmutable();
                }
            }
        }

        /// <summary>
        /// Creates a new instance from an object and its base or implemented types.
        /// </summary>
        /// <param name="services">The object to use as a basis for the services.</param>
        public MatchContext(object services = null)
        {
            if(services == null)
            {
                serviceMap = null;
            }else{
                serviceMap = ImmutableDictionary.CreateRange(GetServices(null, services));
            }
        }

        /// <summary>
        /// Attempts to retrieve a service based on its type,
        /// provided via <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the service to retrieve.</typeparam>
        /// <returns>
        /// An instance of <typeparamref name="T"/>,
        /// if stored in the context, or null otherwise.
        /// </returns>
        public T GetService<T>() where T : class
        {
            return serviceMap != null && serviceMap.TryGetValue(typeof(T), out var value) ? (T)value : null;
        }

        /// <summary>
        /// Adds a new service to the collection of existing ones,
        /// based on its type <typeparamref name="T"/>,
        /// and returns a new instance of <see cref="MatchContext"/>
        /// with the updated collection of services.
        /// </summary>
        /// <typeparam name="T">The type implemented by the service.</typeparam>
        /// <param name="service">The implementing service.</param>
        /// <returns>A new context with the service.</returns>
        public MatchContext WithService<T>(T service)
        {
            return new MatchContext(serviceMap, typeof(T), service);
        }

        /// <summary>
        /// Adds a new service to the collection of existing ones,
        /// based on all types of <paramref name="services"/>,
        /// and returns a new instance of <see cref="MatchContext"/>
        /// with the updated collection of services.
        /// </summary>
        /// <param name="service">The implementing service.</param>
        /// <returns>A new context with the service.</returns>
        public MatchContext WithServices(object services)
        {
            return new MatchContext(serviceMap, null, services);
        }

        private static IEnumerable<KeyValuePair<Type, object>> GetServices(Type serviceType, object services)
        {
            IEnumerable<Type> interfaces = services.GetType().GetInterfaces();
            interfaces = interfaces.Where(i => i.Namespace != "System.Collections" && !i.Namespace.StartsWith("System.Collections.", StringComparison.Ordinal));
            if(serviceType != null)
            {
                interfaces = interfaces.Where(i => i.IsAssignableFrom(serviceType));
            }
            return interfaces.Select(i => new KeyValuePair<Type, object>(i, services));
        }
    }
}
