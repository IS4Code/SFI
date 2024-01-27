using IS4.SFI.Analyzers;
using IS4.SFI.Tags;
using IS4.SFI.Tools;
using IS4.SFI.Tools.Images;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using static IS4.SFI.Vocabulary.Properties;

namespace IS4.SFI.Tests
{
    /// <summary>
    /// Tests for <see cref="ImageAnalyzer"/>.
    /// </summary>
    [TestClass]
    [SupportedOSPlatform("windows")]
    public class ImageAnalyzerTests : AnalyzerTests
    {
        /// <summary>
        /// The analyzer instance to use.
        /// </summary>
        public ImageAnalyzer Analyzer { get; } = new ImageAnalyzer();

        readonly Formats.ImageFormat imageFormat = new();

        Services.IImage CreateImage(int width, int height, PixelFormat pixelFormat, bool native, ImageTag? tag = null, Color? backgroundColor = null)
        {
            if(native)
            {
                var bitmap = new Bitmap(width, height, pixelFormat)
                {
                    Tag = tag
                };
                if(backgroundColor is { } color)
                {
                    using var gr = Graphics.FromImage(bitmap);
                    gr.Clear(color);
                }
                return new DrawingImage(bitmap, imageFormat);
            }
            SixLabors.ImageSharp.Image image;
            switch(pixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    image = new SixLabors.ImageSharp.Image<Bgra32>(width, height);
                    break;
                case PixelFormat.Format24bppRgb:
                    image = new SixLabors.ImageSharp.Image<Bgr24>(width, height);
                    break;
                case PixelFormat.Format16bppArgb1555:
                    image = new SixLabors.ImageSharp.Image<Bgra5551>(width, height);
                    break;
                default:
                    throw new NotSupportedException();
            }
            if(backgroundColor is { } bg)
            {
                var color = SixLabors.ImageSharp.Color.FromRgba(bg.R, bg.G, bg.B, bg.A);
                image.Mutate(context => context.BackgroundColor(color));
            }
            return new SharpImage(image, imageFormat)
            {
                Tag = tag
            };
        }

        /// <summary>
        /// Tests that dimensions and pixel format are correctly stored.
        /// </summary>
        /// <param name="native">Whether to create the image as native or not.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="pixelFormat">The pixel format.</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(true, 16, 16, PixelFormat.Format32bppArgb)]
        [DataRow(true, 8, 32, PixelFormat.Format24bppRgb)]
        [DataRow(true, 64, 1, PixelFormat.Format16bppArgb1555)]
        [DataRow(false, 16, 16, PixelFormat.Format32bppArgb)]
        [DataRow(false, 8, 32, PixelFormat.Format24bppRgb)]
        [DataRow(false, 64, 1, PixelFormat.Format16bppArgb1555)]
        public async Task FormatAndDimensions(bool native, int width, int height, PixelFormat pixelFormat)
        {
            var image = CreateImage(width, height, pixelFormat, native);

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
        /// <param name="native">Whether to create the image as native or not.</param>
        /// <param name="dimensions">Whether to store dimensions.</param>
        /// <param name="thumbnail">Whether to store the thumbnail.</param>
        /// <param name="byteHash">Whether to compute a hash.</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(true, false, false, false)]
        [DataRow(true, true, false, false)]
        [DataRow(true, false, true, false)]
        [DataRow(true, false, false, true)]
        [DataRow(false, false, false, false)]
        [DataRow(false, true, false, false)]
        [DataRow(false, false, true, false)]
        [DataRow(false, false, false, true)]
        public async Task TagHandling(bool native, bool dimensions, bool thumbnail, bool byteHash)
        {
            Analyzer.DataHashAlgorithms.Add(BuiltInHash.SHA256!);

            var image = CreateImage(16, 16, PixelFormat.Format32bppArgb, native, new()
            {
                StoreDimensions = dimensions,
                ByteHash = byteHash,
                MakeThumbnail = thumbnail
            });

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
        /// <param name="native">Whether to create the image as native or not.</param>
        /// <param name="r">The red component of the color.</param>
        /// <param name="g">The green component of the color.</param>
        /// <param name="b">The blue component of the color.</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(true, 0, 0, 0)]
        [DataRow(true, 255, 0, 0)]
        [DataRow(true, 0, 255, 0)]
        [DataRow(true, 0, 0, 255)]
        [DataRow(true, 255, 255, 255)]
        [DataRow(false, 0, 0, 0)]
        [DataRow(false, 255, 0, 0)]
        [DataRow(false, 0, 255, 0)]
        [DataRow(false, 0, 0, 255)]
        [DataRow(false, 255, 255, 255)]
        public async Task DataHash(bool native, int r, int g, int b)
        {
            var hash = BuiltInHash.SHA256!;
            Analyzer.DataHashAlgorithms.Add(hash);

            var color = Color.FromArgb(r, g, b);

            var image = CreateImage(7, 16, PixelFormat.Format32bppArgb, native, backgroundColor: color);

            using(var data = image.GetData())
            {
                using var stream = data.Open();
                using var buffer = new MemoryStream();
                stream.CopyTo(buffer);
                var arr = buffer.ToArray();
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
