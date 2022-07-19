// <copyright file="SparseVector.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// https://numerics.mathdotnet.com
// https://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-$CURRENT_YEAR$ Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using BenchmarkDotNet.Attributes;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Random;
using System.Linq;

namespace Benchmark.LinearAlgebra
{
    public class SparseVector
    {
        private Matrix<double> matrix;
        private Vector<double> vector;

        [Params(100, 1000, 10000)]
        public int N { get; set; }

        [Params(0.01, 0.1, 0.5)]
        public double SparseRatio { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            const int Seed = 42;

            var rnd = new SystemRandomSource(Seed, true);

            matrix = Matrix<double>.Build.Random(N, N, new Normal(rnd));

            vector = Vector<double>.Build.SparseOfEnumerable(rnd.NextDoubleSequence().Take(N).Select(x => x < SparseRatio ? x : 0));
        }

        [Benchmark]
        public Vector<double> DenseMatrixSparseVector() => matrix.Multiply(vector);
    }
}
