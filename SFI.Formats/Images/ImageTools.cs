using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace IS4.SFI.MediaAnalysis.Images
{
    /// <summary>
    /// Stores utility methods for manipulating images.
    /// </summary>
    public static class ImageTools
    {
        static readonly ImageAttributes drawAttributes = new();

        static ImageTools()
        {
            drawAttributes.SetWrapMode(WrapMode.TileFlipXY);
        }

        /// <summary>
        /// Resizes an image to particular dimensions, while preserving
        /// the original dimensions in metadata.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The new width.</param>
        /// <param name="height">The new height.</param>
        /// <param name="pixelFormat">The pixel format of the new image.</param>
        /// <param name="backgroundColor">The back</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height, PixelFormat pixelFormat, Color backgroundColor)
        {
            var resized = new Bitmap(width, height, pixelFormat);

            using(var gr = Graphics.FromImage(resized))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                if(backgroundColor.A == 0)
                {
                    gr.CompositingMode = CompositingMode.SourceCopy;
                }
                gr.CompositingQuality = CompositingQuality.HighQuality;
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
