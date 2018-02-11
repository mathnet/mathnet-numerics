using System;
using System.Diagnostics;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.LinearAlgebra;

namespace Integration
{
    class Program
    {
        static void Main(string[] args)
        {
            // Code touching all providers
            Control.UseNativeMKL();
            Matrix<Complex> matrix = CreateMatrix.Random<Complex>(10, 10, 100);
            Vector<Complex> vector = matrix.Svd().S;
            Fourier.Forward(vector.AsArray());
            Console.WriteLine(Control.Describe());
            Console.WriteLine($"DC={vector[0].Magnitude}; Low={vector[1].Magnitude}; Hight={vector[5].Magnitude}");

            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }
        }
    }
}
