using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

#pragma warning disable 1591, IDE1006

namespace IS4.SFI.Tests
{
    partial class GraphTests
    {
        [DataRow("https://telparia.com/fileFormatSamples/archive/arj/%231GALAXY.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/arj/BILLY.ARJ")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/arj/HEROES.001")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/arj/mnews35.exe")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/arj/PHRACK7.ARJ")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/arj/PLAN11C.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/arj/SAMPLE_63.ARJ")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/arj/SAVE.ARJ")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/arj/SHOPTRK.ARJ")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/arj/sopmsg.zip")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/arj/UNARJ230.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/arj/WR1-ASP.EXE")]
        [TestMethod]
        public Task arj(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/borlandDelphiForm/1642808122_TFORM2")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/borlandDelphiForm/1647137937_TFORM1")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/borlandDelphiForm/TEST.DFM")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/borlandDelphiForm/TFORM1")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/borlandDelphiForm/TFORM1%20%282%29")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/borlandDelphiForm/TFORM1%20%285%29")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/borlandDelphiForm/TFORM2")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/borlandDelphiForm/TFORMABOUT")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/borlandDelphiForm/TFORMMAINWINDOW")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/borlandDelphiForm/TREGLER")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/borlandDelphiForm/UNIT1.DFM")]
        [TestMethod]
        public Task borlandDelphiForm(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/cab/cablinux-1.0.2.cab")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/cab/cablinux-1.0.3.cab")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/cab/DISK1.IMG")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/cab/IE40CIF.CAB")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/cab/iedata.cab")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/cab/IEJAVA.CAB")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/cab/MINI.CAB")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/cab/NSIE4.CAB")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/cab/PRECOPY1.CAB")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/cab/swflash.cab")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/cab/test.cab")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/cab/win95_02.cab")]
        [TestMethod]
        public Task cab(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/cso/Super%20Mario%20Bros%201.cso")]
        [TestMethod]
        public Task cso(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/gz/allfiles.z")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/gz/epsf.ps.Z")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/gz/example.tar.gz")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/gz/example.tgz")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/gz/example.tz")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/gz/example_multicontent-multilevel.tar.gz")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/gz/example_multicontent.tar.gz")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/gz/example_multilevel.tar.gz")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/gz/GRIEF01.TAR")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/gz/ircseq.tar.gz")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/gz/JOE014.TAZ")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/gz/mod.dina")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/gz/next.taz")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/gz/shp.ur_")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/gz/sm.tar.gz")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/gz/SOURCES.TGZ")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/gz/sping.gz")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/gz/xvdocs.ps.Z")]
        [TestMethod]
        public Task gz(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/httpResponse/ads.tsv")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/httpResponse/COUNT.BEG")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/httpResponse/mpurl.dat")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/httpResponse/reply.ofx")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/httpResponse/sex.gif")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/httpResponse/stores.tsv")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/httpResponse/wanted.tsv")]
        [TestMethod]
        public Task httpResponse(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/22000gifs.cue")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/Dazzling%20-%20Hara%20Kumiko%20by%20Tatsuo%20Watanabe%20%28Japan%29%20%28Track%201%29.bin")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/Dazzling%20-%20Hara%20Kumiko%20by%20Tatsuo%20Watanabe%20%28Japan%29%20%28Track%202%29.bin")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/Dazzling%20-%20Hara%20Kumiko%20by%20Tatsuo%20Watanabe%20%28Japan%29%20%28Track%203%29.bin")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/Dazzling%20-%20Hara%20Kumiko%20by%20Tatsuo%20Watanabe%20%28Japan%29.cue")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/differentName.cue")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/filesys-ELF-2.0.x")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/Graphics%20and%20Sound%20Programming%20Techniques%20for%20the%20Mac.iso")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/Hobby%20PC%2017.cue")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/Interactive_Entertainment-CD_Rom-EPISODE_7.toc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/LAUNCH.CUE")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/Mac%20User%20Ultimate%20Mac%20Companion%201996.cue")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/Odyssey%3A%20The%20Legend%20of%20Nemesis.iso")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/Oldtimer%20%28Germany%29.cue")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/Opus%20II%20-%20The%20Software.cue")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/PCD1235.CUE")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/Photo%20Pro%20Volume%201%20-%20Patterns%20of%20Nature%20%28Wayzata%20Technology%29%281181%29%281992%29.cue")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/Power%20Mac%20Screen%20Saver.toast")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/Random-Dot%203D.iso")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/scop.cue")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/T7G_DISK2.CUE")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/test.iso")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/VIDEORAMA26701.CUE")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/WARCRAFT2_X.cue")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/iso/WWDC.iso")]
        [TestMethod]
        public Task iso(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/kra/abydos.kra")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/kra/Anim-Jp-EN.kra")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/kra/Peggy.kra")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/kra/Texture4k8bitsrgb.kra")]
        [TestMethod]
        public Task kra(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/lha/BATS.LZH")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/lha/CSHOWA.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/lha/DUS110.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/lha/endtheme.lha")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/lha/example.lha")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/lha/EXAMPLES.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/lha/falcngry.lzh")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/lha/fanzine9-2.ham.lha")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/lha/fastcomp.lha")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/lha/hd.lzh")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/lha/hexify.lha")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/lha/LHARK.LZH")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/lha/LOTTO.DAT")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/lha/MANUAL.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/lha/MANUAL.LZH")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/lha/PART4.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/lha/protracksuportxpk.lha")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/lha/videop.lzh")]
        [TestMethod]
        public Task lha(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/macBinary/bdh66306.gif")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/macBinary/BJ%21%20Demo%20Help")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/macBinary/dubiel.afm")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/macBinary/FEDERAL.TXT")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/macBinary/flemish_.fog")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/macBinary/HBO_1")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/macBinary/Heavenly%20Bodies%20Browser")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/macBinary/Icon%E2%86%B5")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/macBinary/LOL-010.ZI1")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/macBinary/mod.Sirkustunnelmaan")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/macBinary/preview%20%281%29.pix")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/macBinary/preview.pix")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/macBinary/rapinblu.mid")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/macBinary/Sounds%20File")]
        [TestMethod]
        public Task macBinary(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/microsoftWindowsInstaller/11%20%D0%9C%D0%B8%D0%BD%D1%83%D1%82.msi")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/microsoftWindowsInstaller/1652961439_instmsi.msi")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/microsoftWindowsInstaller/ACDSee%205.0%20Standard%20Trial.msi")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/microsoftWindowsInstaller/AzureBay%20Screen%20Saver%203.4.msi")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/microsoftWindowsInstaller/install_flash_player_7.msi")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/microsoftWindowsInstaller/instmsi.msi")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/microsoftWindowsInstaller/Lanbyte%20Client-Server%20version%200.01.msi")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/microsoftWindowsInstaller/mdxredist.msi")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/microsoftWindowsInstaller/PabloDraw.msi")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/microsoftWindowsInstaller/The%20Axe%20Effect.msi")]
        [TestMethod]
        public Task microsoftWindowsInstaller(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/mpo/DSCF0036.MPO")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/mpo/DSCF0039.MPO")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/mpo/DSCF0075.MPO")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/mpo/DSCF0615.MPO")]
        [TestMethod]
        public Task mpo(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompound/acheiva.qm")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompound/BJBBS.PHN")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompound/CERTMGR.MSC")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompound/ECED001a.pzp")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompound/ECED003a.pzp")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompound/EGDA003a.pzp")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompound/feather2.max")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompound/g20211.gls")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompound/h10202.shp")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompound/space1.qm")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompound/travel.gal")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompound/XBAA006a.pzp")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompound/XGFA031a.pzp")]
        [TestMethod]
        public Task msCompound(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompress/CCLIB1.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompress/COMMDLG.DL_")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompress/DATA_USA.ID_")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompress/HIFINANC.WRI")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompress/HOLLYWOO.SC_")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompress/MUSCROLL.DL_")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompress/PIXFOLIO.EX_")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompress/PIXGIF.DL_")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompress/PIXHELP.DL_")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompress/REGISTER.TX_")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompress/RLZRUN10.RTS")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompress/SC2000W.WA_")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompress/SETUP.IN_")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompress/SPIN.DL_")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompress/STANDARD.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompress/TEXT_USA.ID_")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/msCompress/USRGUIDE.WR_")]
        [TestMethod]
        public Task msCompress(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/os2BitmapArray/ba.ptr")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/os2BitmapArray/ba_001.ptr")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/os2BitmapArray/BOB.ICO")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/os2BitmapArray/FLAME.ICO")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/os2BitmapArray/GERMANY.ICO")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/os2BitmapArray/GIF32.ba.os2.ico")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/os2BitmapArray/HEART.BMP")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/os2BitmapArray/MEMSIZE.BMP")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/os2BitmapArray/MICROCAD.OS2")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/os2BitmapArray/SPADE.BMP")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/os2BitmapArray/SSAVER.ICO")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/os2BitmapArray/WS1.ICO")]
        [TestMethod]
        public Task os2BitmapArray(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/packIce/FRIDEL.TRP")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/packIce/hiscore.mod")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/packIce/JASMIN.TRP")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/packIce/KOERPER.PI9")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/packIce/MAGGIE.PI9")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/packIce/REBATE.PI9")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/packIce/rebels1.mod")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/packIce/SIEVERT.TRP")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/packIce/SPR%E2%84%A2TTEL.TRP")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/packIce/WARNSIGN.SFX")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/packIce/XGA.MARMOR01")]
        [TestMethod]
        public Task packIce(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/pmarcSFX/BOX2.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/pmarcSFX/crrpatch.com")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/pmarcSFX/HAPNMSYS.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/pmarcSFX/HELP.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/pmarcSFX/KA8YHA.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/pmarcSFX/MYF4KEYS.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/pmarcSFX/MYMCON.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/pmarcSFX/pmautoae.obj")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/pmarcSFX/SWDT70.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/pmarcSFX/WB0OIZ.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/pmarcSFX/zcpr-d-j.com")]
        [TestMethod]
        public Task pmarcSFX(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/rar/CDXATOOL_5.RAR")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rar/comment.rar")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rar/example.rar")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rar/IDICTVXX.RAR")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rar/multiline-comment.rar")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rar/password.rar")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rar/PICKLE.002")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rar/PSXV106J.RAR")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rar/REGISTER.RAR")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rar/RUN.RAR")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rar/test.rar")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rar/ZIPCRACK.R00")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rar/ZIPCRACK.RAR")]
        [TestMethod]
        public Task rar(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/rawPartition/A1.5")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rawPartition/BIDIHPUS.184")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rawPartition/bootdisk.img")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rawPartition/DOCSBOOT.IMG")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rawPartition/example.img")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rawPartition/EXDEMOS2.HFV")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rawPartition/EXSYSTEM.HFV")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rawPartition/PaxImperia_CD.dsk")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rawPartition/WDC33.DSK")]
        [TestMethod]
        public Task rawPartition(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/3D%20Buttons.rsrc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/Bill%20Gates%20Does%20Windows%20QT.rsrc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/Bitsy%E2%80%99s%20Face.rsrc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/blackfor.rsrc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/CD-ROM%20Titles%20Sampler.rsrc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/Demo%20Resorcerer%C3%86%201.1.1.rsrc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/Desktop.rsrc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/Extend%20Demo%20ReadMe.rsrc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/Fonts.rsrc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/Fort%20Point.pmv.rsrc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/GIFConverter%202.2.9-%3E2.2.10.rsrc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/Guide%20Maker%20Lite.rsrc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/Icon%0D_37.rsrc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/King%27sEdit1.36%2868k%29.rsrc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/LightningPaint%201.1.rsrc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/Macintosh%20XL.rsrc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/Odyssey%201.1.rsrc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/PM4.2%20RSRC.rsrc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/ScrapIt%20Pro%20Help.adf")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/Speedometer%204.02.rsrc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/SuziWong.rsrc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/typography.doc.rsrc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/Ultimate%20Mac%20Companion.rsrc")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/rsrc/ZipIt.rsrc")]
        [TestMethod]
        public Task rsrc(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/sevenZip/Asterix%20El%20Golpe%20del%20Menhir%20%28Spanish%29%283.5DD%29.img.7z")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/sevenZip/decision_computer_datacap_extracted.7z")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/sevenZip/example.7z")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/sevenZip/Interactive_travel_sample.7z")]
        [TestMethod]
        public Task sevenZip(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/swf/20001pt1.swf")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swf/back_menu.swf")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swf/club.swf")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swf/cookie-hamster-138683467.swf")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swf/dagobah_badger.swf")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swf/detectFlash.swf")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swf/example.swf")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swf/example_document.swf")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swf/example_small.swf")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swf/flash.swf")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swf/highscoreflash.swf")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swf/test_movie.swf")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swf/The%20Impossible%20Quiz.swf")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swf/wheel.swf")]
        [TestMethod]
        public Task swf(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/swfEXE/A%20doom%20Christmas.exe")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swfEXE/baddudes.exe")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swfEXE/Culture.exe")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swfEXE/inicio.exe")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swfEXE/Marriage.exe")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swfEXE/MOVPICK.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swfEXE/Predator.exe")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swfEXE/Smile.exe")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swfEXE/Stealth%20Urinal.exe")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swfEXE/Urinal.exe")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swfEXE/Windows%2098SE.exe")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/swfEXE/winrg207.exe")]
        [TestMethod]
        public Task swfEXE(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/tar/badTar.tar")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/tar/bash-112.tar")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/tar/example.tar")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/tar/ircseq.tar")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/tar/LC_SRC.TAR")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/tar/next")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/tar/PF_BOOT1")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/tar/sm.tar")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/tar/TAR_14")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/tar/ttywatcher-1_0a_tar")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/tar/unixrzsz.tar")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/tar/xgif.tar")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/tar/xschm22.tar")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/tar/xv200.tar")]
        [TestMethod]
        public Task tar(string source)
        {
            return TestOutputGraph(source);
        }

        [DataRow("https://telparia.com/fileFormatSamples/archive/wad/ARMORY.WAD")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/wad/BIGGCITY.WAD")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/wad/DOOM1.WAD")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/wad/E1M1.WAD")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/wad/EVILR1_1.WAD")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/wad/H2HMUD03.WAD")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/wad/JB.RTS")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/wad/JUSTIFY.WAD")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/wad/KOOL.RTS")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/wad/MUD3.WAD")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/wad/OLDWEST.WAD")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/wad/S7.WAD")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/wad/mine.wad")]
        [TestMethod]
        public Task wad(string source)
        {
            return TestOutputGraph(source);
        }

        [DataRow("https://telparia.com/fileFormatSamples/archive/warc/radius.vintagebox.de.warc")]
        [TestMethod]
        public Task warc(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/windowsThumbDB/1630627129_Thumbs.db")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/windowsThumbDB/1631206612_Thumbs.db")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/windowsThumbDB/1644280295_Thumbs.db")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/windowsThumbDB/1645364648_Thumbs.db")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/windowsThumbDB/1645643365_Thumbs.db")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/windowsThumbDB/Thumbs%20%281%29.db")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/windowsThumbDB/Thumbs.db")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/windowsThumbDB/Thumbs_13.db")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/windowsThumbDB/Thumbs_32.db")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/windowsThumbDB/Thumbs_hm.db")]
        [TestMethod]
        public Task windowsThumbDB(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/APPIAN.ZIP")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/breakout.zip")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/comments.zip")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/deskcopy.zip")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/disp135.zip")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/example.zip")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/example.zipx")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/example_multicontent.zip")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/example_multicontent_multilevel.zip")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/example_multilevel.zip")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/example_protected.zip")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/fractk17.zip")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/NS.ZIP")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/osutils.zip")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/p205.zip")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/password.zip")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/PCBDOCS.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/pscpx102.zip")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/SIGMA.ZIP")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/starstrk.zip")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/svga.exe")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/test.zip")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/ucf-b2sp.zip")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/ucf-tp32.zip")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/uk_demos.zip")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/UNPACK.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/VIDEO7.ZIP")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/vision1.pcb")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/vrow121.zip")]
        [DataRow("https://telparia.com/fileFormatSamples/archive/zip/XOR110.ZIP")]
        [TestMethod]
        public Task zip(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/audio/aac/01%20Track%2001.m4a")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aac/ct_faac-adts.aac")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aac/example.aac")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aac/example.m4a")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aac/example.m4p")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aac/example.m4r")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aac/motr_aac.m4a")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aac/msgsys8.ima")]
        [TestMethod]
        public Task aac(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/audio/aif/01.aif")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aif/4orgasm.aif")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aif/beep1.aif")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aif/brainson")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aif/clip23%20%281%29.8")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aif/example.aif")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aif/example.aiff")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aif/example.caf")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aif/example_small.aiff")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aif/mpublish08.aif")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aif/music.loop")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aif/qd3d02b.aif")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aif/ring_in.aif")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aif/sampler.aif")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aif/test.aif")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aif/uii02.aif")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aif/vivintro.")]
        [TestMethod]
        public Task aif(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/audio/aviAudio/01mwz200.avi")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aviAudio/03msnp00.avi")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aviAudio/04mwwk00.avi")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aviAudio/06mwal00.avi")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aviAudio/2c3B.avi")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aviAudio/aapw3_fr.avi")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aviAudio/apm65_in.avi")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aviAudio/button.avi")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aviAudio/drumz2.avi")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/aviAudio/intro.avi")]
        [TestMethod]
        public Task aviAudio(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/audio/downloadableSoundBank/boids.dls")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/downloadableSoundBank/Brass.dlp")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/downloadableSoundBank/compellion.dlp")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/downloadableSoundBank/GM.dls")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/downloadableSoundBank/gm16.dls")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/downloadableSoundBank/HERBSOUN.DLS")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/downloadableSoundBank/RIFFFUI.DLS")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/downloadableSoundBank/sample.dls")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/downloadableSoundBank/shell.dls")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/downloadableSoundBank/vocalise.dlp")]
        [TestMethod]
        public Task downloadableSoundBank(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/audio/flac/BACKGR_1.043")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/flac/example.flac")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/flac/test.flac")]
        [TestMethod]
        public Task flac(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/audio/mka/nested_tags_lang.mka")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/mka/sample_44_2_16_TTA.mka")]
        [TestMethod]
        public Task mka(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/audio/monkeysAudio/example.ape")]
        [TestMethod]
        public Task monkeysAudio(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/audio/mp2/00071_Sound_Voyage%20interieur")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/mp2/ANTH1253.MP2")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/mp2/ANTH1337.MP2")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/mp2/example.mp2")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/mp2/friendly.mp2")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/mp2/greensleeves-st.mp2")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/mp2/GREETING.MP2")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/mp2/Ncom_tutp2_019.mfx")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/mp2/Ncom_tutp3_017a.mfx")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/mp2/Ncom_tutp5_015.mfx")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/mp2/Ncom_tutp5_019b.mfx")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/mp2/rondo.mp2")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/mp2/SONG1007.MP2")]
        [TestMethod]
        public Task mp2(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/audio/mp3/example.mp3")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/mp3/example.mpga")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/mp3/test.mp3")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/mp3/test2.mp3")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/mp3/valkomnande.mpg")]
        [TestMethod]
        public Task mp3(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/audio/ogg/audio.ogg")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/ogg/Click1.ogg")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/ogg/example.ogg")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/ogg/file_example_OOG_1MG.ogg")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/ogg/firework.ogg")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/ogg/High1.ogg")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/ogg/menu_mus.ogg")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/ogg/mus01.ogg")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/ogg/sample2.ogg")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/ogg/stg0.ogg")]
        [TestMethod]
        public Task ogg(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/audio/opus/example.opus")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/opus/static_examples_ehren-paper_lights-96.opus")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/opus/static_examples_samples_music_128kbps.opus")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/opus/static_examples_samples_music_48kbps.opus")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/opus/static_examples_samples_music_64kbps.opus")]
        [TestMethod]
        public Task opus(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/audio/shockWaveAudio/32bc_darkbird.mp3")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/shockWaveAudio/32bc_fruit.mp3")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/shockWaveAudio/billy_vo_32bit.swa")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/shockWaveAudio/combining.swa")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/shockWaveAudio/CREDITS.SWA")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/shockWaveAudio/Drum.Swa")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/shockWaveAudio/getstarted.swa")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/shockWaveAudio/local6.swa")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/shockWaveAudio/r5.swa")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/shockWaveAudio/simple.swa")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/shockWaveAudio/STORY2.SWA")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/shockWaveAudio/todonow.swa")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/shockWaveAudio/Track_1.swa")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/shockWaveAudio/welcome.swa")]
        [TestMethod]
        public Task shockWaveAudio(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/audio/sonarc/CHORD1.WV")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/sonarc/EMBELISH.WV")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/sonarc/END.WV")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/sonarc/GROOV.WV")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/sonarc/IHOPE.WV")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/sonarc/INTRO.WV")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/sonarc/LETSHERE.WV")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/sonarc/MAKE.WV")]
        [TestMethod]
        public Task sonarc(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/audio/soundFont1/SYNTHGM.SBK")]
        [TestMethod]
        public Task soundFont1(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/audio/soundFont2/7FTGRAND.SF2")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/soundFont2/AUD_IMS1.SF2")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/soundFont2/CT8MGM.SF2")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/soundFont2/FluidR3_GS.sf2")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/soundFont2/mood%20strings.sf2")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/soundFont2/TFX3512.SF2")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/soundFont2/Unison.sf2")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/soundFont2/Windows%20GM.sf2")]
        [TestMethod]
        public Task soundFont2(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/audio/wav/BEBACK.WAV")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wav/boing.wav")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wav/END.WAV")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wav/example.wav")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wav/example_small.wav")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wav/fodfaxn")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wav/KABOOM.WAV")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wav/lips10.wav")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wav/media_tests_contents_media_api_music_bzk_chic.wav")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wav/mmmpmma.wav")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wav/MOVEON.WAV")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wav/ofcclose")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wav/SETUP.WAV")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wav/snd%20_2502_terror.organ.s.wav")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wav/test.wav")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wav/TESTAGAN.WAV")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wav/v3.wav")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wav/Xena2.wav")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wav/youhave")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wav/Z_GROWL")]
        [TestMethod]
        public Task wav(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/audio/wavPack/16bit.wv")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wavPack/32bit_float.wv")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wavPack/32bit_int.wv")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wavPack/8bit.wv")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wavPack/mono-1.wv")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wavPack/multichannel-6.wv")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wavPack/stereo-2.wv")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wavPack/vers-397-hybrid.wvc")]
        [TestMethod]
        public Task wavPack(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/audio/wma/beck.asf")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wma/bleep222.asf")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wma/cc.asf")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wma/defend.asf")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wma/example.wma")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wma/frontier14.wma")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wma/frontier25.wma")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wma/frontier32.wma")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wma/pwin22m.asf")]
        [DataRow("https://telparia.com/fileFormatSamples/audio/wma/que_titl.asf")]
        [TestMethod]
        public Task wma(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/corelSHOW/40WINNER.SHW")]
        [DataRow("https://telparia.com/fileFormatSamples/document/corelSHOW/multi.shw")]
        [TestMethod]
        public Task corelSHOW(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/docBook/AmigaChannel5.xml")]
        [DataRow("https://telparia.com/fileFormatSamples/document/docBook/bi2cf.xml")]
        [DataRow("https://telparia.com/fileFormatSamples/document/docBook/building_blender.xml")]
        [DataRow("https://telparia.com/fileFormatSamples/document/docBook/cfed.xml")]
        [DataRow("https://telparia.com/fileFormatSamples/document/docBook/comet.xml")]
        [DataRow("https://telparia.com/fileFormatSamples/document/docBook/crimson.xml")]
        [DataRow("https://telparia.com/fileFormatSamples/document/docBook/gimpsizeentry.xml")]
        [DataRow("https://telparia.com/fileFormatSamples/document/docBook/mathtrans.xml")]
        [DataRow("https://telparia.com/fileFormatSamples/document/docBook/output.xml")]
        [DataRow("https://telparia.com/fileFormatSamples/document/docBook/structure.xml")]
        [DataRow("https://telparia.com/fileFormatSamples/document/docBook/xinlines.xml")]
        [TestMethod]
        public Task docBook(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/docx/example.docm")]
        [DataRow("https://telparia.com/fileFormatSamples/document/docx/example.docx")]
        [DataRow("https://telparia.com/fileFormatSamples/document/docx/example.dotx")]
        [DataRow("https://telparia.com/fileFormatSamples/document/docx/example_multipage.docx")]
        [DataRow("https://telparia.com/fileFormatSamples/document/docx/out.docx")]
        [DataRow("https://telparia.com/fileFormatSamples/document/docx/testing.docx")]
        [TestMethod]
        public Task docx(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/epocDatabase/AirlinesCodes")]
        [DataRow("https://telparia.com/fileFormatSamples/document/epocDatabase/DESKTOP.HLP")]
        [DataRow("https://telparia.com/fileFormatSamples/document/epocDatabase/Fitness")]
        [DataRow("https://telparia.com/fileFormatSamples/document/epocDatabase/FreeCell.hlp")]
        [DataRow("https://telparia.com/fileFormatSamples/document/epocDatabase/HOLIDAYI.MP3")]
        [DataRow("https://telparia.com/fileFormatSamples/document/epocDatabase/HOSPITAL.MP7")]
        [DataRow("https://telparia.com/fileFormatSamples/document/epocDatabase/Massage")]
        [DataRow("https://telparia.com/fileFormatSamples/document/epocDatabase/MP3-TEMP.MP3")]
        [DataRow("https://telparia.com/fileFormatSamples/document/epocDatabase/PianoHlp.hlp")]
        [DataRow("https://telparia.com/fileFormatSamples/document/epocDatabase/Pizzakurier.dat")]
        [DataRow("https://telparia.com/fileFormatSamples/document/epocDatabase/RMRText.hlp")]
        [DataRow("https://telparia.com/fileFormatSamples/document/epocDatabase/STDDATA")]
        [DataRow("https://telparia.com/fileFormatSamples/document/epocDatabase/Thankyou.hlp")]
        [DataRow("https://telparia.com/fileFormatSamples/document/epocDatabase/TOWNS.MP1")]
        [DataRow("https://telparia.com/fileFormatSamples/document/epocDatabase/TUBEZONE.MP8")]
        [TestMethod]
        public Task epocDatabase(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/epub/childrens-literature.epub")]
        [DataRow("https://telparia.com/fileFormatSamples/document/epub/cole-voyage-of-life-tol.epub")]
        [DataRow("https://telparia.com/fileFormatSamples/document/epub/example.epub")]
        [DataRow("https://telparia.com/fileFormatSamples/document/epub/example_multipage_large.epub")]
        [DataRow("https://telparia.com/fileFormatSamples/document/epub/haruko-html-jpeg.epub")]
        [DataRow("https://telparia.com/fileFormatSamples/document/epub/moby-dick.epub")]
        [DataRow("https://telparia.com/fileFormatSamples/document/epub/out.epub")]
        [TestMethod]
        public Task epub(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/fictionBook/example.fb2")]
        [TestMethod]
        public Task fictionBook(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/hancomWord/example.hwp")]
        [TestMethod]
        public Task hancomWord(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftPublisher/example.pub")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftPublisher/example_multipage.pub")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftPublisher/MSPublisher2000-Sample.pub")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftPublisher/MSPublisher2007-Sample.pub")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftPublisher/MSPublisher2010-Sample.pub")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftPublisher/MSPublisher2013-Sample.pub")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftPublisher/MSPublisher95.PUB")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftPublisher/MSPublisher97.pub")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftPublisher/MSPublisher98-Sample.pub")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftPublisher/MSPublisherv1.PUB")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftPublisher/MSPublisherv2.PUB")]
        [TestMethod]
        public Task microsoftPublisher(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorks/APPLE.DOC")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorks/ASKCREDI.WPS")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorks/AUNTMADG.WP")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorks/BIZLEASE.WPS")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorks/BPMWKS.WPS")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorks/COVERLTR.WPS")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorks/example.wps")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorks/LARGEPRO.WPS")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorks/LETTER1.WPS")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorks/OVERVIEW.WPS")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorks/PICTURE.WP")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorks/TEAMNEWS.WP")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorks/XMAS.WPS")]
        [TestMethod]
        public Task microsoftWorks(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorksDatabase/accounts.wdb")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorksDatabase/ADVWKS.WDB")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorksDatabase/AUCTION.WDB")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorksDatabase/CarCare.wdb")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorksDatabase/ChildBookList.wdb")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorksDatabase/CLIMBERS.WDB")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorksDatabase/Dining.wdb")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorksDatabase/MERGE.WDB")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorksDatabase/MovieFactFile.wdb")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorksDatabase/toys_m.wdb")]
        [DataRow("https://telparia.com/fileFormatSamples/document/microsoftWorksDatabase/Wedding%20list.wdb")]
        [TestMethod]
        public Task microsoftWorksDatabase(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/openDocument/aw-9colorful.ott")]
        [DataRow("https://telparia.com/fileFormatSamples/document/openDocument/cnt-04.ott")]
        [DataRow("https://telparia.com/fileFormatSamples/document/openDocument/example.odm")]
        [DataRow("https://telparia.com/fileFormatSamples/document/openDocument/example.odp")]
        [DataRow("https://telparia.com/fileFormatSamples/document/openDocument/example.ods")]
        [DataRow("https://telparia.com/fileFormatSamples/document/openDocument/example.odt")]
        [DataRow("https://telparia.com/fileFormatSamples/document/openDocument/example_multipage.odt")]
        [DataRow("https://telparia.com/fileFormatSamples/document/openDocument/example_multipage_large.odt")]
        [DataRow("https://telparia.com/fileFormatSamples/document/openDocument/pri-mail_l.ott")]
        [TestMethod]
        public Task openDocument(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/openOfficeWriter/cnt-standard.stw")]
        [DataRow("https://telparia.com/fileFormatSamples/document/openOfficeWriter/DicOOo.sxw")]
        [DataRow("https://telparia.com/fileFormatSamples/document/openOfficeWriter/example.stw")]
        [DataRow("https://telparia.com/fileFormatSamples/document/openOfficeWriter/example.sxw")]
        [DataRow("https://telparia.com/fileFormatSamples/document/openOfficeWriter/example_small.sxw")]
        [DataRow("https://telparia.com/fileFormatSamples/document/openOfficeWriter/FontOOo.sxw")]
        [DataRow("https://telparia.com/fileFormatSamples/document/openOfficeWriter/idxexample.sxw")]
        [DataRow("https://telparia.com/fileFormatSamples/document/openOfficeWriter/stl-music.stw")]
        [DataRow("https://telparia.com/fileFormatSamples/document/openOfficeWriter/stl-nostalg.stw")]
        [DataRow("https://telparia.com/fileFormatSamples/document/openOfficeWriter/wizbrf1.stw")]
        [DataRow("https://telparia.com/fileFormatSamples/document/openOfficeWriter/wizmem2.stw")]
        [TestMethod]
        public Task openOfficeWriter(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/outlookMessage/appt.oft")]
        [DataRow("https://telparia.com/fileFormatSamples/document/outlookMessage/example.msg")]
        [DataRow("https://telparia.com/fileFormatSamples/document/outlookMessage/offer.msg")]
        [DataRow("https://telparia.com/fileFormatSamples/document/outlookMessage/welcome.msg")]
        [TestMethod]
        public Task outlookMessage(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/pagePlus/advert.ppp")]
        [DataRow("https://telparia.com/fileFormatSamples/document/pagePlus/PagePlusX3-Sample.ppp")]
        [DataRow("https://telparia.com/fileFormatSamples/document/pagePlus/PagePlusX5-Sample.ppp")]
        [DataRow("https://telparia.com/fileFormatSamples/document/pagePlus/PagePlusX6-Sample.ppp")]
        [DataRow("https://telparia.com/fileFormatSamples/document/pagePlus/PagePlusX8-Sample.ppp")]
        [DataRow("https://telparia.com/fileFormatSamples/document/pagePlus/PagePlusX9-Sample1.ppp")]
        [DataRow("https://telparia.com/fileFormatSamples/document/pagePlus/press.ppp")]
        [DataRow("https://telparia.com/fileFormatSamples/document/pagePlus/sample.ppt")]
        [DataRow("https://telparia.com/fileFormatSamples/document/pagePlus/tutor1.ppt")]
        [TestMethod]
        public Task pagePlus(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/pdf/abydos.pdf")]
        [DataRow("https://telparia.com/fileFormatSamples/document/pdf/main.pdf")]
        [DataRow("https://telparia.com/fileFormatSamples/document/pdf/ORDER.FRM")]
        [DataRow("https://telparia.com/fileFormatSamples/document/pdf/PYRAMAGE%20CD%20PAGE.pdf")]
        [DataRow("https://telparia.com/fileFormatSamples/document/pdf/sample1.pdf")]
        [DataRow("https://telparia.com/fileFormatSamples/document/pdf/test.pdf")]
        [TestMethod]
        public Task pdf(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/powerPoint/CSH-NT.PPT")]
        [DataRow("https://telparia.com/fileFormatSamples/document/powerPoint/DISPLAY.PP")]
        [DataRow("https://telparia.com/fileFormatSamples/document/powerPoint/example.pps")]
        [DataRow("https://telparia.com/fileFormatSamples/document/powerPoint/example.ppsx")]
        [DataRow("https://telparia.com/fileFormatSamples/document/powerPoint/example.ppt")]
        [DataRow("https://telparia.com/fileFormatSamples/document/powerPoint/example.pptm")]
        [DataRow("https://telparia.com/fileFormatSamples/document/powerPoint/example.pptx")]
        [DataRow("https://telparia.com/fileFormatSamples/document/powerPoint/INTRO.PP")]
        [DataRow("https://telparia.com/fileFormatSamples/document/powerPoint/IVE_BEEN.PP")]
        [DataRow("https://telparia.com/fileFormatSamples/document/powerPoint/KNW.PPT")]
        [DataRow("https://telparia.com/fileFormatSamples/document/powerPoint/O_GIRL.PP")]
        [DataRow("https://telparia.com/fileFormatSamples/document/powerPoint/O_XMAS.PP")]
        [DataRow("https://telparia.com/fileFormatSamples/document/powerPoint/SQLWS30.PPT")]
        [DataRow("https://telparia.com/fileFormatSamples/document/powerPoint/_A_BIKE.PP")]
        [DataRow("https://telparia.com/fileFormatSamples/document/powerPoint/_BINGO.PP")]
        [TestMethod]
        public Task powerPoint(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/proText/SAMPLE.REP")]
        [DataRow("https://telparia.com/fileFormatSamples/document/proText/_svl")]
        [TestMethod]
        public Task proText(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/ps/abydos.ps")]
        [DataRow("https://telparia.com/fileFormatSamples/document/ps/chaos_1.ps")]
        [DataRow("https://telparia.com/fileFormatSamples/document/ps/colorcir.ps")]
        [DataRow("https://telparia.com/fileFormatSamples/document/ps/EazyBBS.ps")]
        [DataRow("https://telparia.com/fileFormatSamples/document/ps/ESCHER.PS")]
        [DataRow("https://telparia.com/fileFormatSamples/document/ps/example.ps")]
        [DataRow("https://telparia.com/fileFormatSamples/document/ps/ImageStudio.ps")]
        [DataRow("https://telparia.com/fileFormatSamples/document/ps/input.ps")]
        [DataRow("https://telparia.com/fileFormatSamples/document/ps/MANUAL_1.PS")]
        [DataRow("https://telparia.com/fileFormatSamples/document/ps/specs.ps")]
        [DataRow("https://telparia.com/fileFormatSamples/document/ps/ug.ps")]
        [TestMethod]
        public Task ps(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/quattroPro/example.qpw")]
        [TestMethod]
        public Task quattroPro(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/starOfficeDraw/example.sda")]
        [TestMethod]
        public Task starOfficeDraw(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/starOfficePresentation/example.sdd")]
        [DataRow("https://telparia.com/fileFormatSamples/document/starOfficePresentation/JUNGLE.SDD")]
        [DataRow("https://telparia.com/fileFormatSamples/document/starOfficePresentation/STAR.SDD")]
        [TestMethod]
        public Task starOfficePresentation(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/starOfficeSpreadsheet/1661888335_BUDGET.SDC")]
        [DataRow("https://telparia.com/fileFormatSamples/document/starOfficeSpreadsheet/BUDGET.SDC")]
        [DataRow("https://telparia.com/fileFormatSamples/document/starOfficeSpreadsheet/example.sdc")]
        [DataRow("https://telparia.com/fileFormatSamples/document/starOfficeSpreadsheet/example.stc")]
        [DataRow("https://telparia.com/fileFormatSamples/document/starOfficeSpreadsheet/example.sxc")]
        [DataRow("https://telparia.com/fileFormatSamples/document/starOfficeSpreadsheet/example.vor")]
        [DataRow("https://telparia.com/fileFormatSamples/document/starOfficeSpreadsheet/FACTURE.SDC")]
        [DataRow("https://telparia.com/fileFormatSamples/document/starOfficeSpreadsheet/MILEAGE.VOR")]
        [DataRow("https://telparia.com/fileFormatSamples/document/starOfficeSpreadsheet/VOITURE.VOR")]
        [TestMethod]
        public Task starOfficeSpreadsheet(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/starWriter/BOOKTEXT.TPL")]
        [DataRow("https://telparia.com/fileFormatSamples/document/starWriter/CV.TPL")]
        [DataRow("https://telparia.com/fileFormatSamples/document/starWriter/DEMO1.TPL")]
        [DataRow("https://telparia.com/fileFormatSamples/document/starWriter/DEMO2.TPL")]
        [DataRow("https://telparia.com/fileFormatSamples/document/starWriter/EINLKART.SDW")]
        [DataRow("https://telparia.com/fileFormatSamples/document/starWriter/EINLKART.VOR")]
        [DataRow("https://telparia.com/fileFormatSamples/document/starWriter/example.sdw")]
        [DataRow("https://telparia.com/fileFormatSamples/document/starWriter/EXPENSES.TPL")]
        [DataRow("https://telparia.com/fileFormatSamples/document/starWriter/FAX.TPL")]
        [DataRow("https://telparia.com/fileFormatSamples/document/starWriter/LETTER1.VOR")]
        [DataRow("https://telparia.com/fileFormatSamples/document/starWriter/OUTLINE.TXT")]
        [DataRow("https://telparia.com/fileFormatSamples/document/starWriter/WIZBRF3.VOR")]
        [TestMethod]
        public Task starWriter(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/visio/brw1.vsd")]
        [DataRow("https://telparia.com/fileFormatSamples/document/visio/example.vsd")]
        [DataRow("https://telparia.com/fileFormatSamples/document/visio/how%20to%20play%20a%20movie%20in%20exclusive%20mode%20using%20VMR.vsd")]
        [DataRow("https://telparia.com/fileFormatSamples/document/visio/Visio2002Test.vsd")]
        [DataRow("https://telparia.com/fileFormatSamples/document/visio/VisioDocument")]
        [DataRow("https://telparia.com/fileFormatSamples/document/visio/VisioDocument%20%281%29")]
        [DataRow("https://telparia.com/fileFormatSamples/document/visio/VisioTest.vdx")]
        [DataRow("https://telparia.com/fileFormatSamples/document/visio/VisioTest.vsd")]
        [DataRow("https://telparia.com/fileFormatSamples/document/visio/VisioTest.vss")]
        [DataRow("https://telparia.com/fileFormatSamples/document/visio/VisioTest.vst")]
        [TestMethod]
        public Task visio(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/wordDoc/ASMTUT.DOC")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wordDoc/cdacceso.man")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wordDoc/example.doc")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wordDoc/example.dot")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wordDoc/example_multipage.doc")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wordDoc/FALCON.WRD")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wordDoc/HILFE.DOC")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wordDoc/integrty.doc")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wordDoc/ITALIANO.DOC")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wordDoc/MANUAL.DOC")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wordDoc/POWWOW.DOC")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wordDoc/Readme.doc")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wordDoc/unix.txt")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wordDoc/USRNEWS.DOC")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wordDoc/WINTER11.DOC")]
        [TestMethod]
        public Task wordDoc(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/wp/CALENDAR.WP5")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wp/DECIMAL.WP")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wp/example.wp")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wp/example.wpd")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wp/example.wpg")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wp/example_small.wpd")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wp/F10.DOC")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wp/GEMFONT.WP")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wp/LABELZ_71.WP5")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wp/NEATKEYS.WP")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wp/README.DOC")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wp/README.WP")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wp/SANFRAN.DOC")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wp/VPIC51D.WP")]
        [DataRow("https://telparia.com/fileFormatSamples/document/wp/WPMAC516_54.DOC")]
        [TestMethod]
        public Task wp(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/xls/BBS.XLS")]
        [DataRow("https://telparia.com/fileFormatSamples/document/xls/CHART.XLS")]
        [DataRow("https://telparia.com/fileFormatSamples/document/xls/DIAL.XLM")]
        [DataRow("https://telparia.com/fileFormatSamples/document/xls/DJNS.XLC")]
        [DataRow("https://telparia.com/fileFormatSamples/document/xls/EAST.XLS")]
        [DataRow("https://telparia.com/fileFormatSamples/document/xls/example.xls")]
        [DataRow("https://telparia.com/fileFormatSamples/document/xls/example.xlsx")]
        [DataRow("https://telparia.com/fileFormatSamples/document/xls/HGRAM2.XLM")]
        [DataRow("https://telparia.com/fileFormatSamples/document/xls/NAVIGATE.XLM")]
        [DataRow("https://telparia.com/fileFormatSamples/document/xls/ORDER.XLS")]
        [DataRow("https://telparia.com/fileFormatSamples/document/xls/PREVIEW.XLM")]
        [DataRow("https://telparia.com/fileFormatSamples/document/xls/QUE.XLS")]
        [DataRow("https://telparia.com/fileFormatSamples/document/xls/TIMELN2.XLS")]
        [DataRow("https://telparia.com/fileFormatSamples/document/xls/WEST.XLS")]
        [DataRow("https://telparia.com/fileFormatSamples/document/xls/WINOYE.XLW")]
        [TestMethod]
        public Task xls(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/document/xps/example.oxps")]
        [DataRow("https://telparia.com/fileFormatSamples/document/xps/example.xps")]
        [DataRow("https://telparia.com/fileFormatSamples/document/xps/mb03.oxps")]
        [DataRow("https://telparia.com/fileFormatSamples/document/xps/mb08.oxps")]
        [DataRow("https://telparia.com/fileFormatSamples/document/xps/testing.xps")]
        [TestMethod]
        public Task xps(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/executable/com/ADLIBG.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/com/DIGISP.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/com/VB102F.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/com/VMSND.COM")]
        [TestMethod]
        public Task com(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/executable/dll/ABOUTWEP_97.DLL")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/dll/angeldll.dll")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/dll/ATIPXDEF.RS")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/dll/BINDDLL.DLL")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/dll/EMBH.DLL")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/dll/ISNA.DLL")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/dll/ISS3_765.DLL")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/dll/METER_84.DLL")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/dll/PARITY_92.DLL")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/dll/S3MM.DLL")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/dll/TDISPLUS.DLL")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/dll/VB40032.DLL")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/dll/VBRUN100.DLL")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/dll/_IsRes.Dll")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/dll/_Setup.dll")]
        [TestMethod]
        public Task dll(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/1647275547_setup.exe")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/BACKWB95.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/BatMemv1.31c.exe")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/Cat_Saver.exe")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/CNDRAW.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/CWPT_FULL.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/DG.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/DUMP.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/Fontmgr.exe")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/FUNCLOCK.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/GETINT13.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/HEART.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/Keypad.exe")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/Musiclf.dll")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/Oidata")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/pkunzip.exe")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/README.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/SETUP.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/SLYFOX_F.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/ST.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/svga.exe")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/VESA.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/exe/VIEWGIF.EXE")]
        [TestMethod]
        public Task exe(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/executable/windowsSCR/DBEAR.SCR")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/windowsSCR/dmorph.scr")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/windowsSCR/factory.scr")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/windowsSCR/gunshot.scr")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/windowsSCR/KALEID95.SCR")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/windowsSCR/savit.scr")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/windowsSCR/ScreenLock.scr")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/windowsSCR/techfile.scr")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/windowsSCR/terrafrm.scr")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/windowsSCR/timetrvl.scr")]
        [DataRow("https://telparia.com/fileFormatSamples/executable/windowsSCR/Weed.scr")]
        [TestMethod]
        public Task windowsSCR(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/font/osXDataForkFont/Basaltic%23C80001_ResourceFork.bin")]
        [DataRow("https://telparia.com/fileFormatSamples/font/osXDataForkFont/Casablanca%23C80001_ResourceFork.bin")]
        [DataRow("https://telparia.com/fileFormatSamples/font/osXDataForkFont/DownTown%23C80001_ResourceFork.bin")]
        [DataRow("https://telparia.com/fileFormatSamples/font/osXDataForkFont/Hamster%23C80001_ResourceFork.bin")]
        [DataRow("https://telparia.com/fileFormatSamples/font/osXDataForkFont/Jennifer%23C80001_ResourceFork%20%281%29.bin")]
        [DataRow("https://telparia.com/fileFormatSamples/font/osXDataForkFont/lem_salt.ttf")]
        [DataRow("https://telparia.com/fileFormatSamples/font/osXDataForkFont/Maelstrom%20Fonts")]
        [DataRow("https://telparia.com/fileFormatSamples/font/osXDataForkFont/NewDeal%23C80001_ResourceFork.bin")]
        [DataRow("https://telparia.com/fileFormatSamples/font/osXDataForkFont/Peoplenstuff%23C80001_ResourceFork.bin")]
        [DataRow("https://telparia.com/fileFormatSamples/font/osXDataForkFont/SkyScraper%23C80001_ResourceFork.bin")]
        [DataRow("https://telparia.com/fileFormatSamples/font/osXDataForkFont/StarsAndBars%23C80001_ResourceFork.bin")]
        [DataRow("https://telparia.com/fileFormatSamples/font/osXDataForkFont/WantedPoster%23C80001_ResourceFork.bin")]
        [TestMethod]
        public Task osXDataForkFont(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/font/ttf/ARAXBARB.TTF")]
        [DataRow("https://telparia.com/fileFormatSamples/font/ttf/avante.ttf")]
        [DataRow("https://telparia.com/fileFormatSamples/font/ttf/BROOK.TTF")]
        [DataRow("https://telparia.com/fileFormatSamples/font/ttf/CAIRO.TTF")]
        [DataRow("https://telparia.com/fileFormatSamples/font/ttf/casopen.ttf")]
        [DataRow("https://telparia.com/fileFormatSamples/font/ttf/CHOPIN.TTF")]
        [DataRow("https://telparia.com/fileFormatSamples/font/ttf/COLLEGE_.TTF")]
        [DataRow("https://telparia.com/fileFormatSamples/font/ttf/earth.ttf")]
        [DataRow("https://telparia.com/fileFormatSamples/font/ttf/example.ttf")]
        [DataRow("https://telparia.com/fileFormatSamples/font/ttf/GREEK.TTF")]
        [DataRow("https://telparia.com/fileFormatSamples/font/ttf/input.ttf")]
        [DataRow("https://telparia.com/fileFormatSamples/font/ttf/INTER.TTF")]
        [DataRow("https://telparia.com/fileFormatSamples/font/ttf/KLINZHAI.TTF")]
        [DataRow("https://telparia.com/fileFormatSamples/font/ttf/lcd.ttf")]
        [DataRow("https://telparia.com/fileFormatSamples/font/ttf/lightbl.ttf")]
        [DataRow("https://telparia.com/fileFormatSamples/font/ttf/LYNX.TTF")]
        [DataRow("https://telparia.com/fileFormatSamples/font/ttf/RIVERSDE.TTF")]
        [DataRow("https://telparia.com/fileFormatSamples/font/ttf/TNGMONI.TTF")]
        [TestMethod]
        public Task ttf(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/adobeIllustrator/001.ai")]
        [DataRow("https://telparia.com/fileFormatSamples/image/adobeIllustrator/004.ai")]
        [DataRow("https://telparia.com/fileFormatSamples/image/adobeIllustrator/005.ai")]
        [DataRow("https://telparia.com/fileFormatSamples/image/adobeIllustrator/014.ai")]
        [DataRow("https://telparia.com/fileFormatSamples/image/adobeIllustrator/049.ai")]
        [DataRow("https://telparia.com/fileFormatSamples/image/adobeIllustrator/056.ai")]
        [DataRow("https://telparia.com/fileFormatSamples/image/adobeIllustrator/061.ai")]
        [DataRow("https://telparia.com/fileFormatSamples/image/adobeIllustrator/121.ai")]
        [DataRow("https://telparia.com/fileFormatSamples/image/adobeIllustrator/162.ai")]
        [DataRow("https://telparia.com/fileFormatSamples/image/adobeIllustrator/DIAMONDS.AI")]
        [TestMethod]
        public Task adobeIllustrator(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/abydos.ani")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/ARROW_R.ANI")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/artcur1.ani")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/artcur11.ani")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/artcur16.ani")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/artcur17.ani")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/artcur17w.ani")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/artcur4.ani")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/artcur6.ani")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/artcur7.ani")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/ATOM.ANI")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/bell.ani")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/COLOREYE.ANI")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/darth%20vader.ani")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/DMCHG3.ANI")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/EYES2.ANI")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/HANDNESW.ANI")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/MOVIE.ANI")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/ROCKET.ANI")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/stairs.ani")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/SUMOBJ.ANI")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/test.ani")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/UNAVAIL.ANI")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ani/YQUEST.ANI")]
        [TestMethod]
        public Task ani(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/arw/example.arw")]
        [DataRow("https://telparia.com/fileFormatSamples/image/arw/light4.arw")]
        [DataRow("https://telparia.com/fileFormatSamples/image/arw/RAW_SONY_SLT-A35.ARW")]
        [DataRow("https://telparia.com/fileFormatSamples/image/arw/shadow8.arw")]
        [DataRow("https://telparia.com/fileFormatSamples/image/arw/shadow9.arw")]
        [DataRow("https://telparia.com/fileFormatSamples/image/arw/sony_a500_04.arw")]
        [DataRow("https://telparia.com/fileFormatSamples/image/arw/sony_a500_09.arw")]
        [DataRow("https://telparia.com/fileFormatSamples/image/arw/sony_nex_5_07.arw")]
        [DataRow("https://telparia.com/fileFormatSamples/image/arw/wizard9.arw")]
        [DataRow("https://telparia.com/fileFormatSamples/image/arw/_DSC8834.ARW")]
        [TestMethod]
        public Task arw(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/avif/abydos.avif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/avif/cosmos1650_yuv444_10bpc_p3pq.avif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/avif/kodim03_yuv420_8bpc.avif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/avif/kodim23_yuv420_8bpc.avif")]
        [TestMethod]
        public Task avif(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/0004.pic")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/006.pic")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/029_011.bmp")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/abydos.dib")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/CADUCEUS.BMP")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/crowd.dt")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/cubes1.bmp")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/example.bmp")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/example.dib")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/EXAMPLE1.BMP")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/example_small.bmp")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/fact.bmp")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/flag_b24.bmp")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/fonts125.bmp")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/input.bmp")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/input.bmp24")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/input.dib")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/nagel02.bmp")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/nissan.bmp")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/QUEENO.BMP")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/ray.bmp")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/sample_640×426.bmp")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/shuttle.bmp")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/Space.bmp")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/test.bmp")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/tru256.bmp")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/VGALOGO.RLE")]
        [DataRow("https://telparia.com/fileFormatSamples/image/bmp/WATER5.BMP")]
        [TestMethod]
        public Task bmp(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/cdr/11ARMRD0.CDR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cdr/70DIVTR1.CDR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cdr/ARMYSEAL.CDR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cdr/ARMYSTF1.CDR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cdr/ARROW5.CDR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cdr/BALLET.CDR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cdr/DALI.CDR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cdr/DRINKS.CDR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cdr/example.cdr")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cdr/FLAG.CDR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cdr/FOX.CDR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cdr/MENU.CDR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cdr/TAKE_ONE.CDR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cdr/test.cdr")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cdr/WI.CDR")]
        [TestMethod]
        public Task cdr(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/anj-tristesse_dedykacja.cin")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/anj-tristesse_katol.attack.cin")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/anj-tristesse_laboratorium.dr.vista.cin")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/anj-tristesse_potaterrorist.cin")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/DEDYKACJ.CIN")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/dracon-tqa_drac2.cin")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/dracon-tqa_fight.cin")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/dracon-tqa_killer.cin")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/dracon-tqa_smus.cin")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/dracon-tqa_sun.cin")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/dracon-tqa_sunv2.cin")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/DRACONUS.CCI")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/FIGHT.CCI")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/heretyk-samar_colin.cin")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/KILLER.CCI")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/pigula_fujibaby.cin")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/pigula_trex.cin")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/piorun-bbsl_priest.cin")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/piorun-bbsl_samurai.cin")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/piorun-bbsl_warriors.cin")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/piorun-bbsl_wiedzma.cin")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/PTCIN192.CIN")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/SMUS.CCI")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/SUN.CCI")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/SUNV2.CCI")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/THESUNV2.CIN")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/TITUS.CIN")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/vidol_death.cin")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/vidol_dragon.cin")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/vidol_dupland.cin")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/vidol_smutek.cin")]
        [DataRow("https://telparia.com/fileFormatSamples/image/championsInterlace/wawrzyn-bbsl_bebok.cin")]
        [TestMethod]
        public Task championsInterlace(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/cmx/balloon.cmx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cmx/bluedrop.cmx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cmx/CM33.CMX")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cmx/goldblue.cmx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cmx/LIZARDS.CMX")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cmx/PC.CMX")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cmx/test.cmx")]
        [TestMethod]
        public Task cmx(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/corelClipart/36C_FLOW.CCX")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelClipart/cooler9.CCX")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelClipart/COTTON.CCX")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelClipart/FLOWER54.CCX")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelClipart/stpat.CCX")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelClipart/suna.CCX")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelClipart/TEST.CCX")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelClipart/WHEATSHE.CCX")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelClipart/WINDMILL.CCX")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelClipart/WORKGLOV.CCX")]
        [TestMethod]
        public Task corelClipart(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/cBlues.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/cCamoflg.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/cCandy.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/cChrstms.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/cCoralrf.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/cEarthy.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/cFemale.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/cFoliage.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/cHair.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/cLipstik.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/cLove.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/cMetalic.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/cMidnite.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/cMud.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/cNeon.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/cPastels.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/cPencils.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/cPenink.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/cRainbow.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/cReds.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/cSky.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/cSummer.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/explorer.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/Gray100.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rApples.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rAutumn.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rCandy.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rCoralrf.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rDesert.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rDirt.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rEaster.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rFlesh.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rGreens.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rHair.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rHallown.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rJungle.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rMetalic.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rMidnite.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rMud.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rNeon.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rPastels.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rPeaches.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rRainbow.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rReds.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rRoses.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rSand.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rSky.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rSpring.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rTiger.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/rWinter.cpl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelColorPalette/vga.cpl")]
        [TestMethod]
        public Task corelColorPalette(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/corelDrawPattern/CUBES2.PAT")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelDrawPattern/GOLDBEV1.PAT")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelDrawPattern/PAT76.PAT")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelDrawPattern/PAT77.PAT")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelDrawPattern/PAT78.PAT")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelDrawPattern/PLAD.PAT")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelDrawPattern/REPTILES.PAT")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelDrawPattern/squares4.pat")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelDrawPattern/TILE03.PAT")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelDrawPattern/TILE08.PAT")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelDrawPattern/weave2.pat")]
        [TestMethod]
        public Task corelDrawPattern(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/corelPhotoPaint/CONER.CPT")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelPhotoPaint/discs%20red%20purple.cpt")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelPhotoPaint/fire.cpt")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelPhotoPaint/food15l.cpt")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelPhotoPaint/Modern2.cpt")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelPhotoPaint/NEW-2.CPT")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelPhotoPaint/PAPER14M.CPT")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelPhotoPaint/tint2.cpt")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelPhotoPaint/tint3.cpt")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelPhotoPaint/tra1samp.cpt")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelPhotoPaint/welsmpl.cpt")]
        [DataRow("https://telparia.com/fileFormatSamples/image/corelPhotoPaint/WILDANIM.CPT")]
        [TestMethod]
        public Task corelPhotoPaint(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/cr2/example.cr2")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cr2/sample1.cr2")]
        [TestMethod]
        public Task cr2(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/cur/ATLUS.011.cur")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cur/BIGARROW.CUR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cur/CAPTURE.cur")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cur/CUR_HOLD.win1.cur")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cur/HAND.CUR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cur/HEART.CUR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cur/PENCIL.CUR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cur/SHOWFONT.ICO")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cur/SLEEP.CUR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cur/SNAGIT.001.cur")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cur/test.cur")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cur/WATCH.CUR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/cur/win1_009_76.cur")]
        [TestMethod]
        public Task cur(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/degasEliteBrush/JET.BRU")]
        [DataRow("https://telparia.com/fileFormatSamples/image/degasEliteBrush/MAN.BRU")]
        [DataRow("https://telparia.com/fileFormatSamples/image/degasEliteBrush/MINIAIR.BRU")]
        [DataRow("https://telparia.com/fileFormatSamples/image/degasEliteBrush/POINTDWN.BRU")]
        [DataRow("https://telparia.com/fileFormatSamples/image/degasEliteBrush/POINTUP.BRU")]
        [DataRow("https://telparia.com/fileFormatSamples/image/degasEliteBrush/ROUND.BRU")]
        [DataRow("https://telparia.com/fileFormatSamples/image/degasEliteBrush/THREEDOT.BRU")]
        [DataRow("https://telparia.com/fileFormatSamples/image/degasEliteBrush/TWODOTS.BRU")]
        [TestMethod]
        public Task degasEliteBrush(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/dng/church-raw.dng")]
        [DataRow("https://telparia.com/fileFormatSamples/image/dng/example.dng")]
        [DataRow("https://telparia.com/fileFormatSamples/image/dng/L1004220.DNG")]
        [DataRow("https://telparia.com/fileFormatSamples/image/dng/sample1.dng")]
        [TestMethod]
        public Task dng(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/emf/1651596601_example.emf")]
        [DataRow("https://telparia.com/fileFormatSamples/image/emf/367")]
        [DataRow("https://telparia.com/fileFormatSamples/image/emf/abydos.emf")]
        [DataRow("https://telparia.com/fileFormatSamples/image/emf/example.emf")]
        [DataRow("https://telparia.com/fileFormatSamples/image/emf/INTRO.WMF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/emf/ISI.EMF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/emf/Light%20Bar.emf")]
        [DataRow("https://telparia.com/fileFormatSamples/image/emf/LOGO5.WMF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/emf/LOGO6.WMF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/emf/LOGO7.WMF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/emf/Text%20Buttons%20%28No%20Print%29.emf")]
        [TestMethod]
        public Task emf(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/3_tulips._hc")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/abydos.eps")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/airplane")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/cute_cup.id")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/dragon_r.oa1")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/eagle")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/eagle.001")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/eagle.eps")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/example.eps")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/example_small.eps")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/flag_b24.eps")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/gemini.eps")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/GOLFER.PS")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/GUNTH.EPS")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/input.eps")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/iris_wat.erc")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/parrot.ps")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/postman-.co1")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/santa_wa.vin")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/SIGN.eps")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/stop_lig.ht1")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/table2_1_76.ps")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/test.eps")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/tiger.ps")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/tru256.eps")]
        [DataRow("https://telparia.com/fileFormatSamples/image/eps/TWILIGHT.EPS")]
        [TestMethod]
        public Task eps(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/erf/RAW_EPSON_RD1.ERF")]
        [TestMethod]
        public Task erf(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/fig/balance.fig")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fig/bedroom2.fig")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fig/breadboard.fig")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fig/bugs.gif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fig/gold.fig")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fig/greenpig.fig")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fig/icebergs.jpg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fig/input.fig")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fig/lidar.fig")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fig/musicnotes.fig")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fig/pictures.fig")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fig/pumpkin.xbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fig/teapot.xpm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fig/transit.fig")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fig/uno_hand.fig")]
        [TestMethod]
        public Task fig(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/fpr/fliprofi_demopic.fpr")]
        [TestMethod]
        public Task fpr(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/fpx/bottom.fpx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fpx/front.fpx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fpx/input_256.fpx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fpx/input_bw.fpx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fpx/input_grayscale.fpx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fpx/input_jpeg.fpx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fpx/input_truecolor.fpx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fpx/left.fpx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fpx/right.fpx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/fpx/top.fpx")]
        [TestMethod]
        public Task fpx(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/fxg/abydos.fxg")]
        [TestMethod]
        public Task fxg(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/ged/HUNTERS.GED")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ged/MARTIAN.GED")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ged/TEST.GED")]
        [TestMethod]
        public Task ged(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/gemMetafile/BORDR101.GEM")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gemMetafile/BORDR102.GEM")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gemMetafile/BORDR103.GEM")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gemMetafile/cameras.gem")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gemMetafile/CORN03LL.GEM")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gemMetafile/CORN03LR.GEM")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gemMetafile/CORN03UL.GEM")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gemMetafile/CORN03UR.GEM")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gemMetafile/CROSS03.GEM")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gemMetafile/doctor.gem")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gemMetafile/LABEL01.GEM")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gemMetafile/light.gem")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gemMetafile/LOOK.GEM")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gemMetafile/safety.gem")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gemMetafile/tools.gem")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gemMetafile/windmill.gem")]
        [TestMethod]
        public Task gemMetafile(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/gif/89aillus.gif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gif/abydos.gif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gif/animated.gif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gif/athome.gif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gif/BOB-89A.GIF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gif/butts09.gif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gif/ds_f_399.gif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gif/example.gif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gif/example_animated.gif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gif/example_animated_small.gif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gif/example_small.gif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gif/input.gif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gif/input.gif87")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gif/sample_1920×1280.gif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gif/test.gif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gif/wfpc04.gif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gif/wornt01.gif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/gif/wtrback.gif")]
        [TestMethod]
        public Task gif(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/ackroyd.gr9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/airwolf.gr9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/AMAZON.GR8")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/atari_girl_by_motionride_gfx_compo_ironia_2017.g10")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/bada.g10")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/BlackLamp_Atari_RichardMunns.gr7")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/boys.gr9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/chess.gr9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/CrumblesCrisis_Atari_RichardMunns.gr7")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/cybill.gr9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/elvis.gr9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/FACES.GR8")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/fievel.gr9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/fonda.gr9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/gabriela.gr9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/gbust3.gr9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/globie.gr9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/GTIA_HIWAY.G11")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/HUSAJN.GR8")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/jen.gr9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/kctoon.gr9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/maiden.gr9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/model.gr9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/MotionRide.gr8")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/naomi.gr9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/number5.gr9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/NUMEN.G10")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/Pixel_GFX_Lucy_DobiegnievButurovich.g10")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/r2d2.gr9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/raisin1.gr9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/screen.gr8")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/ShitHead_Atari_RichardMunns.gr7")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/SHPOON.G9S")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/SHPOON.GR9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/SHPOON.SFD")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/SpaceLobsters_Atari_RichardMunns.gr7")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/SPACE_LOBSTERS.GR7")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/sparkles.gr9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/sundance10.gr8")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/supergir.gr9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/Tagalon_Atari_RichardMunns.gr7")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/the_room_by_Lucy_gfx_compo_ironia_2017.g10")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/topgirl.gr9")]
        [DataRow("https://telparia.com/fileFormatSamples/image/grStar/torus.gr9p")]
        [TestMethod]
        public Task grStar(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/heic/abydos.heic")]
        [DataRow("https://telparia.com/fileFormatSamples/image/heic/example.heic")]
        [DataRow("https://telparia.com/fileFormatSamples/image/heic/sample1.heic")]
        [DataRow("https://telparia.com/fileFormatSamples/image/heic/sample1.heif")]
        [TestMethod]
        public Task heic(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/AGA2.HIP")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/AGA2.HPS")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/agony.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/animalre.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/aplane.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/astaroth.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/BALLERIN.HIP")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/bmp2hip_exhibit.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/bmp2hip_imagine.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/bmp2hip_jurgi_muza.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/BMP2HIP_MANGA.HIP")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/bmp2hip_mroova.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/bmp2hip_nature.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/bmp2hip_psajhoo.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/bmp2hip_tiger-ng_legion.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/CINDY034.HIP")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/computer.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/dhor.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/earth.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/ford.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/giger3.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/girl04.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/girl1.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/guard.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/hardlogo.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/INSETR1.HIP")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/jurassic.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/markus.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/milipede.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/muskete.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/nirjiri.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/nirwolf.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/OKNO.HIP")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/paulina.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/playdash.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/POISON.HIP")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/progdemo.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/ptero.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/rebuff.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/silhouet.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/sparrow.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/techtalk.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/unpack.hip")]
        [DataRow("https://telparia.com/fileFormatSamples/image/hip/wildcat.hip")]
        [TestMethod]
        public Task hip(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/icdrawIcon/blankcoi.ibi")]
        [DataRow("https://telparia.com/fileFormatSamples/image/icdrawIcon/browns1.ibi")]
        [DataRow("https://telparia.com/fileFormatSamples/image/icdrawIcon/buckface.ibi")]
        [DataRow("https://telparia.com/fileFormatSamples/image/icdrawIcon/cdrom_2.ibi")]
        [DataRow("https://telparia.com/fileFormatSamples/image/icdrawIcon/cherry.ib3")]
        [DataRow("https://telparia.com/fileFormatSamples/image/icdrawIcon/CLA.IB3")]
        [DataRow("https://telparia.com/fileFormatSamples/image/icdrawIcon/CLAVHDL.IB3")]
        [DataRow("https://telparia.com/fileFormatSamples/image/icdrawIcon/eyes.ib3")]
        [DataRow("https://telparia.com/fileFormatSamples/image/icdrawIcon/FIXPLIER.IB3")]
        [DataRow("https://telparia.com/fileFormatSamples/image/icdrawIcon/FSM.IB3")]
        [DataRow("https://telparia.com/fileFormatSamples/image/icdrawIcon/FSMEDIT.IB3")]
        [DataRow("https://telparia.com/fileFormatSamples/image/icdrawIcon/grocery1.ib3")]
        [DataRow("https://telparia.com/fileFormatSamples/image/icdrawIcon/pencil1m.ibi")]
        [DataRow("https://telparia.com/fileFormatSamples/image/icdrawIcon/PENCIL2M.IBI")]
        [DataRow("https://telparia.com/fileFormatSamples/image/icdrawIcon/REDPHONE.IB3")]
        [DataRow("https://telparia.com/fileFormatSamples/image/icdrawIcon/toaster2.ib3")]
        [DataRow("https://telparia.com/fileFormatSamples/image/icdrawIcon/xmas1.ib3")]
        [DataRow("https://telparia.com/fileFormatSamples/image/icdrawIcon/zipper.ib3")]
        [TestMethod]
        public Task icdrawIcon(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/ico/abydos.ico")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ico/example.ico")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ico/example_small.ico")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ico/FONT.ICO")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ico/GIFV.ICO")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ico/input.ico")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ico/N.ICO")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ico/sample_1920×1280.ico")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ico/test.ico")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ico/test.pad")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ico/WELMAT.ICO")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ico/XTALK2.ICO")]
        [TestMethod]
        public Task ico(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/jbig/1668051287_input.bie")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jbig/ccitt1.jbg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jbig/ccitt2.bie")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jbig/ccitt6.jbg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jbig/input.bie")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jbig/input.jbig")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jbig/mx.jbg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jbig/NewAmiLogo.jbg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jbig/xvlogo.jbg")]
        [TestMethod]
        public Task jbig(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/jpeg2000/9tailhk0.jp2")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpeg2000/abydos.jp2")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpeg2000/example.jp2")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpeg2000/input.j2k")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpeg2000/input.jp2")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpeg2000/LIGHT-ROM%201%20%28Amiga%20Library%20Services%29%281994%29%20Insert_0000.jp2")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpeg2000/LIGHT-ROM%201%20%28Amiga%20Library%20Services%29%281994%29%20Insert_0001.jp2")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpeg2000/sample1.jp2")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpeg2000/test.jp2")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpeg2000/World_Atlas_V5_Manual_001_0000.jp2")]
        [TestMethod]
        public Task jpeg2000(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/jpg/187645SDCCMYK35.jpg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpg/abydos.jpeg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpg/example.jpe")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpg/example.jpeg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpg/example.jpg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpg/example.jps")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpg/example_small.jpeg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpg/example_small.jpg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpg/greet02.jpg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpg/image5.jpg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpg/mars1.jpg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpg/Martingale4.jpg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpg/mpfeif07.jpg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpg/parrots.jpg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpg/rept053.jpg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpg/ring_mo4.jpg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpg/sample1.jfif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpg/sample_1920×1280.jpg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpg/t2screensaver.jpg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpg/test.jpg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpg/thinskin.jpg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/jpg/WORLD.JPG")]
        [TestMethod]
        public Task jpg(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/kodakDCR/KODAK-DCSPRO.DCR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/kodakDCR/RAW_KODAK_DCSPRO.DCR")]
        [TestMethod]
        public Task kodakDCR(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/nikon/example.nef")]
        [DataRow("https://telparia.com/fileFormatSamples/image/nikon/example_layers_small.tif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/nikon/example_layers_small.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/nikon/General.tif.tif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/nikon/RAW_NIKON_D5000.NEF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/nikon/RAW_NIKON_P7000.NRW")]
        [DataRow("https://telparia.com/fileFormatSamples/image/nikon/sample1.nef")]
        [DataRow("https://telparia.com/fileFormatSamples/image/nikon/sample1.nrw")]
        [TestMethod]
        public Task nikon(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/odg/abydos.odg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/odg/example.odg")]
        [TestMethod]
        public Task odg(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/ora/abydos.ora")]
        [TestMethod]
        public Task ora(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/orf/olympus_om_d_e_m1_28.orf")]
        [DataRow("https://telparia.com/fileFormatSamples/image/orf/olympus_om_d_e_m5_14.orf")]
        [DataRow("https://telparia.com/fileFormatSamples/image/orf/RAW_OLYMPUS_EP1.ORF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/orf/sample1.orf")]
        [TestMethod]
        public Task orf(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/os2Pointer/ba.ptr")]
        [DataRow("https://telparia.com/fileFormatSamples/image/os2Pointer/ba_001_2.ptr")]
        [DataRow("https://telparia.com/fileFormatSamples/image/os2Pointer/CKRDHAND.PTR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/os2Pointer/CKRUHAND.PTR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/os2Pointer/ILLEGAL.PTR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/os2Pointer/PMVIEW.005.ptr")]
        [DataRow("https://telparia.com/fileFormatSamples/image/os2Pointer/PMVIEW.007.ptr")]
        [DataRow("https://telparia.com/fileFormatSamples/image/os2Pointer/PMVIEW.014.ptr")]
        [DataRow("https://telparia.com/fileFormatSamples/image/os2Pointer/RADIOFAX.001.ptr")]
        [DataRow("https://telparia.com/fileFormatSamples/image/os2Pointer/WAIT.PTR")]
        [DataRow("https://telparia.com/fileFormatSamples/image/os2Pointer/wait000.ptr")]
        [TestMethod]
        public Task os2Pointer(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/panasonicRaw/leica_d_lux_5_titanium_02.rwl")]
        [DataRow("https://telparia.com/fileFormatSamples/image/panasonicRaw/panasonic_lumix_dmc_lx3_04.rw2")]
        [DataRow("https://telparia.com/fileFormatSamples/image/panasonicRaw/RAW_PANASONIC_DMC-GF1.RW2")]
        [DataRow("https://telparia.com/fileFormatSamples/image/panasonicRaw/sample1.rw2")]
        [TestMethod]
        public Task panasonicRaw(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/pbm/abydos.pbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pbm/BMAP_3684.pbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pbm/dragon.pbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pbm/example.pbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pbm/input_p1.pbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pbm/input_p4.pbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pbm/PAT_21.pbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pbm/PAT_23_45.pbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pbm/PAT_40.pbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pbm/PAT_9.pbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pbm/sample_1920×1280.pbm")]
        [TestMethod]
        public Task pbm(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/pcx/abydos.pcx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pcx/ann536.pcx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pcx/bad.pcx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pcx/bor069.pcx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pcx/BUGLE.PCX")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pcx/CAT.PCX")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pcx/E-DIODE.ST")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pcx/example.pcx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pcx/FIRE1.PCC")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pcx/FONTS.PCX")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pcx/input.pcx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pcx/kidschl.pcx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pcx/kidsonpc.pcx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pcx/MUSIC-13.ST")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pcx/ROSE.ST")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pcx/sample_1920×1280.pcx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pcx/swinging.pcx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pcx/te%C3%B6s%C3%B6%C3%B6%C3%B6.pcx")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pcx/WINSCR.PCX")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pcx/ZDUDE1R.PCX")]
        [TestMethod]
        public Task pcx(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/pentaxRaw/IMGP6677.PEF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pentaxRaw/NATURAL.PEF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pentaxRaw/pentax_k20d_02.pef")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pentaxRaw/pentax_k5_14.pef")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pentaxRaw/RAW_PENTAX_K100.PEF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pentaxRaw/sample1.pef")]
        [TestMethod]
        public Task pentaxRaw(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/pgm/abydos.pgm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pgm/align_c.ppm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pgm/bg.ppm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pgm/bild4.pgm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pgm/dragon.pgm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pgm/example.pgm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pgm/input_p2.pgm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pgm/input_p5.pgm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pgm/lennasmall.pgm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pgm/logo")]
        [DataRow("https://telparia.com/fileFormatSamples/image/pgm/sample_1920×1280.pgm")]
        [TestMethod]
        public Task pgm(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/photoDrawMIX/button27.mix")]
        [DataRow("https://telparia.com/fileFormatSamples/image/photoDrawMIX/dsb0011.mix")]
        [DataRow("https://telparia.com/fileFormatSamples/image/photoDrawMIX/PTMH05.MIX")]
        [DataRow("https://telparia.com/fileFormatSamples/image/photoDrawMIX/rs0170.mix")]
        [DataRow("https://telparia.com/fileFormatSamples/image/photoDrawMIX/Stroop%20test%20%28setting%29.mix")]
        [DataRow("https://telparia.com/fileFormatSamples/image/photoDrawMIX/TM0511.MIX")]
        [DataRow("https://telparia.com/fileFormatSamples/image/photoDrawMIX/TM0528.MIX")]
        [DataRow("https://telparia.com/fileFormatSamples/image/photoDrawMIX/TM0653.MIX")]
        [DataRow("https://telparia.com/fileFormatSamples/image/photoDrawMIX/TM0659.MIX")]
        [DataRow("https://telparia.com/fileFormatSamples/image/photoDrawMIX/tm1053.mix")]
        [DataRow("https://telparia.com/fileFormatSamples/image/photoDrawMIX/tm1399.mix")]
        [DataRow("https://telparia.com/fileFormatSamples/image/photoDrawMIX/tm1422.mix")]
        [TestMethod]
        public Task photoDrawMIX(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/png/abydos.png")]
        [DataRow("https://telparia.com/fileFormatSamples/image/png/animated.png")]
        [DataRow("https://telparia.com/fileFormatSamples/image/png/antiqhvy.png")]
        [DataRow("https://telparia.com/fileFormatSamples/image/png/arrowdwn.png")]
        [DataRow("https://telparia.com/fileFormatSamples/image/png/input_16.png")]
        [DataRow("https://telparia.com/fileFormatSamples/image/png/input_256.png")]
        [DataRow("https://telparia.com/fileFormatSamples/image/png/input_bw.png")]
        [DataRow("https://telparia.com/fileFormatSamples/image/png/input_mono.png")]
        [DataRow("https://telparia.com/fileFormatSamples/image/png/input_truecolor.png")]
        [DataRow("https://telparia.com/fileFormatSamples/image/png/JUDGE.png")]
        [DataRow("https://telparia.com/fileFormatSamples/image/png/sample_1920×1280.png")]
        [DataRow("https://telparia.com/fileFormatSamples/image/png/test.png")]
        [TestMethod]
        public Task png(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/ppm/1629906048_testimg.ppm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ppm/abydos.ppm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ppm/BARNEY7.PPM")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ppm/BARNICO3.PPM")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ppm/dragon.ppm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ppm/example.ppm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ppm/input_p3.ppm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ppm/input_p6.ppm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ppm/sample_1920×1280.ppm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/ppm/TESTIMG.PPM")]
        [TestMethod]
        public Task ppm(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/psd/abydos.psd")]
        [DataRow("https://telparia.com/fileFormatSamples/image/psd/ColorPubOpen.psd")]
        [DataRow("https://telparia.com/fileFormatSamples/image/psd/cover2.bmp")]
        [DataRow("https://telparia.com/fileFormatSamples/image/psd/derriere.psd")]
        [DataRow("https://telparia.com/fileFormatSamples/image/psd/Endfile.psd")]
        [DataRow("https://telparia.com/fileFormatSamples/image/psd/example.psd")]
        [DataRow("https://telparia.com/fileFormatSamples/image/psd/example_small.psd")]
        [DataRow("https://telparia.com/fileFormatSamples/image/psd/GamerLar.psd")]
        [DataRow("https://telparia.com/fileFormatSamples/image/psd/Horn.psd")]
        [DataRow("https://telparia.com/fileFormatSamples/image/psd/input.psd")]
        [DataRow("https://telparia.com/fileFormatSamples/image/psd/main1")]
        [DataRow("https://telparia.com/fileFormatSamples/image/psd/sample_1920×1280.psd")]
        [DataRow("https://telparia.com/fileFormatSamples/image/psd/Tour.psd")]
        [TestMethod]
        public Task psd(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/raf/example.raf")]
        [DataRow("https://telparia.com/fileFormatSamples/image/raf/fujifilm_x_e1_13.raf")]
        [DataRow("https://telparia.com/fileFormatSamples/image/raf/RAW_FUJI_S5000.RAF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/raf/sample1.raf")]
        [TestMethod]
        public Task raf(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/rpgMakerXYZ/cal0001.xyz")]
        [DataRow("https://telparia.com/fileFormatSamples/image/rpgMakerXYZ/GameOver02.xyz")]
        [DataRow("https://telparia.com/fileFormatSamples/image/rpgMakerXYZ/Mark_03.xyz")]
        [DataRow("https://telparia.com/fileFormatSamples/image/rpgMakerXYZ/Panel45.xyz")]
        [DataRow("https://telparia.com/fileFormatSamples/image/rpgMakerXYZ/UN2_tittle.xyz")]
        [DataRow("https://telparia.com/fileFormatSamples/image/rpgMakerXYZ/%E5%9F%BA%E6%9C%AC%E3%83%81%E3%83%83%E3%83%97%28256%29.xyz")]
        [DataRow("https://telparia.com/fileFormatSamples/image/rpgMakerXYZ/%E6%88%A6%E9%97%98%E7%94%A8%E6%95%B0%E5%80%A42.xyz")]
        [DataRow("https://telparia.com/fileFormatSamples/image/rpgMakerXYZ/%E7%84%A1%E9%A1%8C.xyz")]
        [DataRow("https://telparia.com/fileFormatSamples/image/rpgMakerXYZ/%E8%83%8C%E6%99%AF08.xyz")]
        [TestMethod]
        public Task rpgMakerXYZ(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/svg/abydos.svg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/svg/example.svg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/svg/grandcan.svg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/svg/p17_aj.svg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/svg/sample_1920×1280.svg")]
        [DataRow("https://telparia.com/fileFormatSamples/image/svg/test.svg")]
        [TestMethod]
        public Task svg(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/tga/abydos.tga")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tga/DEMO.TGA")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tga/example.tga")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tga/example_small.tga")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tga/flag_b32.tga")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tga/flag_t24.tga")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tga/ht_l0481.tga")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tga/input.tga")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tga/milssa07.tga")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tga/sample_1920×1280.tga")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tga/test.tga")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tga/xing_b16.tga")]
        [TestMethod]
        public Task tga(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/theDrawCOM/-WARNING.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/image/theDrawCOM/%402CLOUD9.EXE")]
        [DataRow("https://telparia.com/fileFormatSamples/image/theDrawCOM/BEGIN.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/image/theDrawCOM/ENDGAME.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/image/theDrawCOM/GAMEANSI.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/image/theDrawCOM/GOLDEN.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/image/theDrawCOM/MAIN.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/image/theDrawCOM/TECHNODR.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/image/theDrawCOM/TITLE.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/image/theDrawCOM/TS.COM")]
        [TestMethod]
        public Task theDrawCOM(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/abydos.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/CASTLE.IMJ")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/ccitt_4.tif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/CD%20Front%20%28Alt.%29.tif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/CHEETAH.TIF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/example.tif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/example.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/example_small.tif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/example_small.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/g31d.tif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/hi100.tif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/img0030.imj")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/input_16.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/input_16_matte.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/input_256.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/input_256_matte.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/input_256_planar_contig.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/input_256_planar_separate.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/input_gray_12bit.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/input_gray_16bit.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/input_gray_4bit.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/input_gray_4bit_matte.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/input_gray_8bit.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/input_gray_8bit_matte.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/input_mono.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/input_truecolor.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/input_truecolor_16.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/input_truecolor_stripped.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/input_truecolor_tiled32x32.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/IS9409.IMJ")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/ISA001.IMJ")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/ISA016.IMJ")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/L1A.ZZZ")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/pic0037.imj")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/pic0272.imj")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/PIE01.TIF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/PIE02.TIF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/PIE03.TIF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/PIE04.TIF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/sample_1920×1280.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/sample_detail_1.tif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/shiro02.tif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/test.tiff")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/TRAIN1.TIF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/TRAIN2.TIF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/TRAIN3.TIF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/WHALE.TIF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/tiff/xing_t24.tif")]
        [TestMethod]
        public Task tiff(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/vbm/b777.vbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/vbm/barbis.vbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/vbm/cheeta.vbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/vbm/clown.vbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/vbm/coke_can.vbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/vbm/eschmose.vbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/vbm/godteam.vbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/vbm/hawk2.vbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/vbm/icecream.vbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/vbm/lady.vbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/vbm/ln_escort.vbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/vbm/marlene.vbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/vbm/nagel01.vbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/vbm/tiger.vbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/vbm/toucan.bm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/vbm/ufplogo256.vbm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/vbm/winona2.vbm")]
        [TestMethod]
        public Task vbm(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/webp/abydos.webp")]
        [DataRow("https://telparia.com/fileFormatSamples/image/webp/animated.webp")]
        [DataRow("https://telparia.com/fileFormatSamples/image/webp/example.webp")]
        [DataRow("https://telparia.com/fileFormatSamples/image/webp/example_small.webp")]
        [DataRow("https://telparia.com/fileFormatSamples/image/webp/sample1.webp")]
        [DataRow("https://telparia.com/fileFormatSamples/image/webp/test.webp")]
        [TestMethod]
        public Task webp(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/windowsClipboard/BROWSE.CLP")]
        [DataRow("https://telparia.com/fileFormatSamples/image/windowsClipboard/DRIVE.CLP")]
        [DataRow("https://telparia.com/fileFormatSamples/image/windowsClipboard/EDIT.CLP")]
        [DataRow("https://telparia.com/fileFormatSamples/image/windowsClipboard/HA.CLP")]
        [DataRow("https://telparia.com/fileFormatSamples/image/windowsClipboard/MO.CLP")]
        [DataRow("https://telparia.com/fileFormatSamples/image/windowsClipboard/NO.CLP")]
        [DataRow("https://telparia.com/fileFormatSamples/image/windowsClipboard/PGAW9.CLP")]
        [DataRow("https://telparia.com/fileFormatSamples/image/windowsClipboard/SAMPTIM1.CL")]
        [DataRow("https://telparia.com/fileFormatSamples/image/windowsClipboard/SAWGRASS.CLP")]
        [DataRow("https://telparia.com/fileFormatSamples/image/windowsClipboard/SCORE.CLP")]
        [TestMethod]
        public Task windowsClipboard(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/wmf/001.WMF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/wmf/1651598579_example.wmf")]
        [DataRow("https://telparia.com/fileFormatSamples/image/wmf/3dball.wmf")]
        [DataRow("https://telparia.com/fileFormatSamples/image/wmf/abydos.wmf")]
        [DataRow("https://telparia.com/fileFormatSamples/image/wmf/AK_HI.WMF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/wmf/bananas.wmf")]
        [DataRow("https://telparia.com/fileFormatSamples/image/wmf/browse2.wmf")]
        [DataRow("https://telparia.com/fileFormatSamples/image/wmf/clock.wmf")]
        [DataRow("https://telparia.com/fileFormatSamples/image/wmf/example.wmf")]
        [DataRow("https://telparia.com/fileFormatSamples/image/wmf/FLOW1.WMF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/wmf/HORN.OVG")]
        [DataRow("https://telparia.com/fileFormatSamples/image/wmf/MINN.XK4")]
        [DataRow("https://telparia.com/fileFormatSamples/image/wmf/MOUNTAIN.WMF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/wmf/PIANO.OVG")]
        [DataRow("https://telparia.com/fileFormatSamples/image/wmf/run1.wmf")]
        [DataRow("https://telparia.com/fileFormatSamples/image/wmf/SOLIS.WMF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/wmf/test.wmf")]
        [DataRow("https://telparia.com/fileFormatSamples/image/wmf/US_COLOR.WMF")]
        [DataRow("https://telparia.com/fileFormatSamples/image/wmf/wizard.wmf")]
        [TestMethod]
        public Task wmf(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/xvThumbnail/Bladerunner-bg.ppm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/xvThumbnail/coral3.ppm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/xvThumbnail/critical")]
        [DataRow("https://telparia.com/fileFormatSamples/image/xvThumbnail/info.xpm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/xvThumbnail/snare-logo3.xpm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/xvThumbnail/t5.gif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/xvThumbnail/t7.gif")]
        [DataRow("https://telparia.com/fileFormatSamples/image/xvThumbnail/xfm_exec.xpm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/xvThumbnail/xterm.xpm")]
        [DataRow("https://telparia.com/fileFormatSamples/image/xvThumbnail/xview.xpm")]
        [TestMethod]
        public Task xvThumbnail(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/image/zxNXI/Andy%20Green%20-%20Loco-Motion%20%282019%29.nxi")]
        [DataRow("https://telparia.com/fileFormatSamples/image/zxNXI/Andy%20Green%20-%20New%20Zealand%20Story%2C%20The%20%282017%29.nxi")]
        [DataRow("https://telparia.com/fileFormatSamples/image/zxNXI/Andy%20Green%20-%20Toki%20%282019%29.nxi")]
        [TestMethod]
        public Task zxNXI(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/music/digitalSoundInterfaceKit/andante.dsm")]
        [TestMethod]
        public Task digitalSoundInterfaceKit(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/music/digitalSoundInterfaceKitRIFF/LOGO.DSM")]
        [DataRow("https://telparia.com/fileFormatSamples/music/digitalSoundInterfaceKitRIFF/peace%20or%20annihilation.dsm")]
        [DataRow("https://telparia.com/fileFormatSamples/music/digitalSoundInterfaceKitRIFF/rollin%20-%20roll-music1.dsm")]
        [DataRow("https://telparia.com/fileFormatSamples/music/digitalSoundInterfaceKitRIFF/SONG1.DSM")]
        [DataRow("https://telparia.com/fileFormatSamples/music/digitalSoundInterfaceKitRIFF/SONGSSHR.DAT")]
        [DataRow("https://telparia.com/fileFormatSamples/music/digitalSoundInterfaceKitRIFF/SOUND_0.SND")]
        [DataRow("https://telparia.com/fileFormatSamples/music/digitalSoundInterfaceKitRIFF/SOUND_1.SND")]
        [DataRow("https://telparia.com/fileFormatSamples/music/digitalSoundInterfaceKitRIFF/SOUND_2.SND")]
        [DataRow("https://telparia.com/fileFormatSamples/music/digitalSoundInterfaceKitRIFF/trugg%20a.dsm")]
        [DataRow("https://telparia.com/fileFormatSamples/music/digitalSoundInterfaceKitRIFF/visions.dsm")]
        [TestMethod]
        public Task digitalSoundInterfaceKitRIFF(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/music/gameboyTracker/bob.mgb")]
        [DataRow("https://telparia.com/fileFormatSamples/music/gameboyTracker/find%20anything.mgb")]
        [DataRow("https://telparia.com/fileFormatSamples/music/gameboyTracker/paragon5%20successfully%20cracked.mgb")]
        [DataRow("https://telparia.com/fileFormatSamples/music/gameboyTracker/super%20groovy%20boy%20color.mgb")]
        [TestMethod]
        public Task gameboyTracker(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/music/mds/0001ctl.mds")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mds/grunge3.mid")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mds/gui_e.mds")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mds/part2nsf.mds")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mds/PART4.MDS")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mds/TABA1.MDS")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mds/TABA2.MDS")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mds/TABA3.MDS")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mds/TABB4.MDS")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mds/w02l03gm.mds")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mds/w06l01.mds")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mds/WIN4.MDS")]
        [TestMethod]
        public Task mds(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/music/mid/1sttime.mid")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mid/canyon.mid")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mid/DRUM.MID")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mid/example.kar")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mid/example.mid")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mid/example.midi")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mid/FILE13.DA1")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mid/FILE17.DA1")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mid/FILE18.DA1")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mid/INTRO.ROL")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mid/loveme.mid")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mid/media_tests_contents_media_api_music_ants.mid")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mid/PUFFBOY.MUS")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mid/R.ADL")]
        [DataRow("https://telparia.com/fileFormatSamples/music/mid/sleighr2.mid")]
        [TestMethod]
        public Task mid(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/music/pistonCollage/battle%20storm.pttune")]
        [DataRow("https://telparia.com/fileFormatSamples/music/pistonCollage/free-speech.ptcop")]
        [DataRow("https://telparia.com/fileFormatSamples/music/pistonCollage/obj0186-1.ptcop")]
        [DataRow("https://telparia.com/fileFormatSamples/music/pistonCollage/obj0528-2.ptcop")]
        [DataRow("https://telparia.com/fileFormatSamples/music/pistonCollage/obj1461-1.ptcop")]
        [DataRow("https://telparia.com/fileFormatSamples/music/pistonCollage/test3.ptcop")]
        [TestMethod]
        public Task pistonCollage(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/music/pokeyNoise/pn.internationalkarate")]
        [DataRow("https://telparia.com/fileFormatSamples/music/pokeyNoise/pn.internationalkarate.info")]
        [DataRow("https://telparia.com/fileFormatSamples/music/pokeyNoise/pn.schreckenstein")]
        [DataRow("https://telparia.com/fileFormatSamples/music/pokeyNoise/pn.schreckenstein.info")]
        [DataRow("https://telparia.com/fileFormatSamples/music/pokeyNoise/pn.storm")]
        [DataRow("https://telparia.com/fileFormatSamples/music/pokeyNoise/pn.storm.info")]
        [DataRow("https://telparia.com/fileFormatSamples/music/pokeyNoise/pn.tetris")]
        [DataRow("https://telparia.com/fileFormatSamples/music/pokeyNoise/pn.tetris.info")]
        [DataRow("https://telparia.com/fileFormatSamples/music/pokeyNoise/PN.Warhawk")]
        [DataRow("https://telparia.com/fileFormatSamples/music/pokeyNoise/PN.Warhawk.info")]
        [TestMethod]
        public Task pokeyNoise(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/music/renoise/access%20pwd.xrns")]
        [DataRow("https://telparia.com/fileFormatSamples/music/renoise/elegance.rns")]
        [DataRow("https://telparia.com/fileFormatSamples/music/renoise/gummi.xrns")]
        [DataRow("https://telparia.com/fileFormatSamples/music/renoise/jong%20belegen%20%28renoise%202.1%20version%29.xrns")]
        [DataRow("https://telparia.com/fileFormatSamples/music/renoise/jong%20belegen.xrns")]
        [DataRow("https://telparia.com/fileFormatSamples/music/renoise/power%20of%20renoise.rns")]
        [DataRow("https://telparia.com/fileFormatSamples/music/renoise/quality.rns")]
        [DataRow("https://telparia.com/fileFormatSamples/music/renoise/valleyball.xrns")]
        [TestMethod]
        public Task renoise(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/music/rmi/8ball2.rmi")]
        [DataRow("https://telparia.com/fileFormatSamples/music/rmi/BS01.rmi")]
        [DataRow("https://telparia.com/fileFormatSamples/music/rmi/BS02.rmi")]
        [DataRow("https://telparia.com/fileFormatSamples/music/rmi/furelise.rmi")]
        [DataRow("https://telparia.com/fileFormatSamples/music/rmi/hall.mid")]
        [DataRow("https://telparia.com/fileFormatSamples/music/rmi/INTRO.MID")]
        [DataRow("https://telparia.com/fileFormatSamples/music/rmi/mozart.mid")]
        [DataRow("https://telparia.com/fileFormatSamples/music/rmi/thealps.rmi")]
        [DataRow("https://telparia.com/fileFormatSamples/music/rmi/Tune%201.rmi")]
        [DataRow("https://telparia.com/fileFormatSamples/music/rmi/yosemite.rmi")]
        [TestMethod]
        public Task rmi(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/music/samCoupeSNG/chicago.sng")]
        [DataRow("https://telparia.com/fileFormatSamples/music/samCoupeSNG/kapsa3.sng")]
        [DataRow("https://telparia.com/fileFormatSamples/music/samCoupeSNG/neverendingstory.sng")]
        [DataRow("https://telparia.com/fileFormatSamples/music/samCoupeSNG/ofc1.sng")]
        [DataRow("https://telparia.com/fileFormatSamples/music/samCoupeSNG/PROMENA0.SNG")]
        [DataRow("https://telparia.com/fileFormatSamples/music/samCoupeSNG/shanghai.sng")]
        [DataRow("https://telparia.com/fileFormatSamples/music/samCoupeSNG/tetris.sng")]
        [TestMethod]
        public Task samCoupeSNG(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/music/sndh/ads2%20mix.sndh")]
        [DataRow("https://telparia.com/fileFormatSamples/music/sndh/Altair-Title.sndh")]
        [DataRow("https://telparia.com/fileFormatSamples/music/sndh/cpct%20tunes%20-%20space%20debris.sndh")]
        [DataRow("https://telparia.com/fileFormatSamples/music/sndh/CUDDLY4.SND")]
        [DataRow("https://telparia.com/fileFormatSamples/music/sndh/CUDRESET.SND")]
        [DataRow("https://telparia.com/fileFormatSamples/music/sndh/hotel%20california%20%28cover%29.sndh")]
        [DataRow("https://telparia.com/fileFormatSamples/music/sndh/kings%20quest%20ii.sndh")]
        [DataRow("https://telparia.com/fileFormatSamples/music/sndh/leisure%20suit%20larry%20%28main%29.sndh")]
        [DataRow("https://telparia.com/fileFormatSamples/music/sndh/masters%20of%20the%20universe.sndh")]
        [DataRow("https://telparia.com/fileFormatSamples/music/sndh/xybots.sndh")]
        [TestMethod]
        public Task sndh(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/music/soundMon/AXEL-F.SYNTH")]
        [DataRow("https://telparia.com/fileFormatSamples/music/soundMon/BP.Commando")]
        [DataRow("https://telparia.com/fileFormatSamples/music/soundMon/BPHAMMER.BS")]
        [DataRow("https://telparia.com/fileFormatSamples/music/soundMon/cardinals.bp")]
        [DataRow("https://telparia.com/fileFormatSamples/music/soundMon/CIRCUS.BS")]
        [DataRow("https://telparia.com/fileFormatSamples/music/soundMon/CYBERSONG")]
        [DataRow("https://telparia.com/fileFormatSamples/music/soundMon/GHOSTS.bp3")]
        [DataRow("https://telparia.com/fileFormatSamples/music/soundMon/MONTY%20REMIX")]
        [DataRow("https://telparia.com/fileFormatSamples/music/soundMon/popeye%20-%20level%201a.bp3")]
        [DataRow("https://telparia.com/fileFormatSamples/music/soundMon/SANXION")]
        [DataRow("https://telparia.com/fileFormatSamples/music/soundMon/THEME%20OF%20BRIAN")]
        [DataRow("https://telparia.com/fileFormatSamples/music/soundMon/uCommandoHi")]
        [TestMethod]
        public Task soundMon(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/music/soundTracker/10_under_pressure.mod")]
        [DataRow("https://telparia.com/fileFormatSamples/music/soundTracker/99redballoons.mod")]
        [DataRow("https://telparia.com/fileFormatSamples/music/soundTracker/actic-dance.mod")]
        [DataRow("https://telparia.com/fileFormatSamples/music/soundTracker/bachbell.mod")]
        [DataRow("https://telparia.com/fileFormatSamples/music/soundTracker/flight%20of%20bumblebee.mod")]
        [DataRow("https://telparia.com/fileFormatSamples/music/soundTracker/gamesong.mod")]
        [DataRow("https://telparia.com/fileFormatSamples/music/soundTracker/jjk107.mod")]
        [DataRow("https://telparia.com/fileFormatSamples/music/soundTracker/mod.dig.this")]
        [DataRow("https://telparia.com/fileFormatSamples/music/soundTracker/mod.erection")]
        [DataRow("https://telparia.com/fileFormatSamples/music/soundTracker/round%20the%20bend.mod")]
        [DataRow("https://telparia.com/fileFormatSamples/music/soundTracker/who%20else%3F.mod")]
        [TestMethod]
        public Task soundTracker(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/music/svarTracker/demo%20midiout.svar")]
        [DataRow("https://telparia.com/fileFormatSamples/music/svarTracker/demo%20x00%20command.svar")]
        [DataRow("https://telparia.com/fileFormatSamples/music/svarTracker/healing%20tune.svar")]
        [TestMethod]
        public Task svarTracker(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/music/trackerPacker/dat.titlemod")]
        [DataRow("https://telparia.com/fileFormatSamples/music/trackerPacker/EverlastingSunset.TP3")]
        [DataRow("https://telparia.com/fileFormatSamples/music/trackerPacker/leo.elk")]
        [DataRow("https://telparia.com/fileFormatSamples/music/trackerPacker/mod.longstress%28Virna%2793%29")]
        [DataRow("https://telparia.com/fileFormatSamples/music/trackerPacker/mod.unbeliable%28Virna%2792-93%29")]
        [DataRow("https://telparia.com/fileFormatSamples/music/trackerPacker/mod_2.tp3")]
        [DataRow("https://telparia.com/fileFormatSamples/music/trackerPacker/PZLZOO16.DAT")]
        [DataRow("https://telparia.com/fileFormatSamples/music/trackerPacker/soundwork.bin")]
        [DataRow("https://telparia.com/fileFormatSamples/music/trackerPacker/ts.tp3")]
        [DataRow("https://telparia.com/fileFormatSamples/music/trackerPacker/Tukka.Mod")]
        [DataRow("https://telparia.com/fileFormatSamples/music/trackerPacker/tune1.tpc")]
        [TestMethod]
        public Task trackerPacker(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/music/westwoodADL/dune%20ii%20-%2000.adl")]
        [DataRow("https://telparia.com/fileFormatSamples/music/westwoodADL/eob%20-%20adlib.adl")]
        [DataRow("https://telparia.com/fileFormatSamples/music/westwoodADL/eob2%20-%20intro.adl")]
        [DataRow("https://telparia.com/fileFormatSamples/music/westwoodADL/INTRO.ADL")]
        [DataRow("https://telparia.com/fileFormatSamples/music/westwoodADL/K2_DEMO.ADL")]
        [DataRow("https://telparia.com/fileFormatSamples/music/westwoodADL/KYRA2A.ADL")]
        [DataRow("https://telparia.com/fileFormatSamples/music/westwoodADL/KYRAMISC.ADL")]
        [DataRow("https://telparia.com/fileFormatSamples/music/westwoodADL/LORE02A.ADL")]
        [DataRow("https://telparia.com/fileFormatSamples/music/westwoodADL/LOREDEMO.ADL")]
        [DataRow("https://telparia.com/fileFormatSamples/music/westwoodADL/LOREINTR.ADL")]
        [TestMethod]
        public Task westwoodADL(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/other/db2Bind/DOSTORED.BND")]
        [DataRow("https://telparia.com/fileFormatSamples/other/db2Bind/DOUPDATE.BND")]
        [DataRow("https://telparia.com/fileFormatSamples/other/db2Bind/GETQUERY.BND")]
        [DataRow("https://telparia.com/fileFormatSamples/other/db2Bind/HWIDB2.bnd")]
        [DataRow("https://telparia.com/fileFormatSamples/other/db2Bind/QEDBM03.BN")]
        [DataRow("https://telparia.com/fileFormatSamples/other/db2Bind/QRW3AOC.bnd")]
        [DataRow("https://telparia.com/fileFormatSamples/other/db2Bind/QRW3AOR.bnd")]
        [DataRow("https://telparia.com/fileFormatSamples/other/db2Bind/QRW3AOU.bnd")]
        [DataRow("https://telparia.com/fileFormatSamples/other/db2Bind/QRW3COC.bnd")]
        [DataRow("https://telparia.com/fileFormatSamples/other/db2Bind/QRW3COU.bnd")]
        [DataRow("https://telparia.com/fileFormatSamples/other/db2Bind/QRW3OOC1%20%281%29.bnd")]
        [DataRow("https://telparia.com/fileFormatSamples/other/db2Bind/QRW3OOC2.bnd")]
        [DataRow("https://telparia.com/fileFormatSamples/other/db2Bind/QRW3OOU.bnd")]
        [DataRow("https://telparia.com/fileFormatSamples/other/db2Bind/UNLOCK.BND")]
        [DataRow("https://telparia.com/fileFormatSamples/other/db2Bind/WDB2213.BND")]
        [DataRow("https://telparia.com/fileFormatSamples/other/db2Bind/XECQUERY.BND")]
        [TestMethod]
        public Task db2Bind(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/other/derCertificate/msjdbc.cer")]
        [TestMethod]
        public Task derCertificate(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/other/turboPascalCompiledCPMCOM/ARC.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/other/turboPascalCompiledCPMCOM/BANNER.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/other/turboPascalCompiledCPMCOM/BRIDGE.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/other/turboPascalCompiledCPMCOM/EXTRACT.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/other/turboPascalCompiledCPMCOM/INSTALL.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/other/turboPascalCompiledCPMCOM/MEMBER.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/other/turboPascalCompiledCPMCOM/REFSORT.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/other/turboPascalCompiledCPMCOM/SIDEWAYS.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/other/turboPascalCompiledCPMCOM/UUENCODE.COM")]
        [DataRow("https://telparia.com/fileFormatSamples/other/turboPascalCompiledCPMCOM/XFER.COM")]
        [TestMethod]
        public Task turboPascalCompiledCPMCOM(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/other/zztFile/ACECAVES.ZZT")]
        [DataRow("https://telparia.com/fileFormatSamples/other/zztFile/akmag3.zzt")]
        [DataRow("https://telparia.com/fileFormatSamples/other/zztFile/DAMAGED1.ZZT")]
        [DataRow("https://telparia.com/fileFormatSamples/other/zztFile/DRZEEBO.ZZT")]
        [DataRow("https://telparia.com/fileFormatSamples/other/zztFile/NAU1.ZZT")]
        [DataRow("https://telparia.com/fileFormatSamples/other/zztFile/QUEST.ZZT")]
        [DataRow("https://telparia.com/fileFormatSamples/other/zztFile/ZPOWER1.ZZT")]
        [DataRow("https://telparia.com/fileFormatSamples/other/zztFile/ZPOWER4.ZZT")]
        [DataRow("https://telparia.com/fileFormatSamples/other/zztFile/zztv8-3.zzt")]
        [TestMethod]
        public Task zztFile(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/text/html/apple.html")]
        [DataRow("https://telparia.com/fileFormatSamples/text/html/example.htm")]
        [DataRow("https://telparia.com/fileFormatSamples/text/html/example.html")]
        [DataRow("https://telparia.com/fileFormatSamples/text/html/hires.html")]
        [DataRow("https://telparia.com/fileFormatSamples/text/html/images.html")]
        [DataRow("https://telparia.com/fileFormatSamples/text/html/index.htm.ru.cp866")]
        [DataRow("https://telparia.com/fileFormatSamples/text/html/index.html.ru.cp-1251")]
        [DataRow("https://telparia.com/fileFormatSamples/text/html/index.html.ru.iso-ru")]
        [DataRow("https://telparia.com/fileFormatSamples/text/html/index.html.ru.koi8-r")]
        [DataRow("https://telparia.com/fileFormatSamples/text/html/instruct.html")]
        [DataRow("https://telparia.com/fileFormatSamples/text/html/Invalid設定.html")]
        [DataRow("https://telparia.com/fileFormatSamples/text/html/Puzzles.html")]
        [DataRow("https://telparia.com/fileFormatSamples/text/html/test.html")]
        [DataRow("https://telparia.com/fileFormatSamples/text/html/WebNinja.html")]
        [DataRow("https://telparia.com/fileFormatSamples/text/html/wordsrch.html")]
        [TestMethod]
        public Task html(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/text/json/array.json")]
        [DataRow("https://telparia.com/fileFormatSamples/text/json/example.json")]
        [DataRow("https://telparia.com/fileFormatSamples/text/json/number.json")]
        [DataRow("https://telparia.com/fileFormatSamples/text/json/object.json")]
        [DataRow("https://telparia.com/fileFormatSamples/text/json/string.json")]
        [TestMethod]
        public Task json(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/text/lisp/BLOCKS.LSP")]
        [DataRow("https://telparia.com/fileFormatSamples/text/lisp/BOOM.LSP")]
        [DataRow("https://telparia.com/fileFormatSamples/text/lisp/fact.lsp")]
        [DataRow("https://telparia.com/fileFormatSamples/text/lisp/FASTCALC.LSP")]
        [DataRow("https://telparia.com/fileFormatSamples/text/lisp/FASTCMD.LSP")]
        [DataRow("https://telparia.com/fileFormatSamples/text/lisp/FASTLAYR.LSP")]
        [DataRow("https://telparia.com/fileFormatSamples/text/lisp/gblocks.lsp")]
        [DataRow("https://telparia.com/fileFormatSamples/text/lisp/SSETS.LSP")]
        [DataRow("https://telparia.com/fileFormatSamples/text/lisp/test.lsp")]
        [DataRow("https://telparia.com/fileFormatSamples/text/lisp/TEXINFMT.EL")]
        [DataRow("https://telparia.com/fileFormatSamples/text/lisp/tst1.gl")]
        [DataRow("https://telparia.com/fileFormatSamples/text/lisp/ZOOMBACK.LSP")]
        [TestMethod]
        public Task lisp(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/text/php/bulgarian-win1251.inc.php")]
        [DataRow("https://telparia.com/fileFormatSamples/text/php/DB.php")]
        [DataRow("https://telparia.com/fileFormatSamples/text/php/download.php")]
        [DataRow("https://telparia.com/fileFormatSamples/text/php/dutch.inc.php")]
        [DataRow("https://telparia.com/fileFormatSamples/text/php/index.php")]
        [DataRow("https://telparia.com/fileFormatSamples/text/php/insert_category.php")]
        [DataRow("https://telparia.com/fileFormatSamples/text/php/ldi_check.php")]
        [DataRow("https://telparia.com/fileFormatSamples/text/php/russian-koi8.inc.php")]
        [DataRow("https://telparia.com/fileFormatSamples/text/php/russian-win1251.inc.php")]
        [DataRow("https://telparia.com/fileFormatSamples/text/php/_data.inc.php")]
        [TestMethod]
        public Task php(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/text/sgml/1650203769_ISOlat1.sgml")]
        [DataRow("https://telparia.com/fileFormatSamples/text/sgml/1st.sgml")]
        [DataRow("https://telparia.com/fileFormatSamples/text/sgml/cirrus.sgml")]
        [DataRow("https://telparia.com/fileFormatSamples/text/sgml/DOSEMU-HOWTO.sgml")]
        [DataRow("https://telparia.com/fileFormatSamples/text/sgml/index.sgml")]
        [DataRow("https://telparia.com/fileFormatSamples/text/sgml/INSTALL.sgml")]
        [DataRow("https://telparia.com/fileFormatSamples/text/sgml/ISOlat1.sgml")]
        [DataRow("https://telparia.com/fileFormatSamples/text/sgml/lilo.sgml")]
        [DataRow("https://telparia.com/fileFormatSamples/text/sgml/README.sgml")]
        [DataRow("https://telparia.com/fileFormatSamples/text/sgml/VidModes.sgml")]
        [TestMethod]
        public Task sgml(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/0EAF.DBF")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/1651427811_example.tex")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/ADVANCED.REP")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/ANSIREZ.DOC")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/BATCHMAN_58.IFO")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/CALLM.MAC")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/example.log")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/example.txt")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/GIFLITE.NEW")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/guide.bak")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/index.dat")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/install.lgo")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/INVENTOR.REP")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/LIST.DBF")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/Loops")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/MENU_932.txt")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/MHZ288PC.MOD")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/Newsletter")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/NSWP-PC.WS")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/REQADRAT")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/SCE.INX")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/SKILLS1.G3")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/SKILLTXT.G3")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/SPLIFT.PAS")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/STORY.WS")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/TMPL_502_PLPS.bin")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/VA-LAW.STA")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/VEREMPL1")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/WHYNUDE.WS")]
        [DataRow("https://telparia.com/fileFormatSamples/text/txt/xchangel")]
        [TestMethod]
        public Task txt(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/text/unixShellScript/CONF.SH")]
        [DataRow("https://telparia.com/fileFormatSamples/text/unixShellScript/CONFIG.SH")]
        [DataRow("https://telparia.com/fileFormatSamples/text/unixShellScript/generate_tbbvars.sh")]
        [DataRow("https://telparia.com/fileFormatSamples/text/unixShellScript/PCPRINT.SH")]
        [DataRow("https://telparia.com/fileFormatSamples/text/unixShellScript/RCSFREEZ.SH")]
        [DataRow("https://telparia.com/fileFormatSamples/text/unixShellScript/REGRESS.SH")]
        [DataRow("https://telparia.com/fileFormatSamples/text/unixShellScript/RUNTEST.SH")]
        [DataRow("https://telparia.com/fileFormatSamples/text/unixShellScript/test.csh")]
        [TestMethod]
        public Task unixShellScript(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/text/utf16Text/10000")]
        [DataRow("https://telparia.com/fileFormatSamples/text/utf16Text/DKGNTLET.txt")]
        [DataRow("https://telparia.com/fileFormatSamples/text/utf16Text/e2k_uni.txt")]
        [DataRow("https://telparia.com/fileFormatSamples/text/utf16Text/InfoPlist.strings")]
        [DataRow("https://telparia.com/fileFormatSamples/text/utf16Text/javaui.rc")]
        [DataRow("https://telparia.com/fileFormatSamples/text/utf16Text/pdsencd.rc")]
        [DataRow("https://telparia.com/fileFormatSamples/text/utf16Text/README.WIN32.id")]
        [DataRow("https://telparia.com/fileFormatSamples/text/utf16Text/resource.rc")]
        [DataRow("https://telparia.com/fileFormatSamples/text/utf16Text/VB4-RZO.reg")]
        [DataRow("https://telparia.com/fileFormatSamples/text/utf16Text/WINWHOIS.rc")]
        [TestMethod]
        public Task utf16Text(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/text/xml/newton.pad")]
        [DataRow("https://telparia.com/fileFormatSamples/text/xml/test.xml")]
        [TestMethod]
        public Task xml(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-championsInterlace/ANIMALS.png")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-championsInterlace/ANIMALS.__1")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-championsInterlace/ANIMALS.__2")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-championsInterlace/GALACTIX.DAT")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-championsInterlace/MAGIC.PIC")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-championsInterlace/VVJRULES.IMG")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-championsInterlace/VVJRULES.png")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-championsInterlace/VVMTITLE.IMG")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-championsInterlace/VVMTITLE.png")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-championsInterlace/VVPODDS.IMG")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-championsInterlace/VVPOKER.IMG")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-championsInterlace/VVPOKER.png")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-championsInterlace/VVSLOTS.IMG")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-championsInterlace/VVSLOTS.png")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-championsInterlace/VVSTART.IMG")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-championsInterlace/VVSTART.png")]
        [TestMethod]
        public Task like_championsInterlace(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-doodleAtari/BADGUY.DN1")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-doodleAtari/BADGUY.png")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-doodleAtari/BIRD1.png")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-doodleAtari/BIRD1.X")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-doodleAtari/BIRD2.png")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-doodleAtari/BIRD2.X")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-doodleAtari/BLOX.png")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-doodleAtari/BLOX.X")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-doodleAtari/CREDITS.DN1")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-doodleAtari/CREDITS.png")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-doodleAtari/DN.DN1")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-doodleAtari/DN.png")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-doodleAtari/DUKE.DN1")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-doodleAtari/DUKE.png")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-doodleAtari/END.DN1")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-doodleAtari/END.png")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-doodleAtari/INTRO.png")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-doodleAtari/INTRO.X")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-doodleAtari/NEWBIRD.png")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-doodleAtari/NEWBIRD.X")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-doodleAtari/PODZ1.png")]
        [DataRow("https://telparia.com/fileFormatSamples/unknown/image/like-doodleAtari/PODZ1.X")]
        [TestMethod]
        public Task like_doodleAtari(string source)
        {
            return TestOutputGraph(source);
        }

        [DataRow("https://telparia.com/fileFormatSamples/unsupported/olbLib/BGNU.OLB")]
        [DataRow("https://telparia.com/fileFormatSamples/unsupported/olbLib/BIIO.OLB")]
        [DataRow("https://telparia.com/fileFormatSamples/unsupported/olbLib/BWIDGET.OLB")]
        [DataRow("https://telparia.com/fileFormatSamples/unsupported/olbLib/flexlib.olb")]
        [DataRow("https://telparia.com/fileFormatSamples/unsupported/olbLib/gqq.olb")]
        [DataRow("https://telparia.com/fileFormatSamples/unsupported/olbLib/PML.OLB")]
        [DataRow("https://telparia.com/fileFormatSamples/unsupported/olbLib/TERMCAP1_21.OLB")]
        [TestMethod]
        public Task olbLib(string source)
        {
            return TestOutputGraph(source);
        }

        [DataRow("https://telparia.com/fileFormatSamples/video/avi/canyon1.avi")]
        [DataRow("https://telparia.com/fileFormatSamples/video/avi/CAR_RACE.AVI")]
        [DataRow("https://telparia.com/fileFormatSamples/video/avi/DREAMER.WAV")]
        [DataRow("https://telparia.com/fileFormatSamples/video/avi/example.avi")]
        [DataRow("https://telparia.com/fileFormatSamples/video/avi/example.divx")]
        [DataRow("https://telparia.com/fileFormatSamples/video/avi/example_2s.avi")]
        [DataRow("https://telparia.com/fileFormatSamples/video/avi/example_small.avi")]
        [DataRow("https://telparia.com/fileFormatSamples/video/avi/example_small.divx")]
        [DataRow("https://telparia.com/fileFormatSamples/video/avi/Futurama1-tmp.avi")]
        [DataRow("https://telparia.com/fileFormatSamples/video/avi/judidench2.avi")]
        [DataRow("https://telparia.com/fileFormatSamples/video/avi/kudo.avi")]
        [DataRow("https://telparia.com/fileFormatSamples/video/avi/lord14.avi")]
        [DataRow("https://telparia.com/fileFormatSamples/video/avi/processor_burning.avi")]
        [DataRow("https://telparia.com/fileFormatSamples/video/avi/q1.avi")]
        [DataRow("https://telparia.com/fileFormatSamples/video/avi/sample_1920x1080.avi")]
        [DataRow("https://telparia.com/fileFormatSamples/video/avi/test.avi")]
        [DataRow("https://telparia.com/fileFormatSamples/video/avi/v21.avi")]
        [TestMethod]
        public Task avi(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/video/corelMOVE/CROAK.CMV")]
        [DataRow("https://telparia.com/fileFormatSamples/video/corelMOVE/kitchen.cmv")]
        [DataRow("https://telparia.com/fileFormatSamples/video/corelMOVE/SAMPLE.CMV")]
        [DataRow("https://telparia.com/fileFormatSamples/video/corelMOVE/sunshine.cmv")]
        [TestMethod]
        public Task corelMOVE(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/video/mkv/atlantis405-test.mkv")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mkv/example.mkv")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mkv/example_small.mkv")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mkv/switzler084d_dl.mkv")]
        [TestMethod]
        public Task mkv(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/assist.movie")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/assist.movie.flattend")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/Aztec%20Statue.omv")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/Bill%20Gates%20Does%20Windows%20QT")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/Casa%20Nova.omv")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/comp%20intro%20anim")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/example.mov")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/example_hevc.mov")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/example_small.mov")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/healthcl.mov")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/larsen.mov")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/Lincoln%20Memorial.pmv")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/NNM3022_LiquidArmor_ISDN.mov")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/nuggets.mov")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/PowerBook.omv")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/QuickTime%20Logo%20Movie")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/sample_1920x1080.mov")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/screensr.mov")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/Seen%20the%20Doctor.mov")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/T-Shirt.omv")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/test.mov")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/WISHBONE.MOV")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mov/yuv2.mov")]
        [TestMethod]
        public Task mov(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/video/mp4/2019-11-08_09-25-37-front.mp4")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mp4/example.f4v")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mp4/example.h264")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mp4/example.m4v")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mp4/example.mp4")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mp4/example_2s.mp4")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mp4/example_small.f4v")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mp4/example_small.mp4")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mp4/sample_1920x1080.mp4")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mp4/test.mp4")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mp4/turn-on-off.mp4")]
        [TestMethod]
        public Task mp4(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/video/mpeg1/delta.mpg")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mpeg1/example_small.mpg")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mpeg1/form.mpg")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mpeg1/input.mpg")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mpeg1/lamp.mpg")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mpeg1/melt.mpg")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mpeg1/ROCKET.MP")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mpeg1/ROCKET.MPG")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mpeg1/rond.mpg")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mpeg1/zapruder.mpg")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mpeg1/zelda%20first%20commercial.mpeg")]
        [TestMethod]
        public Task mpeg1(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/video/mpeg2/example.m2ts")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mpeg2/example.mpeg")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mpeg2/example.ts")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mpeg2/example.vob")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mpeg2/example_small.m2ts")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mpeg2/input.m2v")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mpeg2/test.m2v")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mpeg2/test422.m2v")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mpeg2/TITLE01-ANGLE1.VOB")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mpeg2/XTITLE.MPG")]
        [TestMethod]
        public Task mpeg2(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/video/mtvVideo/comedian.amv")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mtvVideo/example.amv")]
        [DataRow("https://telparia.com/fileFormatSamples/video/mtvVideo/MLMC_Night-2min.amv")]
        [TestMethod]
        public Task mtvVideo(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/video/oggTheoraVideo/demoVid.ogg")]
        [TestMethod]
        public Task oggTheoraVideo(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/video/riffANIM/FIREW.PAF")]
        [DataRow("https://telparia.com/fileFormatSamples/video/riffANIM/HEAVY.PAF")]
        [DataRow("https://telparia.com/fileFormatSamples/video/riffANIM/PANTHE.PAF")]
        [DataRow("https://telparia.com/fileFormatSamples/video/riffANIM/SCHNEE.PAF")]
        [DataRow("https://telparia.com/fileFormatSamples/video/riffANIM/SPOT.PAF")]
        [DataRow("https://telparia.com/fileFormatSamples/video/riffANIM/TESTB.PAF")]
        [DataRow("https://telparia.com/fileFormatSamples/video/riffANIM/TROPF.PAF")]
        [DataRow("https://telparia.com/fileFormatSamples/video/riffANIM/WELLE.PAF")]
        [DataRow("https://telparia.com/fileFormatSamples/video/riffANIM/WUERF.PAF")]
        [TestMethod]
        public Task riffANIM(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/video/threeGVideo/example.3g2")]
        [DataRow("https://telparia.com/fileFormatSamples/video/threeGVideo/example.3gp")]
        [DataRow("https://telparia.com/fileFormatSamples/video/threeGVideo/example.3gpp")]
        [TestMethod]
        public Task threeGVideo(string source)
        {
            return TestOutputGraph(source);
        }
        
        [DataRow("https://telparia.com/fileFormatSamples/video/wmv/avale.asf")]
        [DataRow("https://telparia.com/fileFormatSamples/video/wmv/example.asf")]
        [DataRow("https://telparia.com/fileFormatSamples/video/wmv/example.wmv")]
        [DataRow("https://telparia.com/fileFormatSamples/video/wmv/example.xesc")]
        [DataRow("https://telparia.com/fileFormatSamples/video/wmv/example_small.wmv")]
        [DataRow("https://telparia.com/fileFormatSamples/video/wmv/Green%20Goblin%20Last%20Stand.asx")]
        [DataRow("https://telparia.com/fileFormatSamples/video/wmv/karine_douche2.asf")]
        [DataRow("https://telparia.com/fileFormatSamples/video/wmv/m%26ms.wmv")]
        [DataRow("https://telparia.com/fileFormatSamples/video/wmv/quicktime.wmv")]
        [DataRow("https://telparia.com/fileFormatSamples/video/wmv/The_Matrix_2.asf")]
        [DataRow("https://telparia.com/fileFormatSamples/video/wmv/welcome3.wmv")]
        [DataRow("https://telparia.com/fileFormatSamples/video/wmv/wonderbrapro.wmv")]
        [TestMethod]
        public Task wmv(string source)
        {
            return TestOutputGraph(source);
        }
    }
}
