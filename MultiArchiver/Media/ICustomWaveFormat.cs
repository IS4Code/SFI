using System;

namespace IS4.MultiArchiver.Media
{
    public interface ICustomWaveFormat
    {
        int? SampleRate { get; }
        int? ChannelCount { get; }
        int? BitsPerSample { get; }
        int? AverageBytesPerSecond { get; }
        Guid? SubType { get; }
        Guid? MajorType { get; }
    }
}
