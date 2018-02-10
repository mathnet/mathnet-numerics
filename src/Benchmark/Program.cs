using System;
using System.Linq;
using BenchmarkDotNet.Running;
using MathNet.Numerics;
using MathNet.Numerics.Providers.FourierTransform;
using MathNet.Numerics.Providers.LinearAlgebra;

namespace Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(Control.Describe());
            foreach (var provider in Enum.GetValues(typeof(Provider)).Cast<Provider>())
            {
                Providers.ForceProvider(provider);
                Console.WriteLine($"{provider}: LA={LinearAlgebraControl.Provider}, FFT={FourierTransformControl.Provider}");
            }

            var switcher = new BenchmarkSwitcher(
                new[]
                    {
                        typeof(Transforms.FFT),
                        typeof(LinearAlgebra.DenseMatrixProduct),
                    });

            switcher.Run(args);
        }
    }
}
