using IS4.MultiArchiver.Services;
using System.Collections.Generic;
using System.IO;

namespace IS4.MultiArchiver
{
    public interface IApplicationEnvironment
    {
        int WindowWidth { get; }

        string NewLine { get; }

        TextWriter LogWriter { get; }

        IEnumerable<IFileInfo> GetFiles(string path);
        Stream CreateFile(string path, string mediaType);
    }
}
