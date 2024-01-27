using IS4.SFI.Formats;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.SFI.Tools.Images;
using IS4.SFI.Vocabulary;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;

namespace IS4.SFI.MediaAnalysis.Images
{
    /// <summary>
    /// A dHash is a type of image hashing algorithm that encodes the difference between
    /// neighboring pixels in a scaled-down version of the input image.
    /// This version scales the image down to two 9×8 and 8×9 images
    /// and interlaces the resulting hash. The pixels are compared
    /// based on the result of <see cref="Color.GetBrightness"/>,
    /// and if the values are equal, the result differs
    /// between the two scaled-down variants.
    /// </summary>
    [Description("A dHash is a type of image hashing algorithm that encodes the difference between " +
        "neighboring pixels in a scaled-down version of the input image. " +
        "This version scales the image down to two 9×8 and 8×9 images " +
        "and interlaces the resulting hash. The pixels are compared " +
        "based on their brightness and if the values are equal, the " +
        "result differs between the two scaled-down variants.")]
    public class DHash : ObjectHashAlgorithm<IImage>, IObjectHashAlgorithm<Image>
    {
        /// <summary>
        /// The color to use when filling the background.
        /// </summary>
        [Description("The color to use when filling the background.")]
        public Color BackgroundColor { get; set; } = Color.FromArgb(0xBC, 0xBC, 0xBC);

        /// <inheritdoc cref="ObjectHashAlgorithm{T}.ObjectHashAlgorithm(IndividualUri, int, string, FormattingMethod)"/>
        public DHash() : base(Individuals.DHash, 16, "urn:dhash:", FormattingMethod.Hex)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<byte[]> ComputeHash(IImage image)
        {
            using var horiz = image.Resize(9, 8, false, true, BackgroundColor);
            using var vert = image.Resize(8, 9, false, true, BackgroundColor);
            using var horizBits = horiz.GetData();
            using var vertBits = vert.GetData();
            return ComputeDHash(horizBits, vertBits);
        }

        readonly ImageFormat imageFormat = new();

        /// <inheritdoc/>
        public async ValueTask<byte[]> ComputeHash(Image image)
        {
            return await ComputeHash(new DrawingImage(image, imageFormat));
        }

        static readonly (byte, byte)[] path = { (0, 3), (1, 3), (1, 2), (0, 2), (0, 1), (0, 0), (1, 0), (1, 1), (2, 1), (2, 0), (3, 0), (3, 1), (3, 2), (2, 2), (2, 3), (3, 3) };

        static (byte x, byte y) GetPoint(int i)
        {
            var pt = path[i % 16];
            switch(i / 16)
            {
                case 0: return pt;
                case 1: return ((byte)(4 + pt.Item1), pt.Item2);
                case 2: return ((byte)(7 - pt.Item1), (byte)(7 - pt.Item2));
                case 3: return ((byte)(3 - pt.Item1), (byte)(7 - pt.Item2));
                default: throw new ArgumentOutOfRangeException(nameof(i));
            }
        }

        static unsafe byte[] ComputeDHash(IImageData horiz, IImageData vert)
        {
            var hash = new BitArray(128);

            if(horiz.IsContiguous && vert.IsContiguous)
            {
                var hdata = horiz.Memory.Span;
                var vdata = vert.Memory.Span;

                for(int i = 0; i < 64; i++)
                {
                    var (x, y) = GetPoint(i);

                    var h = horiz.Scan0 + horiz.Stride * y + x * sizeof(int);
                    var v = vert.Scan0 + vert.Stride * y + x * sizeof(int);

                    var hspan = hdata.Slice(h).MemoryCast<int>();

                    var hresult = Compare(hspan[0], hspan[1], false);
                    var vresult = Compare(vdata.Slice(v).MemoryCast<int>()[0], vdata.Slice(v + vert.Stride).MemoryCast<int>()[0], true);

                    hash.Set(2 * i, hresult);
                    hash.Set(2 * i + 1, vresult);
                }
            }else{
                for(int i = 0; i < 64; i++)
                {
                    var (x, y) = GetPoint(i);

                    var hspan = horiz[y].Slice(x * sizeof(int));

                    var hresult = Compare(hspan[0], hspan[1], false);
                    var vresult = Compare(vert[x, y].MemoryCast<int>()[0], vert[x, y + 1].MemoryCast<int>()[0], true);

                    hash.Set(2 * i, hresult);
                    hash.Set(2 * i + 1, vresult);
                }
            }

            var bytes = new byte[16];
            hash.CopyTo(bytes, 0);
            return bytes;
        }

        static bool Compare(int col1, int col2, bool ifEqual)
        {
            var c1 = Color.FromArgb(col1);
            var c2 = Color.FromArgb(col2);
            switch(c2.GetBrightness().CompareTo(c1.GetBrightness()))
            {
                case -1:
                    return false;
                case 1:
                    return true;
                default:
                    return ifEqual;
            }
        }
    }
}
