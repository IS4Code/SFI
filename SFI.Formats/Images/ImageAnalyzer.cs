using IS4.SFI.Services;
using IS4.SFI.Tags;
using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
        public ICollection<IObjectHashAlgorithm<IImage>> LowFrequencyImageHashAlgorithms { get; } = new List<IObjectHashAlgorithm<IImage>>();

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
        public bool MakeThumbnail { get; set; } = Environment.ProcessorCount > 1;

        /// <summary>
        /// The size of created thumbnails.
        /// </summary>
        [Description("The size of created thumbnails.")]
        public Size ThumbnailSize { get; set; } = new(12, 12);

        /// <summary>
        /// The media type to use when creating the thumbnail.
        /// </summary>
        [Description("The media type to use when creating the thumbnail.")]
        public string ThumbnailFormat { get; set; } = "image/png";

        /// <summary>
        /// The rectangles in pixels where the image is to be cropped.
        /// </summary>
        [Description("The rectangles in pixels where the image is to be cropped.")]
        public ICollection<Rectangle> MakeCroppedInPixels { get; } = new HashSet<Rectangle>();

        /// <summary>
        /// The rectangles in ratios where the image is to be cropped.
        /// </summary>
        [Description("The rectangles in ratios where the image is to be cropped.")]
        public ICollection<RectangleF> MakeCroppedInRatios { get; } = new HashSet<RectangleF>();

        private bool MakeCropped => MakeCroppedInPixels.Count > 0 || MakeCroppedInRatios.Count > 0;

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

            if(tag.StoreEncodingProperties)
            {
                node.Set(Properties.Width, image.Width);
                node.Set(Properties.Height, image.Height);
                node.Set(Properties.HorizontalResolution, (decimal)image.HorizontalResolution);
                node.Set(Properties.VerticalResolution, (decimal)image.VerticalResolution);
                var paletteSize = image.PaletteSize;
                int bpp = image.BitDepth;
                if(bpp != 0) node.Set(paletteSize == 0 ? Properties.ColorDepth : Properties.BitDepth, bpp);
                if(paletteSize is int size and > 0) node.Set(Properties.PaletteSize, size);
                if(image.IsLossless is bool lossless)
                {
                    node.Set(Properties.CompressionType, lossless ? Individuals.LosslessCompressionType : Individuals.LossyCompressionType);
                }
            }

            if(!storedAsData)
            {
                if(MakeThumbnail && tag.MakeThumbnail)
                {
                    ArraySegment<byte> thumbnailData;
                    var thumbSize = ThumbnailSize;
                    using(var thumbnail = image.Resize(thumbSize.Width, thumbSize.Height, true, true, Color.Transparent))
                    {
                        using var stream = new MemoryStream();
                        thumbnail.Save(stream, ThumbnailFormat);
                        thumbnailData = stream.GetData();
                    }

                    var thumbNode = context.NodeFactory.Create(UriTools.DataUriFormatter, ((string?)ThumbnailFormat, default(string), thumbnailData));
                    node.Set(Properties.Thumbnail, thumbNode);
                    thumbNode.Set(Properties.AtPrefLabel, "Thumbnail image");
                }

                if(tag.LowFrequencyHash)
                {
                    foreach(var hash in LowFrequencyImageHashAlgorithms)
                    {
                        var hashBytes = await hash.ComputeHash(image);
                        await HashAlgorithm.AddHash(node, hash, hashBytes, context.NodeFactory, OnOutputFile);
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

                if(tag.MakeThumbnail && MakeCropped)
                {
                    foreach(var (cropped, fragment) in GetCroppedParts(image))
                    {
                        cropped.Tag = new ImageTag
                        {
                            StoreEncodingProperties = false,
                            HighFrequencyHash = tag.HighFrequencyHash,
                            LowFrequencyHash = tag.LowFrequencyHash,
                            ByteHash = tag.ByteHash,
                            MakeThumbnail = false
                        };

                        var childContext = context
                            .WithParentLink(node, Properties.HasPart)
                            .WithNode(node[fragment]);
                        await analyzers.Analyze(cropped, childContext);
                    }
                }
            }

            return new AnalysisResult(node);
        }

        IEnumerable<(IImage cropped, string fragment)> GetCroppedParts(IImage image)
        {
            var wholePixels = new Rectangle(0, 0, image.Width, image.Height);
            foreach(var crop in MakeCroppedInPixels)
            {
                var rect = crop;
                if(rect.X < 0)
                {
                    rect.X += image.Width;
                }
                if(rect.Y < 0)
                {
                    rect.Y += image.Height;
                }
                rect.Intersect(wholePixels);
                if(rect == wholePixels || rect.Width <= 0 || rect.Height <= 0)
                {
                    continue;
                }
                yield return (image.Crop(rect, true), $"#xywh={rect.X},{rect.Y},{rect.Width},{rect.Height}");
            }
            foreach(var crop in MakeCroppedInRatios)
            {
                var rectRatio = crop;
                if(rectRatio.X < 0)
                {
                    rectRatio.X += 1;
                }
                if(rectRatio.Y < 0)
                {
                    rectRatio.Y += 1;
                }
                var rect = new Rectangle(
                    (int)Math.Round(rectRatio.X * image.Width),
                    (int)Math.Round(rectRatio.Y * image.Height),
                    (int)Math.Round(rectRatio.Width * image.Width),
                    (int)Math.Round(rectRatio.Height * image.Height)
                );
                rect.Intersect(wholePixels);
                if(rect == wholePixels || rect.Width <= 0 || rect.Height <= 0)
                {
                    continue;
                }
                yield return (image.Crop(rect, true), $"#xywh={rect.X},{rect.Y},{rect.Width},{rect.Height}");
            }
        }
    }
}
