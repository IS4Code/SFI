using IS4.SFI.Services;
using NAudio.MediaFoundation;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents an audio format backed by the WASAPI layer.
    /// </summary>
    public class WasapiFormat : BinaryFileFormat<WaveStream>
    {
        static readonly MediaFoundationReader.MediaFoundationReaderSettings settings = new MediaFoundationReader.MediaFoundationReaderSettings
        {
            SingleReaderObject = true,
            RepositionInRead = false,
            RequestFloatOutput = true
        };

        /// <summary>
        /// Source: https://gix.github.io/media-types/
        /// </summary>
        static readonly Dictionary<Guid, (string type, string ext)> GuidTypes = new Dictionary<Guid, (string, string)>
        {
            { new("00000001-0000-0010-8000-00AA00389B71"), ("audio/x-raw-int", "pcm") },
            { new("00000002-0000-0010-8000-00AA00389B71"), ("audio/x-adpcm", "adpcm") },
            { new("0000000A-0000-0010-8000-00AA00389B71"), ("audio/x-wms", "wms") },
            { new("00000160-0000-0010-8000-00AA00389B71"), ("audio/x-wma", "wma") },
            { new("00000161-0000-0010-8000-00AA00389B71"), ("audio/x-wma", "wma") },
            { new("00000162-0000-0010-8000-00AA00389B71"), ("audio/x-wma", "wma") },
            { new("00000163-0000-0010-8000-00AA00389B71"), ("audio/x-wma", "wma") },
            { new("00000164-0000-0010-8000-00AA00389B71"), ("audio/x-wma", "wma") },
            { new("00000050-0000-0010-8000-00AA00389B71"), ("audio/mpeg", "mp1") },
            { new("E06D802B-DB46-11CF-B4D1-00805F6CBBEA"), ("audio/mpeg", "mp2") },
            { new("00000055-0000-0010-8000-00AA00389B71"), ("audio/mpeg", "mp3") },
            { new("E06D802C-DB46-11CF-B4D1-00805F6CBBEA"), ("audio/x-ac3", "ac3") },
            { new("97663A80-8FFB-4445-A6BA-792D908F497F"), ("audio/x-ac3", "ac3") },
            { new("00000092-0000-0010-8000-00AA00389B71"), ("audio/x-ac3", "ac3") },
            { new("00002000-0000-0010-8000-00AA00389B71"), ("audio/x-ac3", "ac3") },
            { new("00001610-0000-0010-8000-00AA00389B71"), ("audio/mpeg", "aac") }
        };

        static readonly WaveFormat waveFormat = new WaveFormat();

        readonly bool allowMp3;

        /// <inheritdoc/>
        public override string? GetExtension(WaveStream value)
        {
            return GuidTypes.TryGetValue((value as CustomStreamMediaFoundationReader)?.SubType ?? default, out var info)
                ? info.ext : null;
        }

        /// <inheritdoc/>
        public override string? GetMediaType(WaveStream value)
        {
            return GuidTypes.TryGetValue((value as CustomStreamMediaFoundationReader)?.SubType ?? default, out var info)
                ? info.type : null;
        }
        
        /// <summary>
        /// Creates a new instance of the format.
        /// </summary>
        /// <param name="allowMp3">Whether to allow the MP3 format to be recognized.</param>
        public WasapiFormat(bool allowMp3) : base(allowMp3 ? 4 : 0, null, null)
        {
            this.allowMp3 = allowMp3;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return base.ToString() + ";mp3=" + (allowMp3 ? "yes" : "no");
        }

        /// <inheritdoc/>
        public override bool CheckHeader(ReadOnlySpan<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
        {
            if(!allowMp3) return !waveFormat.CheckHeader(header, isBinary, encodingDetector);
            if(header.Length < HeaderLength) return false;
            if(header[0] == 0x49 && header[1] == 0x44 && header[2] == 0x33)
            {
                //ID3
                return true;
            }
            if(header[0] == 0xFF && (header[1] & 0xF0) == 0xF0)
            {
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<WaveStream, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            using var reader = new CustomStreamMediaFoundationReader(allowMp3, stream, settings);
            return await resultFactory(reader, args);
        }

        class CustomStreamMediaFoundationReader : StreamMediaFoundationReader, ICustomWaveFormat
        {
            [ThreadStatic]
            static bool readerAllowMp3;

            MediaType? type;

            public CustomStreamMediaFoundationReader(bool allowMp3, Stream stream, MediaFoundationReaderSettings? settings = null) : base(SetAllowMp3(allowMp3, stream), settings)
            {

            }

            static Stream SetAllowMp3(bool allowMp3, Stream stream)
            {
                readerAllowMp3 = allowMp3;
                return stream;
            }

            protected override IMFSourceReader CreateReader(MediaFoundationReaderSettings settings)
            {
                var ppSourceReader = base.CreateReader(settings);

                ppSourceReader.GetNativeMediaType(-3, 0, out var nativeType);

                type = new MediaType(nativeType);

                bool isMp3 = GuidTypes.TryGetValue(type.SubType, out var info) && info.ext == "mp3";
                if(isMp3 != readerAllowMp3)
                {
                    throw new InvalidOperationException("Non-MP3 format was used on an MP3 file or vice versa.");
                }

                return ppSourceReader;
            }

            public int? SampleRate {
                get {
                    try{
                        return type?.SampleRate;
                    }catch(COMException)
                    {
                        return null;
                    }
                }
            }

            public int? ChannelCount {
                get {
                    try{
                        return type?.ChannelCount;
                    }catch(COMException)
                    {
                        return null;
                    }
                }
            }

            public int? BitsPerSample {
                get {
                    try{
                        return type?.BitsPerSample;
                    }catch(COMException)
                    {
                        return null;
                    }
                }
            }

            public int? AverageBytesPerSecond {
                get {
                    try{
                        return type?.AverageBytesPerSecond;
                    }catch(COMException)
                    {
                        return null;
                    }
                }
            }

            public Guid? SubType {
                get {
                    try{
                        return type?.SubType;
                    }catch(COMException)
                    {
                        return null;
                    }
                }
            }

            public Guid? MajorType {
                get {
                    try{
                        return type?.MajorType;
                    }catch(COMException)
                    {
                        return null;
                    }
                }
            }
        }
    }
}
