using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Providers.Common.Mkl;
using MathNet.Numerics.Providers.LinearAlgebra;

using Complex = System.Numerics.Complex;

namespace Benchmark.LinearAlgebra
{
    [Config(typeof(Config))]
    public class DenseVector
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
            ManagedReference,
            NativeMKL,
        }

        [Params(4, 32, 128, 4096, 16384, 524288)]
        public int N { get; set; }

        [Params(ProviderId.Managed, ProviderId.ManagedReference)] //, ProviderId.NativeMKL)]
        public ProviderId Provider { get; set; }

        double[] _a;
        double[] _b;
        Complex[] _ac;
        Complex[] _bc;
        //Vector<double> _av;
        //Vector<double> _bv;

        [GlobalSetup]
        public void Setup()
        {
            switch (Provider)
            {
                case ProviderId.Managed:
                    Control.UseManaged();
                    break;
                case ProviderId.ManagedReference:
                    Control.UseManagedReference();
                    break;
                case ProviderId.NativeMKL:
                    Control.UseNativeMKL(MklConsistency.Auto, MklPrecision.Double, MklAccuracy.High);
                    break;
            }

            _a = Generate.Normal(N, 2.0, 10.0);
            _b = Generate.Normal(N, 200.0, 10.0);
            _ac = Generate.Map2(_a, Generate.Normal(N, 2.0, 10.0), (a, i) => new Complex(a, i));
            _bc = Generate.Map2(_b, Generate.Normal(N, 200.0, 10.0), (b, i) => new Complex(b, i));
            //_av = Vector<double>.Build.Dense(_a);
            //_bv = Vector<double>.Build.Dense(_b);
        }

        //[Benchmark(OperationsPerInvoke = 1)]
        public double[] ProviderAddArrays()
        {
            double[] r = new double[_a.Length];
            LinearAlgebraControl.Provider.AddArrays(_a, _b, r);
            return r;

            //Complex[] r = new Complex[_a.Length];
            //LinearAlgebraControl.Provider.AddArrays(_ac, _bc, r);
            //return r;
        }

        //[Benchmark(OperationsPerInvoke = 1)]
        public double[] ProviderScaleArrays()
        {
            double[] r = new double[_a.Length];
            LinearAlgebraControl.Provider.ScaleArray(2.0, _a, r);
            return r;
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public double[] ProviderPointMultiply()
        {
            double[] r = new double[_a.Length];
            LinearAlgebraControl.Provider.PointWiseMultiplyArrays(_a, _b, r);
            return r;

            //Complex[] r = new Complex[_a.Length];
            //LinearAlgebraControl.Provider.PointWiseMultiplyArrays(_ac, _bc, r);
            //return r;
        }

        //[Benchmark(OperationsPerInvoke = 1)]
        public double[] ProviderPointDivide()
        {
            double[] r = new double[_a.Length];
            LinearAlgebraControl.Provider.PointWiseDivideArrays(_a, _b, r);
            return r;
        }

        //[Benchmark(OperationsPerInvoke = 1)]
        public double[] ProviderPointPower()
        {
            double[] r = new double[_a.Length];
            LinearAlgebraControl.Provider.PointWisePowerArrays(_a, _b, r);
            return r;
        }

        //[Benchmark(OperationsPerInvoke = 1)]
        //public Vector<double> VectorAddOp()
        //{
        //    return _av + _bv;
        //}

        //[Benchmark(OperationsPerInvoke = 1, Baseline = true)]
        //public double[] ForLoop()
        //{
        //    double[] r = new double[_a.Length];
        //    for (int i = 0; i < r.Length; i++)
        //    {
        //        r[i] = _a[i] + _b[i];
        //    }

        //    return r;
        //}
    }
}
