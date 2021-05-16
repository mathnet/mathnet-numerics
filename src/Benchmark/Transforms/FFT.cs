using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.Providers.MKL;
using Complex = System.Numerics.Complex;

namespace Benchmark.Transforms
{
    [Config(typeof(Config))]
    public class FFT
    {
        class Config : ManualConfig
        {
            public Config()
            {
                AddJob(Job.Default.WithRuntime(ClrRuntime.Net461).WithPlatform(Platform.X64).WithJit(Jit.RyuJit));
                AddJob(Job.Default.WithRuntime(ClrRuntime.Net461).WithPlatform(Platform.X86).WithJit(Jit.LegacyJit));
#if NET5_0_OR_GREATER
                AddJob(Job.Default.WithRuntime(CoreRuntime.Core50).WithPlatform(Platform.X64).WithJit(Jit.RyuJit));
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
                    MklControl.UseNativeMKL(MklConsistency.Auto, MklPrecision.Double, MklAccuracy.High);
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
