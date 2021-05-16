using IS4.MultiArchiver.Analyzers;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using IS4.MultiArchiver.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Vanara.PInvoke;
using static Vanara.PInvoke.Kernel32;

namespace IS4.MultiArchiver.Analyzers
{
    public class WinModuleAnalyzer : BinaryFormatAnalyzer<SafeHINSTANCE>
    {
        public override string Analyze(ILinkedNode node, SafeHINSTANCE module, ILinkedNodeFactory nodeFactory)
        {
            foreach(var type in EnumResourceTypesEx(module, RESOURCE_ENUM_FLAGS.RESOURCE_ENUM_LN))
            {
                using(var safeType = new SafeResourceId(type.DangerousGetHandle()))
                {
                    foreach(var name in EnumResourceNamesEx(module, safeType, RESOURCE_ENUM_FLAGS.RESOURCE_ENUM_LN))
                    {
                        using(var safeName = new SafeResourceId(name.DangerousGetHandle()))
                        {
                            var res = FindResource(module, safeName, safeType);
                            if(!res.IsNull)
                            {
                                var factory = new ResourceStreamFactory(module, res);
                                var dataNode = nodeFactory.Create<IStreamFactory>(node, factory);
                                if(dataNode != null)
                                {
                                    node.Set(Properties.HasPart, dataNode);
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        class ResourceStreamFactory : IStreamFactory
        {
            readonly SafeHINSTANCE module;
            readonly HRSRC res;

            public ResourceStreamFactory(SafeHINSTANCE module, HRSRC res)
            {
                this.module = module;
                this.res = res;
            }

            public long Length => SizeofResource(module, res);

            public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

            public object ReferenceKey => module;

            public object DataKey => res.DangerousGetHandle();

            public unsafe Stream Open()
            {
                var len = Length;
                var hResData = LoadResource(module, res);
                var pResData = LockResource(hResData);
                return new UnmanagedMemoryStream((byte*)pResData, len, len, System.IO.FileAccess.Read);
            }
        }
    }
}
