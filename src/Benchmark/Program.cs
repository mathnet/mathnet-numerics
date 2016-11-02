using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using MathNet.Numerics;

namespace Benchmark
{
    public class Program
    {
        public static void Main()
        {
            Providers.ForceNativeMKL();
            Console.WriteLine("Linear Algebra:  " + Control.LinearAlgebraProvider);
            Console.WriteLine("FFT:             " + Control.FourierTransformProvider);

            var subject = new LinearAlgebra.DenseMatrixProduct();
            subject.Verify();
            Console.WriteLine("Verified.");

            var config = ManualConfig.Create(DefaultConfig.Instance);
            config.Add(Job.RyuJitX64, Job.LegacyJitX86);
            //config.Add(new MemoryDiagnoser());

            BenchmarkRunner.Run<Transforms.FFT>(config);
            //BenchmarkRunner.Run<LinearAlgebra.DenseMatrixProduct>(config);

            //Benchmark(new LinearAlgebra.DenseVectorAdd(10000000,1), 10, "Large (10'000'000) - 10x1 iterations");
            //Benchmark(new LinearAlgebra.DenseVectorAdd(100,1000), 100, "Small (100) - 100x1000 iterations");

            //DenseMatrixProduct.Verify(5);
            //DenseMatrixProduct.Verify(100);
            //Benchmark(new DenseMatrixProduct(10,100), 100, "10 - 100x100 iterations");
            //Benchmark(new DenseMatrixProduct(25, 100), 100, "25 - 100x100 iterations");
            //Benchmark(new DenseMatrixProduct(50, 10), 100, "50 - 100x10 iterations");
            //Benchmark(new DenseMatrixProduct(100, 10), 100, "100 - 100x10 iterations");
            //Benchmark(new DenseMatrixProduct(250, 1), 10, "250 - 10x1 iterations");
            //Benchmark(new DenseMatrixProduct(500,1), 10, "500 - 10x1 iterations");
            //Benchmark(new DenseMatrixProduct(1000,1), 2, "1000 - 2x1 iterations");
        }
    }
}
