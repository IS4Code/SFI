using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace IS4.MultiArchiver.Analysis.Images
{
    public static class ImageTools
    {
        static readonly ImageAttributes drawAttributes = new ImageAttributes();

        static ImageTools()
        {
            drawAttributes.SetWrapMode(WrapMode.TileFlipXY);
        }
        
        public static Bitmap ResizeImage(Image image, int width, int height, PixelFormat pixelFormat, Color backgroundColor)
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
    }
}
