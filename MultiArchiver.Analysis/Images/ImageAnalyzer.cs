using IS4.MultiArchiver.Analysis.Images;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tags;
using IS4.MultiArchiver.Tools.IO;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    public class ImageAnalyzer : BinaryFormatAnalyzer<Image>
    {
        public ICollection<IObjectHashAlgorithm<Image>> LowFrequencyImageHashAlgorithms { get; } = new List<IObjectHashAlgorithm<Image>>();
        public ICollection<IDataHashAlgorithm> DataHashAlgorithms { get; } = new List<IDataHashAlgorithm>();

        public ImageAnalyzer() : base(Common.ImageClasses)
        {

        }

        public override string Analyze(ILinkedNode node, Image image, object source, ILinkedNodeFactory nodeFactory)
        {
            var tag = (image.Tag as IImageTag) ?? DefaultTag;

            bool storedAsData = node.Scheme.Equals("data", StringComparison.OrdinalIgnoreCase);

            if(source is IImageResourceTag imageTag && imageTag.IsTransparent)
            {
                (image as Bitmap)?.MakeTransparent();
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
                    ArraySegment<byte> thumbnailDta;
                    using(var thumbnail = ImageTools.ResizeImage(image, 12, 12, PixelFormat.Format32bppArgb, Color.Transparent))
                    {
                        using(var stream = new MemoryStream())
                        {
                            thumbnail.Save(stream, ImageFormat.Png);
                            if(!stream.TryGetBuffer(out thumbnailDta))
                            {
                                thumbnailDta = new ArraySegment<byte>(stream.ToArray());
                            }
                        }
                    }

                    var thumbNode = nodeFactory.Create(UriTools.DataUriFormatter, ("image/png", thumbnailDta));
                    thumbNode.Set(Properties.AtPrefLabel, "Thumbnail image");
                    node.Set(Properties.Thumbnail, thumbNode);
                }

                if(tag.LowFrequencyHash)
                {
                    foreach(var hash in LowFrequencyImageHashAlgorithms)
                    {
                        var hashBytes = hash.ComputeHash(image);
                        HashAlgorithm.AddHash(node, hash, hashBytes, nodeFactory);
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
                        Parallel.ForEach(DataHashAlgorithms, hash => {
                            using(var stream = new BitmapDataStream(data.Scan0, data.Stride, data.Height, data.Width, bpp))
                            {
                                var hashBytes = hash.ComputeHash(stream);
                                HashAlgorithm.AddHash(node, hash, hashBytes, nodeFactory);
                            }
                        });
                    }finally{
                        bmp.UnlockBits(data);
                    }
                }
            }

            return null;
        }

        static readonly ImageTag DefaultTag = new ImageTag();

    }
}
