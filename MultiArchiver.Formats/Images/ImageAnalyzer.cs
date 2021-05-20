using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace IS4.MultiArchiver.Analyzers
{
    public class ImageAnalyzer : BinaryFormatAnalyzer<Image>
    {
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

            node.Set(Properties.Thumbnail, UriTools.DataUriFormatter, ("image/png", data));

            return null;
        }

        static Image ResizeImage(Image image, int width, int height, PixelFormat pixelFormat, Color backgroundColor)
        {
            var resized = new Bitmap(width, height, pixelFormat);

            using(var gr = Graphics.FromImage(resized))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.Clear(backgroundColor);
                gr.DrawImage(image, new Rectangle(0, 0, width, height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            }

            float xCoef = (float)resized.Width / image.Width;
            float yCoef = (float)resized.Height / image.Height;
            resized.SetResolution(image.HorizontalResolution * xCoef, image.VerticalResolution * yCoef);

            return resized;
        }
    }
}
