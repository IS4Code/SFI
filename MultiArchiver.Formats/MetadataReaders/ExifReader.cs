using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace IS4.MultiArchiver.Analyzers.MetadataReaders
{
    public class ExifReader : IMetadataReader<ExifDirectoryBase>
    {
        public bool Describe(ILinkedNode node, ExifDirectoryBase directory, ILinkedNodeFactory nodeFactory)
        {
            foreach(var tag in directory.Tags)
            {
                if(!exifFields.TryGetValue(tag.Type, out var key))
                {
                    continue;
                }
                var id = key.Split('.');
                string value;
                Datatypes datatype;
                switch(directory.GetObject(tag.Type))
                {
                    case Rational r:
                        r = r.GetSimplifiedInstance();
                        if(r.Denominator == 0)
                        {
                            node.Set(ExifFormatter.Instance, id, r.Numerator);
                            continue;
                        }
                        value = r.GetSimplifiedInstance().ToString();
                        datatype = Datatypes.Rational;
                        break;
                    case StringValue s:
                        value = s.ToString();
                        datatype = Datatypes.String;
                        break;
                    case byte[] b:
                        value = Convert.ToBase64String(b);
                        datatype = Datatypes.Base64Binary;
                        break;
                    case ValueType v:
                        node.TrySet(ExifFormatter.Instance, id, v);
                        continue;
                    case object o:
                        var node2 = nodeFactory.TryCreate(o);
                        if(node2 != null)
                        {
                            node.Set(ExifFormatter.Instance, id, node2);
                        }
                        continue;
                    default:
                        continue;
                }
                if(String.IsNullOrWhiteSpace(value)) continue;
                if(datatype == Datatypes.String && DateTime.TryParseExact(value, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
                {
                    node.Set(ExifFormatter.Instance, id, dateTime);
                }else{
                    node.Set(ExifFormatter.Instance, id, value, datatype);
                }
            }
            return false;
        }

        class ExifFormatter : IPropertyUriFormatter<string[]>
        {
            public static readonly IPropertyUriFormatter<string[]> Instance = new ExifFormatter();

            private ExifFormatter()
            {

            }

            public Uri FormatUri(string[] value)
            {
                var key = value[value.Length - 1];
                key = key.Substring(0, 1).ToLowerInvariant() + key.Substring(1);
                return new Uri($"http://www.w3.org/2003/12/exif/ns#{key}", UriKind.Absolute);
            }
        }

        static readonly Dictionary<int, string> exifFields = new Dictionary<int, string>
        {
            { 0x0100, "Exif.Image.ImageWidth" },
            { 0x0101, "Exif.Image.ImageLength" },
            { 0x0102, "Exif.Image.BitsPerSample" },
            { 0x0103, "Exif.Image.Compression" },
            { 0x0106, "Exif.Image.PhotometricInterpretation" },
            { 0x010e, "Exif.Image.ImageDescription" },
            { 0x010f, "Exif.Image.Make" },
            { 0x0110, "Exif.Image.Model" },
            { 0x0111, "Exif.Image.StripOffsets" },
            { 0x0112, "Exif.Image.Orientation" },
            { 0x0115, "Exif.Image.SamplesPerPixel" },
            { 0x0116, "Exif.Image.RowsPerStrip" },
            { 0x0117, "Exif.Image.StripByteCounts" },
            { 0x011a, "Exif.Image.XResolution" },
            { 0x011b, "Exif.Image.YResolution" },
            { 0x011c, "Exif.Image.PlanarConfiguration" },
            { 0x0128, "Exif.Image.ResolutionUnit" },
            { 0x012d, "Exif.Image.TransferFunction" },
            { 0x0131, "Exif.Image.Software" },
            { 0x0132, "Exif.Image.DateTime" },
            { 0x013b, "Exif.Image.Artist" },
            { 0x013e, "Exif.Image.WhitePoint" },
            { 0x013f, "Exif.Image.PrimaryChromaticities" },
            { 0x0201, "Exif.Image.JPEGInterchangeFormat" },
            { 0x0202, "Exif.Image.JPEGInterchangeFormatLength" },
            { 0x0211, "Exif.Image.YCbCrCoefficients" },
            { 0x0212, "Exif.Image.YCbCrSubSampling" },
            { 0x0213, "Exif.Image.YCbCrPositioning" },
            { 0x0214, "Exif.Image.ReferenceBlackWhite" },
            { 0x8298, "Exif.Image.Copyright" },
            { 0x829a, "Exif.Image.ExposureTime" },
            { 0x829d, "Exif.Image.FNumber" },
            { 0x8822, "Exif.Image.ExposureProgram" },
            { 0x8824, "Exif.Image.SpectralSensitivity" },
            { 0x8827, "Exif.Image.ISOSpeedRatings" },
            { 0x8828, "Exif.Image.OECF" },
            { 0x9000, "Exif.Photo.ExifVersion" },
            { 0x9003, "Exif.Image.DateTimeOriginal" },
            { 0x9004, "Exif.Photo.DateTimeDigitized" },
            { 0x9101, "Exif.Photo.ComponentsConfiguration" },
            { 0x9102, "Exif.Image.CompressedBitsPerPixel" },
            { 0x9201, "Exif.Image.ShutterSpeedValue" },
            { 0x9202, "Exif.Image.ApertureValue" },
            { 0x9203, "Exif.Image.BrightnessValue" },
            { 0x9204, "Exif.Image.ExposureBiasValue" },
            { 0x9205, "Exif.Image.MaxApertureValue" },
            { 0x9206, "Exif.Image.SubjectDistance" },
            { 0x9207, "Exif.Image.MeteringMode" },
            { 0x9208, "Exif.Image.LightSource" },
            { 0x9209, "Exif.Image.Flash" },
            { 0x920a, "Exif.Image.FocalLength" },
            { 0x9214, "Exif.Photo.SubjectArea" },
            { 0x927c, "Exif.Photo.MakerNote" },
            { 0x9286, "Exif.Photo.UserComment" },
            { 0x9290, "Exif.Photo.SubSecTime" },
            { 0x9291, "Exif.Photo.SubSecTimeOriginal" },
            { 0x9292, "Exif.Photo.SubSecTimeDigitized" },
            { 0xa000, "Exif.Photo.FlashpixVersion" },
            { 0xa001, "Exif.Photo.ColorSpace" },
            { 0xa002, "Exif.Photo.PixelXDimension" },
            { 0xa003, "Exif.Photo.PixelYDimension" },
            { 0xa004, "Exif.Photo.RelatedSoundFile" },
            { 0xa20b, "Exif.Photo.FlashEnergy" },
            { 0xa20c, "Exif.Photo.SpatialFrequencyResponse" },
            { 0xa20e, "Exif.Photo.FocalPlaneXResolution" },
            { 0xa20f, "Exif.Photo.FocalPlaneYResolution" },
            { 0xa210, "Exif.Photo.FocalPlaneResolutionUnit" },
            { 0xa214, "Exif.Photo.SubjectLocation" },
            { 0xa215, "Exif.Photo.ExposureIndex" },
            { 0xa217, "Exif.Photo.SensingMethod" },
            { 0xa300, "Exif.Photo.FileSource" },
            { 0xa301, "Exif.Photo.SceneType" },
            { 0xa302, "Exif.Photo.CFAPattern" },
            { 0xa401, "Exif.Photo.CustomRendered" },
            { 0xa402, "Exif.Photo.ExposureMode" },
            { 0xa403, "Exif.Photo.WhiteBalance" },
            { 0xa404, "Exif.Photo.DigitalZoomRatio" },
            { 0xa405, "Exif.Photo.FocalLengthIn35mmFilm" },
            { 0xa406, "Exif.Photo.SceneCaptureType" },
            { 0xa407, "Exif.Photo.GainControl" },
            { 0xa408, "Exif.Photo.Contrast" },
            { 0xa409, "Exif.Photo.Saturation" },
            { 0xa40a, "Exif.Photo.Sharpness" },
            { 0xa40b, "Exif.Photo.DeviceSettingDescription" },
            { 0xa40c, "Exif.Photo.SubjectDistanceRange" },
            { 0xa420, "Exif.Photo.ImageUniqueID" },
        };
    }
}
