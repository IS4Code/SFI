﻿using IS4.SFI.Application.Plugins;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF.Query.Paths;

namespace IS4.SFI.Application
{
    /// <summary>
    /// Stores information about a collection of configurable components.
    /// </summary>
    public abstract class ComponentCollection : IEquatable<ComponentCollection>
    {
        /// <summary>
        /// The collection of all components.
        /// </summary>
        public IEnumerable Collection { get; }

        /// <summary>
        /// The type of elements of the collection.
        /// </summary>
        public abstract Type ElementType { get; }

        /// <summary>
        /// The property defining the collection.
        /// </summary>
        public PropertyDescriptor? Property { get; }

        /// <summary>
        /// The component defining the collection.
        /// </summary>
        public object? Component { get; }

        /// <summary>
        /// The attribute describing the collection.
        /// </summary>
        public ComponentCollectionAttribute Attribute { get; }

        /// <summary>
        /// Creates a new instance of the collection.
        /// </summary>
        /// <param name="collection">The value of <see cref="Collection"/>.</param>
        /// <param name="property">The value of <see cref="Property"/>.</param>
        /// <param name="component">The value of <see cref="Component"/>.</param>
        /// <param name="attribute">The value of <see cref="Attribute"/>.</param>
        public ComponentCollection(IEnumerable collection, PropertyDescriptor? property, object? component, ComponentCollectionAttribute attribute)
        {
            Collection = collection;
            Property = property;
            Component = component;
            Attribute = attribute;
        }

        /// <inheritdoc/>
        public bool Equals(ComponentCollection other)
        {
            return Collection == other.Collection;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is ComponentCollection info && Equals(info);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return RuntimeHelpers.GetHashCode(Collection);
        }

        /// <summary>
        /// Returns the identifier of a component with the prefix of this collection.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <returns>The identifier of the component.</returns>
        public virtual string GetIdentifier(object component)
        {
            return $"{Attribute.Prefix}:{TextTools.GetUserFriendlyName(component)}";
        }

        static readonly Type genericType = typeof(ComponentCollection<>);

        /// <summary>
        /// Checks whether <paramref name="type"/> is valid to construct
        /// an instance of <see cref="ComponentCollection{T}"/>.
        /// </summary>
        /// <param name="type">The type argument to check.</param>
        /// <returns>
        /// Whether a <see cref="ComponentCollection{T}"/> instance can
        /// be constructed from <paramref name="type"/>.
        /// </returns>
        public static bool IsTypeArgumentValid(Type type)
        {
            return !type.IsValueType;
        }

        /// <summary>
        /// Creates a new instance of the collection with a particular element type.
        /// </summary>
        /// <param name="elementType">The element type of the collection.</param>
        /// <param name="collection"><inheritdoc path="/param[@name='collection']" cref="ComponentCollection.ComponentCollection(IEnumerable, PropertyDescriptor, object, ComponentCollectionAttribute)"/></param>
        /// <param name="property"><inheritdoc path="/param[@name='property']" cref="ComponentCollection.ComponentCollection(IEnumerable, PropertyDescriptor, object, ComponentCollectionAttribute)"/></param>
        /// <param name="component"><inheritdoc path="/param[@name='component']" cref="ComponentCollection.ComponentCollection(IEnumerable, PropertyDescriptor, object, ComponentCollectionAttribute)"/></param>
        /// <param name="attribute"><inheritdoc path="/param[@name='attribute']" cref="ComponentCollection.ComponentCollection(IEnumerable, PropertyDescriptor, object, ComponentCollectionAttribute)"/></param>
        /// <returns>A new instance of the collection.</returns>
        public static ComponentCollection Create(Type elementType, IEnumerable collection, PropertyDescriptor property, object? component, ComponentCollectionAttribute attribute)
        {
            return (ComponentCollection)Activator.CreateInstance(genericType.MakeGenericType(elementType), collection, property, component, attribute);
        }

        /// <summary>
        /// Creates a new instance of a component for this collection.
        /// </summary>
        /// <typeparam name="TResult">The return type of <paramref name="resultFactory"/>.</typeparam>
        /// <typeparam name="TArgs">The arguments type of <paramref name="resultFactory"/>.</typeparam>
        /// <param name="component">The component type to create an instance of.</param>
        /// <param name="resultFactory">A factory object receiving the created instance.</param>
        /// <param name="args">Arguments of <paramref name="resultFactory"/>.</param>
        /// <returns>The results of <paramref name="resultFactory"/>.</returns>
        public abstract IAsyncEnumerable<TResult?> CreateInstance<TResult, TArgs>(ComponentType component, IResultFactory<TResult, TArgs> resultFactory, TArgs args);

        /// <summary>
        /// Invokes <paramref name="resultFactory"/> over each element in the collection.
        /// </summary>
        /// <param name="resultFactory">The function object, receiving the element and its name.</param>
        /// <returns>An empty task.</returns>
        public abstract ValueTask ForEach(IResultFactory<ValueTuple, string> resultFactory);

        /// <summary>
        /// Invokes <paramref name="resultFactory"/> for each element in the collection
        /// and removes non-matching elements.
        /// </summary>
        /// <param name="resultFactory">
        /// The function object, receiving the element and its name.
        /// If it returns <see langword="false"/>, the element is removed from the collection.
        /// </param>
        /// <returns>The total number of elements.</returns>
        public abstract ValueTask<int> Filter(IResultFactory<bool, string> resultFactory);
    }
    /// <summary>
    /// Stores information about a collection of configurable components, constrained to
    /// type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the components in the collection.</typeparam>
    public class ComponentCollection<T> : ComponentCollection where T : class
    {
        static readonly Type elementType = typeof(T);

        /// <inheritdoc/>
        public override Type ElementType => elementType;

        /// <inheritdoc cref="ComponentCollection.Collection"/>
        public new ICollection<T> Collection => (ICollection<T>)base.Collection;

        /// <inheritdoc cref="ComponentCollection.ComponentCollection(IEnumerable, PropertyDescriptor, object, ComponentCollectionAttribute)"/>
        public ComponentCollection(ICollection<T> collection, PropertyDescriptor? property, object? component, ComponentCollectionAttribute attribute) : base(collection, property, component, attribute)
        {

        }

        /// <inheritdoc/>
        public override async IAsyncEnumerable<TResult?> CreateInstance<TResult, TArgs>(ComponentType component, IResultFactory<TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            // Check that a type produced from the component can be added to the collection
            if(MatchType(component.Type, true).Any(elementType.IsAssignableFrom))
            {
                var inst = component.GetInstance();
                if(inst is T instance)
                {
                    yield return await resultFactory.Invoke(instance, args);
                }
                if(inst is IAsyncEnumerable<T> asyncEnumerable)
                {
                    await foreach(var obj in asyncEnumerable)
                    {
                        yield return await resultFactory.Invoke(obj, args);
                    }
                }else if(inst is IEnumerable<T> enumerable)
                {
                    foreach(var obj in enumerable)
                    {
                        yield return await resultFactory.Invoke(obj, args);
                    }
                }
            }
        }

        static readonly Type enumerableType = typeof(IEnumerable<>);
        static readonly Type asyncEnumerableType = typeof(IAsyncEnumerable<>);

        /// <summary>
        /// Checks whether <paramref name="componentType"/> matches the type desired by this
        /// collection, directly and through implementation of <see cref="IEnumerable{T}"/>
        /// or <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        /// <param name="componentType">The type of the component.</param>
        /// <param name="allowCollections">Whether to allow collection types.</param>
        /// <returns>The sequence of all types returned from the component.</returns>
        IEnumerable<Type> MatchType(Type componentType, bool allowCollections)
        {
            var commonType = Attribute.CommonType;
            if(commonType != null)
            {
                if(!commonType.IsGenericTypeDefinition)
                {
                    // Type is directly assignable
                    if(commonType.IsAssignableFrom(componentType))
                    {
                        yield return componentType;
                    }
                    // Continues below
                }else{
                    foreach(var i in componentType.GetInterfaces())
                    {
                        if(i.IsGenericType)
                        {
                            var def = i.GetGenericTypeDefinition();
                            // The interface definition matches the collection type
                            if(commonType.Equals(def))
                            {
                                yield return componentType;
                                if(!allowCollections)
                                {
                                    // No need to check further
                                    break;
                                }
                            }
                            // Top-level interface is IEnumerable or IAsyncEnumerable
                            if(allowCollections && (enumerableType.Equals(def) || asyncEnumerableType.Equals(def)))
                            {
                                foreach(var type in MatchType(i.GetGenericArguments()[0], false))
                                {
                                    yield return type;
                                }
                            }
                        }
                    }
                    yield break;
                }
            }else{
                // Automatically available
                yield return componentType;
            }
            if(allowCollections)
            {
                // It implements IEnumerable or IAsyncEnumerable of a matching type (only top-level)
                foreach(var i in componentType.GetInterfaces())
                {
                    if(i.IsGenericType)
                    {
                        var def = i.GetGenericTypeDefinition();
                        if(enumerableType.Equals(def) || asyncEnumerableType.Equals(def))
                        {
                            foreach(var type in MatchType(i.GetGenericArguments()[0], false))
                            {
                                yield return type;
                            }
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override async ValueTask ForEach(IResultFactory<ValueTuple, string> resultFactory)
        {
            foreach(var item in Collection.OrderBy(item => item, NameComparer.Instance))
            {
                await resultFactory.Invoke(item, GetIdentifier(item));
            }
        }

        /// <inheritdoc/>
        public override async ValueTask<int> Filter(IResultFactory<bool, string> resultFactory)
        {
            if(!Collection.IsReadOnly) switch(Collection)
            {
                case List<T> list:
                    try{
                        list.RemoveAll(item => !resultFactory.Invoke(item, GetIdentifier(item)).Result);
                    }catch(SynchronizationLockException)
                    {
                        goto default;
                    }
                    break;
                case ICollection<T> collection:
                    var removing = new List<T>();
                    foreach(var item in collection)
                    {
                        if(!await resultFactory.Invoke(item, GetIdentifier(item)))
                        {
                            removing.Add(item);
                        }
                    }
                    try{
                        foreach(var removed in removing)
                        {
                            collection.Remove(removed);
                        }
                    }catch(NotSupportedException)
                    {
                        goto default;
                    }
                    break;
                default:
                    var remaining = new List<T>();
                    foreach(var item in Collection)
                    {
                        if(await resultFactory.Invoke(item, GetIdentifier(item)))
                        {
                            remaining.Add(item);
                        }
                    }
                    if(remaining.Count < Collection.Count)
                    {
                        Collection.Clear();
                        foreach(var item in remaining)
                        {
                            Collection.Add(item);
                        }
                    }
                    break;
            }

            return Collection.Count;
        }

        class NameComparer : IComparer<T>
        {
            const string baseName = nameof(IS4) + ".";

            public static readonly NameComparer Instance = new();

            static readonly Func<char, bool> dots = c => c == '.';

            public int Compare(T x, T y)
            {
                var tx = x.GetType();
                var ty = y.GetType();
                var axname = tx.Assembly.GetName().Name;
                var ayname = ty.Assembly.GetName().Name;
                if(axname.StartsWith(baseName) && !ayname.StartsWith(baseName))
                {
                    return -1;
                }
                if(ayname.StartsWith(baseName) && !axname.StartsWith(baseName))
                {
                    return 1;
                }
                var result = axname.Count(dots).CompareTo(ayname.Count(dots));
                if(result != 0) return result;
                result = tx.Namespace.Count(dots).CompareTo(ty.Namespace.Count(dots));
                if(result != 0) return result;
                return TextTools.GetUserFriendlyName(x).CompareTo(TextTools.GetUserFriendlyName(y));
            }
        }
    }
}
