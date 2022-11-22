using IS4.SFI.Media;
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

        static readonly Dictionary<Guid, (string type, string ext)> GuidTypes = new Dictionary<Guid, (string, string)>
        {
            { Guid.Parse("00000055-0000-0010-8000-00AA00389B71"), ("audio/mpeg", "mp3") }
        };

        static readonly WaveFormat waveFormat = new WaveFormat();

        [ThreadStatic]
        static bool readerAllowMp3;

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
        /// Craetes a new instance of the format.
        /// </summary>
        /// <param name="allowMp3">Whether to allow the MP3 format to be recognized.</param>
        public WasapiFormat(bool allowMp3) : base(allowMp3 ? 4 : 0, null, null)
        {
            this.allowMp3 = allowMp3;
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
            readerAllowMp3 = allowMp3;
            using var reader = new CustomStreamMediaFoundationReader(stream, settings);
            return await resultFactory(reader, args);
        }

        class CustomStreamMediaFoundationReader : StreamMediaFoundationReader, ICustomWaveFormat
        {
            static readonly Guid mp3Subtype = Guid.Parse("00000055-0000-0010-8000-00AA00389B71");

            MediaType? type;

            public CustomStreamMediaFoundationReader(Stream stream, MediaFoundationReaderSettings? settings = null) : base(stream, settings)
            {

            }

            protected override IMFSourceReader CreateReader(MediaFoundationReaderSettings settings)
            {
                var ppSourceReader = base.CreateReader(settings);

                ppSourceReader.GetNativeMediaType(-3, 0, out var nativeType);

                type = new MediaType(nativeType);

                if((type.SubType == mp3Subtype) != readerAllowMp3)
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
