using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace Benchmark.Transforms
{
    [Config(typeof(Config))]
    public class FFT
    {
        class Config : ManualConfig
        {
            public Config()
            {
                Add(
                    new Job("CLR RyuJit x64", RunMode.Default, EnvMode.RyuJitX64)
                    {
                        Env = { Runtime = Runtime.Clr, Platform = Platform.X64 }
                    },
                    new Job("CLR RyuJit x86", RunMode.Default, EnvMode.RyuJitX86)
                    {
                        Env = { Runtime = Runtime.Clr, Platform = Platform.X86 }
                    },
                    new Job("CLR LegacyJit x64", RunMode.Default, EnvMode.LegacyJitX64)
                    {
                        Env = { Runtime = Runtime.Clr, Platform = Platform.X64 }
                    },
                    new Job("CLR LegacyJit x86", RunMode.Default, EnvMode.LegacyJitX86)
                    {
                        Env = { Runtime = Runtime.Clr, Platform = Platform.X86 }
                    });
            }
        }

        [Params(32, 128)] //, 64, 1024, 8192, 65536)]
        public int N { get; set; }

        [Params(Provider.Managed, Provider.NativeMKLAutoHigh, Provider.NativeMKLAvx2High)]
        public Provider Provider { get; set; }

        Complex[] _data;

        [GlobalSetup]
        public void Setup()
        {
            Providers.ForceProvider(Provider);
            var realSinusoidal = Generate.Sinusoidal(N, 32, -2.0, 2.0);
            var imagSawtooth = Generate.Sawtooth(N, 32, -20.0, 20.0);
            _data = Generate.Map2(realSinusoidal, imagSawtooth, (r, i) => new Complex(r, i));
        }

        [Benchmark(OperationsPerInvoke = 2)]
        public void Transform()
        {
            Fourier.Forward(_data, FourierOptions.NoScaling);
            Fourier.Inverse(_data, FourierOptions.NoScaling);
        }
    }
}
