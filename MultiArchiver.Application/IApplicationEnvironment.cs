using System.IO;

namespace IS4.MultiArchiver
{
    public interface IApplicationEnvironment
    {
        int WindowWidth { get; }

        string NewLine { get; }

        TextWriter LogWriter { get; }

        Stream OpenInputFile(string path);
        Stream OpenOutputFile(string path);
    }
}
