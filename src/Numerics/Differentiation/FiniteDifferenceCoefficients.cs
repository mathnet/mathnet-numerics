// <copyright file="FiniteDifferenceCoefficients.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2015 Math.NET
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

using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MathNet.Numerics.Differentiation
{
    /// <summary>
    /// Class to calculate finite difference coefficients using Taylor series expansion method.
    /// <remarks>
    /// <para>
    /// For n points, coefficients are calculated up to the maximum derivative order possible (n-1).
    /// The current function value position specifies the "center" for surrounding coefficients.
    /// Selecting the first, middle or last positions represent forward, backwards and central difference methods.
    /// </para>
    /// </remarks>
    /// </summary>
    public class FiniteDifferenceCoefficients
    {
        /// <summary>
        /// Number of points for finite difference coefficients. Changing this value recalculates the coefficients table.
        /// </summary>
        public int Points
        {
            get => _points;
            set
            {
                CalculateCoefficients(value);
                _points = value;
            }
        }

        double[][,] _coefficients;
        int _points;

        /// <summary>
        /// Initializes a new instance of the <see cref="FiniteDifferenceCoefficients"/> class.
        /// </summary>
        /// <param name="points">Number of finite difference coefficients.</param>
        public FiniteDifferenceCoefficients(int points)
        {
            Points = points;
            CalculateCoefficients(Points);
        }

        /// <summary>
        /// Gets the finite difference coefficients for a specified center and order.
        /// </summary>
        /// <param name="center">Current function position with respect to coefficients. Must be within point range.</param>
        /// <param name="order">Order of finite difference coefficients.</param>
        /// <returns>Vector of finite difference coefficients.</returns>
        public double[] GetCoefficients(int center, int order)
        {
            if (center >= _coefficients.Length)
                throw new ArgumentOutOfRangeException(nameof(center), "Center position must be within the point range.");
            if (order >= _coefficients.Length)
                throw new ArgumentOutOfRangeException(nameof(order), "Maximum difference order is points-1.");

            // Return proper row
            var columns = _coefficients[center].GetLength(1);
            var array = new double[columns];
            for (int i = 0; i < columns; ++i)
                array[i] = _coefficients[center][order, i];
            return array;
        }

        /// <summary>
        /// Gets the finite difference coefficients for all orders at a specified center.
        /// </summary>
        /// <param name="center">Current function position with respect to coefficients. Must be within point range.</param>
        /// <returns>Rectangular array of coefficients, with columns specifying order.</returns>
        public double[,] GetCoefficientsForAllOrders(int center)
        {
            if (center >= _coefficients.Length)
                throw new ArgumentOutOfRangeException(nameof(center), "Center position must be within the point range.");

            return _coefficients[center];
        }

        void CalculateCoefficients(int points)
        {
            var c = new double[points][,];

            // For ever possible center given the number of points, compute ever possible coefficient for all possible orders.
            for (int center = 0; center < points; center++)
            {
                // Deltas matrix for center located at 'center'.
                var A = new DenseMatrix(points);
                var l = points - center - 1;
                for (int row = points - 1; row >= 0; row--)
                {
                    A[row, 0] = 1.0;
                    for (int col = 1; col < points; col++)
                    {
                        A[row, col] = A[row, col - 1] * l / col;
                    }
                    l -= 1;
                }

                c[center] = A.Inverse().ToArray();

                // "Polish" results by rounding.
                var fac = SpecialFunctions.Factorial(points);
                for (int j = 0; j < points; j++)
                {
                    for (int k = 0; k < points; k++)
                    {
                        c[center][j, k] = (Math.Round(c[center][j, k] * fac, MidpointRounding.AwayFromZero)) / fac;
                    }
                }
            }

            _coefficients = c;
        }
    }
}
