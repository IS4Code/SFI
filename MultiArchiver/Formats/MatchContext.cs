using IS4.MultiArchiver.Tools;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace IS4.MultiArchiver.Formats
{
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

        public MatchContext(object services = null, Stream stream = null)
        {
            if(services == null)
            {
                serviceMap = null;
            }else{
                serviceMap = ImmutableDictionary.CreateRange(GetServices(null, services));
            }
        }

        public T GetService<T>() where T : class
        {
            return serviceMap != null && serviceMap.TryGetValue(typeof(T), out var value) ? (T)value : null;
        }

        public MatchContext WithService<T>(T service)
        {
            return new MatchContext(serviceMap, typeof(T), service);
        }

        public MatchContext WithServices(object services)
        {
            return new MatchContext(serviceMap, null, services);
        }

        public MatchContext WithStream(Stream stream)
        {
            return new MatchContext(serviceMap, stream);
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
