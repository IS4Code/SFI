using System;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Stores the description of an audio format.
    /// </summary>
    public interface ICustomWaveFormat
    {
        /// <summary>
        /// The sample rate of the audio in Hz.
        /// </summary>
        int? SampleRate { get; }

        /// <summary>
        /// The number of channels in the audio.
        /// </summary>
        int? ChannelCount { get; }

        /// <summary>
        /// The bit depth of the audio.
        /// </summary>
        int? BitsPerSample { get; }

        /// <summary>
        /// Average bitrate, in B/s.
        /// </summary>
        int? AverageBytesPerSecond { get; }

        /// <summary>
        /// The <see cref="Guid"/> that specifies the major type of the media sample,
        /// see <see href="https://docs.microsoft.com/en-us/windows/win32/directshow/media-types"/>.
        /// </summary>
        Guid? MajorType { get; }

        /// <summary>
        /// The <see cref="Guid"/> that specifies the subtype of the media sample,
        /// see <see href="https://docs.microsoft.com/en-us/windows/win32/directshow/media-types"/>.
        /// </summary>
        Guid? SubType { get; }
    }
}
