using IS4.SFI.Tools;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace IS4.SFI.MediaAnalysis.Audio
{
    /// <summary>
    /// Generates spectrum in frequency space from provided frames of audio data.
    /// </summary>
    /// <typeparam name="TSample">The primitive number type for input audio samples.</typeparam>
    /// <typeparam name="TComplex">The complex number type to use to represent the values in the spectrum.</typeparam>
    public abstract class AudioSpectrum<TSample, TComplex> : IReadOnlyList<ChannelSpectrum<TComplex>>
        where TSample : struct, IComparable, IComparable<TSample>, IConvertible, IEquatable<TSample>
        where TComplex : struct, IEquatable<Complex>
    {
        /// <summary>
        /// The size of the frequency space, i.e. the number of <typeparamref name="TComplex"/>
        /// values in each spectrum.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// The number of audio frames provided and the length of the spectrogram.
        /// </summary>
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

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="sampleRate">The sample rate of the audio in Hz.</param>
        /// <param name="channels">The number of channels in the audio data.</param>
        /// <param name="frameSize">The size of the input audio frames in samples.</param>
        /// <param name="stepSize">The distance between each frames of the audio in samples.</param>
        /// <param name="minFreq">The minimum frequency captured in the spectrum.</param>
        /// <param name="maxFreq">The maximum frequency captured in the spectrum.</param>
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
            audioBuffer = Array.Empty<TSample>();

            this.channels = channels;
            channelFrames = new List<TComplex[]>[channels];
            for(int i = 0; i < channels; i++)
            {
                channelFrames[i] = new List<TComplex[]>();
            }
        }

        /// <summary>
        /// Retrieves the specific spectrogram for the given channel.
        /// </summary>
        /// <param name="channel">The zero-based channel index.</param>
        /// <returns>
        /// An instance of <see cref="ChannelSpectrum{TComplex}"/> for
        /// <paramref name="channel"/>.
        /// </returns>
        public ChannelSpectrum<TComplex> this[int channel] {
            get {
                return new ChannelSpectrum<TComplex>(channelFrames[channel], Count, offset, Size);
            }
        }

        /// <summary>
        /// Adds the next collection of samples to the spectrum.
        /// </summary>
        /// <param name="audio">The next samples to append to the spectrograms.</param>
        public void Add(ArraySegment<TSample> audio)
        {
            while(audio.Count > 0)
            {
                int remaining = audioBufferSize - audioFilled;
                int read = Math.Min(audio.Count, remaining);
                audio.Slice(0, read).CopyTo(audioBuffer, audioFilled);
                audio = audio.Slice(read);
                audioFilled += read;
                if(audioBufferSize == audioFilled)
                {
                    AddFrames();
                }
            }

            Process();
        }

        /// <summary>
        /// Indicates that no more audio samples will be added,
        /// pads the last frame with silence, and adds it to the spectrograms.
        /// </summary>
        public void Finish()
        {
            AddFrames();

            if(audioFilled > 0)
            {
                int fill;
                while((fill = samplesPerStep + samplesPerFrame - audioFilled) > 0)
                {
                    fill = Math.Min(fill, audioFilled);
                    Array.Copy(audioBuffer, 0, audioBuffer, audioFilled, fill);
                    audioFilled += fill;
                }
                AddFrames();
            }

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
                Convert(audioBuffer.Slice(start, samplesPerFrame), frames, channels);
            }
            numTotal += numSteps;

            int wraparound = numSteps * samplesPerStep;
            Array.Copy(audioBuffer, wraparound, audioBuffer, 0, audioFilled - wraparound);
            audioFilled -= wraparound;
        }

        /// <summary>
        /// When implemented, converts the samples for an individual frame to an array of
        /// <typeparamref name="TComplex"/> values for each individual channel.
        /// </summary>
        /// <param name="samples">The input frame; its size is a multiple of <paramref name="channels"/>.</param>
        /// <param name="frames">The array that should receive the data for each channel, indexed by the channel index.</param>
        /// <param name="channels">The number of channels.</param>
        protected abstract void Convert(ArraySegment<TSample> samples, TComplex[][] frames, int channels);

        private void Process()
        {
            Parallel.For(0, (numTotal - Count) * channels, index => {
                var frame = channelFrames[index % channels][Count + index / channels];

                Transform(frame.Slice(offset, Size));
            });
            Count = numTotal;
        }

        /// <summary>
        /// Applies the specific time space-to-frequency space transformation
        /// on the values in <paramref name="frame"/>.
        /// </summary>
        /// <param name="frame">The array segment that should be transformed.</param>
        protected abstract void Transform(ArraySegment<TComplex> frame);

        /// <inheritdoc/>
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

    /// <summary>
    /// Stores spectrogram data for a single channel.
    /// </summary>
    /// <typeparam name="TComplex">The complex number type to use to represent the values in the spectrum.</typeparam>
    public struct ChannelSpectrum<TComplex> : IReadOnlyList<ArraySegment<TComplex>>
        where TComplex : struct, IEquatable<Complex>
    {
        /// <summary>
        /// The size of the frequency space, i.e. the number of <typeparamref name="TComplex"/>
        /// values in each spectrum.
        /// </summary>
        public int Size { get; }

        readonly List<TComplex[]> data;
        readonly int offset;

        /// <summary>
        /// Retrieves the individual values of the spectrum at a particular index
        /// between 0 and <see cref="Count"/>.
        /// </summary>
        /// <param name="index">The </param>
        /// <returns>
        /// A segment of <typeparamref name="TComplex"/> values of size <see cref="Size"/>.
        /// </returns>
        public ArraySegment<TComplex> this[int index] => data[index].Slice(offset, Size);

        /// <summary>
        /// The number of audio frames provided and the length of the spectrogram.
        /// </summary>
        public int Count { get; }

        internal ChannelSpectrum(List<TComplex[]> data, int count, int offset, int size)
        {
            this.data = data;
            Count = count;
            this.offset = offset;
            Size = size;
        }

        /// <inheritdoc/>
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

    /// <summary>
    /// Provides an implementation of <see cref="AudioSpectrum{TSample, TComplex}"/>
    /// that uses the <see cref="Complex"/> type for complex numbers and the
    /// <see cref="Fourier.Forward(Complex[], FourierOptions)"/> function
    /// as the frequency space transformation.
    /// </summary>
    /// <typeparam name="TSample">The primitive number type for input audio samples.</typeparam>
    public abstract class AudioSpectrumComplex<TSample> : AudioSpectrum<TSample, Complex>
        where TSample : struct, IComparable, IComparable<TSample>, IConvertible, IEquatable<TSample>
    {
        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="sampleRate">The sample rate of the audio in Hz.</param>
        /// <param name="channels">The number of channels in the audio data.</param>
        /// <param name="frameSize">The size of the input audio frames in samples.</param>
        /// <param name="stepSize">The distance between each frames of the audio in samples.</param>
        /// <param name="minFreq">The minimum frequency captured in the spectrum.</param>
        /// <param name="maxFreq">The maximum frequency captured in the spectrum.</param>
        public AudioSpectrumComplex(int sampleRate, int channels, int frameSize, int stepSize, double minFreq = 0, double maxFreq = double.PositiveInfinity) : base(sampleRate, channels, frameSize, stepSize, minFreq, maxFreq)
        {

        }

        /// <inheritdoc/>
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

    /// <summary>
    /// Provides an implementation of <see cref="AudioSpectrumComplex{TSample}"/>
    /// using the <see cref="Single"/> number type for samples, and supporting
    /// a window function to apply to each frame.
    /// </summary>
    public class AudioSpectrumSingle : AudioSpectrumComplex<float>
    {
        readonly double sampleScale;
        readonly double[] window;

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="sampleRate">The sample rate of the audio in Hz.</param>
        /// <param name="channels">The number of channels in the audio data.</param>
        /// <param name="frameSize">The size of the input audio frames in samples.</param>
        /// <param name="stepSize">The distance between each frames of the audio in samples.</param>
        /// <param name="minFreq">The minimum frequency captured in the spectrum.</param>
        /// <param name="maxFreq">The maximum frequency captured in the spectrum.</param>
        /// <param name="sampleScale">The number to multiply each sample by between transformation.</param>
        /// <param name="window">
        /// An array of coefficients to multiply each frame by, of length <paramref name="frameSize"/>.
        /// By default, it is the result of <see cref="Window.Hann(int)"/>.
        /// </param>
        public AudioSpectrumSingle(
            int sampleRate, int channels, int frameSize, int stepSize,
            double minFreq = 0, double maxFreq = double.PositiveInfinity,
            double sampleScale = Int16.MaxValue, double[]? window = null) : base(sampleRate, channels, frameSize, stepSize, minFreq, maxFreq)
        {
            this.sampleScale = sampleScale;
            this.window = window ?? Window.Hann(frameSize);
        }

        /// <inheritdoc/>
        protected override void Convert(ArraySegment<float> samples, Complex[][] frames, int channels)
        {
            for(int i = 0; i < samples.Count; i++)
            {
                int channel = i % channels;
                int frameIndex = i / channels;
                frames[channel][frameIndex] = samples.At(i) * sampleScale * window[frameIndex];
            }
        }
    }
}
