using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace IS4.SFI.Tests
{
    partial class GraphTests
    {
        [DataRow("Samples/lnk/notepad.exe.lnk")]
        [DataRow("Samples/lnk/ProgramFiles(x86).lnk")]
        [DataRow("Samples/lnk/ExpandedVariables.lnk")]
        [TestMethod]
        public Task lnk(string source)
        {
            return TestOutputGraph(source);
        }

        [DataRow("Samples/url/Google.url")]
        [TestMethod]
        public Task url(string source)
        {
            return TestOutputGraph(source);
        }

        [DataRow("Samples/rdfxml/rdf.xml")]
        [TestMethod]
        public Task rdfxml(string source)
        {
            return TestOutputGraph(source);
        }
    }
}
