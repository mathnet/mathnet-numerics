// <copyright file="ManagedExperimentalLinearAlgebraProvider.Complex.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2014 Math.NET
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

using MathNet.Numerics.LinearAlgebra.Storage;
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.Providers.ExperimentalLinearAlgebra.Managed
{

#if !NOSYSNUMERICS
    using Complex = System.Numerics.Complex;
#endif

    public partial class ManagedExperimentalLinearAlgebraProvider
    {
        public override void AddVectors(VectorStorage<Complex> x, VectorStorage<Complex> y, VectorStorage<Complex> result)
        {
            var xd = x as DenseVectorStorage<Complex>;
            var yd = y as DenseVectorStorage<Complex>;
            var rd = result as DenseVectorStorage<Complex>;
            if (xd != null && yd != null && rd != null)
            {
                CommonParallel.For(0, y.Length, 4096, (a, b) =>
                {
                    for (int i = a; i < b; i++)
                    {
                        result[i] = x[i] + y[i];
                    }
                });
                return;
            }

            var xs = x as SparseVectorStorage<Complex>;
            var ys = y as SparseVectorStorage<Complex>;
            var rs = result as SparseVectorStorage<Complex>;
            if (xs != null && ys != null && rs != null)
            {

                // NOTE: this algorithm could be improved, just here for demonstration with same behavior as before

                if (ReferenceEquals(xs, rs))
                {
                    int i = 0, j = 0;
                    while (j < ys.ValueCount)
                    {
                        if (i >= xs.ValueCount || xs.Indices[i] > ys.Indices[j])
                        {
                            var yValues = ys.Values[j];
                            if (!Complex.Zero.Equals(yValues))
                            {
                                xs.InsertAtIndexUnchecked(i++, ys.Indices[j], yValues);
                            }
                            j++;
                        }
                        else if (xs.Indices[i] == ys.Indices[j])
                        {
                            // TODO: result can be zero, remove?
                            xs.Values[i++] += ys.Values[j++];
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
                else
                {
                    rs.Clear();
                    int i = 0, j = 0, last = -1;
                    while (i < xs.ValueCount || j < ys.ValueCount)
                    {
                        if (j >= ys.ValueCount || i < xs.ValueCount && xs.Indices[i] <= ys.Indices[j])
                        {
                            var next = xs.Indices[i];
                            if (next != last)
                            {
                                last = next;
                                rs.At(next, xs.Values[i] + ys.At(next));
                            }
                            i++;
                        }
                        else
                        {
                            var next = ys.Indices[j];
                            if (next != last)
                            {
                                last = next;
                                rs.At(next, xs.At(next) + ys.Values[j]);
                            }
                            j++;
                        }
                    }
                }
                return;
            }

            base.AddVectors(x, y, result);
        }
    }
}
