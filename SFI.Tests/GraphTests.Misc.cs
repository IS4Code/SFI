using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace IS4.SFI.Tests
{
    partial class GraphTests
    {
        [DataRow("https://tspindler.de/arma/arma3/Download/unsung/csj_123_c.pbo")]
        [DataRow("https://tspindler.de/arma/arma3/Download/unsung/%5bSP%5dChickenhawkUnsungAlpha.DaKrong.pbo")]
        [DataRow("https://tspindler.de/arma/arma3/Download/unsung/%5bSP%5dChickenhawk.DaKrong.pbo")]
        [DataRow("https://tspindler.de/arma/arma3/Download/unsung/uns_woodbridge.pbo")]
        [TestMethod]
        public Task pbo(string source)
        {
            return TestOutputGraph(source);
        }
    }
}
