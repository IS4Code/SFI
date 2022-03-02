﻿using IS4.MultiArchiver.Media;
using IS4.MultiArchiver.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    public class GenericModuleFormat : ModuleFormat<GenericModuleFormat.Module>
    {
        public GenericModuleFormat() : base("", null, "exe")
        {

        }
        
        public override async ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<Module, TResult, TArgs> resultFactory, TArgs args)
        {
            return await resultFactory(new Module(stream), args);
        }
        
        public override string GetMediaType(Module value)
        {
            return $"application/x-msdownload;format={value.Signature.ToLowerInvariant()}";
        }

        public class Module : IModule
        {
            public string Signature { get; }

            IModuleSignature IModule.Signature => null;

            public Module(Stream stream)
            {
                var reader = new BinaryReader(stream, Encoding.ASCII, true);
                stream.Position = 0x3C;
                var headerPosition = reader.ReadUInt32();
                if(headerPosition <= 1 || headerPosition >= stream.Length - 1)
                {
                    throw new ArgumentException("Not an extended module file!");
                }
                stream.Position = headerPosition;
                var sigBuffer = new byte[8];
                var sigBytes = sigBuffer.Slice(0, stream.Read(sigBuffer, 0, sigBuffer.Length));
                var sig = DataTools.ExtractSignature(sigBytes);
                if(sig == null)
                {
                    throw new ArgumentException("Not an extended module file!");
                }
                if(sig == "LE" || sig == "LX" || sig == "NE" || sig == "PE")
                {
                    throw new ArgumentException("This format is recognized by other instances.");
                }
                Signature = sig;
            }

            public ModuleType Type => ModuleType.Unknown;

            public IEnumerable<IModuleResource> ReadResources()
            {
                return Array.Empty<IModuleResource>();
            }
        }
    }
}
