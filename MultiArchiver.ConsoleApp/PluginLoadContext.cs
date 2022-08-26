using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace IS4.MultiArchiver.ConsoleApp
{
    class PluginLoadContext : AssemblyLoadContext
    {
        readonly List<string> directories = new List<string>();

        public PluginLoadContext()
        {
            Resolving += AssemblyResolve;
        }

        public void AddDirectory(string dir)
        {
            directories.Add(dir);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null;
        }

        private Assembly AssemblyResolve(AssemblyLoadContext context, AssemblyName name)
        {
            foreach(var dir in directories)
            {
                var path = Path.Combine(dir, name.Name + ".dll");
                if(File.Exists(path))
                {
                    return context.LoadFromAssemblyPath(Path.GetFullPath(path));
                }
            }
            return null;
        }
    }
}
