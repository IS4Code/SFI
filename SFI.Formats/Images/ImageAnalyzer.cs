using IS4.SFI.MediaAnalysis.Images;
using IS4.SFI.Services;
using IS4.SFI.Tags;
using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of images as instances of <see cref="IImage"/>.
    /// </summary>
    [Description("An analyzer of images.")]
    public class ImageAnalyzer : MediaObjectAnalyzer<IImage>
    {
        /// <summary>
        /// A collection of <see cref="Image"/>-based hash algorithms that produce
        /// hashes from the low-detail form of the image.
        /// </summary>
        [ComponentCollection("image-hash")]
        [Description("A collection of image-based hash algorithms that produce hashes from the low-detail form of the image.")]
        public ICollection<IObjectHashAlgorithm<Image>> LowFrequencyImageHashAlgorithms { get; } = new List<IObjectHashAlgorithm<Image>>();

        /// <summary>
        /// A collection of byte-based hash algorithms producing hashes
        /// from the individual pixels of the image.
        /// </summary>
        [ComponentCollection("pixel-hash")]
        [Description("A collection of byte-based hash algorithms producing hashes from the individual pixels of the image.")]
        public ICollection<IDataHashAlgorithm> DataHashAlgorithms { get; } = new List<IDataHashAlgorithm>();

        /// <summary>
        /// Whether to produce a small thumbnail <c>data:</c> node from the image.
        /// </summary>
        [Description("Whether to produce a small thumbnail data: node from the image.")]
        public bool MakeThumbnail { get; set; } = true;

        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public ImageAnalyzer() : base(Common.ImageClasses)
        {

        }

        static readonly ImageTag DefaultTag = new();

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(IImage image, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            var tag = image.Tag ?? DefaultTag;

            bool storedAsData = node.Scheme.Equals("data", StringComparison.OrdinalIgnoreCase);

            if(context.MatchContext.GetService<IImageResourceTag>() is IImageResourceTag imageTag && imageTag.IsTransparent)
            {
                if(image is IImage<Image> { UnderlyingImage: Bitmap { Width: > 1, Height: > 1 } bmp })
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
                int paletteSize = image.Palette.Count;
                int bpp = image.BitDepth;
                if(bpp != 0) node.Set(paletteSize > 0 ? Properties.BitDepth : Properties.ColorDepth, bpp);
                if(paletteSize > 0) node.Set(Properties.PaletteSize, paletteSize);
            }

            if(!storedAsData)
            {
                if(image is IImage<Image> { UnderlyingImage: { } img })
                {
                    if(MakeThumbnail && tag.MakeThumbnail)
                    {
                        ArraySegment<byte> thumbnailData;
                        using(var thumbnail = ImageTools.ResizeImage(img, 12, 12, PixelFormat.Format32bppArgb, Color.Transparent))
                        {
                            using var stream = new MemoryStream();
                            thumbnail.Save(stream, ImageFormat.Png);
                            thumbnailData = stream.GetData();
                        }

                        var thumbNode = context.NodeFactory.Create(UriTools.DataUriFormatter, ("image/png", null, thumbnailData));
                        node.Set(Properties.Thumbnail, thumbNode);
                        thumbNode.Set(Properties.AtPrefLabel, "Thumbnail image");
                    }

                    if(tag.LowFrequencyHash)
                    {
                        foreach(var hash in LowFrequencyImageHashAlgorithms)
                        {
                            var hashBytes = await hash.ComputeHash(img);
                            await HashAlgorithm.AddHash(node, hash, hashBytes, context.NodeFactory, OnOutputFile);
                        }
                    }
                }

                if(tag.ByteHash && DataHashAlgorithms.Count > 0)
                {
                    using var data = image.GetData();
                    await Task.WhenAll(DataHashAlgorithms.Select(hash => Task.Run(async () => {
                        using var stream = data.Open();
                        var hashBytes = await hash.ComputeHash(stream);
                        await HashAlgorithm.AddHash(node, hash, hashBytes, context.NodeFactory, OnOutputFile);
                    })));
                }
            }

            return new AnalysisResult(node);
        }
    }
}
