using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IS4.SFI.Services
{
    using INamespaceGrouping = IGrouping<string?, DirectoryTools.EntryInfo<Type>>;

    /// <summary>
    /// Represents a collection of types and namespaces, as instances of
    /// <see cref="Type"/> and <see cref="Namespace"/> sharing the same namespace,
    /// in a particular assembly.
    /// </summary>
    public abstract class Namespace : ICustomAttributeProvider, IGrouping<string, Type>, IGrouping<string, Namespace>, IIdentityKey
    {
        /// <summary>
        /// The assembly containing this namespace.
        /// </summary>
        public abstract Assembly Assembly { get; }

        /// <summary>
        /// Whether this is the global namespace in the assembly.
        /// </summary>
        public bool IsGlobal => String.IsNullOrEmpty(FullName);

        /// <summary>
        /// The full name of the namespace, including parent namespaces.
        /// </summary>
        public abstract string FullName { get; }

        /// <summary>
        /// The local name of the namespace.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The full name of the parent namespace.
        /// </summary>
        public abstract string NamespaceName { get; }

        /// <summary>
        /// The full name of the namespace, including the assembly.
        /// </summary>
        public string AssemblyQualifiedName => FullName + ", " + Assembly;

        /// <summary>
        /// All types located in this namespace.
        /// </summary>
        public virtual IEnumerable<Type> DefinedTypes => GetTypes();

        /// <summary>
        /// All exported types located in this namespace.
        /// </summary>
        public virtual IEnumerable<Type> ExportedTypes => GetExportedTypes();

        /// <summary>
        /// All namespaces located in this namespace.
        /// </summary>
        public virtual IEnumerable<Namespace> Namespaces => GetNamespaces();

        /// <summary>
        /// Retrieves all types located in this namespace.
        /// </summary>
        /// <returns>A collection of all types in this namespace.</returns>
        public abstract IReadOnlyCollection<Type> GetTypes();

        /// <summary>
        /// Retrieves the exported types located in this namespace.
        /// </summary>
        /// <returns>A collection of exported types in this namespace.</returns>
        public abstract IReadOnlyCollection<Type> GetExportedTypes();

        /// <summary>
        /// Retrieves the namespaces located in this namespace.
        /// </summary>
        /// <returns>A collection of all namespaces in this namespace.</returns>
        public abstract IReadOnlyCollection<Namespace> GetNamespaces();

        /// <inheritdoc/>
        public sealed override string ToString()
        {
            return (FullName == "" ? "global::" : FullName) + ", " + Assembly;
        }

        string IGrouping<string, Type>.Key => FullName;

        string IGrouping<string, Namespace>.Key => FullName;

        object? IIdentityKey.ReferenceKey => Assembly;

        object? IIdentityKey.DataKey => FullName;

        /// <inheritdoc cref="ICustomAttributeProvider.GetCustomAttributes(bool)" />
        public virtual IReadOnlyList<Attribute> GetCustomAttributes(bool inherit)
        {
            return Array.Empty<Attribute>();
        }

        /// <inheritdoc cref="ICustomAttributeProvider.GetCustomAttributes(Type, bool)" />
        public virtual IReadOnlyList<Attribute> GetCustomAttributes(Type attributeType, bool inherit)
        {
            return GetCustomAttributes(inherit).Where(obj => obj != null && attributeType.IsAssignableFrom(obj.GetType())).ToArray();
        }

        /// <inheritdoc cref="MemberInfo.GetCustomAttributesData" />
        public virtual IReadOnlyList<CustomAttributeData> GetCustomAttributesData()
        {
            return Array.Empty<CustomAttributeData>();
        }

        object[] ICustomAttributeProvider.GetCustomAttributes(bool inherit)
        {
            var list = GetCustomAttributes(inherit);
            return list as object[] ?? list.ToArray();
        }

        object[] ICustomAttributeProvider.GetCustomAttributes(Type attributeType, bool inherit)
        {
            var list = GetCustomAttributes(attributeType, inherit);
            return list as object[] ?? list.ToArray();
        }

        /// <inheritdoc/>
        public virtual bool IsDefined(Type attributeType, bool inherit)
        {
            return GetCustomAttributes(attributeType, inherit).Any();
        }

        IEnumerator<Type> IEnumerable<Type>.GetEnumerator()
        {
            return GetTypes().GetEnumerator();
        }

        IEnumerator<Namespace> IEnumerable<Namespace>.GetEnumerator()
        {
            return GetNamespaces().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetNamespaces().Cast<object>().Concat(GetTypes()).GetEnumerator();
        }

        /// <summary>
        /// Retrieves the root namespace in an assembly.
        /// </summary>
        /// <param name="assembly">The assembly to retrieve the namespace from.</param>
        /// <returns>
        /// The root namespace of the assembly, i.e. the collection of all types
        /// and namespaces with no parent namespace.
        /// </returns>
        public static Namespace FromAssembly(Assembly assembly)
        {
            return new Node(assembly);
        }

        sealed class Node : Namespace
        {
            public override Assembly Assembly { get; }

            public override string FullName => NamespaceName == "" ? Name : (NamespaceName + "." + Name);

            public override string Name { get; }

            public override string NamespaceName { get; }

            readonly Dictionary<string, Node> namespaces;
            readonly List<Type> exportedTypes;
            readonly List<Type> allTypes;

            bool initialized;
            IEnumerable<INamespaceGrouping> allGrouping = Array.Empty<INamespaceGrouping>();
            IEnumerable<INamespaceGrouping> exportedGrouping = Array.Empty<INamespaceGrouping>();

            public Node(Assembly assembly, string namespaceName, string localName)
            {
                Assembly = assembly;
                NamespaceName = namespaceName;
                Name = localName;

                namespaces = new();
                exportedTypes = new();
                allTypes = new();
            }

            public Node(Assembly assembly) : this(assembly, "", "")
            {
                allGrouping = GroupTypes(assembly.GetTypes());
                exportedGrouping = GroupTypes(assembly.GetExportedTypes());
            }

            public override IReadOnlyCollection<Type> GetTypes()
            {
                Initialize();
                return allTypes;
            }

            public override IReadOnlyCollection<Type> GetExportedTypes()
            {
                Initialize();
                return exportedTypes;
            }

            public override IReadOnlyCollection<Namespace> GetNamespaces()
            {
                Initialize();
                return namespaces.Values;
            }

            public override IReadOnlyList<Attribute> GetCustomAttributes(bool inherit)
            {
                if(!IsGlobal)
                {
                    return base.GetCustomAttributes(inherit);
                }
                return Attribute.GetCustomAttributes(Assembly, inherit).Concat(
                    Assembly.Modules.SelectMany(m => Attribute.GetCustomAttributes(m, inherit))
                ).ToList();
            }

            public override IReadOnlyList<Attribute> GetCustomAttributes(Type attributeType, bool inherit)
            {
                if(!IsGlobal)
                {
                    return base.GetCustomAttributes(attributeType, inherit);
                }
                return Attribute.GetCustomAttributes(Assembly, attributeType, inherit).Concat(
                    Assembly.Modules.SelectMany(m => Attribute.GetCustomAttributes(m, attributeType, inherit))
                ).ToList();
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                if(!IsGlobal)
                {
                    return base.IsDefined(attributeType, inherit);
                }
                return Assembly.IsDefined(attributeType, inherit) || Assembly.Modules.Any(m => m.IsDefined(attributeType, inherit));
            }

            public override IReadOnlyList<CustomAttributeData> GetCustomAttributesData()
            {
                if(!IsGlobal)
                {
                    return base.GetCustomAttributesData();
                }
                return Assembly.GetCustomAttributesData().Concat(
                    Assembly.Modules.SelectMany(m => m.GetCustomAttributesData())
                ).ToList();
            }

            void Initialize()
            {
                if(initialized)
                {
                    return;
                }
                lock(namespaces)
                {
                    if(initialized)
                    {
                        return;
                    }
                    foreach(var grouping in allGrouping)
                    {
                        if(grouping.Key is string key)
                        {
                            GetNode(key).allGrouping = GroupTypes(grouping);
                        }else{
                            allTypes.AddRange(grouping.Select(e => e.Entry));
                        }
                    }
                    foreach(var grouping in exportedGrouping)
                    {
                        if(grouping.Key is string key)
                        {
                            GetNode(key).exportedGrouping = GroupTypes(grouping);
                        }else{
                            exportedTypes.AddRange(grouping.Select(e => e.Entry));
                        }
                    }
                    initialized = true;
                }
            }

            Node GetNode(string name)
            {
                return
                    namespaces.TryGetValue(name, out var node)
                    ? node
                    : namespaces[name] = new(Assembly, FullName, name);
            }

            static IEnumerable<INamespaceGrouping> GroupTypes(IEnumerable<Type> types)
            {
                return DirectoryTools.GroupByDirectories(types, t => t.FullName, '.');
            }

            static IEnumerable<INamespaceGrouping> GroupTypes(INamespaceGrouping grouping)
            {
                return DirectoryTools.GroupByDirectories(grouping, t => t.SubPath, t => t.Entry, '.');
            }
        }
    }
}
