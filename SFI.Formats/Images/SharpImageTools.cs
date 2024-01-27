using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;

namespace IS4.SFI.MediaAnalysis.Images
{
    /// <summary>
    /// Stores utility methods for manipulating images from <see cref="SixLabors.ImageSharp"/>.
    /// </summary>
    public static class SharpImageTools
    {
        static readonly Configuration createConfiguration = new()
        {
            PreferContiguousImageBuffers = true
        };

        /// <summary>
        /// Resizes an image to particular dimensions, while preserving
        /// the original dimensions in metadata.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The new width.</param>
        /// <param name="height">The new height.</param>
        /// <param name="use32bppArgb">Whether to create the image using 32-bit ARGB pixel format, or the original one.</param>
        /// <param name="backgroundColor">The back</param>
        /// <param name="preserveResolution">Whether to preserve the original resolution.</param>
        /// <returns>The resized image.</returns>
        public static Image ResizeImage(Image image, int width, int height, bool use32bppArgb, System.Drawing.Color backgroundColor, bool preserveResolution)
        {
            var resizeOptions = new ResizeOptions
            {
                Size = new(width, height),
                Mode = ResizeMode.Stretch
            };
            Action<IImageProcessingContext> operation = context => {
                if(backgroundColor.A != 0)
                {
                    context = context.BackgroundColor(new(new Bgra32(backgroundColor.R, backgroundColor.G, backgroundColor.B, backgroundColor.A)));
                }
                context = context.Resize(resizeOptions);
            };

            Image resized;
            if(use32bppArgb)
            {
                resized = image.CloneAs<Bgra32>(createConfiguration);
                resized.Mutate(operation);
            }else{
                resized = image.Clone(createConfiguration, operation);
            }

            if(preserveResolution)
            {
                float xCoef = (float)resized.Width / image.Width;
                float yCoef = (float)resized.Height / image.Height;
                resized.Metadata.HorizontalResolution = image.Metadata.HorizontalResolution * xCoef;
                resized.Metadata.VerticalResolution = image.Metadata.VerticalResolution * yCoef;
            }

            return resized;
        }
    }
}
