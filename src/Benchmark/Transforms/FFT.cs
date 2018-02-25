using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.Providers.Common.Mkl;

namespace Benchmark.Transforms
{
    [Config(typeof(Config))]
    public class FFT
    {
        class Config : ManualConfig
        {
            public Config()
            {
                Add(new Job("CLR x64", RunMode.Default, EnvMode.RyuJitX64)
                {
                    Env = { Runtime = Runtime.Clr, Platform = Platform.X64 }
                });
#if NET461
                Add(new Job("CLR x86", RunMode.Default, EnvMode.LegacyJitX86)
                {
                    Env = { Runtime = Runtime.Clr, Platform = Platform.X86 }
                });
#else
                Add(new Job("Core RyuJit x64", RunMode.Default, EnvMode.RyuJitX64)
                {
                    Env = { Runtime = Runtime.Core, Platform = Platform.X64 }
                });
#endif
            }
        }

        public enum ProviderId
        {
            Managed,
            NativeMKL,
        }

        [Params(32, 128, 1024)] // 32, 64, 128, 1024, 8192, 65536
        public int N { get; set; }

        [Params(ProviderId.Managed, ProviderId.NativeMKL)]
        public ProviderId Provider { get; set; }

        Complex[] _data;

        [GlobalSetup]
        public void GlobalSetup()
        {
            switch (Provider)
            {
                case ProviderId.Managed:
                    Control.UseManaged();
                    break;
                case ProviderId.NativeMKL:
                    Control.UseNativeMKL(MklConsistency.Auto, MklPrecision.Double, MklAccuracy.High);
                    break;
            }

            var realSinusoidal = Generate.Sinusoidal(N, 32, -2.0, 2.0);
            var imagSawtooth = Generate.Sawtooth(N, 32, -20.0, 20.0);
            _data = Generate.Map2(realSinusoidal, imagSawtooth, (r, i) => new Complex(r, i));
        }

        [Benchmark(OperationsPerInvoke = 2)]
        public void Transform()
        {
            Fourier.Forward(_data, FourierOptions.Default);
            Fourier.Inverse(_data, FourierOptions.Default);
        }
    }
}
