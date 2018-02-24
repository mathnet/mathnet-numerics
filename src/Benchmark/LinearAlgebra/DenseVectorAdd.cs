using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Providers.Common.Mkl;
using MathNet.Numerics.Providers.LinearAlgebra;

namespace Benchmark.LinearAlgebra
{
    [Config(typeof(Config))]
    public class DenseVectorAdd
    {
        class Config : ManualConfig
        {
            public Config()
            {
                Add(
                    new Job("CLR x64", RunMode.Default, EnvMode.RyuJitX64)
                    {
                        Env = { Runtime = Runtime.Clr, Platform = Platform.X64 }
                    },
                    new Job("CLR x86", RunMode.Default, EnvMode.LegacyJitX86)
                    {
                        Env = { Runtime = Runtime.Clr, Platform = Platform.X86 }
                    });
#if !NET461
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

        [Params(4, 32, 128, 4096, 524288)]
        public int N { get; set; }

        [Params(ProviderId.Managed, ProviderId.NativeMKL)]
        public ProviderId Provider { get; set; }

        //const int Rounds = 1024;

        double[] _a;
        double[] _b;
        Vector<double> _av;
        Vector<double> _bv;

        [GlobalSetup]
        public void Setup()
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

            _a = Generate.Normal(N, 2.0, 10.0);
            _b = Generate.Normal(N, 200.0, 10.0);
            _av = Vector<double>.Build.Dense(_a);
            _bv = Vector<double>.Build.Dense(_b);
        }

        [Benchmark(OperationsPerInvoke = 1, Baseline = true)]
        public double[] ForLoop()
        {
            double[] r = new double[_a.Length];
            for (int i = 0; i < r.Length; i++)
            {
                r[i] = _a[i] + _b[i];
            }

            return r;
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public double[] ProviderAddArrays()
        {
            double[] r = new double[_a.Length];
            LinearAlgebraControl.Provider.AddArrays(_a, _b, r);
            return r;
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public Vector<double> VectorAddOp()
        {
            return _av + _bv;
        }
    }
}
