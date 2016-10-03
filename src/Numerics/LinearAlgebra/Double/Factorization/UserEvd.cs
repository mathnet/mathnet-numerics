﻿// <copyright file="UserEvd.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2013 Math.NET
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
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Double.Factorization
{

#if NOSYSNUMERICS
    using Numerics;
#else
    using System.Numerics;
#endif

    /// <summary>
    /// Eigenvalues and eigenvectors of a real matrix.
    /// </summary>
    /// <remarks>
    /// If A is symmetric, then A = V*D*V' where the eigenvalue matrix D is
    /// diagonal and the eigenvector matrix V is orthogonal.
    /// I.e. A = V*D*V' and V*VT=I.
    /// If A is not symmetric, then the eigenvalue matrix D is block diagonal
    /// with the real eigenvalues in 1-by-1 blocks and any complex eigenvalues,
    /// lambda + i*mu, in 2-by-2 blocks, [lambda, mu; -mu, lambda].  The
    /// columns of V represent the eigenvectors in the sense that A*V = V*D,
    /// i.e. A.Multiply(V) equals V.Multiply(D).  The matrix V may be badly
    /// conditioned, or even singular, so the validity of the equation
    /// A = V*D*Inverse(V) depends upon V.Condition().
    /// </remarks>
    internal sealed class UserEvd : Evd
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserEvd"/> class. This object will compute the
        /// the eigenvalue decomposition when the constructor is called and cache it's decomposition.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <param name="symmetricity">If it is known whether the matrix is symmetric or not the routine can skip checking it itself.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If EVD algorithm failed to converge with matrix <paramref name="matrix"/>.</exception>
        public static UserEvd Create(Matrix<double> matrix, Symmetricity symmetricity)
        {
            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare);
            }

            var order = matrix.RowCount;

            // Initialize matricies for eigenvalues and eigenvectors
            var eigenVectors = Matrix<double>.Build.SameAs(matrix, order, order);
            var blockDiagonal = Matrix<double>.Build.SameAs(matrix, order, order);
            var eigenValues = new LinearAlgebra.Complex.DenseVector(order);

            bool isSymmetric;
            switch (symmetricity)
            {
                case Symmetricity.Symmetric:
                case Symmetricity.Hermitian:
                    isSymmetric = true;
                    break;
                case Symmetricity.Asymmetric:
                    isSymmetric = false;
                    break;
                default:
                    isSymmetric = matrix.IsSymmetric();
                    break;
            }

            var d = new double[order];
            var e = new double[order];

            if (isSymmetric)
            {
                matrix.CopyTo(eigenVectors);
                d = eigenVectors.Row(order - 1).ToArray();

                SymmetricTridiagonalize(eigenVectors, d, e, order);
                SymmetricDiagonalize(eigenVectors, d, e, order);
            }
            else
            {
                var matrixH = matrix.ToArray();

                NonsymmetricReduceToHessenberg(eigenVectors, matrixH, order);
                NonsymmetricReduceHessenberToRealSchur(eigenVectors, matrixH, d, e, order);
            }

            for (var i = 0; i < order; i++)
            {
                blockDiagonal.At(i, i, d[i]);

                if (e[i] > 0)
                {
                    blockDiagonal.At(i, i + 1, e[i]);
                }
                else if (e[i] < 0)
                {
                    blockDiagonal.At(i, i - 1, e[i]);
                }
            }

            for (var i = 0; i < order; i++)
            {
                eigenValues[i] = new Complex(d[i], e[i]);
            }

            return new UserEvd(eigenVectors, eigenValues, blockDiagonal, isSymmetric);
        }

        UserEvd(Matrix<double> eigenVectors, Vector<Complex> eigenValues, Matrix<double> blockDiagonal, bool isSymmetric)
            : base(eigenVectors, eigenValues, blockDiagonal, isSymmetric)
        {
        }

        /// <summary>
        /// Symmetric Householder reduction to tridiagonal form.
        /// </summary>
        /// <param name="eigenVectors">The eigen vectors to work on.</param>
        /// <param name="d">Arrays for internal storage of real parts of eigenvalues</param>
        /// <param name="e">Arrays for internal storage of imaginary parts of eigenvalues</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedures tred2 by 
        /// Bowdler, Martin, Reinsch, and Wilkinson, Handbook for 
        /// Auto. Comp., Vol.ii-Linear Algebra, and the corresponding 
        /// Fortran subroutine in EISPACK.</remarks>
        static void SymmetricTridiagonalize(Matrix<double> eigenVectors, double[] d, double[] e, int order)
        {
            // Householder reduction to tridiagonal form.
            for (var i = order - 1; i > 0; i--)
            {
                // Scale to avoid under/overflow.
                var scale = 0.0;
                var h = 0.0;

                for (var k = 0; k < i; k++)
                {
                    scale = scale + Math.Abs(d[k]);
                }

                if (scale == 0.0)
                {
                    e[i] = d[i - 1];
                    for (var j = 0; j < i; j++)
                    {
                        d[j] = eigenVectors.At(i - 1, j);
                        eigenVectors.At(i, j, 0.0);
                        eigenVectors.At(j, i, 0.0);
                    }
                }
                else
                {
                    // Generate Householder vector.
                    for (var k = 0; k < i; k++)
                    {
                        d[k] /= scale;
                        h += d[k]*d[k];
                    }

                    var f = d[i - 1];
                    var g = Math.Sqrt(h);
                    if (f > 0)
                    {
                        g = -g;
                    }

                    e[i] = scale*g;
                    h = h - (f*g);
                    d[i - 1] = f - g;

                    for (var j = 0; j < i; j++)
                    {
                        e[j] = 0.0;
                    }

                    // Apply similarity transformation to remaining columns.
                    for (var j = 0; j < i; j++)
                    {
                        f = d[j];
                        eigenVectors.At(j, i, f);
                        g = e[j] + (eigenVectors.At(j, j)*f);

                        for (var k = j + 1; k <= i - 1; k++)
                        {
                            g += eigenVectors.At(k, j)*d[k];
                            e[k] += eigenVectors.At(k, j)*f;
                        }

                        e[j] = g;
                    }

                    f = 0.0;

                    for (var j = 0; j < i; j++)
                    {
                        e[j] /= h;
                        f += e[j]*d[j];
                    }

                    var hh = f/(h + h);

                    for (var j = 0; j < i; j++)
                    {
                        e[j] -= hh*d[j];
                    }

                    for (var j = 0; j < i; j++)
                    {
                        f = d[j];
                        g = e[j];

                        for (var k = j; k <= i - 1; k++)
                        {
                            eigenVectors.At(k, j, eigenVectors.At(k, j) - (f*e[k]) - (g*d[k]));
                        }

                        d[j] = eigenVectors.At(i - 1, j);
                        eigenVectors.At(i, j, 0.0);
                    }
                }

                d[i] = h;
            }

            // Accumulate transformations.
            for (var i = 0; i < order - 1; i++)
            {
                eigenVectors.At(order - 1, i, eigenVectors.At(i, i));
                eigenVectors.At(i, i, 1.0);
                var h = d[i + 1];
                if (h != 0.0)
                {
                    for (var k = 0; k <= i; k++)
                    {
                        d[k] = eigenVectors.At(k, i + 1)/h;
                    }

                    for (var j = 0; j <= i; j++)
                    {
                        var g = 0.0;
                        for (var k = 0; k <= i; k++)
                        {
                            g += eigenVectors.At(k, i + 1)*eigenVectors.At(k, j);
                        }

                        for (var k = 0; k <= i; k++)
                        {
                            eigenVectors.At(k, j, eigenVectors.At(k, j) - g*d[k]);
                        }
                    }
                }

                for (var k = 0; k <= i; k++)
                {
                    eigenVectors.At(k, i + 1, 0.0);
                }
            }

            for (var j = 0; j < order; j++)
            {
                d[j] = eigenVectors.At(order - 1, j);
                eigenVectors.At(order - 1, j, 0.0);
            }

            eigenVectors.At(order - 1, order - 1, 1.0);
            e[0] = 0.0;
        }

        /// <summary>
        /// Symmetric tridiagonal QL algorithm.
        /// </summary>
        /// <param name="eigenVectors">The eigen vectors to work on.</param>
        /// <param name="d">Arrays for internal storage of real parts of eigenvalues</param>
        /// <param name="e">Arrays for internal storage of imaginary parts of eigenvalues</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedures tql2, by
        /// Bowdler, Martin, Reinsch, and Wilkinson, Handbook for
        /// Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutine in EISPACK.</remarks>
        /// <exception cref="NonConvergenceException"></exception>
        static void SymmetricDiagonalize(Matrix<double> eigenVectors, double[] d, double[] e, int order)
        {
            const int maxiter = 1000;

            for (var i = 1; i < order; i++)
            {
                e[i - 1] = e[i];
            }

            e[order - 1] = 0.0;

            var f = 0.0;
            var tst1 = 0.0;
            var eps = Precision.DoublePrecision;
            for (var l = 0; l < order; l++)
            {
                // Find small subdiagonal element
                tst1 = Math.Max(tst1, Math.Abs(d[l]) + Math.Abs(e[l]));
                var m = l;
                while (m < order)
                {
                    if (Math.Abs(e[m]) <= eps*tst1)
                    {
                        break;
                    }

                    m++;
                }

                // If m == l, d[l] is an eigenvalue,
                // otherwise, iterate.
                if (m > l)
                {
                    var iter = 0;
                    do
                    {
                        iter = iter + 1; // (Could check iteration count here.)

                        // Compute implicit shift
                        var g = d[l];
                        var p = (d[l + 1] - g)/(2.0*e[l]);
                        var r = SpecialFunctions.Hypotenuse(p, 1.0);
                        if (p < 0)
                        {
                            r = -r;
                        }

                        d[l] = e[l]/(p + r);
                        d[l + 1] = e[l]*(p + r);

                        var dl1 = d[l + 1];
                        var h = g - d[l];
                        for (var i = l + 2; i < order; i++)
                        {
                            d[i] -= h;
                        }

                        f = f + h;

                        // Implicit QL transformation.
                        p = d[m];
                        var c = 1.0;
                        var c2 = c;
                        var c3 = c;
                        var el1 = e[l + 1];
                        var s = 0.0;
                        var s2 = 0.0;
                        for (var i = m - 1; i >= l; i--)
                        {
                            c3 = c2;
                            c2 = c;
                            s2 = s;
                            g = c*e[i];
                            h = c*p;
                            r = SpecialFunctions.Hypotenuse(p, e[i]);
                            e[i + 1] = s*r;
                            s = e[i]/r;
                            c = p/r;
                            p = (c*d[i]) - (s*g);
                            d[i + 1] = h + (s*((c*g) + (s*d[i])));

                            // Accumulate transformation.
                            for (var k = 0; k < order; k++)
                            {
                                h = eigenVectors.At(k, i + 1);
                                eigenVectors.At(k, i + 1, (s*eigenVectors.At(k, i)) + (c*h));
                                eigenVectors.At(k, i, (c*eigenVectors.At(k, i)) - (s*h));
                            }
                        }

                        p = (-s)*s2*c3*el1*e[l]/dl1;
                        e[l] = s*p;
                        d[l] = c*p;

                        // Check for convergence. If too many iterations have been performed, 
                        // throw exception that Convergence Failed
                        if (iter >= maxiter)
                        {
                            throw new NonConvergenceException();
                        }
                    } while (Math.Abs(e[l]) > eps*tst1);
                }

                d[l] = d[l] + f;
                e[l] = 0.0;
            }

            // Sort eigenvalues and corresponding vectors.
            for (var i = 0; i < order - 1; i++)
            {
                var k = i;
                var p = d[i];
                for (var j = i + 1; j < order; j++)
                {
                    if (d[j] < p)
                    {
                        k = j;
                        p = d[j];
                    }
                }

                if (k != i)
                {
                    d[k] = d[i];
                    d[i] = p;
                    for (var j = 0; j < order; j++)
                    {
                        p = eigenVectors.At(j, i);
                        eigenVectors.At(j, i, eigenVectors.At(j, k));
                        eigenVectors.At(j, k, p);
                    }
                }
            }
        }

        /// <summary>
        /// Nonsymmetric reduction to Hessenberg form.
        /// </summary>
        /// <param name="eigenVectors">The eigen vectors to work on.</param>
        /// <param name="matrixH">Array for internal storage of nonsymmetric Hessenberg form.</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedures orthes and ortran,
        /// by Martin and Wilkinson, Handbook for Auto. Comp.,
        /// Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutines in EISPACK.</remarks>
        static void NonsymmetricReduceToHessenberg(Matrix<double> eigenVectors, double[,] matrixH, int order)
        {
            var ort = new double[order];

            for (var m = 1; m < order - 1; m++)
            {
                // Scale column.
                var scale = 0.0;
                for (var i = m; i < order; i++)
                {
                    scale = scale + Math.Abs(matrixH[i, m - 1]);
                }

                if (scale != 0.0)
                {
                    // Compute Householder transformation.
                    var h = 0.0;
                    for (var i = order - 1; i >= m; i--)
                    {
                        ort[i] = matrixH[i, m - 1]/scale;
                        h += ort[i]*ort[i];
                    }

                    var g = Math.Sqrt(h);
                    if (ort[m] > 0)
                    {
                        g = -g;
                    }

                    h = h - (ort[m]*g);
                    ort[m] = ort[m] - g;

                    // Apply Householder similarity transformation
                    // H = (I-u*u'/h)*H*(I-u*u')/h)
                    for (var j = m; j < order; j++)
                    {
                        var f = 0.0;
                        for (var i = order - 1; i >= m; i--)
                        {
                            f += ort[i]*matrixH[i, j];
                        }

                        f = f/h;
                        for (var i = m; i < order; i++)
                        {
                            matrixH[i, j] -= f*ort[i];
                        }
                    }

                    for (var i = 0; i < order; i++)
                    {
                        var f = 0.0;
                        for (var j = order - 1; j >= m; j--)
                        {
                            f += ort[j]*matrixH[i, j];
                        }

                        f = f/h;
                        for (var j = m; j < order; j++)
                        {
                            matrixH[i, j] -= f*ort[j];
                        }
                    }

                    ort[m] = scale*ort[m];
                    matrixH[m, m - 1] = scale*g;
                }
            }

            // Accumulate transformations (Algol's ortran).
            for (var i = 0; i < order; i++)
            {
                for (var j = 0; j < order; j++)
                {
                    eigenVectors.At(i, j, i == j ? 1.0 : 0.0);
                }
            }

            for (var m = order - 2; m >= 1; m--)
            {
                if (matrixH[m, m - 1] != 0.0)
                {
                    for (var i = m + 1; i < order; i++)
                    {
                        ort[i] = matrixH[i, m - 1];
                    }

                    for (var j = m; j < order; j++)
                    {
                        var g = 0.0;
                        for (var i = m; i < order; i++)
                        {
                            g += ort[i]*eigenVectors.At(i, j);
                        }

                        // Double division avoids possible underflow
                        g = (g/ort[m])/matrixH[m, m - 1];
                        for (var i = m; i < order; i++)
                        {
                            eigenVectors.At(i, j, eigenVectors.At(i, j) + g*ort[i]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Nonsymmetric reduction from Hessenberg to real Schur form.
        /// </summary>
        /// <param name="eigenVectors">The eigen vectors to work on.</param>
        /// <param name="matrixH">Array for internal storage of nonsymmetric Hessenberg form.</param>
        /// <param name="d">Arrays for internal storage of real parts of eigenvalues</param>
        /// <param name="e">Arrays for internal storage of imaginary parts of eigenvalues</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedure hqr2,
        /// by Martin and Wilkinson, Handbook for Auto. Comp.,
        /// Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutine in EISPACK.</remarks>
        static void NonsymmetricReduceHessenberToRealSchur(Matrix<double> eigenVectors, double[,] matrixH, double[] d, double[] e, int order)
        {
            // Initialize
            var n = order - 1;
            var eps = Precision.DoublePrecision;
            var exshift = 0.0;
            double p = 0, q = 0, r = 0, s = 0, z = 0, w, x, y;

            // Store roots isolated by balanc and compute matrix norm
            var norm = 0.0;
            for (var i = 0; i < order; i++)
            {
                for (var j = Math.Max(i - 1, 0); j < order; j++)
                {
                    norm = norm + Math.Abs(matrixH[i, j]);
                }
            }

            // Outer loop over eigenvalue index
            var iter = 0;
            while (n >= 0)
            {
                // Look for single small sub-diagonal element
                var l = n;
                while (l > 0)
                {
                    s = Math.Abs(matrixH[l - 1, l - 1]) + Math.Abs(matrixH[l, l]);

                    if (s == 0.0)
                    {
                        s = norm;
                    }

                    if (Math.Abs(matrixH[l, l - 1]) < eps*s)
                    {
                        break;
                    }

                    l--;
                }

                // Check for convergence
                // One root found
                if (l == n)
                {
                    matrixH[n, n] = matrixH[n, n] + exshift;
                    d[n] = matrixH[n, n];
                    e[n] = 0.0;
                    n--;
                    iter = 0;

                    // Two roots found
                }
                else if (l == n - 1)
                {
                    w = matrixH[n, n - 1]*matrixH[n - 1, n];
                    p = (matrixH[n - 1, n - 1] - matrixH[n, n])/2.0;
                    q = (p*p) + w;
                    z = Math.Sqrt(Math.Abs(q));
                    matrixH[n, n] = matrixH[n, n] + exshift;
                    matrixH[n - 1, n - 1] = matrixH[n - 1, n - 1] + exshift;
                    x = matrixH[n, n];

                    // Real pair
                    if (q >= 0)
                    {
                        if (p >= 0)
                        {
                            z = p + z;
                        }
                        else
                        {
                            z = p - z;
                        }

                        d[n - 1] = x + z;

                        d[n] = d[n - 1];
                        if (z != 0.0)
                        {
                            d[n] = x - (w/z);
                        }

                        e[n - 1] = 0.0;
                        e[n] = 0.0;
                        x = matrixH[n, n - 1];
                        s = Math.Abs(x) + Math.Abs(z);
                        p = x/s;
                        q = z/s;
                        r = Math.Sqrt((p*p) + (q*q));
                        p = p/r;
                        q = q/r;

                        // Row modification
                        for (var j = n - 1; j < order; j++)
                        {
                            z = matrixH[n - 1, j];
                            matrixH[n - 1, j] = (q*z) + (p*matrixH[n, j]);
                            matrixH[n, j] = (q*matrixH[n, j]) - (p*z);
                        }

                        // Column modification
                        for (var i = 0; i <= n; i++)
                        {
                            z = matrixH[i, n - 1];
                            matrixH[i, n - 1] = (q*z) + (p*matrixH[i, n]);
                            matrixH[i, n] = (q*matrixH[i, n]) - (p*z);
                        }

                        // Accumulate transformations
                        for (var i = 0; i < order; i++)
                        {
                            z = eigenVectors.At(i, n - 1);
                            eigenVectors.At(i, n - 1, (q*z) + (p*eigenVectors.At(i, n)));
                            eigenVectors.At(i, n, (q*eigenVectors.At(i, n)) - (p*z));
                        }

                        // Complex pair
                    }
                    else
                    {
                        d[n - 1] = x + p;
                        d[n] = x + p;
                        e[n - 1] = z;
                        e[n] = -z;
                    }

                    n = n - 2;
                    iter = 0;

                    // No convergence yet
                }
                else
                {
                    // Form shift
                    x = matrixH[n, n];
                    y = 0.0;
                    w = 0.0;
                    if (l < n)
                    {
                        y = matrixH[n - 1, n - 1];
                        w = matrixH[n, n - 1]*matrixH[n - 1, n];
                    }

                    // Wilkinson's original ad hoc shift
                    if (iter == 10)
                    {
                        exshift += x;
                        for (var i = 0; i <= n; i++)
                        {
                            matrixH[i, i] -= x;
                        }

                        s = Math.Abs(matrixH[n, n - 1]) + Math.Abs(matrixH[n - 1, n - 2]);
                        x = y = 0.75*s;
                        w = (-0.4375)*s*s;
                    }

                    // MATLAB's new ad hoc shift
                    if (iter == 30)
                    {
                        s = (y - x)/2.0;
                        s = (s*s) + w;
                        if (s > 0)
                        {
                            s = Math.Sqrt(s);
                            if (y < x)
                            {
                                s = -s;
                            }

                            s = x - (w/(((y - x)/2.0) + s));
                            for (var i = 0; i <= n; i++)
                            {
                                matrixH[i, i] -= s;
                            }

                            exshift += s;
                            x = y = w = 0.964;
                        }
                    }

                    iter = iter + 1; // (Could check iteration count here.)

                    // Look for two consecutive small sub-diagonal elements
                    var m = n - 2;
                    while (m >= l)
                    {
                        z = matrixH[m, m];
                        r = x - z;
                        s = y - z;
                        p = (((r*s) - w)/matrixH[m + 1, m]) + matrixH[m, m + 1];
                        q = matrixH[m + 1, m + 1] - z - r - s;
                        r = matrixH[m + 2, m + 1];
                        s = Math.Abs(p) + Math.Abs(q) + Math.Abs(r);
                        p = p/s;
                        q = q/s;
                        r = r/s;

                        if (m == l)
                        {
                            break;
                        }

                        if (Math.Abs(matrixH[m, m - 1])*(Math.Abs(q) + Math.Abs(r)) < eps*(Math.Abs(p)*(Math.Abs(matrixH[m - 1, m - 1]) + Math.Abs(z) + Math.Abs(matrixH[m + 1, m + 1]))))
                        {
                            break;
                        }

                        m--;
                    }

                    for (var i = m + 2; i <= n; i++)
                    {
                        matrixH[i, i - 2] = 0.0;
                        if (i > m + 2)
                        {
                            matrixH[i, i - 3] = 0.0;
                        }
                    }

                    // Double QR step involving rows l:n and columns m:n
                    for (var k = m; k <= n - 1; k++)
                    {
                        bool notlast = k != n - 1;

                        if (k != m)
                        {
                            p = matrixH[k, k - 1];
                            q = matrixH[k + 1, k - 1];
                            r = notlast ? matrixH[k + 2, k - 1] : 0.0;
                            x = Math.Abs(p) + Math.Abs(q) + Math.Abs(r);
                            if (x != 0.0)
                            {
                                p = p/x;
                                q = q/x;
                                r = r/x;
                            }
                        }

                        if (x == 0.0)
                        {
                            break;
                        }

                        s = Math.Sqrt((p*p) + (q*q) + (r*r));
                        if (p < 0)
                        {
                            s = -s;
                        }

                        if (s != 0.0)
                        {
                            if (k != m)
                            {
                                matrixH[k, k - 1] = (-s)*x;
                            }
                            else if (l != m)
                            {
                                matrixH[k, k - 1] = -matrixH[k, k - 1];
                            }

                            p = p + s;
                            x = p/s;
                            y = q/s;
                            z = r/s;
                            q = q/p;
                            r = r/p;

                            // Row modification
                            for (var j = k; j < order; j++)
                            {
                                p = matrixH[k, j] + (q*matrixH[k + 1, j]);

                                if (notlast)
                                {
                                    p = p + (r*matrixH[k + 2, j]);
                                    matrixH[k + 2, j] = matrixH[k + 2, j] - (p*z);
                                }

                                matrixH[k, j] = matrixH[k, j] - (p*x);
                                matrixH[k + 1, j] = matrixH[k + 1, j] - (p*y);
                            }

                            // Column modification
                            for (var i = 0; i <= Math.Min(n, k + 3); i++)
                            {
                                p = (x*matrixH[i, k]) + (y*matrixH[i, k + 1]);

                                if (notlast)
                                {
                                    p = p + (z*matrixH[i, k + 2]);
                                    matrixH[i, k + 2] = matrixH[i, k + 2] - (p*r);
                                }

                                matrixH[i, k] = matrixH[i, k] - p;
                                matrixH[i, k + 1] = matrixH[i, k + 1] - (p*q);
                            }

                            // Accumulate transformations
                            for (var i = 0; i < order; i++)
                            {
                                p = (x*eigenVectors.At(i, k)) + (y*eigenVectors.At(i, k + 1));

                                if (notlast)
                                {
                                    p = p + (z*eigenVectors.At(i, k + 2));
                                    eigenVectors.At(i, k + 2, eigenVectors.At(i, k + 2) - (p*r));
                                }

                                eigenVectors.At(i, k, eigenVectors.At(i, k) - p);
                                eigenVectors.At(i, k + 1, eigenVectors.At(i, k + 1) - (p*q));
                            }
                        } // (s != 0)
                    } // k loop
                } // check convergence
            } // while (n >= low)

            // Backsubstitute to find vectors of upper triangular form
            if (norm == 0.0)
            {
                return;
            }

            for (n = order - 1; n >= 0; n--)
            {
                double t;

                p = d[n];
                q = e[n];

                // Real vector
                if (q == 0.0)
                {
                    var l = n;
                    matrixH[n, n] = 1.0;
                    for (var i = n - 1; i >= 0; i--)
                    {
                        w = matrixH[i, i] - p;
                        r = 0.0;
                        for (var j = l; j <= n; j++)
                        {
                            r = r + (matrixH[i, j]*matrixH[j, n]);
                        }

                        if (e[i] < 0.0)
                        {
                            z = w;
                            s = r;
                        }
                        else
                        {
                            l = i;
                            if (e[i] == 0.0)
                            {
                                if (w != 0.0)
                                {
                                    matrixH[i, n] = (-r)/w;
                                }
                                else
                                {
                                    matrixH[i, n] = (-r)/(eps*norm);
                                }

                                // Solve real equations
                            }
                            else
                            {
                                x = matrixH[i, i + 1];
                                y = matrixH[i + 1, i];
                                q = ((d[i] - p)*(d[i] - p)) + (e[i]*e[i]);
                                t = ((x*s) - (z*r))/q;
                                matrixH[i, n] = t;
                                if (Math.Abs(x) > Math.Abs(z))
                                {
                                    matrixH[i + 1, n] = (-r - (w*t))/x;
                                }
                                else
                                {
                                    matrixH[i + 1, n] = (-s - (y*t))/z;
                                }
                            }

                            // Overflow control
                            t = Math.Abs(matrixH[i, n]);
                            if ((eps*t)*t > 1)
                            {
                                for (var j = i; j <= n; j++)
                                {
                                    matrixH[j, n] = matrixH[j, n]/t;
                                }
                            }
                        }
                    }

                    // Complex vector
                }
                else if (q < 0)
                {
                    var l = n - 1;

                    // Last vector component imaginary so matrix is triangular
                    if (Math.Abs(matrixH[n, n - 1]) > Math.Abs(matrixH[n - 1, n]))
                    {
                        matrixH[n - 1, n - 1] = q/matrixH[n, n - 1];
                        matrixH[n - 1, n] = (-(matrixH[n, n] - p))/matrixH[n, n - 1];
                    }
                    else
                    {
                        var res = Cdiv(0.0, -matrixH[n - 1, n], matrixH[n - 1, n - 1] - p, q);
                        matrixH[n - 1, n - 1] = res.Real;
                        matrixH[n - 1, n] = res.Imaginary;
                    }

                    matrixH[n, n - 1] = 0.0;
                    matrixH[n, n] = 1.0;
                    for (var i = n - 2; i >= 0; i--)
                    {
                        double ra = 0.0;
                        double sa = 0.0;
                        for (var j = l; j <= n; j++)
                        {
                            ra = ra + (matrixH[i, j]*matrixH[j, n - 1]);
                            sa = sa + (matrixH[i, j]*matrixH[j, n]);
                        }

                        w = matrixH[i, i] - p;

                        if (e[i] < 0.0)
                        {
                            z = w;
                            r = ra;
                            s = sa;
                        }
                        else
                        {
                            l = i;
                            if (e[i] == 0.0)
                            {
                                var res = Cdiv(-ra, -sa, w, q);
                                matrixH[i, n - 1] = res.Real;
                                matrixH[i, n] = res.Imaginary;
                            }
                            else
                            {
                                // Solve complex equations
                                x = matrixH[i, i + 1];
                                y = matrixH[i + 1, i];

                                double vr = ((d[i] - p)*(d[i] - p)) + (e[i]*e[i]) - (q*q);
                                double vi = (d[i] - p)*2.0*q;
                                if ((vr == 0.0) && (vi == 0.0))
                                {
                                    vr = eps*norm*(Math.Abs(w) + Math.Abs(q) + Math.Abs(x) + Math.Abs(y) + Math.Abs(z));
                                }

                                var res = Cdiv((x*r) - (z*ra) + (q*sa), (x*s) - (z*sa) - (q*ra), vr, vi);
                                matrixH[i, n - 1] = res.Real;
                                matrixH[i, n] = res.Imaginary;
                                if (Math.Abs(x) > (Math.Abs(z) + Math.Abs(q)))
                                {
                                    matrixH[i + 1, n - 1] = (-ra - (w*matrixH[i, n - 1]) + (q*matrixH[i, n]))/x;
                                    matrixH[i + 1, n] = (-sa - (w*matrixH[i, n]) - (q*matrixH[i, n - 1]))/x;
                                }
                                else
                                {
                                    res = Cdiv(-r - (y*matrixH[i, n - 1]), -s - (y*matrixH[i, n]), z, q);
                                    matrixH[i + 1, n - 1] = res.Real;
                                    matrixH[i + 1, n] = res.Imaginary;
                                }
                            }

                            // Overflow control
                            t = Math.Max(Math.Abs(matrixH[i, n - 1]), Math.Abs(matrixH[i, n]));
                            if ((eps*t)*t > 1)
                            {
                                for (var j = i; j <= n; j++)
                                {
                                    matrixH[j, n - 1] = matrixH[j, n - 1]/t;
                                    matrixH[j, n] = matrixH[j, n]/t;
                                }
                            }
                        }
                    }
                }
            }

            // Back transformation to get eigenvectors of original matrix
            for (var j = order - 1; j >= 0; j--)
            {
                for (var i = 0; i < order; i++)
                {
                    z = 0.0;
                    for (var k = 0; k <= j; k++)
                    {
                        z = z + (eigenVectors.At(i, k)*matrixH[k, j]);
                    }

                    eigenVectors.At(i, j, z);
                }
            }
        }

        /// <summary>
        /// Complex scalar division X/Y.
        /// </summary>
        /// <param name="xreal">Real part of X</param>
        /// <param name="ximag">Imaginary part of X</param>
        /// <param name="yreal">Real part of Y</param>
        /// <param name="yimag">Imaginary part of Y</param>
        /// <returns>Division result as a <see cref="Complex"/> number.</returns>
        static Complex Cdiv(double xreal, double ximag, double yreal, double yimag)
        {
            if (Math.Abs(yimag) < Math.Abs(yreal))
            {
                return new Complex((xreal + (ximag*(yimag/yreal)))/(yreal + (yimag*(yimag/yreal))), (ximag - (xreal*(yimag/yreal)))/(yreal + (yimag*(yimag/yreal))));
            }

            return new Complex((ximag + (xreal*(yreal/yimag)))/(yimag + (yreal*(yreal/yimag))), (-xreal + (ximag*(yreal/yimag)))/(yimag + (yreal*(yreal/yimag))));
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A SVD factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</param>
        public override void Solve(Matrix<double> input, Matrix<double> result)
        {
            // The solution X should have the same number of columns as B
            if (input.ColumnCount != result.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension);
            }

            // The dimension compatibility conditions for X = A\B require the two matrices A and B to have the same number of rows
            if (EigenValues.Count != input.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension);
            }

            // The solution X row dimension is equal to the column dimension of A
            if (EigenValues.Count != result.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension);
            }

            if (IsSymmetric)
            {
                var order = EigenValues.Count;
                var tmp = new double[order];

                for (var k = 0; k < order; k++)
                {
                    for (var j = 0; j < order; j++)
                    {
                        double value = 0;
                        if (j < order)
                        {
                            for (var i = 0; i < order; i++)
                            {
                                value += EigenVectors.At(i, j)*input.At(i, k);
                            }

                            value /= EigenValues[j].Real;
                        }

                        tmp[j] = value;
                    }

                    for (var j = 0; j < order; j++)
                    {
                        double value = 0;
                        for (var i = 0; i < order; i++)
                        {
                            value += EigenVectors.At(j, i)*tmp[i];
                        }

                        result.At(j, k, value);
                    }
                }
            }
            else
            {
                throw new ArgumentException(Resources.ArgumentMatrixSymmetric);
            }
        }

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A EVD factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>x</b>.</param>
        public override void Solve(Vector<double> input, Vector<double> result)
        {
            // Ax=b where A is an m x m matrix
            // Check that b is a column vector with m entries
            if (EigenValues.Count != input.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            // Check that x is a column vector with n entries
            if (EigenValues.Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            if (IsSymmetric)
            {
                // Symmetric case -> x = V * inv(λ) * VT * b;
                var order = EigenValues.Count;
                var tmp = new double[order];
                double value;

                for (var j = 0; j < order; j++)
                {
                    value = 0;
                    if (j < order)
                    {
                        for (var i = 0; i < order; i++)
                        {
                            value += EigenVectors.At(i, j)*input[i];
                        }

                        value /= EigenValues[j].Real;
                    }

                    tmp[j] = value;
                }

                for (var j = 0; j < order; j++)
                {
                    value = 0;
                    for (int i = 0; i < order; i++)
                    {
                        value += EigenVectors.At(j, i)*tmp[i];
                    }

                    result[j] = value;
                }
            }
            else
            {
                throw new ArgumentException(Resources.ArgumentMatrixSymmetric);
            }
        }
    }
}
