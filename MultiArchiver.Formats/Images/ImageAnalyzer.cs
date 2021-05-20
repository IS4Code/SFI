using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace IS4.MultiArchiver.Analyzers
{
    public class ImageAnalyzer : BinaryFormatAnalyzer<Image>
    {
        static readonly Color gray = Color.FromArgb(0xBC, 0xBC, 0xBC);
        static readonly ImageAttributes drawAttributes = new ImageAttributes();

        static ImageAnalyzer()
        {
            drawAttributes.SetWrapMode(WrapMode.TileFlipXY);
        }

        public ImageAnalyzer() : base(Classes.ImageObject)
        {

        }

        public override string Analyze(ILinkedNode node, Image image, ILinkedNodeFactory nodeFactory)
        {
            ArraySegment<byte> data;
            using(var thumbnail = ResizeImage(image, 8, 8, PixelFormat.Format32bppArgb, Color.Transparent))
            {
                using(var stream = new MemoryStream())
                {
                    thumbnail.Save(stream, ImageFormat.Png);
                    if(!stream.TryGetBuffer(out data))
                    {
                        data = new ArraySegment<byte>(stream.ToArray());
                    }
                }
            }

            var thumbNode = nodeFactory.Create(UriTools.DataUriFormatter, ("image/png", data));
            thumbNode.Set(Properties.AtPrefLabel, "Thumbnail image");
            node.Set(Properties.Thumbnail, thumbNode);

            byte[] hash;
            using(var horiz = ResizeImage(image, 9, 8, PixelFormat.Format32bppArgb, gray))
            {
                using(var vert = ResizeImage(image, 8, 9, PixelFormat.Format32bppArgb, gray))
                {
                    var horizBits = horiz.LockBits(new Rectangle(0, 0, 9, 8), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
                    try{
                        var vertBits = vert.LockBits(new Rectangle(0, 0, 8, 9), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
                        try{
                            hash = ComputeDHash(horizBits, vertBits);
                        }finally{
                            vert.UnlockBits(vertBits);
                        }
                    }finally{
                        horiz.UnlockBits(horizBits);
                    }
                }
            }
            HashAlgorithm.AddHash(node, DHash.Instance, hash, nodeFactory);

            return null;
        }

        static Bitmap ResizeImage(Image image, int width, int height, PixelFormat pixelFormat, Color backgroundColor)
        {
            var resized = new Bitmap(width, height, pixelFormat);

            using(var gr = Graphics.FromImage(resized))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.Clear(backgroundColor);
                gr.DrawImage(image, new Rectangle(0, 0, width, height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, drawAttributes);
            }

            float xCoef = (float)resized.Width / image.Width;
            float yCoef = (float)resized.Height / image.Height;
            resized.SetResolution(image.HorizontalResolution * xCoef, image.VerticalResolution * yCoef);

            return resized;
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

        class DHash : HashAlgorithm
        {
            public static readonly DHash Instance = new DHash();

            public DHash() : base(Individuals.DHash, 16, "urn:dhash:", FormattingMethod.Hex)
            {

            }
        }
    }
}
