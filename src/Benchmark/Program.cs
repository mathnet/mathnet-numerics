using System;
using BenchmarkDotNet.Running;
using MathNet.Numerics;

namespace Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(Control.Describe());

            var switcher = new BenchmarkSwitcher(
                new[]
                    {
                        typeof(Transforms.FFT),
                        typeof(LinearAlgebra.DenseMatrixProduct),
                        typeof(LinearAlgebra.DenseVector),
                    });

            switcher.Run(args);
        }
    }
}
