using System;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;
using MathNet.Numerics.Providers.LinearAlgebra;
using MathNet.Numerics.Providers.LinearAlgebra.Mkl;
using MathNet.Numerics.Threading;

namespace Benchmark.LinearAlgebra
{
//    public class DenseVectorAdd
//    {
//        readonly int _rounds;
//        readonly Vector<double> _a;
//        readonly Vector<double> _b;

//        readonly ILinearAlgebraProvider _managed = new ManagedLinearAlgebraProvider();
//        readonly ILinearAlgebraProvider _mkl = new MklLinearAlgebraProvider();

//        public DenseVectorAdd(int size, int rounds)
//        {
//            _rounds = rounds;

//            _b = Vector<double>.Build.Random(size);
//            _a = Vector<double>.Build.Random(size);

//            _managed.InitializeVerify();
//            Control.LinearAlgebraProvider = _managed;

//#if NATIVE
//            _mkl.InitializeVerify();
//#endif
//        }

//        [BenchSharkTask("AddOperator")]
//        public Vector<double> AddOperator()
//        {
//            var z = _b;
//            for (int i = 0; i < _rounds; i++)
//            {
//                z = _a + z;
//            }
//            return z;
//        }

//        [BenchSharkTask("Map2")]
//        public Vector<double> Map2()
//        {
//            var z = _b;
//            for (int i = 0; i < _rounds; i++)
//            {
//                z = _a.Map2((u, v) => u + v, z);
//            }
//            return z;
//        }

//        [BenchSharkTask("Loop")]
//        public Vector<double> Loop()
//        {
//            var z = _b;
//            for (int i = 0; i < _rounds; i++)
//            {
//                var aa = ((DenseVectorStorage<double>)_a.Storage).Data;
//                var az = ((DenseVectorStorage<double>)z.Storage).Data;
//                var ar = new Double[aa.Length];
//                for (int k = 0; k < ar.Length; k++)
//                {
//                    ar[k] = aa[k] + az[k];
//                }
//                z = Vector<double>.Build.Dense(ar);
//            }
//            return z;
//        }

//        [BenchSharkTask("ParallelLoop4096")]
//        public Vector<double> ParallelLoop4096()
//        {
//            var z = _b;
//            for (int i = 0; i < _rounds; i++)
//            {
//                var aa = ((DenseVectorStorage<double>)_a.Storage).Data;
//                var az = ((DenseVectorStorage<double>)z.Storage).Data;
//                var ar = new Double[aa.Length];
//                CommonParallel.For(0, ar.Length, 4096, (u, v) =>
//                {
//                    for (int k = u; k < v; k++)
//                    {
//                        ar[k] = aa[k] + az[k];
//                    }
//                });
//                z = Vector<double>.Build.Dense(ar);
//            }
//            return z;
//        }

//        [BenchSharkTask("ParallelLoop32768")]
//        public Vector<double> ParallelLoop32768()
//        {
//            var z = _b;
//            for (int i = 0; i < _rounds; i++)
//            {
//                var aa = ((DenseVectorStorage<double>)_a.Storage).Data;
//                var az = ((DenseVectorStorage<double>)z.Storage).Data;
//                var ar = new Double[aa.Length];
//                CommonParallel.For(0, ar.Length, 32768, (u, v) =>
//                {
//                    for (int k = u; k < v; k++)
//                    {
//                        ar[k] = aa[k] + az[k];
//                    }
//                });
//                z = Vector<double>.Build.Dense(ar);
//            }
//            return z;
//        }

//        [BenchSharkTask("ManagedProvider")]
//        public Vector<double> ManagedProvider()
//        {
//            var z = _b;
//            for (int i = 0; i < _rounds; i++)
//            {
//                var aa = ((DenseVectorStorage<double>)_a.Storage).Data;
//                var az = ((DenseVectorStorage<double>)z.Storage).Data;
//                var ar = new Double[aa.Length];
//                _managed.AddArrays(aa, az, ar);
//                z = Vector<double>.Build.Dense(ar);
//            }
//            return z;
//        }

//#if NATIVEMKL
//        [BenchSharkTask("MklProvider")]
//        public Vector<double> MklProvider()
//        {
//            var z = _b;
//            for (int i = 0; i < _rounds; i++)
//            {
//                var aa = ((DenseVectorStorage<double>)_a.Storage).Data;
//                var az = ((DenseVectorStorage<double>)z.Storage).Data;
//                var ar = new Double[aa.Length];
//                _mkl.AddArrays(aa, az, ar);
//                z = Vector<double>.Build.Dense(ar);
//            }
//            return z;
//        }
//#endif
//    }
}
