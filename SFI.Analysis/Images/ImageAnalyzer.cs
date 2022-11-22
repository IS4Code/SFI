using IS4.SFI.Analysis.Images;
using IS4.SFI.Services;
using IS4.SFI.Tags;
using IS4.SFI.Tools;
using IS4.SFI.Tools.IO;
using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of images as instances of <see cref="Image"/>.
    /// </summary>
    public class ImageAnalyzer : MediaObjectAnalyzer<Image>
    {
        /// <summary>
        /// A collection of <see cref="Image"/>-based hash algorithms that produce
        /// hashes from the low-detail form of the image.
        /// </summary>
        [ComponentCollection("image-hash")]
        public ICollection<IObjectHashAlgorithm<Image>> LowFrequencyImageHashAlgorithms { get; } = new List<IObjectHashAlgorithm<Image>>();

        /// <summary>
        /// A collection of byte-based hash algorithms producing hashes
        /// from the individual pixels of the image.
        /// </summary>
        [ComponentCollection("pixel-hash")]
        public ICollection<IDataHashAlgorithm> DataHashAlgorithms { get; } = new List<IDataHashAlgorithm>();

        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public ImageAnalyzer() : base(Common.ImageClasses)
        {

        }

        static readonly ImageTag DefaultTag = new ImageTag();

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(Image image, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            var tag = (image.Tag as IImageTag) ?? DefaultTag;

            bool storedAsData = node.Scheme.Equals("data", StringComparison.OrdinalIgnoreCase);

            if(context.MatchContext.GetService<IImageResourceTag>() is IImageResourceTag imageTag && imageTag.IsTransparent)
            {
                if(image is Bitmap bmp && bmp.Width > 1 && bmp.Height > 1)
                {
                    var colors = new[]
                    {
                        (0, 0),
                        (bmp.Width - 1, 0),
                        (0, bmp.Height - 1),
                        (bmp.Width - 1, bmp.Height - 1)
                    }.GroupBy(p => bmp.GetPixel(p.Item1, p.Item2)).OrderByDescending(g => g.Count());
                    var common = colors.First();
                    if(colors.Skip(1).TakeWhile(g => g.Count() == common.Count()).Count() == 0)
                    {
                        bmp.MakeTransparent(common.Key);
                    }else{
                        bmp.MakeTransparent();
                    }
                }
            }

            if(tag.StoreDimensions)
            {
                node.Set(Properties.Width, image.Width);
                node.Set(Properties.Height, image.Height);
                node.Set(Properties.HorizontalResolution, (decimal)image.HorizontalResolution);
                node.Set(Properties.VerticalResolution, (decimal)image.VerticalResolution);
                int paletteSize;
                try{
                    paletteSize = image.Palette?.Entries?.Length ?? 0;
                }catch(ExternalException)
                {
                    paletteSize = 0;
                }
                int bpp = Image.GetPixelFormatSize(image.PixelFormat);
                if(bpp != 0) node.Set(paletteSize > 0 ? Properties.BitDepth : Properties.ColorDepth, bpp);
                if(paletteSize > 0) node.Set(Properties.PaletteSize, paletteSize);
            }

            if(!storedAsData)
            {
                if(tag.MakeThumbnail)
                {
                    ArraySegment<byte> thumbnailData;
                    using(var thumbnail = ImageTools.ResizeImage(image, 12, 12, PixelFormat.Format32bppArgb, Color.Transparent))
                    {
                        using var stream = new MemoryStream();
                        thumbnail.Save(stream, ImageFormat.Png);
                        thumbnailData = stream.GetData();
                    }

                    var thumbNode = context.NodeFactory.Create(UriTools.DataUriFormatter, ("image/png", null, thumbnailData));
                    thumbNode.Set(Properties.AtPrefLabel, "Thumbnail image");
                    node.Set(Properties.Thumbnail, thumbNode);
                }

                if(tag.LowFrequencyHash)
                {
                    foreach(var hash in LowFrequencyImageHashAlgorithms)
                    {
                        var hashBytes = await hash.ComputeHash(image);
                        await HashAlgorithm.AddHash(node, hash, hashBytes, context.NodeFactory, OnOutputFile);
                    }
                }

                if(tag.ByteHash && DataHashAlgorithms.Count > 0 && image is Bitmap bmp)
                {
                    var format = image.PixelFormat;
                    if(Image.GetPixelFormatSize(format) == 0)
                    {
                        format = PixelFormat.Format32bppArgb;
                    }

                    var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, format);
                    try{
                        int bpp = Image.GetPixelFormatSize(data.PixelFormat);
                        await Task.WhenAll(DataHashAlgorithms.Select(hash => Task.Run(async () => {
                            using var stream = new BitmapDataStream(data.Scan0, data.Stride, data.Height, data.Width, bpp);
                            var hashBytes = await hash.ComputeHash(stream);
                            await HashAlgorithm.AddHash(node, hash, hashBytes, context.NodeFactory, OnOutputFile);
                        })));
                    }finally{
                        bmp.UnlockBits(data);
                    }
                }
            }

            return new AnalysisResult(node);
        }
    }
}
