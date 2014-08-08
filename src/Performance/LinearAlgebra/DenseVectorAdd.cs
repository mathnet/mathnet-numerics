using System;
using Binarysharp.Benchmark;
using MathNet.Numerics;
using MathNet.Numerics.Providers.LinearAlgebra;
using MathNet.Numerics.Providers.LinearAlgebra.Mkl;
using MathNet.Numerics.Threading;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;

namespace Performance.LinearAlgebra
{
    public class DenseVectorAdd
    {
        readonly Vector<double> a;
        readonly Vector<double> b;

        readonly ILinearAlgebraProvider managed = new ManagedLinearAlgebraProvider();
        readonly ILinearAlgebraProvider mkl = new MklLinearAlgebraProvider();

        public DenseVectorAdd(int size)
        {
            b = Vector<double>.Build.Random(size);
            a = Vector<double>.Build.Random(size);

            managed.InitializeVerify();
            Control.LinearAlgebraProvider = managed;

#if NATIVEMKL
            mkl.InitializeVerify();
            Console.WriteLine("MklProvider: {0}", mkl);
            //Control.LinearAlgebraProvider = mkl;
#endif
        }

        [BenchSharkTask("AddOperator")]
        public Vector<double> AddOperator()
        {
            return a + b;
        }

        [BenchSharkTask("Map2")]
        public Vector<double> Map2()
        {
            return a.Map2((u, v) => u + v, b);
        }

        [BenchSharkTask("Loop")]
        public Vector<double> Loop()
        {
            var aa = ((DenseVectorStorage<double>)a.Storage).Data;
            var ab = ((DenseVectorStorage<double>)b.Storage).Data;
            var ar = new Double[aa.Length];
            for (int i = 0; i < ar.Length; i++)
            {
                ar[i] = aa[i] + ab[i];
            }
            return Vector<double>.Build.Dense(ar);
        }

        [BenchSharkTask("ParallelLoop4096")]
        public Vector<double> ParallelLoop4096()
        {
            var aa = ((DenseVectorStorage<double>)a.Storage).Data;
            var ab = ((DenseVectorStorage<double>)b.Storage).Data;
            var ar = new Double[aa.Length];
            CommonParallel.For(0, ar.Length, 4096, (u, v) =>
            {
                for (int i = u; i < v; i++)
                {
                    ar[i] = aa[i] + ab[i];
                }
            });
            return Vector<double>.Build.Dense(ar);
        }

        [BenchSharkTask("ParallelLoop32768")]
        public Vector<double> ParallelLoop32768()
        {
            var aa = ((DenseVectorStorage<double>)a.Storage).Data;
            var ab = ((DenseVectorStorage<double>)b.Storage).Data;
            var ar = new Double[aa.Length];
            CommonParallel.For(0, ar.Length, 32768, (u, v) =>
            {
                for (int i = u; i < v; i++)
                {
                    ar[i] = aa[i] + ab[i];
                }
            });
            return Vector<double>.Build.Dense(ar);
        }

        [BenchSharkTask("ManagedProvider")]
        public Vector<double> ManagedProvider()
        {
            var aa = ((DenseVectorStorage<double>)a.Storage).Data;
            var ab = ((DenseVectorStorage<double>)b.Storage).Data;
            var ar = new Double[aa.Length];
            managed.AddArrays(aa, ab, ar);
            return Vector<double>.Build.Dense(ar);
        }

#if NATIVEMKL
        [BenchSharkTask("MklProvider")]
        public Vector<double> MklProvider()
        {
            var aa = ((DenseVectorStorage<double>)a.Storage).Data;
            var ab = ((DenseVectorStorage<double>)b.Storage).Data;
            var ar = new Double[aa.Length];
            mkl.AddArrays(aa, ab, ar);
            return Vector<double>.Build.Dense(ar);
        }
#endif
    }
}
