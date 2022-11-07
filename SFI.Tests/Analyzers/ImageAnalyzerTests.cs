using IS4.SFI.Analyzers;
using IS4.SFI.Tags;
using IS4.SFI.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using static IS4.SFI.Vocabulary.Properties;

namespace IS4.SFI.Tests.Analyzers
{
    /// <summary>
    /// Tests for <see cref="ImageAnalyzer"/>.
    /// </summary>
    [TestClass]
    public class ImageAnalyzerTests : AnalyzerTests
    {
        /// <summary>
        /// The analyzer instance to use.
        /// </summary>
        public ImageAnalyzer Analyzer { get; } = new ImageAnalyzer();

        /// <summary>
        /// Tests that dimensions and pixel format are correctly stored.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="pixelFormat">The pixel format.</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(16, 16, PixelFormat.Format32bppArgb)]
        [DataRow(8, 32, PixelFormat.Format24bppRgb)]
        [DataRow(64, 1, PixelFormat.Format16bppArgb1555)]
        public async Task FormatAndDimensions(int width, int height, PixelFormat pixelFormat)
        {
            var image = new Bitmap(width, height, pixelFormat);

            var result = await Analyzer.Analyze(image, Context, this);

            Assert.AreSame(Node, result.Node);
            Assert.AreEqual(width, Node[Width]);
            Assert.AreEqual(height, Node[Height]);
            Assert.AreEqual((decimal)image.HorizontalResolution, Node[HorizontalResolution]);
            Assert.AreEqual((decimal)image.VerticalResolution, Node[VerticalResolution]);
            Assert.AreEqual(Bitmap.GetPixelFormatSize(pixelFormat), Node[ColorDepth]);
        }

        /// <summary>
        /// Tests that a present image tag affects which metadata is stored.
        /// </summary>
        /// <param name="dimensions">Whether to store dimensions.</param>
        /// <param name="thumbnail">Whether to store the thumbnail.</param>
        /// <param name="byteHash">Whether to compute a hash.</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, false, false)]
        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        public async Task TagHandling(bool dimensions, bool thumbnail, bool byteHash)
        {
            Analyzer.DataHashAlgorithms.Add(BuiltInHash.SHA256!);

            var image = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            image.Tag = new ImageTag
            {
                StoreDimensions = dimensions,
                ByteHash = byteHash,
                MakeThumbnail = thumbnail
            };

            var result = await Analyzer.Analyze(image, Context, this);

            Assert.AreEqual(Node, result.Node);
            if(dimensions)
            {
                Assert.IsNotNull(Node[Width]);
                Assert.IsNotNull(Node[Height]);
            }else{
                Assert.IsNull(Node[Width]);
                Assert.IsNull(Node[Height]);
            }
            if(thumbnail)
            {
                Assert.IsNotNull(Node[Thumbnail]);
            }else{
                Assert.IsNull(Node[Thumbnail]);
            }
            if(byteHash)
            {
                Assert.IsNotNull(Node[Digest]);
            }else{
                Assert.IsNull(Node[Digest]);
            }
        }

        /// <summary>
        /// Tests that pixel data hash is correctly computed from an image filled with
        /// a single color.
        /// </summary>
        /// <param name="r">The red component of the color.</param>
        /// <param name="g">The green component of the color.</param>
        /// <param name="b">The blue component of the color.</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(0, 0, 0)]
        [DataRow(255, 0, 0)]
        [DataRow(0, 255, 0)]
        [DataRow(0, 0, 255)]
        [DataRow(255, 255, 255)]
        public async Task DataHash(int r, int g, int b)
        {
            var hash = BuiltInHash.SHA256!;
            Analyzer.DataHashAlgorithms.Add(hash);

            var color = Color.FromArgb(r, g, b);

            var image = new Bitmap(7, 16, PixelFormat.Format32bppArgb);
            using(var gr = Graphics.FromImage(image))
            {
                gr.Clear(color);
            }

            var simulatedData = new byte[image.Width * image.Height * sizeof(int)];
            simulatedData.AsSpan().MemoryCast<int>().Fill(color.ToArgb());
            var expectedHashResult = await hash.ComputeHash(simulatedData, null);
            var expectedUri = hash[new ArraySegment<byte>(expectedHashResult)];

            var result = await Analyzer.Analyze(image, Context, this);

            Assert.AreEqual(Node, result.Node);
            var digest = Node[Digest];
            Assert.AreEqual(expectedUri, digest);
        }
    }
}
