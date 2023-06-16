using IS4.SFI.Application.Plugins;
using IS4.SFI.Application.Tools;
using IS4.SFI.RDF;
using Microsoft.Extensions.DependencyInjection;
using MorseCode.ITask;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace IS4.SFI.Application
{
    /// <summary>
    /// An implementation of <see cref="Inspector"/> allowing automatic management
    /// of component collections.
    /// </summary>
    public class ComponentInspector : Inspector, IResultFactory<object?, IEnumerable>
    {
        readonly HashSet<ComponentCollection> componentCollections = new();

        /// <summary>
        /// Stores all component collections managed by the instance.
        /// </summary>
        public IReadOnlyCollection<ComponentCollection> ComponentCollections => componentCollections;

        /// <summary>
        /// Stores the configuration for all instances of <see cref="ITripleFormatter"/>.
        /// </summary>
        public TypeConfigurationCollection FormatterCollection { get; } = new("rdf-formatter");

        /// <summary>
        /// Stores the configuration for all instances of <see cref="IRdfWriter"/>
        /// and <see cref="IStoreWriter"/>.
        /// </summary>
        public TypeConfigurationCollection RdfWriterCollection { get; } = new("rdf-writer");

        /// <summary>
        /// Stores the configuration for all instances of <see cref="IRdfHandler"/>.
        /// </summary>
        public TypeConfigurationCollection RdfHandlerCollection { get; } = new("rdf-handler");

        /// <summary>
        /// Stores the configuration for all instances of <see cref="ISparqlResultsWriter"/>.
        /// </summary>
        public TypeConfigurationCollection SparqlWriterCollection { get; } = new("sparql-writer");

        /// <inheritdoc/>
        public ComponentInspector()
        {
            CaptureCollections(this);

            foreach(var definition in MimeTypesHelper.Definitions)
            {
                var id = TextTools.GetMimeTypeSimpleName(definition.CanonicalMimeType);
                if(definition.CanWriteRdf)
                {
                    RdfWriterCollection.AddType(definition.RdfWriterType, id);
                    if(definition.GetRdfWriter() is IFormatterBasedWriter { TripleFormatterType: Type formatter })
                    {
                        FormatterCollection.AddType(formatter, id);
                    }
                }
                if(definition.CanWriteRdfDatasets)
                {
                    RdfWriterCollection.AddType(definition.RdfDatasetWriterType, id);
                    if(definition.GetRdfDatasetWriter() is IFormatterBasedWriter { TripleFormatterType: Type formatter })
                    {
                        FormatterCollection.AddType(formatter, id);
                    }
                }
                if(definition.CanWriteSparqlResults)
                {
                    SparqlWriterCollection.AddType(definition.SparqlResultsWriterType, id);
                    if(definition.GetSparqlResultsWriter() is IFormatterBasedWriter { TripleFormatterType: Type formatter })
                    {
                        FormatterCollection.AddType(formatter, id);
                    }
                }
            }

            RdfHandlerCollection.AddType(typeof(TurtleHandler<TurtleFormatter>), TextTools.GetMimeTypeSimpleName("text/turtle"));
            RdfHandlerCollection.AddType(typeof(JsonLdHandler), TextTools.GetMimeTypeSimpleName("application/ld+json"));

            componentCollections.Add(FormatterCollection);
            componentCollections.Add(RdfWriterCollection);
            componentCollections.Add(RdfHandlerCollection);
            componentCollections.Add(SparqlWriterCollection);
        }

        /// <inheritdoc/>
        protected override void ConfigureNewComponent(object component)
        {
            base.ConfigureNewComponent(component);

            var type = component.GetType();
            foreach(var collection in ComponentCollections.OfType<TypeConfigurationCollection>())
            {
                if(collection.TypeMap.TryGetValue(type, out var configuration))
                {
                    foreach(var property in configuration.Properties)
                    {
                        property.Key.SetValue(component, property.Value);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async override ValueTask AddDefault()
        {
            await base.AddDefault();

            CaptureCollections(this, true);
        }

        static readonly Type enumerableType = typeof(IEnumerable);
        static readonly Type collectionType = typeof(ICollection<>);

        /// <summary>
        /// Browses an object, looking for all properties with <see cref="ComponentCollectionAttribute"/>
        /// and loading the component collections within.
        /// </summary>
        /// <param name="instance">The instance to browse.</param>
        /// <param name="updateExisting">Whether to update previously found collections.</param>
        protected virtual void CaptureCollections(object instance, bool updateExisting = false)
        {
            foreach(System.ComponentModel.PropertyDescriptor property in System.ComponentModel.TypeDescriptor.GetProperties(instance))
            {
                if(property.IsBrowsable && enumerableType.IsAssignableFrom(property.PropertyType))
                {
                    var attribute = property.Attributes.OfType<ComponentCollectionAttribute>().FirstOrDefault();
                    if(attribute != null)
                    {
                        var collection = (IEnumerable)property.GetValue(instance);
                        if(collection != null)
                        {
                            var type = collection.GetType().GetInterfaces().FirstOrDefault(i => i.IsGenericType && collectionType.Equals(i.GetGenericTypeDefinition()));
                            if(type != null)
                            {
                                var itemType = type.GetGenericArguments()[0];
                                var components = ComponentCollection.Create(itemType, collection, attribute);
                                if(componentCollections.Add(components) || updateExisting)
                                {
                                    foreach(var element in collection)
                                    {
                                        CaptureCollections(element);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds the given <see cref="ComponentType"/> instance into the existing collections.
        /// </summary>
        /// <param name="component">The component to add.</param>
        /// <returns>The number of individual component items that were added.</returns>
        protected virtual async ValueTask<int> LoadIntoCollections(ComponentType component)
        {
            int componentCount = 0;

            foreach(var collection in ComponentCollections.ToArray())
            {
                await foreach(var instance in collection.CreateInstance(component, this, collection.Collection))
                {
                    if(instance != null)
                    {
                        componentCount++;
                        CaptureCollections(instance);
                    }
                }
            }

            return componentCount;
        }

        async ITask<object?> IResultFactory<object?, IEnumerable>.Invoke<T>(T value, IEnumerable sequence)
        {
            if(sequence is ICollection<T> collection)
            {
                collection.Add(value);
                return value;
            }
            return null;
        }

        /// <summary>
        /// Retrieves all types in an assembly that could be loaded as components.
        /// </summary>
        /// <param name="assembly">The assembly to browse.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> instance that is used for initializing the components.</param>
        /// <returns>The collection of all applicable component types.</returns>
        protected virtual IEnumerable<ComponentType> OpenAssembly(Assembly assembly, IServiceProvider serviceProvider)
        {
            foreach(var type in assembly.ExportedTypes.Concat(assembly.GetForwardedOrReferencedTypes().Where(t => t.IsPublic)))
            {
                // Only yield concrete instantiable browsable types
                if(!type.IsAbstract && !type.IsGenericTypeDefinition && IsTypeLoadable(type))
                {
                    yield return new ComponentType(type, () => ActivatorUtilities.CreateInstance(serviceProvider, type), this);
                }
            }
        }

        /// <summary>
        /// Loads all components in an assembly.
        /// </summary>
        /// <param name="assembly">The assembly to load from.</param>
        /// <returns>The number of individual component items that were added.</returns>
        protected virtual async ValueTask<int> LoadAssembly(Assembly assembly)
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            int count = 0;

            foreach(var component in OpenAssembly(assembly, serviceProvider))
            {
                count += await LoadIntoCollections(component);
            }

            return count;
        }

        static readonly Type BrowsableAttributeType = typeof(BrowsableAttribute);

        static bool IsTypeLoadable(Type type)
        {
            var attributes = TypeDescriptor.GetAttributes(type);
            if(attributes[BrowsableAttributeType] is BrowsableAttribute { Browsable: false })
            {
                return false;
            }
            bool foundSupported = false;
            foreach(var attributeData in type.GetCustomAttributesData())
            {
                var args = attributeData.ConstructorArguments;
                if(args.Count == 1 && args[0].Value is string arg)
                {
                    switch(attributeData.AttributeType.FullName)
                    {
                        case "System.Runtime.Versioning.SupportedOSPlatformAttribute":
                            foundSupported = true;
                            if(IsOSPlatform(arg))
                            {
                                return true;
                            }
                            break;
                        case "System.Runtime.Versioning.UnsupportedOSPlatformAttribute":
                            if(IsOSPlatform(arg))
                            {
                                return false;
                            }
                            break;
                    }
                }
            }
            return !foundSupported;
        }

        static readonly Regex architectureRegex = new Regex("^(.+?)-([^-]+)$", RegexOptions.Compiled);

        static readonly Regex versionRegex = new Regex(@"^(.+?)\.?((?:\d+\.)*\d+)$", RegexOptions.Compiled);

        static bool IsOSPlatform(string platform)
        {
            if(architectureRegex.Match(platform) is { Success: true } architectureMatch && Enum.TryParse<Architecture>(architectureMatch.Groups[2].Value, out var architecture))
            {
                if(RuntimeInformation.ProcessArchitecture != architecture)
                {
                    return false;
                }
                platform = architectureMatch.Groups[1].Value;
            }
            if(versionRegex.Match(platform) is { Success: true } versionMatch && Version.TryParse(versionMatch.Groups[2].Value, out var version))
            {
                if(Environment.OSVersion.Version < version)
                {
                    return false;
                }
                platform = versionMatch.Groups[1].Value;
            }
            if(platform is "*" or "")
            {
                return true;
            }
            var osPlatform = OSPlatform.Create(platform);
            if(!RuntimeInformation.IsOSPlatform(osPlatform))
            {
                return false;
            }
            return true;
        }
    }
}
