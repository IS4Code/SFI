using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace MultiArchiver.Analysis.Audio
{
    public abstract class AudioSpectrum<TFloat> : IReadOnlyList<ChannelSpectrum>
        where TFloat : struct, IComparable, IComparable<TFloat>, IConvertible, IEquatable<TFloat>, IFormattable
    {
        public int Size { get; }

        public int Count { get; private set; }

        readonly int offset;

        readonly int frameSize;

        readonly int channels;
        readonly int audioBufferSize;
        readonly int samplesPerStep;
        readonly int samplesPerFrame;

        readonly List<Complex[]>[] channelFrames;
        readonly TFloat[] audioBuffer;
        int audioFilled;
        int numTotal;

        public AudioSpectrum(int sampleRate, int channels, int frameSize, int stepSize,
            double minFreq = 0,
            double maxFreq = double.PositiveInfinity) : this(channels)
        {
            this.frameSize = frameSize;

            audioBufferSize = (frameSize + stepSize) * channels * 8;
            audioBuffer = new TFloat[audioBufferSize];
            samplesPerStep = stepSize * channels;
            samplesPerFrame = frameSize * channels;

            offset = (int)Math.Round(minFreq / sampleRate * frameSize);
            Size = (int)Math.Min(frameSize, Math.Round(maxFreq / sampleRate * frameSize));
        }

        private AudioSpectrum(int channels)
        {
            this.channels = channels;
            channelFrames = new List<Complex[]>[channels];
            for(int i = 0; i < channels; i++)
            {
                channelFrames[i] = new List<Complex[]>();
            }
        }

        public ChannelSpectrum this[int channel] {
            get {
                return new ChannelSpectrum(channelFrames[channel], Count, offset, Size);
            }
        }

        public void Add(ArraySegment<float> audio)
        {
            while(audio.Count > 0)
            {
                int remaining = audioBufferSize - audioFilled;
                int read = Math.Min(audio.Count, remaining);
                Array.Copy(audio.Array, audio.Offset, audioBuffer, audioFilled, read);
                audio = new ArraySegment<float>(audio.Array, audio.Offset + read, audio.Count - read);
                audioFilled += read;
                if(audioBufferSize == audioFilled)
                {
                    AddFrames();
                }
            }

            Process();
        }

        public void Finish()
        {
            AddFrames();
            int overlap = (audioFilled - samplesPerFrame) % samplesPerStep;
            int added = samplesPerStep - overlap;
            Array.Clear(audioBuffer, audioFilled, added);
            audioFilled += added;
            AddFrames();

            Process();
        }

        private void AddFrames()
        {
            int numSteps = (audioFilled - samplesPerFrame) / samplesPerStep;
            var frames = new Complex[channels][];
            for(int step = 0; step < numSteps; step++)
            {
                for(int channel = 0; channel < channels; channel++)
                {
                    channelFrames[channel].Add(frames[channel] = new Complex[frameSize]);
                }
                int start = step * samplesPerStep;
                Convert(new ArraySegment<TFloat>(audioBuffer, start, samplesPerFrame), frames, channels);
            }
            numTotal += numSteps;

            int wraparound = numSteps * samplesPerStep;
            Array.Copy(audioBuffer, wraparound, audioBuffer, 0, audioFilled - wraparound);
            audioFilled -= wraparound;
        }

        protected abstract void Convert(ArraySegment<TFloat> samples, Complex[][] frames, int channels);

        private void Process()
        {
            Parallel.For(0, (numTotal - Count) * channels, index => {
                var frame = channelFrames[index % channels][Count + index / channels];

                Fourier.Forward(frame, FourierOptions.NoScaling);

                for(int i = 0; i < Size; i++)
                {
                    frame[offset + i] /= frameSize;
                }
            });
            Count = numTotal;
        }

        public IEnumerator<ChannelSpectrum> GetEnumerator()
        {
            for(int i = 0; i < Count; i++)
            {
                yield return this[Count];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public struct ChannelSpectrum : IReadOnlyList<ArraySegment<Complex>>
    {
        public int Size { get; }
        readonly List<Complex[]> data;
        readonly int offset;

        public ArraySegment<Complex> this[int index] => new ArraySegment<Complex>(data[index], offset, Size);

        public int Count { get; }

        public ChannelSpectrum(List<Complex[]> data, int count, int offset, int size)
        {
            this.data = data;
            Count = count;
            this.offset = offset;
            Size = size;
        }

        public IEnumerator<ArraySegment<Complex>> GetEnumerator()
        {
            for(int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class AudioSpectrumSingle : AudioSpectrum<float>
    {
        readonly double sampleScale;
        readonly double[] window;

        public AudioSpectrumSingle(
            int sampleRate, int channels, int frameSize, int stepSize,
            double minFreq = 0, double maxFreq = double.PositiveInfinity,
            double sampleScale = Int16.MaxValue, double[] window = null) : base(sampleRate, channels, frameSize, stepSize, minFreq, maxFreq)
        {
            this.sampleScale = sampleScale;
            this.window = window ?? Window.Hann(frameSize);
        }

        protected override void Convert(ArraySegment<float> samples, Complex[][] frames, int channels)
        {
            for(int i = 0; i < samples.Count; i++)
            {
                int channel = i % channels;
                int frameIndex = i / channels;
                frames[channel][frameIndex] = samples.Array[samples.Offset + i] * sampleScale * window[frameIndex];
            }
        }
    }
}
