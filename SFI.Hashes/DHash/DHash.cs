﻿using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
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
    public class DHash : ObjectHashAlgorithm<Image>
    {
        static readonly Color gray = Color.FromArgb(0xBC, 0xBC, 0xBC);

        /// <inheritdoc cref="ObjectHashAlgorithm{T}.ObjectHashAlgorithm(IndividualUri, int, string, FormattingMethod)"/>
        public DHash() : base(Individuals.DHash, 16, "urn:dhash:", FormattingMethod.Hex)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<byte[]> ComputeHash(Image image)
        {
            using var horiz = ImageTools.ResizeImage(image, 9, 8, PixelFormat.Format32bppArgb, gray);
            using var vert = ImageTools.ResizeImage(image, 8, 9, PixelFormat.Format32bppArgb, gray);
            var horizBits = horiz.LockBits(new Rectangle(0, 0, 9, 8), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            try{
                var vertBits = vert.LockBits(new Rectangle(0, 0, 8, 9), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
                try{
                    return ComputeDHash(horizBits, vertBits);
                }finally{
                    vert.UnlockBits(vertBits);
                }
            }finally{
                horiz.UnlockBits(horizBits);
            }
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

        static unsafe byte[] ComputeDHash(BitmapData horiz, BitmapData vert)
        {
            var hash = new BitArray(128);

            byte* hdata = (byte*)horiz.Scan0;
            byte* vdata = (byte*)vert.Scan0;

            for(int i = 0; i < 64; i++)
            {
                var point = GetPoint(i);

                var h = hdata + horiz.Stride * point.y + point.x * sizeof(int);
                var v = vdata + vert.Stride * point.y + point.x * sizeof(int);

                var hresult = Compare(*(int*)h, *(int*)(h + sizeof(int)), false);
                var vresult = Compare(*(int*)v, *(int*)(v + vert.Stride), true);

                hash.Set(2 * i, hresult);
                hash.Set(2 * i + 1, vresult);
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
