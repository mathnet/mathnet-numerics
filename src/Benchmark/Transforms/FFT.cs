using System;
using System.Collections.Generic;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace Benchmark.Transforms
{
    public class FFT
    {
        readonly Dictionary<int, Complex[]> _data = new Dictionary<int, Complex[]>();

        [Params(32, 64, 128, 1024, 8192, 65536)]
        public int N { get; set; }

        public FFT()
        {
            var realSinusoidal = Generate.Sinusoidal(65536, 32, -2.0, 2.0);
            var imagSawtooth = Generate.Sawtooth(65536, 32, -20.0, 20.0);
            var signal = Generate.Map2(realSinusoidal, imagSawtooth, (r, i) => new Complex(r, i));
            foreach (var n in new[] { 32, 64, 128, 1024, 8192, 65536 })
            {
                var s = new Complex[n];
                Array.Copy(signal, 0, s, 0, n);
                _data[n] = s;
            }

            Providers.ForceNativeMKL();
        }

        [Setup]
        public void Setup()
        {
        }

        [Benchmark(Baseline = true, OperationsPerInvoke = 2)]
        public void Managed()
        {
            Fourier.Radix2Forward(_data[N], FourierOptions.NoScaling);
            Fourier.Radix2Inverse(_data[N], FourierOptions.NoScaling);
        }

        [Benchmark(OperationsPerInvoke = 2)]
        public void NativeMKL()
        {
            Fourier.Forward(_data[N], FourierOptions.NoScaling);
            Fourier.Inverse(_data[N], FourierOptions.NoScaling);
        }
    }
}
