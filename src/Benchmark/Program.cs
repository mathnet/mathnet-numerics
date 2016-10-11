using System;
using Benchmark.Transforms;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Running;
using MathNet.Numerics;

namespace Benchmark
{
    public class Program
    {
        public static void Main()
        {
            //Control.NativeProviderPath = @"..\..\..\..\out\MKL\Windows\";
            Control.NativeProviderPath = @"C:\Triage\NATIVE-Win\";

            Console.WriteLine("Providers:");
            if (Control.TryUseNativeMKL()) Console.WriteLine(Control.LinearAlgebraProvider);
            if (Control.TryUseNativeCUDA()) Console.WriteLine(Control.LinearAlgebraProvider);
            if (Control.TryUseNativeOpenBLAS()) Console.WriteLine(Control.LinearAlgebraProvider);

            var config = ManualConfig.Create(DefaultConfig.Instance)
                .With(new MemoryDiagnoser(), new InliningDiagnoser());

            BenchmarkRunner.Run<FFT>(config);

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
