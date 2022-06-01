using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Services
{
    public delegate ValueTask OutputFileDelegate(string name, bool isBinary, IReadOnlyDictionary<string, object> properties, Func<Stream, ValueTask> writer);

    public interface IHasFileOutput
    {
        event OutputFileDelegate OutputFile;
    }
}
