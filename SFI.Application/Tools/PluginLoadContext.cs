using IS4.SFI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace IS4.SFI.Application.Tools
{
    /// <summary>
    /// Supports loading assemblies from a collection of directories.
    /// </summary>
    public class PluginLoadContext : AssemblyLoadContext
    {
        readonly AssemblyLoadContext? parentContext;

        readonly List<IDirectoryInfo> directories = new();

        /// <summary>
        /// Creates a new instance of the context.
        /// </summary>
        /// <param name="parentContext">Parent context to use for resolution.</param>
        public PluginLoadContext(AssemblyLoadContext? parentContext = null)
        {
            Resolving += AssemblyResolve;
            this.parentContext = parentContext;
        }

        /// <summary>
        /// Adds a new directory using its local file system path.
        /// </summary>
        /// <param name="dir">The path of the directory.</param>
        public void AddDirectory(string dir)
        {
            if(Directory.Exists(dir))
            {
                directories.Add(new DirectoryInfoWrapper(new DirectoryInfo(dir)));
            }
        }

        /// <summary>
        /// Adds a new directory represented as an <see cref="IDirectoryInfo"/> instance.
        /// </summary>
        /// <param name="info">The directory info.</param>
        public void AddDirectory(IDirectoryInfo info)
        {
            directories.Add(info);
        }

        /// <summary>
        /// Loads an assembly from a file.
        /// </summary>
        /// <param name="fileInfo">The <see cref="IFileInfo"/> instance representing the assembly.</param>
        /// <returns>The newly loaded assembly within this context.</returns>
        public Assembly LoadFromFile(IFileInfo fileInfo)
        {
            return LoadFromFile(this, fileInfo);
        }

        static Assembly LoadFromFile(AssemblyLoadContext context, IFileInfo fileInfo)
        {
            if(fileInfo is FileInfoWrapper wrapper)
            {
                return context.LoadFromAssemblyPath(wrapper.BaseInfo.FullName);
            }
            using var stream = fileInfo.Open();
            return context.LoadFromStream(stream);
        }

        /// <inheritdoc/>
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            try{
                return parentContext?.LoadFromAssemblyName(assemblyName);
            }catch(FileNotFoundException)
            {
                return null;
            }
        }

        private Assembly? AssemblyResolve(AssemblyLoadContext context, AssemblyName name)
        {
            var fileName = name.Name + ".dll";
            foreach(var dir in directories)
            {
                var entry = dir.Entries.OfType<IFileInfo>().FirstOrDefault(e => fileName.Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                if(entry != null)
                {
                    return LoadFromFile(context, entry);
                }
            }
            return null;
        }
    }
}
