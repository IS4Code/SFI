﻿using IS4.MultiArchiver.Media;
using IS4.MultiArchiver.Media.Modules;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    public class LinearModuleFormat : WinModuleFormat
    {
        public LinearModuleFormat() : base("L", "application/x-msdownload;format=le", null)
        {

        }

        public override async ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IModule, TResult, TArgs> resultFactory, TArgs args)
        {
            return await resultFactory(new LeReader(stream), args);
        }
    }
}
