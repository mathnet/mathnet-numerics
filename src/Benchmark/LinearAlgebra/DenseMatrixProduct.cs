using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using MathNet.Numerics.LinearAlgebra;

namespace Benchmark.LinearAlgebra
{

    //BenchmarkRunner.Run<LinearAlgebra.DenseMatrixProduct>(config);
    //Benchmark(new DenseMatrixProduct(10,100), 100, "10 - 100x100 iterations");
    //Benchmark(new DenseMatrixProduct(25, 100), 100, "25 - 100x100 iterations");
    //Benchmark(new DenseMatrixProduct(50, 10), 100, "50 - 100x10 iterations");
    //Benchmark(new DenseMatrixProduct(100, 10), 100, "100 - 100x10 iterations");
    //Benchmark(new DenseMatrixProduct(250, 1), 10, "250 - 10x1 iterations");
    //Benchmark(new DenseMatrixProduct(500,1), 10, "500 - 10x1 iterations");
    //Benchmark(new DenseMatrixProduct(1000,1), 2, "1000 - 2x1 iterations");

    public class DenseMatrixProduct
    {
        readonly Dictionary<string, Matrix<double>> _data = new Dictionary<string, Matrix<double>>();

        [Params(8, 64, 128)]
        public int M { get; set; }

        [Params(8, 64, 128)]
        public int N { get; set; }

        [Params(Provider.Managed, Provider.NativeMKLAutoHigh, Provider.NativeMKLAvx2High)]
        public Provider Provider { get; set; }

        public DenseMatrixProduct()
        {
            foreach (var m in new[] { 8, 64, 128 })
            foreach (var n in new[] { 8, 64, 128 })
            {
                var key = Key(m, n);
                _data[key] = Matrix<double>.Build.Random(m, n);
            }
        }

        static string Key(int m, int n)
        {
            return $"{m}x{n}";
        }

        [GlobalSetup]
        public void Setup()
        {
            Providers.ForceProvider(Provider);
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public Matrix<double> Product()
        {
            if (M != N)
            {
                return _data[Key(M, N)].TransposeAndMultiply(_data[Key(M, N)]);
            }

            return _data[Key(M, N)]*_data[Key(M, N)];
        }
    }
}
