using IS4.MultiArchiver.Tools;
using NAudio.Wave;
using System;
using System.Buffers;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

namespace IS4.MultiArchiver.Analysis.Audio
{
    public class PolarSpectrumGenerator
    {
        public int Width { get; }
        public int Height { get; }

        public static bool IsSupported { get; } = true;

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

        public Complex[][] CreateSpectrum(int sampleRate, int channels, ISampleProvider provider)
        {
            var gen = new AudioSpectrumSingle(sampleRate, channels, 2048, 1536, maxFreq: 4000);

            var buffer = ArrayPool<float>.Shared.Rent(16384);
            try{
                int read;
                while((read = provider.Read(buffer, 0, buffer.Length)) > 0)
                {
                    gen.Add(buffer.Slice(0, read));
                }
                gen.Finish();
            }finally{
                ArrayPool<float>.Shared.Return(buffer);
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
                            c = row.Array[row.Offset + sy] / 256;
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
