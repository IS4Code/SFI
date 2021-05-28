using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analysis.Audio
{
    public abstract class AudioSpectrum<TSample, TComplex> : IReadOnlyList<ChannelSpectrum<TComplex>>
        where TSample : struct, IComparable, IComparable<TSample>, IConvertible, IEquatable<TSample>
        where TComplex : struct, IEquatable<Complex>
    {
        public int Size { get; }

        public int Count { get; private set; }

        readonly int offset;

        readonly int frameSize;

        readonly int channels;
        readonly int audioBufferSize;
        readonly int samplesPerStep;
        readonly int samplesPerFrame;

        readonly List<TComplex[]>[] channelFrames;
        readonly TSample[] audioBuffer;
        int audioFilled;
        int numTotal;

        public AudioSpectrum(int sampleRate, int channels, int frameSize, int stepSize,
            double minFreq = 0,
            double maxFreq = double.PositiveInfinity) : this(channels)
        {
            this.frameSize = frameSize;

            audioBufferSize = (frameSize + stepSize) * channels * 8;
            audioBuffer = new TSample[audioBufferSize];
            samplesPerStep = stepSize * channels;
            samplesPerFrame = frameSize * channels;

            offset = (int)Math.Round(minFreq / sampleRate * frameSize);
            Size = (int)Math.Min(frameSize, Math.Round(maxFreq / sampleRate * frameSize));
        }

        private AudioSpectrum(int channels)
        {
            this.channels = channels;
            channelFrames = new List<TComplex[]>[channels];
            for(int i = 0; i < channels; i++)
            {
                channelFrames[i] = new List<TComplex[]>();
            }
        }

        public ChannelSpectrum<TComplex> this[int channel] {
            get {
                return new ChannelSpectrum<TComplex>(channelFrames[channel], Count, offset, Size);
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
            if(numSteps <= 0) return;

            var frames = new TComplex[channels][];
            for(int step = 0; step < numSteps; step++)
            {
                for(int channel = 0; channel < channels; channel++)
                {
                    channelFrames[channel].Add(frames[channel] = new TComplex[frameSize]);
                }
                int start = step * samplesPerStep;
                Convert(new ArraySegment<TSample>(audioBuffer, start, samplesPerFrame), frames, channels);
            }
            numTotal += numSteps;

            int wraparound = numSteps * samplesPerStep;
            Array.Copy(audioBuffer, wraparound, audioBuffer, 0, audioFilled - wraparound);
            audioFilled -= wraparound;
        }

        protected abstract void Convert(ArraySegment<TSample> samples, TComplex[][] frames, int channels);

        private void Process()
        {
            Parallel.For(0, (numTotal - Count) * channels, index => {
                var frame = channelFrames[index % channels][Count + index / channels];

                Transform(new ArraySegment<TComplex>(frame, offset, Size));
            });
            Count = numTotal;
        }

        protected abstract void Transform(ArraySegment<TComplex> frame);

        public IEnumerator<ChannelSpectrum<TComplex>> GetEnumerator()
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

    public struct ChannelSpectrum<TComplex> : IReadOnlyList<ArraySegment<TComplex>>
        where TComplex : struct, IEquatable<Complex>
    {
        public int Size { get; }
        readonly List<TComplex[]> data;
        readonly int offset;

        public ArraySegment<TComplex> this[int index] => new ArraySegment<TComplex>(data[index], offset, Size);

        public int Count { get; }

        internal ChannelSpectrum(List<TComplex[]> data, int count, int offset, int size)
        {
            this.data = data;
            Count = count;
            this.offset = offset;
            Size = size;
        }

        public IEnumerator<ArraySegment<TComplex>> GetEnumerator()
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

    public abstract class AudioSpectrumComplex<TSample> : AudioSpectrum<TSample, Complex>
        where TSample : struct, IComparable, IComparable<TSample>, IConvertible, IEquatable<TSample>
    {
        public AudioSpectrumComplex(int sampleRate, int channels, int frameSize, int stepSize, double minFreq = 0, double maxFreq = double.PositiveInfinity) : base(sampleRate, channels, frameSize, stepSize, minFreq, maxFreq)
        {

        }

        protected override void Transform(ArraySegment<Complex> frame)
        {
            Fourier.Forward(frame.Array, FourierOptions.NoScaling);

            int frameSize = frame.Array.Length;
            for(int i = 0; i < frame.Count; i++)
            {
                frame.Array[frame.Offset + i] /= frameSize;
            }
        }
    }

    public class AudioSpectrumSingle : AudioSpectrumComplex<float>
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
