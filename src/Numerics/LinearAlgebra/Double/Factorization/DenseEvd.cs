// <copyright file="DenseEvd.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
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

namespace MathNet.Numerics.LinearAlgebra.Double.Factorization
{
    using System;
    using System.Numerics;
    using Generic;
    using Properties;

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
    public class DenseEvd : Evd
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DenseEvd"/> class. This object will compute the
        /// the eigenvalue decomposition when the constructor is called and cache it's decomposition.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If EVD algorithm failed to converge with matrix <paramref name="matrix"/>.</exception>
        public DenseEvd(DenseMatrix matrix)
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

            // Initialize matrices for eigenvalues and eigenvectors
            MatrixEv = matrix.CreateMatrix(order, order);
            MatrixD = matrix.CreateMatrix(order, order);
            VectorEv = new LinearAlgebra.Complex.DenseVector(order);

            IsSymmetric = true;

            for (var i = 0; IsSymmetric && i < order; i++)
            {
                for (var j = 0; IsSymmetric && j < order; j++)
                {
                    IsSymmetric &= matrix.At(i, j) == matrix.At(j, i);
                }
            }

            Control.LinearAlgebraProvider.EigenDecomp(IsSymmetric, order, matrix.Values, ((DenseMatrix) MatrixEv).Values,
                ((LinearAlgebra.Complex.DenseVector)VectorEv).Values, ((DenseMatrix)MatrixD).Values);
        }

        /// <summary>
        /// Symmetric Householder reduction to tridiagonal form.
        /// </summary>
        /// <param name="a">Data array of matrix V (eigenvectors)</param>
        /// <param name="d">Arrays for internal storage of real parts of eigenvalues</param>
        /// <param name="e">Arrays for internal storage of imaginary parts of eigenvalues</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedures tred2 by 
        /// Bowdler, Martin, Reinsch, and Wilkinson, Handbook for 
        /// Auto. Comp., Vol.ii-Linear Algebra, and the corresponding 
        /// Fortran subroutine in EISPACK.</remarks>
        internal static void SymmetricTridiagonalize(double[] a, double[] d, double[] e, int order)
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
                        d[j] = a[(j*order) + i - 1];
                        a[(j*order) + i] = 0.0;
                        a[(i*order) + j] = 0.0;
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
                        a[(i*order) + j] = f;
                        g = e[j] + (a[(j*order) + j]*f);

                        for (var k = j + 1; k <= i - 1; k++)
                        {
                            g += a[(j*order) + k]*d[k];
                            e[k] += a[(j*order) + k]*f;
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
                            a[(j*order) + k] -= (f*e[k]) + (g*d[k]);
                        }

                        d[j] = a[(j*order) + i - 1];
                        a[(j*order) + i] = 0.0;
                    }
                }

                d[i] = h;
            }

            // Accumulate transformations.
            for (var i = 0; i < order - 1; i++)
            {
                a[(i*order) + order - 1] = a[(i*order) + i];
                a[(i*order) + i] = 1.0;
                var h = d[i + 1];
                if (h != 0.0)
                {
                    for (var k = 0; k <= i; k++)
                    {
                        d[k] = a[((i + 1)*order) + k]/h;
                    }

                    for (var j = 0; j <= i; j++)
                    {
                        var g = 0.0;
                        for (var k = 0; k <= i; k++)
                        {
                            g += a[((i + 1)*order) + k]*a[(j*order) + k];
                        }

                        for (var k = 0; k <= i; k++)
                        {
                            a[(j*order) + k] -= g*d[k];
                        }
                    }
                }

                for (var k = 0; k <= i; k++)
                {
                    a[((i + 1)*order) + k] = 0.0;
                }
            }

            for (var j = 0; j < order; j++)
            {
                d[j] = a[(j*order) + order - 1];
                a[(j*order) + order - 1] = 0.0;
            }

            a[(order*order) - 1] = 1.0;
            e[0] = 0.0;
        }

        /// <summary>
        /// Symmetric tridiagonal QL algorithm.
        /// </summary>
        /// <param name="a">Data array of matrix V (eigenvectors)</param>
        /// <param name="d">Arrays for internal storage of real parts of eigenvalues</param>
        /// <param name="e">Arrays for internal storage of imaginary parts of eigenvalues</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedures tql2, by
        /// Bowdler, Martin, Reinsch, and Wilkinson, Handbook for
        /// Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutine in EISPACK.</remarks>
        /// <exception cref="NonConvergenceException"></exception>
        internal static void SymmetricDiagonalize(double[] a, double[] d, double[] e, int order)
        {
            const int maxiter = 1000;

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
                                h = a[((i + 1)*order) + k];
                                a[((i + 1)*order) + k] = (s*a[(i*order) + k]) + (c*h);
                                a[(i*order) + k] = (c*a[(i*order) + k]) - (s*h);
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
                        p = a[(i*order) + j];
                        a[(i*order) + j] = a[(k*order) + j];
                        a[(k*order) + j] = p;
                    }
                }
            }
        }

        /// <summary>
        /// Nonsymmetric reduction to Hessenberg form.
        /// </summary>
        /// <param name="a">Data array of matrix V (eigenvectors)</param>
        /// <param name="matrixH">Array for internal storage of nonsymmetric Hessenberg form.</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedures orthes and ortran,
        /// by Martin and Wilkinson, Handbook for Auto. Comp.,
        /// Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutines in EISPACK.</remarks>
        internal static void NonsymmetricReduceToHessenberg(double[] a, double[] matrixH, int order)
        {
            var ort = new double[order];
            var high = order - 1;
            for (var m = 1; m <= high - 1; m++)
            {
                var mm1 = m - 1;
                var mm1O = mm1*order;
                // Scale column.
                var scale = 0.0;
                for (var i = m; i <= high; i++)
                {
                    scale += Math.Abs(matrixH[mm1O + i]);
                }

                if (scale != 0.0)
                {
                    // Compute Householder transformation.
                    var h = 0.0;
                    for (var i = high; i >= m; i--)
                    {
                        ort[i] = matrixH[mm1O + i]/scale;
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
                        var jO = j*order;
                        var f = 0.0;
                        for (var i = order - 1; i >= m; i--)
                        {
                            f += ort[i]*matrixH[jO + i];
                        }

                        f = f/h;

                        for (var i = m; i <= high; i++)
                        {
                            matrixH[jO + i] -= f*ort[i];
                        }
                    }

                    for (var i = 0; i <= high; i++)
                    {
                        var f = 0.0;
                        for (var j = high; j >= m; j--)
                        {
                            f += ort[j]*matrixH[j*order + i];
                        }
                        f = f/h;

                        for (var j = m; j <= high; j++)
                        {
                            matrixH[j*order + i] -= f*ort[j];
                        }
                    }

                    ort[m] = scale*ort[m];
                    matrixH[mm1O + m] = scale*g;
                }
            }

            // Accumulate transformations (Algol's ortran).
            for (var i = 0; i < order; i++)
            {
                for (var j = 0; j < order; j++)
                {
                    a[(j*order) + i] = i == j ? 1.0 : 0.0;
                }
            }

            for (var m = high - 1; m >= 1; m--)
            {
                var mm1 = m - 1;
                var mm1O = mm1*order;
                var mm1Om = mm1O + m;
                if (matrixH[mm1Om] != 0.0)
                {
                    for (var i = m + 1; i <= high; i++)
                    {
                        ort[i] = matrixH[mm1O + i];
                    }

                    for (var j = m; j <= high; j++)
                    {
                        var g = 0.0;
                        var jO = j*order;
                        for (var i = m; i <= high; i++)
                        {
                            g += ort[i]*a[jO + i];
                        }

                        // Double division avoids possible underflow
                        g = (g/ort[m])/matrixH[mm1Om];

                        for (var i = m; i <= high; i++)
                        {
                            a[jO + i] += g*ort[i];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Nonsymmetric reduction from Hessenberg to real Schur form.
        /// </summary>
        /// <param name="a">Data array of matrix V (eigenvectors)</param>
        /// <param name="matrixH">Array for internal storage of nonsymmetric Hessenberg form.</param>
        /// <param name="d">Arrays for internal storage of real parts of eigenvalues</param>
        /// <param name="e">Arrays for internal storage of imaginary parts of eigenvalues</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedure hqr2,
        /// by Martin and Wilkinson, Handbook for Auto. Comp.,
        /// Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutine in EISPACK.</remarks>
        /// <exception cref="NonConvergenceException"></exception>
        internal static void NonsymmetricReduceHessenberToRealSchur(double[] a, double[] matrixH, double[] d, double[] e, int order)
        {
            // Initialize
            var n = order - 1;
            var eps = Math.Pow(2.0, -52.0);
            var exshift = 0.0;
            double p = 0, q = 0, r = 0, s = 0, z = 0;
            double w, x, y;

            // Store roots isolated by balanc and compute matrix norm
            var norm = 0.0;
            for (var i = 0; i < order; i++)
            {
                for (var j = Math.Max(i - 1, 0); j < order; j++)
                {
                    norm = norm + Math.Abs(matrixH[j*order + i]);
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
                    var lm1 = l - 1;
                    var lm1O = lm1*order;
                    s = Math.Abs(matrixH[lm1O + lm1]) + Math.Abs(matrixH[l*order + l]);

                    if (s == 0.0)
                    {
                        s = norm;
                    }

                    if (Math.Abs(matrixH[lm1O + l]) < eps*s)
                    {
                        break;
                    }

                    l--;
                }

                // Check for convergence
                // One root found
                if (l == n)
                {
                    var index = n*order + n;
                    matrixH[index] += exshift;
                    d[n] = matrixH[index];
                    e[n] = 0.0;
                    n--;
                    iter = 0;

                    // Two roots found
                }
                else if (l == n - 1)
                {
                    var nO = n*order;
                    var nm1 = n - 1;
                    var nm1O = nm1*order;
                    var nOn = nO + n;

                    w = matrixH[nm1O + n]*matrixH[nO + nm1];
                    p = (matrixH[nm1O + nm1] - matrixH[nOn])/2.0;
                    q = (p*p) + w;
                    z = Math.Sqrt(Math.Abs(q));

                    matrixH[nOn] += exshift;
                    matrixH[nm1O + nm1] += exshift;
                    x = matrixH[nOn];

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

                        d[nm1] = x + z;

                        d[n] = d[nm1];
                        if (z != 0.0)
                        {
                            d[n] = x - (w/z);
                        }

                        e[n - 1] = 0.0;
                        e[n] = 0.0;
                        x = matrixH[nm1O + n];
                        s = Math.Abs(x) + Math.Abs(z);
                        p = x/s;
                        q = z/s;
                        r = Math.Sqrt((p*p) + (q*q));
                        p = p/r;
                        q = q/r;

                        // Row modification
                        for (var j = n - 1; j < order; j++)
                        {
                            var jO = j*order;
                            var jOn = jO + n;
                            z = matrixH[jO + nm1];
                            matrixH[jO + nm1] = (q*z) + (p*matrixH[jOn]);
                            matrixH[jOn] = (q*matrixH[jOn]) - (p*z);
                        }

                        // Column modification
                        for (var i = 0; i <= n; i++)
                        {
                            var nOi = nO + i;
                            z = matrixH[nm1O + i];
                            matrixH[nm1O + i] = (q*z) + (p*matrixH[nOi]);
                            matrixH[nOi] = (q*matrixH[nOi]) - (p*z);
                        }

                        // Accumulate transformations
                        for (var i = 0; i < order; i++)
                        {
                            var nOi = nO + i;
                            z = a[nm1O + i];
                            a[nm1O + i] = (q*z) + (p*a[nOi]);
                            a[nOi] = (q*a[nOi]) - (p*z);
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
                    var nO = n*order;
                    var nm1 = n - 1;
                    var nm1O = nm1*order;
                    var nOn = nO + n;

                    // Form shift
                    x = matrixH[nOn];
                    y = 0.0;
                    w = 0.0;
                    if (l < n)
                    {
                        y = matrixH[nm1O + nm1];
                        w = matrixH[nm1O + n]*matrixH[nO + nm1];
                    }

                    // Wilkinson's original ad hoc shift
                    if (iter == 10)
                    {
                        exshift += x;
                        for (var i = 0; i <= n; i++)
                        {
                            matrixH[i*order + i] -= x;
                        }

                        s = Math.Abs(matrixH[nm1O + n]) + Math.Abs(matrixH[(n - 2)*order + nm1]);
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
                                matrixH[i*order + i] -= s;
                            }

                            exshift += s;
                            x = y = w = 0.964;
                        }
                    }

                    iter = iter + 1;
                    if (iter >= 30*order)
                    {
                        throw new NonConvergenceException();
                    }

                    // Look for two consecutive small sub-diagonal elements
                    var m = n - 2;
                    while (m >= l)
                    {
                        var mp1 = m + 1;
                        var mm1 = m - 1;
                        var mO = m*order;
                        var mp1O = mp1*order;
                        var mm1O = mm1*order;

                        z = matrixH[mO + m];
                        r = x - z;
                        s = y - z;
                        p = (((r*s) - w)/matrixH[mO + mp1]) + matrixH[mp1O + m];
                        q = matrixH[mp1O + mp1] - z - r - s;
                        r = matrixH[mp1O + (m + 2)];
                        s = Math.Abs(p) + Math.Abs(q) + Math.Abs(r);
                        p = p/s;
                        q = q/s;
                        r = r/s;

                        if (m == l)
                        {
                            break;
                        }

                        if (Math.Abs(matrixH[mm1O + m])*(Math.Abs(q) + Math.Abs(r)) < eps*(Math.Abs(p)*(Math.Abs(matrixH[mm1O + mm1]) + Math.Abs(z) + Math.Abs(matrixH[mp1O + mp1]))))
                        {
                            break;
                        }

                        m--;
                    }

                    var mp2 = m + 2;
                    for (var i = mp2; i <= n; i++)
                    {
                        matrixH[(i - 2)*order + i] = 0.0;
                        if (i > mp2)
                        {
                            matrixH[(i - 3)*order + i] = 0.0;
                        }
                    }

                    // Double QR step involving rows l:n and columns m:n
                    for (var k = m; k <= n - 1; k++)
                    {
                        var notlast = k != n - 1;
                        var kO = k*order;
                        var km1 = k - 1;
                        var kp1 = k + 1;
                        var kp2 = k + 2;
                        var kp1O = kp1*order;
                        var kp2O = kp2*order;
                        var km1O = km1*order;
                        if (k != m)
                        {
                            p = matrixH[km1O + k];
                            q = matrixH[km1O + kp1];
                            r = notlast ? matrixH[km1O + kp2] : 0.0;
                            x = Math.Abs(p) + Math.Abs(q) + Math.Abs(r);
                            if (x == 0.0)
                            {
                                continue;
                            }

                            p = p/x;
                            q = q/x;
                            r = r/x;
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
                                matrixH[km1O + k] = (-s)*x;
                            }
                            else if (l != m)
                            {
                                matrixH[km1O + k] = -matrixH[km1O + k];
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
                                var jO = j*order;
                                var jOk = jO + k;
                                var jOkp1 = jO + kp1;
                                var jOkp2 = jO + kp2;
                                p = matrixH[jOk] + (q*matrixH[jOkp1]);
                                if (notlast)
                                {
                                    p = p + (r*matrixH[jOkp2]);
                                    matrixH[jOkp2] -= (p*z);
                                }

                                matrixH[jOk] -= (p*x);
                                matrixH[jOkp1] -= (p*y);
                            }

                            // Column modification
                            for (var i = 0; i <= Math.Min(n, k + 3); i++)
                            {
                                p = (x*matrixH[kO + i]) + (y*matrixH[kp1O + i]);

                                if (notlast)
                                {
                                    p = p + (z*matrixH[kp2O + i]);
                                    matrixH[kp2O + i] -= (p*r);
                                }

                                matrixH[kO + i] -= p;
                                matrixH[kp1O + i] -= (p*q);
                            }

                            // Accumulate transformations
                            for (var i = 0; i < order; i++)
                            {
                                p = (x*a[kO + i]) + (y*a[kp1O + i]);

                                if (notlast)
                                {
                                    p = p + (z*a[kp2O + i]);
                                    a[kp2O + i] -= p*r;
                                }

                                a[kO + i] -= p;
                                a[kp1O + i] -= p*q;
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
                var nO = n*order;
                var nm1 = n - 1;
                var nm1O = nm1*order;

                p = d[n];
                q = e[n];


                // Real vector
                double t;
                if (q == 0.0)
                {
                    var l = n;
                    matrixH[nO + n] = 1.0;
                    for (var i = n - 1; i >= 0; i--)
                    {
                        var ip1 = i + 1;
                        var iO = i*order;
                        var ip1O = ip1*order;

                        w = matrixH[iO + i] - p;
                        r = 0.0;
                        for (var j = l; j <= n; j++)
                        {
                            r = r + (matrixH[j*order + i]*matrixH[nO + j]);
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
                                    matrixH[nO + i] = (-r)/w;
                                }
                                else
                                {
                                    matrixH[nO + i] = (-r)/(eps*norm);
                                }

                                // Solve real equations
                            }
                            else
                            {
                                x = matrixH[ip1O + i];
                                y = matrixH[iO + ip1];
                                q = ((d[i] - p)*(d[i] - p)) + (e[i]*e[i]);
                                t = ((x*s) - (z*r))/q;
                                matrixH[nO + i] = t;
                                if (Math.Abs(x) > Math.Abs(z))
                                {
                                    matrixH[nO + ip1] = (-r - (w*t))/x;
                                }
                                else
                                {
                                    matrixH[nO + ip1] = (-s - (y*t))/z;
                                }
                            }

                            // Overflow control
                            t = Math.Abs(matrixH[nO + i]);
                            if ((eps*t)*t > 1)
                            {
                                for (var j = i; j <= n; j++)
                                {
                                    matrixH[nO + j] /= t;
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
                    if (Math.Abs(matrixH[nm1O + n]) > Math.Abs(matrixH[nO + nm1]))
                    {
                        matrixH[nm1O + nm1] = q/matrixH[nm1O + n];
                        matrixH[nO + nm1] = (-(matrixH[nO + n] - p))/matrixH[nm1O + n];
                    }
                    else
                    {
                        var res = Cdiv(0.0, -matrixH[nO + nm1], matrixH[nm1O + nm1] - p, q);
                        matrixH[nm1O + nm1] = res.Real;
                        matrixH[nO + nm1] = res.Imaginary;
                    }

                    matrixH[nm1O + n] = 0.0;
                    matrixH[nO + n] = 1.0;
                    for (var i = n - 2; i >= 0; i--)
                    {
                        var ip1 = i + 1;
                        var iO = i*order;
                        var ip1O = ip1*order;
                        var ra = 0.0;
                        var sa = 0.0;
                        for (var j = l; j <= n; j++)
                        {
                            var jO = j*order;
                            var jOi = jO + i;
                            ra = ra + (matrixH[jOi]*matrixH[nm1O + j]);
                            sa = sa + (matrixH[jOi]*matrixH[nO + j]);
                        }

                        w = matrixH[iO + i] - p;

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
                                matrixH[nm1O + i] = res.Real;
                                matrixH[nO + i] = res.Imaginary;
                            }
                            else
                            {
                                // Solve complex equations
                                x = matrixH[ip1O + i];
                                y = matrixH[iO + ip1];

                                var vr = ((d[i] - p)*(d[i] - p)) + (e[i]*e[i]) - (q*q);
                                var vi = (d[i] - p)*2.0*q;
                                if ((vr == 0.0) && (vi == 0.0))
                                {
                                    vr = eps*norm*(Math.Abs(w) + Math.Abs(q) + Math.Abs(x) + Math.Abs(y) + Math.Abs(z));
                                }

                                var res = Cdiv((x*r) - (z*ra) + (q*sa), (x*s) - (z*sa) - (q*ra), vr, vi);
                                matrixH[nm1O + i] = res.Real;
                                matrixH[nO + i] = res.Imaginary;
                                if (Math.Abs(x) > (Math.Abs(z) + Math.Abs(q)))
                                {
                                    matrixH[nm1O + ip1] = (-ra - (w*matrixH[nm1O + i]) + (q*matrixH[nO + i]))/x;
                                    matrixH[nO + ip1] = (-sa - (w*matrixH[nO + i]) - (q*matrixH[nm1O + i]))/x;
                                }
                                else
                                {
                                    res = Cdiv(-r - (y*matrixH[nm1O + i]), -s - (y*matrixH[nO + i]), z, q);
                                    matrixH[nm1O + ip1] = res.Real;
                                    matrixH[nO + ip1] = res.Imaginary;
                                }
                            }

                            // Overflow control
                            t = Math.Max(Math.Abs(matrixH[nm1O + i]), Math.Abs(matrixH[nO + i]));
                            if ((eps*t)*t > 1)
                            {
                                for (var j = i; j <= n; j++)
                                {
                                    matrixH[nm1O + j] /= t;
                                    matrixH[nO + j] /= t;
                                }
                            }
                        }
                    }
                }
            }

            // Back transformation to get eigenvectors of original matrix
            for (var j = order - 1; j >= 0; j--)
            {
                var jO = j*order;
                for (var i = 0; i < order; i++)
                {
                    z = 0.0;
                    for (var k = 0; k <= j; k++)
                    {
                        z = z + (a[k*order + i]*matrixH[jO + k]);
                    }

                    a[jO + i] = z;
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
        static System.Numerics.Complex Cdiv(double xreal, double ximag, double yreal, double yimag)
        {
            if (Math.Abs(yimag) < Math.Abs(yreal))
            {
                return new System.Numerics.Complex((xreal + (ximag*(yimag/yreal)))/(yreal + (yimag*(yimag/yreal))), (ximag - (xreal*(yimag/yreal)))/(yreal + (yimag*(yimag/yreal))));
            }

            return new System.Numerics.Complex((ximag + (xreal*(yreal/yimag)))/(yimag + (yreal*(yreal/yimag))), (-xreal + (ximag*(yreal/yimag)))/(yimag + (yreal*(yreal/yimag))));
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A SVD factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</param>
        public override void Solve(Matrix<double> input, Matrix<double> result)
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
                                value += ((DenseMatrix) MatrixEv).Values[(j*order) + i]*input.At(i, k);
                            }

                            value /= VectorEv[j].Real;
                        }

                        tmp[j] = value;
                    }

                    for (var j = 0; j < order; j++)
                    {
                        double value = 0;
                        for (var i = 0; i < order; i++)
                        {
                            value += ((DenseMatrix) MatrixEv).Values[(i*order) + j]*tmp[i];
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
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            if (IsSymmetric)
            {
                // Symmetric case -> x = V * inv(λ) * VT * b;
                var order = VectorEv.Count;
                var tmp = new double[order];
                double value;

                for (var j = 0; j < order; j++)
                {
                    value = 0;
                    if (j < order)
                    {
                        for (var i = 0; i < order; i++)
                        {
                            value += ((DenseMatrix) MatrixEv).Values[(j*order) + i]*input[i];
                        }

                        value /= VectorEv[j].Real;
                    }

                    tmp[j] = value;
                }

                for (var j = 0; j < order; j++)
                {
                    value = 0;
                    for (var i = 0; i < order; i++)
                    {
                        value += ((DenseMatrix) MatrixEv).Values[(i*order) + j]*tmp[i];
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
