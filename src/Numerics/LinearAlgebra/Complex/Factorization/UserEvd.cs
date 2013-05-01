// <copyright file="UserEvd.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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
namespace MathNet.Numerics.LinearAlgebra.Complex.Factorization
{
    using System;
    using System.Numerics;
    using Generic;
    using Properties;

    /// <summary>
    /// Eigenvalues and eigenvectors of a complex matrix.
    /// </summary>
    /// <remarks>
    /// If A is hermitan, then A = V*D*V' where the eigenvalue matrix D is
    /// diagonal and the eigenvector matrix V is hermitan.
    /// I.e. A = V*D*V' and V*VH=I.
    /// If A is not symmetric, then the eigenvalue matrix D is block diagonal
    /// with the real eigenvalues in 1-by-1 blocks and any complex eigenvalues,
    /// lambda + i*mu, in 2-by-2 blocks, [lambda, mu; -mu, lambda].  The
    /// columns of V represent the eigenvectors in the sense that A*V = V*D,
    /// i.e. A.Multiply(V) equals V.Multiply(D).  The matrix V may be badly
    /// conditioned, or even singular, so the validity of the equation
    /// A = V*D*Inverse(V) depends upon V.Condition().
    /// </remarks>
    public class UserEvd : Evd
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserEvd"/> class. This object will compute the
        /// the eigenvalue decomposition when the constructor is called and cache it's decomposition.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If EVD algorithm failed to converge with matrix <paramref name="matrix"/>.</exception>
        public UserEvd(Matrix<Complex> matrix)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare);
            }

            var order = matrix.RowCount;

            // Initialize matricies for eigenvalues and eigenvectors
            MatrixEv = DenseMatrix.Identity(order);
            MatrixD = matrix.CreateMatrix(order, order);
            VectorEv = new DenseVector(order);
           
            IsSymmetric = true;

            for (var i = 0; IsSymmetric && i < order; i++)
            {
                for (var j = 0; IsSymmetric && j < order; j++)
                {
                    IsSymmetric &= matrix.At(i, j) == matrix.At(j, i).Conjugate();
                }
            }

            if (IsSymmetric)
            {
                var matrixCopy = matrix.ToArray();
                var tau = new Complex[order];
                var d = new double[order];
                var e = new double[order];

                SymmetricTridiagonalize(matrixCopy, d, e, tau, order);
                SymmetricDiagonalize(d, e, order);
                SymmetricUntridiagonalize(matrixCopy, tau, order);

                for (var i = 0; i < order; i++)
                {
                    VectorEv[i] = new Complex(d[i], e[i]);
                }
            }
            else
            {
                var matrixH = matrix.ToArray();
                NonsymmetricReduceToHessenberg(matrixH, order);
                NonsymmetricReduceHessenberToRealSchur(matrixH, order);
            }

            MatrixD.SetDiagonal(VectorEv);
        }

        /// <summary>
        /// Reduces a complex hermitian matrix to a real symmetric tridiagonal matrix using unitary similarity transformations.
        /// </summary>
        /// <param name="matrixA">Source matrix to reduce</param>
        /// <param name="d">Output: Arrays for internal storage of real parts of eigenvalues</param>
        /// <param name="e">Output: Arrays for internal storage of imaginary parts of eigenvalues</param>
        /// <param name="tau">Output: Arrays that contains further information about the transformations.</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedures HTRIDI by 
        /// Smith, Boyle, Dongarra, Garbow, Ikebe, Klema, Moler, and Wilkinson, Handbook for 
        /// Auto. Comp., Vol.ii-Linear Algebra, and the corresponding 
        /// Fortran subroutine in EISPACK.</remarks>
        private static void SymmetricTridiagonalize(Complex[,] matrixA, double[] d, double[] e, Complex[] tau, int order)
        {
            double hh;
            tau[order - 1] = Complex.One;

            for (var i = 0; i < order; i++)
            {
                d[i] = matrixA[i, i].Real;
            }

            // Householder reduction to tridiagonal form.
            for (var i = order - 1; i > 0; i--)
            {
                // Scale to avoid under/overflow.
                var scale = 0.0;
                var h = 0.0;

                for (var k = 0; k < i; k++)
                {
                    scale = scale + Math.Abs(matrixA[i, k].Real) + Math.Abs(matrixA[i, k].Imaginary);
                }

                if (scale == 0.0)
                {
                    tau[i - 1] = Complex.One;
                    e[i] = 0.0;
                }
                else
                {
                    for (var k = 0; k < i; k++)
                    {
                        matrixA[i, k] /= scale;
                        h += matrixA[i, k].MagnitudeSquared();
                    }

                    Complex g = Math.Sqrt(h);
                    e[i] = scale * g.Real;

                    Complex temp;
                    var f = matrixA[i, i - 1];
                    if (f.Magnitude != 0)
                    {
                        temp = -(matrixA[i, i - 1].Conjugate() * tau[i].Conjugate()) / f.Magnitude;
                        h += f.Magnitude * g.Real;
                        g = 1.0 + (g / f.Magnitude);
                        matrixA[i, i - 1] *= g;
                    }
                    else
                    {
                        temp = -tau[i].Conjugate();
                        matrixA[i, i - 1] = g;
                    }

                    if ((f.Magnitude == 0) || (i != 1))
                    {
                        f = Complex.Zero;
                        for (var j = 0; j < i; j++)
                        {
                            var tmp = Complex.Zero;

                            // Form element of A*U.
                            for (var k = 0; k <= j; k++)
                            {
                                tmp += matrixA[j, k] * matrixA[i, k].Conjugate();
                            }

                            for (var k = j + 1; k <= i - 1; k++)
                            {
                                tmp += matrixA[k, j].Conjugate() * matrixA[i, k].Conjugate();
                            }

                            // Form element of P
                            tau[j] = tmp / h;
                            f += (tmp / h) * matrixA[i, j];
                        }

                        hh = f.Real / (h + h);

                        // Form the reduced A.
                        for (var j = 0; j < i; j++)
                        {
                            f = matrixA[i, j].Conjugate();
                            g = tau[j] - (hh * f);
                            tau[j] = g.Conjugate();

                            for (var k = 0; k <= j; k++)
                            {
                                matrixA[j, k] -= (f * tau[k]) + (g * matrixA[i, k]);
                            }
                        }
                    }

                    for (var k = 0; k < i; k++)
                    {
                        matrixA[i, k] *= scale;
                    }

                    tau[i - 1] = temp.Conjugate();
                }

                hh = d[i];
                d[i] = matrixA[i, i].Real;
                matrixA[i, i] = new Complex(hh, scale * Math.Sqrt(h));
            }

            hh = d[0];
            d[0] = matrixA[0, 0].Real;
            matrixA[0, 0] = hh;
            e[0] = 0.0;
        }

        /// <summary>
        /// Symmetric tridiagonal QL algorithm.
        /// </summary>
        /// <param name="d">Arrays for internal storage of real parts of eigenvalues</param>
        /// <param name="e">Arrays for internal storage of imaginary parts of eigenvalues</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedures tql2, by
        /// Bowdler, Martin, Reinsch, and Wilkinson, Handbook for
        /// Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutine in EISPACK.</remarks>
        /// <exception cref="NonConvergenceException"></exception>
        private void SymmetricDiagonalize(double[] d, double[] e, int order)
        {
            const int Maxiter = 1000;

            for (var i = 1; i < order; i++)
            {
                e[i - 1] = e[i];
            }

            e[order - 1] = 0.0;

            var f = 0.0;
            var tst1 = 0.0;
            var eps = Precision.DoubleMachinePrecision;
            for (var l = 0; l < order; l++)
            {
                // Find small subdiagonal element
                tst1 = Math.Max(tst1, Math.Abs(d[l]) + Math.Abs(e[l]));
                var m = l;
                while (m < order)
                {
                    if (Math.Abs(e[m]) <= eps * tst1)
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
                        var p = (d[l + 1] - g) / (2.0 * e[l]);
                        var r = SpecialFunctions.Hypotenuse(p, 1.0);
                        if (p < 0)
                        {
                            r = -r;
                        }

                        d[l] = e[l] / (p + r);
                        d[l + 1] = e[l] * (p + r);

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
                            g = c * e[i];
                            h = c * p;
                            r = SpecialFunctions.Hypotenuse(p, e[i]);
                            e[i + 1] = s * r;
                            s = e[i] / r;
                            c = p / r;
                            p = (c * d[i]) - (s * g);
                            d[i + 1] = h + (s * ((c * g) + (s * d[i])));

                            // Accumulate transformation.
                            for (var k = 0; k < order; k++)
                            {
                                h = MatrixEv.At(k, i + 1).Real;
                                MatrixEv.At(k, i + 1, (s * MatrixEv.At(k, i).Real) + (c * h));
                                MatrixEv.At(k, i, (c * MatrixEv.At(k, i).Real) - (s * h));
                            }
                        }

                        p = (-s) * s2 * c3 * el1 * e[l] / dl1;
                        e[l] = s * p;
                        d[l] = c * p;

                        // Check for convergence. If too many iterations have been performed, 
                        // throw exception that Convergence Failed
                        if (iter >= Maxiter)
                        {
                            throw new NonConvergenceException();
                        }
                    }
                    while (Math.Abs(e[l]) > eps * tst1);
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
                        p = MatrixEv.At(j, i).Real;
                        MatrixEv.At(j, i, MatrixEv.At(j, k));
                        MatrixEv.At(j, k, p);
                    }
                }
            }
        }

        /// <summary>
        /// Determines eigenvectors by undoing the symmetric tridiagonalize transformation
        /// </summary>
        /// <param name="matrixA">Previously tridiagonalized matrix by <see cref="SymmetricTridiagonalize"/>.</param>
        /// <param name="tau">Contains further information about the transformations</param>
        /// <param name="order">Input matrix order</param>
        /// <remarks>This is derived from the Algol procedures HTRIBK, by
        /// by Smith, Boyle, Dongarra, Garbow, Ikebe, Klema, Moler, and Wilkinson, Handbook for
        /// Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutine in EISPACK.</remarks>
        private void SymmetricUntridiagonalize(Complex[,] matrixA, Complex[] tau, int order)
        {
            for (var i = 0; i < order; i++)
            {
                for (var j = 0; j < order; j++)
                {
                    MatrixEv.At(i, j, MatrixEv.At(i, j).Real * tau[i].Conjugate());
                }
            }

            // Recover and apply the Householder matrices.
            for (var i = 1; i < order; i++)
            {
                var h = matrixA[i, i].Imaginary;
                if (h != 0)
                {
                    for (var j = 0; j < order; j++)
                    {
                        var s = Complex.Zero;
                        for (var k = 0; k < i; k++)
                        {
                            s += MatrixEv.At(k, j) * matrixA[i, k];
                        }

                        s = (s / h) / h;

                        for (var k = 0; k < i; k++)
                        {
                            MatrixEv.At(k, j, MatrixEv.At(k, j) - s * matrixA[i, k].Conjugate());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Nonsymmetric reduction to Hessenberg form.
        /// </summary>
        /// <param name="matrixH">Array for internal storage of nonsymmetric Hessenberg form.</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedures orthes and ortran,
        /// by Martin and Wilkinson, Handbook for Auto. Comp.,
        /// Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutines in EISPACK.</remarks>
        private void NonsymmetricReduceToHessenberg(Complex[,] matrixH, int order)
        {
            var ort = new Complex[order];

            for (var m = 1; m < order - 1; m++)
            {
                // Scale column.
                var scale = 0.0;
                for (var i = m; i < order; i++)
                {
                    scale += Math.Abs(matrixH[i, m - 1].Real) + Math.Abs(matrixH[i, m - 1].Imaginary);
                }

                if (scale != 0.0)
                {
                    // Compute Householder transformation.
                    var h = 0.0;
                    for (var i = order - 1; i >= m; i--)
                    {
                        ort[i] = matrixH[i, m - 1] / scale;
                        h += ort[i].MagnitudeSquared();
                    }

                    var g = Math.Sqrt(h);
                    if (ort[m].Magnitude != 0)
                    {
                        h = h + (ort[m].Magnitude * g);
                        g /= ort[m].Magnitude;
                        ort[m] = (1.0 + g) * ort[m];
                    }
                    else
                    {
                        ort[m] = g;
                        matrixH[m, m - 1] = scale;
                    }

                    // Apply Householder similarity transformation
                    // H = (I-u*u'/h)*H*(I-u*u')/h)
                    for (var j = m; j < order; j++)
                    {
                        var f = Complex.Zero;
                        for (var i = order - 1; i >= m; i--)
                        {
                            f += ort[i].Conjugate() * matrixH[i, j];
                        }

                        f = f / h;
                        for (var i = m; i < order; i++)
                        {
                            matrixH[i, j] -= f * ort[i];
                        }
                    }

                    for (var i = 0; i < order; i++)
                    {
                        var f = Complex.Zero;
                        for (var j = order - 1; j >= m; j--)
                        {
                            f += ort[j] * matrixH[i, j];
                        }

                        f = f / h;
                        for (var j = m; j < order; j++)
                        {
                            matrixH[i, j] -= f * ort[j].Conjugate();
                        }
                    }

                    ort[m] = scale * ort[m];
                    matrixH[m, m - 1] *= -g;
                }
            }

            // Accumulate transformations (Algol's ortran).
            for (var i = 0; i < order; i++)
            {
                for (var j = 0; j < order; j++)
                {
                    MatrixEv.At(i, j, i == j ? Complex.One : Complex.Zero);
                }
            }

            for (var m = order - 2; m >= 1; m--)
            {
                if (matrixH[m, m - 1] != Complex.Zero && ort[m] != Complex.Zero)
                {
                    var norm = (matrixH[m, m - 1].Real * ort[m].Real) + (matrixH[m, m - 1].Imaginary * ort[m].Imaginary);

                    for (var i = m + 1; i < order; i++)
                    {
                        ort[i] = matrixH[i, m - 1];
                    }

                    for (var j = m; j < order; j++)
                    {
                        var g = Complex.Zero;
                        for (var i = m; i < order; i++)
                        {
                            g += ort[i].Conjugate() * MatrixEv.At(i, j);
                        }

                        // Double division avoids possible underflow
                        g /= norm;
                        for (var i = m; i < order; i++)
                        {
                            MatrixEv.At(i, j, MatrixEv.At(i, j) + g * ort[i]);
                        }
                    }
                }
            }
            
            // Create real subdiagonal elements.
            for (var i = 1; i < order; i++)
            {
                if (matrixH[i, i - 1].Imaginary != 0.0)
                {
                    var y = matrixH[i, i - 1] / matrixH[i, i - 1].Magnitude;
                    matrixH[i, i - 1] = matrixH[i, i - 1].Magnitude;
                    for (var j = i; j < order; j++)
                    {
                        matrixH[i, j] *= y.Conjugate();
                    }

                    for (var j = 0; j <= Math.Min(i + 1, order - 1); j++)
                    {
                        matrixH[j, i] *= y;
                    }

                    for (var j = 0; j < order; j++)
                    {
                        MatrixEv.At(j, i, MatrixEv.At(j, i) * y);
                    }
                }
            }
        }

        /// <summary>
        /// Nonsymmetric reduction from Hessenberg to real Schur form.
        /// </summary>
        /// <param name="matrixH">Array for internal storage of nonsymmetric Hessenberg form.</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedure hqr2,
        /// by Martin and Wilkinson, Handbook for Auto. Comp.,
        /// Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutine in EISPACK.</remarks>
        private void NonsymmetricReduceHessenberToRealSchur(Complex[,] matrixH, int order)
        {
            // Initialize
            var n = order - 1;
            var eps = Precision.DoubleMachinePrecision;

            double norm;
            Complex x, y, z, exshift = Complex.Zero;

            // Outer loop over eigenvalue index
            var iter = 0;
            while (n >= 0)
            {
                // Look for single small sub-diagonal element
                var l = n;
                while (l > 0)
                {
                    var tst1 = Math.Abs(matrixH[l - 1, l - 1].Real) + Math.Abs(matrixH[l - 1, l - 1].Imaginary) + Math.Abs(matrixH[l, l].Real) + Math.Abs(matrixH[l, l].Imaginary);
                    if (Math.Abs(matrixH[l, l - 1].Real) < eps * tst1)
                    {
                        break;
                    }

                    l--;
                }

                // Check for convergence
                // One root found
                if (l == n)
                {
                    matrixH[n, n] += exshift;
                    VectorEv[n] = matrixH[n, n];
                    n--;
                    iter = 0;
                }
                else
                {
                    // Form shift
                    Complex s;
                    if (iter != 10 && iter != 20)
                    {
                        s = matrixH[n, n];
                        x = matrixH[n - 1, n] * matrixH[n, n - 1].Real;

                        if (x.Real != 0.0 || x.Imaginary != 0.0)
                        {
                            y = (matrixH[n - 1, n - 1] - s) / 2.0;
                            z = ((y * y) + x).SquareRoot();
                            if ((y.Real * z.Real) + (y.Imaginary * z.Imaginary) < 0.0)
                            {
                                z *= -1.0;
                            }

                            x /= y + z; 
                            s = s - x;
                        }
                    }
                    else
                    {
                        // Form exceptional shift
                        s = Math.Abs(matrixH[n, n - 1].Real) + Math.Abs(matrixH[n - 1, n - 2].Real);
                    }

                    for (var i = 0; i <= n; i++)
                    {
                        matrixH[i, i] -= s;
                    }

                    exshift += s;
                    iter++;

                    // Reduce to triangle (rows)
                    for (var i = l + 1; i <= n; i++)
                    {
                        s = matrixH[i, i - 1].Real;
                        norm = SpecialFunctions.Hypotenuse(matrixH[i - 1, i - 1].Magnitude, s.Real);
                        x = matrixH[i - 1, i - 1] / norm;
                        VectorEv[i - 1] = x;
                        matrixH[i - 1, i - 1] = norm;
                        matrixH[i, i - 1] = new Complex(0.0, s.Real / norm);

                        for (var j = i; j < order; j++)
                        {
                            y = matrixH[i - 1, j];
                            z = matrixH[i, j];
                            matrixH[i - 1, j] = (x.Conjugate() * y) + (matrixH[i, i - 1].Imaginary * z);
                            matrixH[i, j] = (x * z) - (matrixH[i, i - 1].Imaginary * y);
                        }
                    }

                    s = matrixH[n, n];
                    if (s.Imaginary != 0.0)
                    {
                        s /= matrixH[n, n].Magnitude;
                        matrixH[n, n] = matrixH[n, n].Magnitude;

                        for (var j = n + 1; j < order; j++)
                        {
                            matrixH[n, j] *= s.Conjugate();
                        }
                    }

                    // Inverse operation (columns).
                    for (var j = l + 1; j <= n; j++)
                    {
                        x = VectorEv[j - 1];
                        for (var i = 0; i <= j; i++)
                        {
                            z = matrixH[i, j];
                            if (i != j)
                            {
                                y = matrixH[i, j - 1];
                                matrixH[i, j - 1] = (x * y) + (matrixH[j, j - 1].Imaginary * z);
                            }
                            else
                            {
                                y = matrixH[i, j - 1].Real;
                                matrixH[i, j - 1] = new Complex((x.Real * y.Real) - (x.Imaginary * y.Imaginary) + (matrixH[j, j - 1].Imaginary * z.Real), matrixH[i, j - 1].Imaginary);
                            }

                            matrixH[i, j] = (x.Conjugate() * z) - (matrixH[j, j - 1].Imaginary * y);
                        }

                        for (var i = 0; i < order; i++)
                        {
                            y = MatrixEv.At(i, j - 1);
                            z = MatrixEv.At(i, j);
                            MatrixEv.At(i, j - 1, (x * y) + (matrixH[j, j - 1].Imaginary * z));
                            MatrixEv.At(i, j, (x.Conjugate() * z) - (matrixH[j, j - 1].Imaginary * y));
                        }
                    }

                    if (s.Imaginary != 0.0)
                    {
                        for (var i = 0; i <= n; i++)
                        {
                            matrixH[i, n] *= s;
                        }

                        for (var i = 0; i < order; i++)
                        {
                            MatrixEv.At(i, n, MatrixEv.At(i, n) * s);
                        }
                    }
                }
            }

            // All roots found.  
            // Backsubstitute to find vectors of upper triangular form
            norm = 0.0;
            for (var i = 0; i < order; i++)
            {
                for (var j = i; j < order; j++)
                {
                    norm = Math.Max(norm, Math.Abs(matrixH[i, j].Real) + Math.Abs(matrixH[i, j].Imaginary));
                }
            }

            if (order == 1)
            {
                return;
            }

            if (norm == 0.0)
            {
                return;
            }

            for (n = order - 1; n > 0; n--)
            {
                x = VectorEv[n];
                matrixH[n, n] = 1.0;

                for (var i = n - 1; i >= 0; i--)
                {
                    z = 0.0;
                    for (var j = i + 1; j <= n; j++)
                    {
                        z += matrixH[i, j] * matrixH[j, n];
                    }

                    y = x - VectorEv[i];
                    if (y.Real == 0.0 && y.Imaginary == 0.0)
                    {
                        y = eps * norm;
                    }

                    matrixH[i, n] = z / y;

                    // Overflow control
                    var tr = Math.Abs(matrixH[i, n].Real) + Math.Abs(matrixH[i, n].Imaginary);
                    if ((eps * tr) * tr > 1)
                    {
                        for (var j = i; j <= n; j++)
                        {
                            matrixH[j, n] = matrixH[j, n] / tr;
                        }
                    }
                }
            }

            // Back transformation to get eigenvectors of original matrix
            for (var j = order - 1; j > 0; j--)
            {
                for (var i = 0; i < order; i++)
                {
                    z = Complex.Zero;
                    for (var k = 0; k <= j; k++)
                    {
                        z += MatrixEv.At(i, k) * matrixH[k, j];
                    }

                    MatrixEv.At(i, j, z);
                }
            }
        }
        
        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A SVD factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</param>
        public override void Solve(Matrix<Complex> input, Matrix<Complex> result)
        {
            // Check for proper arguments.
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            // The solution X should have the same number of columns as B
            if (input.ColumnCount != result.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension);
            }

            // The dimension compatibility conditions for X = A\B require the two matrices A and B to have the same number of rows
            if (VectorEv.Count != input.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension);
            }

            // The solution X row dimension is equal to the column dimension of A
            if (VectorEv.Count != result.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension);
            }

            if (IsSymmetric)
            {
                var order = VectorEv.Count;
                var tmp = new Complex[order];

                for (var k = 0; k < order; k++)
                {
                    for (var j = 0; j < order; j++)
                    {
                        Complex value = 0.0;
                        if (j < order)
                        {
                            for (var i = 0; i < order; i++)
                            {
                                value += MatrixEv.At(i, j).Conjugate() * input.At(i, k);
                            }

                            value /= VectorEv[j].Real;
                        }

                        tmp[j] = value;
                    }

                    for (var j = 0; j < order; j++)
                    {
                        Complex value = 0.0;
                        for (var i = 0; i < order; i++)
                        {
                            value += MatrixEv.At(j, i) * tmp[i];
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
        public override void Solve(Vector<Complex> input, Vector<Complex> result)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            // Ax=b where A is an m x m matrix
            // Check that b is a column vector with m entries
            if (VectorEv.Count != input.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            // Check that x is a column vector with n entries
            if (VectorEv.Count != result.Count)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(VectorEv, result);
            }

            if (IsSymmetric)
            {
                // Symmetric case -> x = V * inv(λ) * VH * b;
                var order = VectorEv.Count;
                var tmp = new Complex[order];
                Complex value;

                for (var j = 0; j < order; j++)
                {
                    value = 0;
                    if (j < order)
                    {
                        for (var i = 0; i < order; i++)
                        {
                            value += MatrixEv.At(i, j).Conjugate() * input[i];
                        }

                        value /= VectorEv[j].Real;
                    }

                    tmp[j] = value;
                }

                for (var j = 0; j < order; j++)
                {
                    value = 0;
                    for (int i = 0; i < order; i++)
                    {
                        value += MatrixEv.At(j, i) * tmp[i];
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