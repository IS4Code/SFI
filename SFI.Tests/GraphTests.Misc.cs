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

        [DataRow("https://cdn.pvpgn.pro/mpq/DRTL_IX86_108_109.mpq")]
        [DataRow("https://nwc3l.com/upload/maps/(2)RuinedRainbow.w3x")]
        [DataRow("https://sfsrealm.hopto.org/download/patch/Cloak.mpq")]
        [DataRow("https://sfsrealm.hopto.org/download/patch/Difficult.mpq")]
        [DataRow("https://sfsrealm.hopto.org/download/patch/Dropship.mpq")]
        [DataRow("https://sfsrealm.hopto.org/download/patch/Shadow.mpq")]
        [DataRow("https://sfsrealm.hopto.org/download/patch/colors%201%20SC.mpq")]
        [DataRow("https://sfsrealm.hopto.org/download/patch/colors%201%20WC.mpq")]
        [DataRow("https://sfsrealm.hopto.org/download/patch/colors%202%20SC.mpq")]
        [DataRow("https://sfsrealm.hopto.org/download/patch/colors%202%20WC.mpq")]
        [DataRow("https://sfsrealm.hopto.org/download/patch/sc_playlist.mpq")]
        [TestMethod]
        public Task mpq(string source)
        {
            return TestOutputGraph(source);
        }
    }
}
