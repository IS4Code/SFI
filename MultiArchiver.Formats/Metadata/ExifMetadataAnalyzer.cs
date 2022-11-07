using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    /// <summary>
    /// An analyzer of EXIF metadata, as instances
    /// of <see cref="ExifDirectoryBase"/>.
    /// </summary>
    public class ExifMetadataAnalyzer : EntityAnalyzer<ExifDirectoryBase>
    {
        public async override ValueTask<AnalysisResult> Analyze(ExifDirectoryBase directory, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            node.SetClass(Classes.IFD);
            foreach(var tag in directory.Tags)
            {
                if(!exifFields.TryGetValue(tag.Type, out var id))
                {
                    continue;
                }
                string value;
                DatatypeUri datatype;
                var property = GetProperty(id);
                switch(directory.GetObject(tag.Type))
                {
                    case Rational r:
                        r = r.GetSimplifiedInstance();
                        if(r.Denominator == 1)
                        {
                            node.Set(property, r.Numerator);
                            continue;
                        }
                        value = r.GetSimplifiedInstance().ToString();
                        datatype = Datatypes.Rational;
                        break;
                    case StringValue s:
                        value = s.ToString();
                        datatype = Datatypes.String;
                        break;
                    case byte[] bs:
                        if(bs.Length <= 8)
                        {
                            var sb = new StringBuilder();
                            foreach(byte b in bs)
                            {
                                sb.Append(b.ToString("X2"));
                            }
                            value = sb.ToString();
                            datatype = Datatypes.HexBinary;
                        }else{
                            value = Convert.ToBase64String(bs);
                            datatype = Datatypes.Base64Binary;
                        }
                        break;
                    case ValueType v:
                        node.TrySet(property, v);
                        continue;
                    case object o:
                        var node2 = (await analyzers.TryAnalyze(o, context)).Node;
                        if(node2 != null)
                        {
                            node.Set(property, node2);
                        }
                        continue;
                    default:
                        continue;
                }
                if(String.IsNullOrWhiteSpace(value)) continue;
                if(datatype == Datatypes.String && DateTime.TryParseExact(value, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
                {
                    node.Set(property, dateTime);
                }else{
                    node.Set(property, value, datatype);
                }
            }
            return new AnalysisResult(node);
        }

        static readonly VocabularyUri exifVocabulary = new VocabularyUri(Vocabularies.Uri.Exif);

        static PropertyUri GetProperty(string key)
        {
            key = key.Substring(0, 1).ToLowerInvariant() + key.Substring(1);
            return new PropertyUri(exifVocabulary, key);
        }

        static readonly Dictionary<int, string> exifFields = new Dictionary<int, string>
        {
            { 0x0100, "ImageWidth" },
            { 0x0101, "ImageLength" },
            { 0x0102, "BitsPerSample" },
            { 0x0103, "Compression" },
            { 0x0106, "PhotometricInterpretation" },
            { 0x010e, "ImageDescription" },
            { 0x010f, "Make" },
            { 0x0110, "Model" },
            { 0x0111, "StripOffsets" },
            { 0x0112, "Orientation" },
            { 0x0115, "SamplesPerPixel" },
            { 0x0116, "RowsPerStrip" },
            { 0x0117, "StripByteCounts" },
            { 0x011a, "XResolution" },
            { 0x011b, "YResolution" },
            { 0x011c, "PlanarConfiguration" },
            { 0x0128, "ResolutionUnit" },
            { 0x012d, "TransferFunction" },
            { 0x0131, "Software" },
            { 0x0132, "DateTime" },
            { 0x013b, "Artist" },
            { 0x013e, "WhitePoint" },
            { 0x013f, "PrimaryChromaticities" },
            { 0x0201, "JpegInterchangeFormat" },
            { 0x0202, "JpegInterchangeFormatLength" },
            { 0x0211, "YCbCrCoefficients" },
            { 0x0212, "YCbCrSubSampling" },
            { 0x0213, "YCbCrPositioning" },
            { 0x0214, "ReferenceBlackWhite" },
            { 0x8298, "Copyright" },
            { 0x829a, "ExposureTime" },
            { 0x829d, "FNumber" },
            { 0x8822, "ExposureProgram" },
            { 0x8824, "SpectralSensitivity" },
            { 0x8827, "IsoSpeedRatings" },
            { 0x8828, "Oecf" },
            { 0x9000, "ExifVersion" },
            { 0x9003, "DateTimeOriginal" },
            { 0x9004, "DateTimeDigitized" },
            { 0x9101, "ComponentsConfiguration" },
            { 0x9102, "CompressedBitsPerPixel" },
            { 0x9201, "ShutterSpeedValue" },
            { 0x9202, "ApertureValue" },
            { 0x9203, "BrightnessValue" },
            { 0x9204, "ExposureBiasValue" },
            { 0x9205, "MaxApertureValue" },
            { 0x9206, "SubjectDistance" },
            { 0x9207, "MeteringMode" },
            { 0x9208, "LightSource" },
            { 0x9209, "Flash" },
            { 0x920a, "FocalLength" },
            { 0x9214, "SubjectArea" },
            { 0x927c, "MakerNote" },
            { 0x9286, "UserComment" },
            { 0x9290, "SubSecTime" },
            { 0x9291, "SubSecTimeOriginal" },
            { 0x9292, "SubSecTimeDigitized" },
            { 0xa000, "FlashpixVersion" },
            { 0xa001, "ColorSpace" },
            { 0xa002, "PixelXDimension" },
            { 0xa003, "PixelYDimension" },
            { 0xa004, "RelatedSoundFile" },
            { 0xa20b, "FlashEnergy" },
            { 0xa20c, "SpatialFrequencyResponse" },
            { 0xa20e, "FocalPlaneXResolution" },
            { 0xa20f, "FocalPlaneYResolution" },
            { 0xa210, "FocalPlaneResolutionUnit" },
            { 0xa214, "SubjectLocation" },
            { 0xa215, "ExposureIndex" },
            { 0xa217, "SensingMethod" },
            { 0xa300, "FileSource" },
            { 0xa301, "SceneType" },
            { 0xa302, "CfaPattern" },
            { 0xa401, "CustomRendered" },
            { 0xa402, "ExposureMode" },
            { 0xa403, "WhiteBalance" },
            { 0xa404, "DigitalZoomRatio" },
            { 0xa405, "FocalLengthIn35mmFilm" },
            { 0xa406, "SceneCaptureType" },
            { 0xa407, "GainControl" },
            { 0xa408, "Contrast" },
            { 0xa409, "Saturation" },
            { 0xa40a, "Sharpness" },
            { 0xa40b, "DeviceSettingDescription" },
            { 0xa40c, "SubjectDistanceRange" },
            { 0xa420, "ImageUniqueID" },
        };
    }
}
