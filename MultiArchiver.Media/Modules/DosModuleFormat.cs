﻿using IS4.MultiArchiver.Analyzers;
using IS4.MultiArchiver.Tools;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    public class DosModuleFormat : ModuleFormat<DosModuleAnalyzer.Module>
    {
        public DosModuleFormat() : base(null, "application/x-dosexec", "exe")
        {

        }
        
        protected override bool CheckSignature(Span<byte> header)
        {
            var fields = header.MemoryCast<ushort>();
            return (fields.Length > 0 && fields[0] == 0x4D5A) || base.CheckSignature(header);
        }

        public override async ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<DosModuleAnalyzer.Module, TResult, TArgs> resultFactory, TArgs args)
        {
            return await resultFactory(new DosModuleAnalyzer.Module(stream), args);
        }
    }
}
