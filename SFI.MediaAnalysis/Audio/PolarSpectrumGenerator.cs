using IS4.SFI.Tools;
using NAudio.Wave;
using System;
using System.Buffers;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

namespace IS4.SFI.MediaAnalysis.Audio
{
    /// <summary>
    /// Generates polar spectrum images.
    /// </summary>
    public class PolarSpectrumGenerator
    {
        /// <summary>
        /// The width of the image.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// The height of the image.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Whether <see cref="MathNet.Numerics.IntegralTransforms.Fourier.Inverse2D(Complex[], int, int, MathNet.Numerics.IntegralTransforms.FourierOptions)"/>
        /// is supported and therefore this class could be used.
        /// </summary>
        public static bool IsSupported { get; } = true;

        /// <summary>
        /// Creates a new instance of the generator.
        /// </summary>
        /// <param name="width">The value of <see cref="Width"/>.</param>
        /// <param name="height">The value of <see cref="Height"/>.</param>
        public PolarSpectrumGenerator(int width, int height)
        {
            Width = width;
            Height = height;
        }

        static PolarSpectrumGenerator()
        {
            try
            {
                MathNet.Numerics.Control.UseNativeMKL();
            }catch(NotSupportedException)
            {
                IsSupported = false;
            }
        }

        /// <summary>
        /// Computes a polar spectrogram from an audio provider.
        /// </summary>
        /// <param name="sampleRate">The sample rate of the audio.</param>
        /// <param name="channels">The number of channels in the audio.</param>
        /// <param name="provider">The audio sample provider.</param>
        /// <returns>
        /// An array of <see cref="Complex"/> arrays for each channel in the audio.
        /// Each spectrum is formed by applying the inverse Fourier transformation on
        /// the polar coordinate conversion (via <see cref="CreatePolarMatrix(ChannelSpectrum{Complex})"/>)
        /// of each spectrogram.
        /// </returns>
        public Complex[][] CreateSpectrum(int sampleRate, int channels, ISampleProvider provider)
        {
            var gen = new AudioSpectrumSingle(sampleRate, channels, 2048, 1536, maxFreq: 4000);

            {
                using var bufferLease = ArrayPool<float>.Shared.Rent(16384, out var buffer);
                int read;
                while((read = provider.Read(buffer, 0, buffer.Length)) > 0)
                {
                    gen.Add(buffer.Slice(0, read));
                }
                gen.Finish();
            }

            var result = new Complex[channels][];
            for(int i = 0; i < channels; i++)
            {
                var matrix = CreatePolarMatrix(gen[i]);
                MathNet.Numerics.IntegralTransforms.Fourier.Inverse2D(matrix, Height, Width);
                result[i] = matrix;
            }
            return result;
        }

        /// <summary>
        /// Creates a new instance of <see cref="Bitmap"/> as a visualisation
        /// of two <see cref="Complex"/> arrays for each stereo audio channel.
        /// </summary>
        /// <param name="m1">The left channel.</param>
        /// <param name="m2">The right channel.</param>
        /// <returns>A bitmap combining the left and right channel</returns>
        public unsafe Bitmap DrawStereo(Complex[] m1, Complex[] m2)
        {
            var bmp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            var bits = bmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try{
                for(int y = 0; y < Height; y++)
                {
                    int* row = (int*)(bits.Scan0 + bits.Stride * y);
                    for(int x = 0; x < Width; x++)
                    {
                        var c1 = GetColor(m1, x, y);
                        var c2 = GetColor(m2, x, y);

                        var c = Color.FromArgb(c1, c2, Math.Min(c1, c2));
                        row[x] = c.ToArgb();
                    }
                }
            }finally{
                bmp.UnlockBits(bits);
            }
            return bmp;
        }

        /// <summary>
        /// Creates a new instance of <see cref="Bitmap"/> as a visualisation of
        /// the <see cref="Complex"/> array.
        /// </summary>
        /// <param name="m">The array to draw.</param>
        /// <returns>A bitmap created from <paramref name="m"/>.</returns>
        public unsafe Bitmap DrawMono(Complex[] m)
        {
            var bmp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            var bits = bmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try{
                for(int y = 0; y < Height; y++)
                {
                    int* row = (int*)(bits.Scan0 + bits.Stride * y);
                    for(int x = 0; x < Width; x++)
                    {
                        var c0 = GetColor(m, x, y);

                        var c = Color.FromArgb(c0, c0, c0);
                        row[x] = c.ToArgb();
                    }
                }
            }finally{
                bmp.UnlockBits(bits);
            }
            return bmp;
        }

        int GetColor(Complex[] matrix, int x, int y)
        {
            var pt = matrix[y * Width + x].Magnitude * 255;
            return (int)Math.Round(Math.Min(Math.Max(pt, 0), 255));
        }

        /// <summary>
        /// Produces an array of <see cref="Complex"/> values representing a spectrogram
        /// transformed from cartesian to polar coordinates.
        /// </summary>
        /// <param name="spectrum">The spectrum to transform.</param>
        /// <returns>
        /// An array of <see cref="Width"/>×<see cref="Height"/> <see cref="Complex"/>
        /// values taken from <paramref name="spectrum"/>, with the lower frequencies
        /// closer to the center and higher frequencies further from the center.
        /// </returns>
        public Complex[] CreatePolarMatrix(ChannelSpectrum<Complex> spectrum)
        {
            var matrix = new Complex[Width * Height];
            for(int y = 0; y < Height; y++)
            {
                for(int x = 0; x < Width; x++)
                {
                    var pt = new Complex((double)x / Width * 2 - 1, (double)y / Height * 2 - 1);

                    var mag = pt.Magnitude;
                    var phase = (pt.Phase / Math.PI + 1) / 2 % 1;

                    var sx = (int)Math.Round(phase * spectrum.Count);

                    Complex c;
                    if(sx < 0 || sx >= spectrum.Count)
                    {
                        c = Complex.Zero;
                    }else{
                        var row = spectrum[sx];
                        var sy = (int)Math.Round(mag * row.Count);
                        if(sy <= 0 || sy >= row.Count)
                        {
                            c = Complex.Zero;
                        }else{
                            var coef = 1 / mag / Math.PI;
                            c = row.At(sy) / 256;
                            if(c.Magnitude > 1) c /= c.Magnitude;
                            c = coef * c;
                        }
                    }
                    matrix[y * Width + x] = c;
                }
            }
            return matrix;
        }
    }
}
