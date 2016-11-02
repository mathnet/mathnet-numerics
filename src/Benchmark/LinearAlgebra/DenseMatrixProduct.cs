using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Providers.LinearAlgebra;
using MathNet.Numerics.Providers.LinearAlgebra.Mkl;
using MathNet.Numerics.Threading;

namespace Benchmark.LinearAlgebra
{
    public class DenseMatrixProduct
    {
        readonly Dictionary<string, Matrix<double>> _data = new Dictionary<string, Matrix<double>>();

        readonly ILinearAlgebraProvider _mathnetMkl;
        readonly ILinearAlgebraProvider _mathnetManaged;
        readonly ILinearAlgebraProvider _mathnetExperimental;

        [Params(8, 64, 128)]
        public int M { get; set; }

        [Params(8, 64, 128)]
        public int N { get; set; }

        static string Key(int m, int n)
        {
            return $"{m}x{n}";
        }

        public DenseMatrixProduct()
        {
            foreach (var m in new[] {8, 64, 128})
            foreach (var n in new[] {8, 64, 128})
            {
                var key = Key(m, n);
                _data[key] = Matrix<double>.Build.Random(m, n);
            }

            Control.NativeProviderPath = @"..\..\..\..\out\MKL\Windows\";
            _mathnetMkl = new MklLinearAlgebraProvider();
            _mathnetManaged = new ManagedLinearAlgebraProvider();
            _mathnetExperimental = new ManagedLinearAlgebraProvider(Variation.Experimental);

            _mathnetMkl.InitializeVerify();
            _mathnetManaged.InitializeVerify();
            _mathnetExperimental.InitializeVerify();
        }

        public void Verify()
        {
            M = 8;
            N = 8;
            var resultMkl = MathNetMKL().ToRowArrays();
            var resultManaged = MathNetManaged().ToRowArrays();
            var resultExperimental = MathNetExperimental().ToRowArrays();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (!resultMkl[i][j].AlmostEqual(resultManaged[i][j], 1e-14))
                    {
                        throw new Exception($"Managed [{i}][{j}] {resultManaged[i][j]} != {resultMkl[i][j]}");
                    }
                    if (!resultMkl[i][j].AlmostEqual(resultExperimental[i][j], 1e-14))
                    {
                        throw new Exception($"Experimental [{i}][{j}] {resultExperimental[i][j]} != {resultMkl[i][j]}");
                    }
                }
            }
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public Matrix<double> MathNetMKL()
        {
            Control.LinearAlgebraProvider = _mathnetMkl;
            if (M != N)
            {
                return _data[Key(M, N)].TransposeAndMultiply(_data[Key(M, N)]);
            }

            return _data[Key(M, N)]*_data[Key(M, N)];
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public Matrix<double> MathNetManaged()
        {
            Control.LinearAlgebraProvider = _mathnetManaged;
            if (M != N)
            {
                return _data[Key(M, N)].TransposeAndMultiply(_data[Key(M, N)]);
            }

            return _data[Key(M, N)] * _data[Key(M, N)];
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public Matrix<double> MathNetExperimental()
        {
            Control.LinearAlgebraProvider = _mathnetExperimental;
            if (M != N)
            {
                return _data[Key(M, N)].TransposeAndMultiply(_data[Key(M, N)]);
            }

            return _data[Key(M, N)] * _data[Key(M, N)];
        }
    }
}
