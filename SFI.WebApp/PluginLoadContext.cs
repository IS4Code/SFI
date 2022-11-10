using IS4.SFI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace IS4.SFI.WebApp
{
    class PluginLoadContext : AssemblyLoadContext
    {
        readonly List<IDirectoryInfo> directories = new();

        public PluginLoadContext()
        {
            Resolving += AssemblyResolve;
        }

        public void AddDirectory(IDirectoryInfo info)
        {
            directories.Add(info);
        }

        public Assembly LoadFromFile(IFileInfo fileInfo)
        {
            return LoadFromFile(this, fileInfo);
        }

        static Assembly LoadFromFile(AssemblyLoadContext context, IFileInfo fileInfo)
        {
            using(var stream = fileInfo.Open())
            {
                return context.LoadFromStream(stream);
            }
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            return null;
        }

        private Assembly? AssemblyResolve(AssemblyLoadContext context, AssemblyName name)
        {
            foreach(var dir in directories)
            {
                var fileName = name.Name + ".dll";
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
