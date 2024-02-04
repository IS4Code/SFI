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
        /// <returns></returns>
        public abstract IReadOnlyCollection<Namespace> GetNamespaces();

        string IGrouping<string, Type>.Key => FullName;

        string IGrouping<string, Namespace>.Key => FullName;

        object? IIdentityKey.ReferenceKey => Assembly;

        object? IIdentityKey.DataKey => FullName;

        /// <inheritdoc/>
        public virtual object[] GetCustomAttributes(bool inherit)
        {
            return Array.Empty<object>();
        }

        /// <inheritdoc/>
        public virtual object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return Array.Empty<object>();
        }

        /// <inheritdoc/>
        public virtual bool IsDefined(Type attributeType, bool inherit)
        {
            return false;
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
            return new Root(assembly, GroupTypes(assembly.GetTypes()), GroupTypes(assembly.GetExportedTypes()));
        }

        static IEnumerable<INamespaceGrouping> GroupTypes(IEnumerable<Type> types)
        {
            return DirectoryTools.GroupByDirectories(types, t => t.FullName, '.');
        }

        static IEnumerable<INamespaceGrouping> GroupTypes(INamespaceGrouping? grouping)
        {
            if(grouping == null)
            {
                return Array.Empty<INamespaceGrouping>();
            }
            return DirectoryTools.GroupByDirectories(grouping, t => t.SubPath, t => t.Entry, '.');
        }

        abstract class Base : Namespace
        {
            public override Assembly Assembly { get; }

            protected NamespaceCollection Namespaces { get; }
            protected List<Type> ExportedTypes { get; }
            protected List<Type> AllTypes { get; }

            public Base(Assembly assembly)
            {
                Assembly = assembly;
                Namespaces = new();
                ExportedTypes = new();
                AllTypes = new();
            }

            protected Node GetNode(string name)
            {
                return
                    Namespaces.TryGetValue(name, out var node)
                    ? node
                    : Namespaces[name] = new(Assembly, FullName, name);
            }

            public override IReadOnlyCollection<Type> GetExportedTypes()
            {
                return ExportedTypes;
            }

            public override IReadOnlyCollection<Type> GetTypes()
            {
                return AllTypes;
            }

            public override IReadOnlyCollection<Namespace> GetNamespaces()
            {
                return Namespaces;
            }

            protected void Fill(IEnumerable<INamespaceGrouping> allGrouping, IEnumerable<INamespaceGrouping> exportedGrouping)
            {
                foreach(var grouping in allGrouping)
                {
                    if(grouping.Key is string key)
                    {
                        GetNode(key).SetAll(grouping);
                    }else{
                        AllTypes.AddRange(grouping.Select(e => e.Entry));
                    }
                }
                foreach(var grouping in exportedGrouping)
                {
                    if(grouping.Key is string key)
                    {
                        GetNode(key).SetExported(grouping);
                    }else{
                        ExportedTypes.AddRange(grouping.Select(e => e.Entry));
                    }
                }
            }
        }

        sealed class Root : Base
        {
            public override string FullName => "";

            public override string Name => "";

            public override string NamespaceName => "";

            public Root(Assembly assembly, IEnumerable<INamespaceGrouping> allGrouping, IEnumerable<INamespaceGrouping> exportedGrouping) : base(assembly)
            {
                Fill(allGrouping, exportedGrouping);
            }
        }

        sealed class Node : Base
        {
            public override string FullName => NamespaceName == "" ? Name : (NamespaceName + "." + Name);

            public override string Name { get; }

            public override string NamespaceName { get; }

            bool initialized;
            INamespaceGrouping? allGrouping;
            INamespaceGrouping? exportedGrouping;

            public Node(Assembly assembly, string namespaceName, string localName) : base(assembly)
            {
                NamespaceName = namespaceName;
                Name = localName;
            }

            void Initialize()
            {
                if(initialized)
                {
                    return;
                }
                lock(Namespaces)
                {
                    if(initialized)
                    {
                        return;
                    }
                    Fill(GroupTypes(allGrouping), GroupTypes(exportedGrouping));
                    initialized = true;
                }
            }

            public override IReadOnlyCollection<Type> GetTypes()
            {
                Initialize();
                return base.GetTypes();
            }

            public override IReadOnlyCollection<Type> GetExportedTypes()
            {
                Initialize();
                return base.GetExportedTypes();
            }

            public override IReadOnlyCollection<Namespace> GetNamespaces()
            {
                Initialize();
                return base.GetNamespaces();
            }

            public void SetAll(INamespaceGrouping grouping)
            {
                allGrouping = grouping;
            }

            public void SetExported(INamespaceGrouping grouping)
            {
                exportedGrouping = grouping;
            }
        }

        class NamespaceCollection : Dictionary<string, Node>, IReadOnlyCollection<Namespace>
        {
            public NamespaceCollection() : base(StringComparer.Ordinal)
            {

            }

            IEnumerator<Namespace> IEnumerable<Namespace>.GetEnumerator()
            {
                return Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return Values.GetEnumerator();
            }
        }
    }
}
