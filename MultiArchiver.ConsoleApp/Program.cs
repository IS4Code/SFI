using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.ConsoleApp
{
    class Program : IApplicationEnvironment
    {
        public int WindowWidth => Console.WindowWidth;

        public TextWriter LogWriter => Console.Error;

        public string NewLine => Environment.NewLine;

        static async Task Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture =
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

            var application = new Application<ConsoleArchiver>(new Program());
            await application.Run(args);
        }

        public Stream OpenInputFile(string path)
        {
            if(path == "-") return Console.OpenStandardInput();
            return File.OpenRead(path);
        }

        public Stream OpenOutputFile(string path)
        {
            if(path == "-") return Console.OpenStandardOutput();
            return File.Create(path);
        }
    }
}
