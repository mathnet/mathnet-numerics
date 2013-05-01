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


namespace MathNet.Numerics.LinearAlgebra.Complex32.Factorization
{
    using System;
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
            MatrixEv = DenseMatrix.Identity(order);
            MatrixD = matrix.CreateMatrix(order, order);
            VectorEv = new Complex.DenseVector(order);

            IsSymmetric = true;

            for (var i = 0; IsSymmetric && i < order; i++)
            {
                for (var j = 0; IsSymmetric && j < order; j++)
                {
                    IsSymmetric &= matrix.At(i, j) == matrix.At(j, i).Conjugate();
                }
            }

            Control.LinearAlgebraProvider.EigenDecomp(IsSymmetric, order, matrix.Values, ((DenseMatrix) MatrixEv).Values,
                ((Complex.DenseVector) VectorEv).Values, ((DenseMatrix) MatrixD).Values);
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
        internal static void SymmetricTridiagonalize(Numerics.Complex32[] matrixA, float[] d, float[] e, Numerics.Complex32[] tau, int order)
        {
            float hh;
            tau[order - 1] = Numerics.Complex32.One;

            for (var i = 0; i < order; i++)
            {
                d[i] = matrixA[i*order + i].Real;
            }

            // Householder reduction to tridiagonal form.
            for (var i = order - 1; i > 0; i--)
            {
                // Scale to avoid under/overflow.
                var scale = 0.0f;
                var h = 0.0f;

                for (var k = 0; k < i; k++)
                {
                    scale = scale + Math.Abs(matrixA[k*order + i].Real) + Math.Abs(matrixA[k*order + i].Imaginary);
                }

                if (scale == 0.0f)
                {
                    tau[i - 1] = Numerics.Complex32.One;
                    e[i] = 0.0f;
                }
                else
                {
                    for (var k = 0; k < i; k++)
                    {
                        matrixA[k*order + i] /= scale;
                        h += matrixA[k*order + i].MagnitudeSquared;
                    }

                    Numerics.Complex32 g = (float) Math.Sqrt(h);
                    e[i] = scale*g.Real;

                    Numerics.Complex32 temp;
                    var im1Oi = (i - 1)*order + i;
                    var f = matrixA[im1Oi];
                    if (f.Magnitude != 0.0f)
                    {
                        temp = -(matrixA[im1Oi].Conjugate()*tau[i].Conjugate())/f.Magnitude;
                        h += f.Magnitude*g.Real;
                        g = 1.0f + (g/f.Magnitude);
                        matrixA[im1Oi] *= g;
                    }
                    else
                    {
                        temp = -tau[i].Conjugate();
                        matrixA[im1Oi] = g;
                    }

                    if ((f.Magnitude == 0.0f) || (i != 1))
                    {
                        f = Numerics.Complex32.Zero;
                        for (var j = 0; j < i; j++)
                        {
                            var tmp = Numerics.Complex32.Zero;
                            var jO = j*order;
                            // Form element of A*U.
                            for (var k = 0; k <= j; k++)
                            {
                                tmp += matrixA[k*order + j]*matrixA[k*order + i].Conjugate();
                            }

                            for (var k = j + 1; k <= i - 1; k++)
                            {
                                tmp += matrixA[jO + k].Conjugate()*matrixA[k*order + i].Conjugate();
                            }

                            // Form element of P
                            tau[j] = tmp/h;
                            f += (tmp/h)*matrixA[jO + i];
                        }

                        hh = f.Real/(h + h);

                        // Form the reduced A.
                        for (var j = 0; j < i; j++)
                        {
                            f = matrixA[j*order + i].Conjugate();
                            g = tau[j] - (hh*f);
                            tau[j] = g.Conjugate();

                            for (var k = 0; k <= j; k++)
                            {
                                matrixA[k*order + j] -= (f*tau[k]) + (g*matrixA[k*order + i]);
                            }
                        }
                    }

                    for (var k = 0; k < i; k++)
                    {
                        matrixA[k*order + i] *= scale;
                    }

                    tau[i - 1] = temp.Conjugate();
                }

                hh = d[i];
                d[i] = matrixA[i*order + i].Real;
                matrixA[i*order + i] = new Numerics.Complex32(hh, scale*(float) Math.Sqrt(h));
            }

            hh = d[0];
            d[0] = matrixA[0].Real;
            matrixA[0] = hh;
            e[0] = 0.0f;
        }

        /// <summary>
        /// Symmetric tridiagonal QL algorithm.
        /// </summary>
        /// <param name="dataEv">Data array of matrix V (eigenvectors)</param>
        /// <param name="d">Arrays for internal storage of real parts of eigenvalues</param>
        /// <param name="e">Arrays for internal storage of imaginary parts of eigenvalues</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedures tql2, by
        /// Bowdler, Martin, Reinsch, and Wilkinson, Handbook for
        /// Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutine in EISPACK.</remarks>
        /// <exception cref="NonConvergenceException"></exception>
        internal static void SymmetricDiagonalize(Numerics.Complex32[] dataEv, float[] d, float[] e, int order)
        {
            const int Maxiter = 1000;

            for (var i = 1; i < order; i++)
            {
                e[i - 1] = e[i];
            }

            e[order - 1] = 0.0f;

            var f = 0.0f;
            var tst1 = 0.0f;
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
                        var p = (d[l + 1] - g)/(2.0f*e[l]);
                        var r = SpecialFunctions.Hypotenuse(p, 1.0f);
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
                        var c = 1.0f;
                        var c2 = c;
                        var c3 = c;
                        var el1 = e[l + 1];
                        var s = 0.0f;
                        var s2 = 0.0f;
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
                                h = dataEv[((i + 1)*order) + k].Real;
                                dataEv[((i + 1)*order) + k] = (s*dataEv[(i*order) + k].Real) + (c*h);
                                dataEv[(i*order) + k] = (c*dataEv[(i*order) + k].Real) - (s*h);
                            }
                        }

                        p = (-s)*s2*c3*el1*e[l]/dl1;
                        e[l] = s*p;
                        d[l] = c*p;

                        // Check for convergence. If too many iterations have been performed, 
                        // throw exception that Convergence Failed
                        if (iter >= Maxiter)
                        {
                            throw new NonConvergenceException();
                        }
                    } while (Math.Abs(e[l]) > eps*tst1);
                }

                d[l] = d[l] + f;
                e[l] = 0.0f;
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
                        p = dataEv[(i*order) + j].Real;
                        dataEv[(i*order) + j] = dataEv[(k*order) + j];
                        dataEv[(k*order) + j] = p;
                    }
                }
            }
        }

        /// <summary>
        /// Determines eigenvectors by undoing the symmetric tridiagonalize transformation
        /// </summary>
        /// <param name="dataEv">Data array of matrix V (eigenvectors)</param>
        /// <param name="matrixA">Previously tridiagonalized matrix by <see cref="SymmetricTridiagonalize"/>.</param>
        /// <param name="tau">Contains further information about the transformations</param>
        /// <param name="order">Input matrix order</param>
        /// <remarks>This is derived from the Algol procedures HTRIBK, by
        /// by Smith, Boyle, Dongarra, Garbow, Ikebe, Klema, Moler, and Wilkinson, Handbook for
        /// Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutine in EISPACK.</remarks>
        internal static void SymmetricUntridiagonalize(Numerics.Complex32[] dataEv, Numerics.Complex32[] matrixA, Numerics.Complex32[] tau, int order)
        {
            for (var i = 0; i < order; i++)
            {
                for (var j = 0; j < order; j++)
                {
                    dataEv[(j*order) + i] = dataEv[(j*order) + i].Real*tau[i].Conjugate();
                }
            }

            // Recover and apply the Householder matrices.
            for (var i = 1; i < order; i++)
            {
                var h = matrixA[i*order + i].Imaginary;
                if (h != 0)
                {
                    for (var j = 0; j < order; j++)
                    {
                        var s = Numerics.Complex32.Zero;
                        for (var k = 0; k < i; k++)
                        {
                            s += dataEv[(j*order) + k]*matrixA[k*order + i];
                        }

                        s = (s/h)/h;

                        for (var k = 0; k < i; k++)
                        {
                            dataEv[(j*order) + k] -= s*matrixA[k*order + i].Conjugate();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Nonsymmetric reduction to Hessenberg form.
        /// </summary>
        /// <param name="dataEv">Data array of matrix V (eigenvectors)</param>
        /// <param name="matrixH">Array for internal storage of nonsymmetric Hessenberg form.</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedures orthes and ortran,
        /// by Martin and Wilkinson, Handbook for Auto. Comp.,
        /// Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutines in EISPACK.</remarks>
        internal static void NonsymmetricReduceToHessenberg(Numerics.Complex32[] dataEv, Numerics.Complex32[] matrixH, int order)
        {
            var ort = new Numerics.Complex32[order];

            for (var m = 1; m < order - 1; m++)
            {
                // Scale column.
                var scale = 0.0f;
                var mm1O = (m - 1)*order;
                for (var i = m; i < order; i++)
                {
                    scale += Math.Abs(matrixH[mm1O + i].Real) + Math.Abs(matrixH[mm1O + i].Imaginary);
                }

                if (scale != 0.0f)
                {
                    // Compute Householder transformation.
                    var h = 0.0f;
                    for (var i = order - 1; i >= m; i--)
                    {
                        ort[i] = matrixH[mm1O + i]/scale;
                        h += ort[i].MagnitudeSquared;
                    }

                    var g = (float) Math.Sqrt(h);
                    if (ort[m].Magnitude != 0)
                    {
                        h = h + (ort[m].Magnitude*g);
                        g /= ort[m].Magnitude;
                        ort[m] = (1.0f + g)*ort[m];
                    }
                    else
                    {
                        ort[m] = g;
                        matrixH[mm1O + m] = scale;
                    }

                    // Apply Householder similarity transformation
                    // H = (I-u*u'/h)*H*(I-u*u')/h)
                    for (var j = m; j < order; j++)
                    {
                        var f = Numerics.Complex32.Zero;
                        var jO = j*order;
                        for (var i = order - 1; i >= m; i--)
                        {
                            f += ort[i].Conjugate()*matrixH[jO + i];
                        }

                        f = f/h;
                        for (var i = m; i < order; i++)
                        {
                            matrixH[jO + i] -= f*ort[i];
                        }
                    }

                    for (var i = 0; i < order; i++)
                    {
                        var f = Numerics.Complex32.Zero;
                        for (var j = order - 1; j >= m; j--)
                        {
                            f += ort[j]*matrixH[j*order + i];
                        }

                        f = f/h;
                        for (var j = m; j < order; j++)
                        {
                            matrixH[j*order + i] -= f*ort[j].Conjugate();
                        }
                    }

                    ort[m] = scale*ort[m];
                    matrixH[mm1O + m] *= -g;
                }
            }

            // Accumulate transformations (Algol's ortran).
            for (var i = 0; i < order; i++)
            {
                for (var j = 0; j < order; j++)
                {
                    dataEv[(j*order) + i] = i == j ? Numerics.Complex32.One : Numerics.Complex32.Zero;
                }
            }

            for (var m = order - 2; m >= 1; m--)
            {
                var mm1O = (m - 1)*order;
                var mm1Om = mm1O + m;
                if (matrixH[mm1Om] != Numerics.Complex32.Zero && ort[m] != Numerics.Complex32.Zero)
                {
                    var norm = (matrixH[mm1Om].Real*ort[m].Real) + (matrixH[mm1Om].Imaginary*ort[m].Imaginary);

                    for (var i = m + 1; i < order; i++)
                    {
                        ort[i] = matrixH[mm1O + i];
                    }

                    for (var j = m; j < order; j++)
                    {
                        var g = Numerics.Complex32.Zero;
                        for (var i = m; i < order; i++)
                        {
                            g += ort[i].Conjugate()*dataEv[(j*order) + i];
                        }

                        // Double division avoids possible underflow
                        g /= norm;
                        for (var i = m; i < order; i++)
                        {
                            dataEv[(j*order) + i] += g*ort[i];
                        }
                    }
                }
            }

            // Create real subdiagonal elements.
            for (var i = 1; i < order; i++)
            {
                var im1 = i - 1;
                var im1O = im1*order;
                var im1Oi = im1O + i;
                var iO = i*order;
                if (matrixH[im1Oi].Imaginary != 0.0f)
                {
                    var y = matrixH[im1Oi]/matrixH[im1Oi].Magnitude;
                    matrixH[im1Oi] = matrixH[im1Oi].Magnitude;
                    for (var j = i; j < order; j++)
                    {
                        matrixH[j*order + i] *= y.Conjugate();
                    }

                    for (var j = 0; j <= Math.Min(i + 1, order - 1); j++)
                    {
                        matrixH[iO + j] *= y;
                    }

                    for (var j = 0; j < order; j++)
                    {
                        dataEv[(i*order) + j] *= y;
                    }
                }
            }
        }

        /// <summary>
        /// Nonsymmetric reduction from Hessenberg to real Schur form.
        /// </summary>
        /// <param name="vectorV">Data array of the eigenvectors</param>
        /// <param name="dataEv">Data array of matrix V (eigenvectors)</param>
        /// <param name="matrixH">Array for internal storage of nonsymmetric Hessenberg form.</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedure hqr2,
        /// by Martin and Wilkinson, Handbook for Auto. Comp.,
        /// Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutine in EISPACK.</remarks>
        internal static void NonsymmetricReduceHessenberToRealSchur(Numerics.Complex32[] vectorV, Numerics.Complex32[] dataEv, Numerics.Complex32[] matrixH, int order)
        {
            // Initialize
            var n = order - 1;
            var eps = (float) Precision.SingleMachinePrecision;

            float norm;
            Numerics.Complex32 x, y, z, exshift = Numerics.Complex32.Zero;

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
                    var lO = l*order;
                    var tst1 = Math.Abs(matrixH[lm1O + lm1].Real) + Math.Abs(matrixH[lm1O + lm1].Imaginary) + Math.Abs(matrixH[lO + l].Real) + Math.Abs(matrixH[lO + l].Imaginary);
                    if (Math.Abs(matrixH[lm1O + l].Real) < eps*tst1)
                    {
                        break;
                    }

                    l--;
                }

                var nm1 = n - 1;
                var nm1O = nm1*order;
                var nO = n*order;
                var nOn = nO + n;
                // Check for convergence
                // One root found
                if (l == n)
                {
                    matrixH[nOn] += exshift;
                    vectorV[n] = matrixH[nOn];
                    n--;
                    iter = 0;
                }
                else
                {
                    // Form shift
                    Numerics.Complex32 s;
                    if (iter != 10 && iter != 20)
                    {
                        s = matrixH[nOn];
                        x = matrixH[nO + nm1]*matrixH[nm1O + n].Real;

                        if (x.Real != 0.0f || x.Imaginary != 0.0f)
                        {
                            y = (matrixH[nm1O + nm1] - s)/2.0f;
                            z = ((y*y) + x).SquareRoot();
                            if ((y.Real*z.Real) + (y.Imaginary*z.Imaginary) < 0.0)
                            {
                                z *= -1.0f;
                            }

                            x /= y + z;
                            s = s - x;
                        }
                    }
                    else
                    {
                        // Form exceptional shift
                        s = Math.Abs(matrixH[nm1O + n].Real) + Math.Abs(matrixH[(n - 2)*order + nm1].Real);
                    }

                    for (var i = 0; i <= n; i++)
                    {
                        matrixH[i*order + i] -= s;
                    }

                    exshift += s;
                    iter++;

                    // Reduce to triangle (rows)
                    for (var i = l + 1; i <= n; i++)
                    {
                        var im1 = i - 1;
                        var im1O = im1*order;
                        var im1Oim1 = im1O + im1;
                        s = matrixH[im1O + i].Real;
                        norm = SpecialFunctions.Hypotenuse(matrixH[im1Oim1].Magnitude, s.Real);
                        x = matrixH[im1Oim1]/norm;
                        vectorV[i - 1] = x;
                        matrixH[im1Oim1] = norm;
                        matrixH[im1O + i] = new Numerics.Complex32(0.0f, s.Real/norm);

                        for (var j = i; j < order; j++)
                        {
                            var jO = j*order;
                            y = matrixH[jO + im1];
                            z = matrixH[jO + i];
                            matrixH[jO + im1] = (x.Conjugate()*y) + (matrixH[im1O + i].Imaginary*z);
                            matrixH[jO + i] = (x*z) - (matrixH[im1O + i].Imaginary*y);
                        }
                    }

                    s = matrixH[nOn];
                    if (s.Imaginary != 0.0f)
                    {
                        s /= matrixH[nOn].Magnitude;
                        matrixH[nOn] = matrixH[nOn].Magnitude;

                        for (var j = n + 1; j < order; j++)
                        {
                            matrixH[j*order + n] *= s.Conjugate();
                        }
                    }

                    // Inverse operation (columns).
                    for (var j = l + 1; j <= n; j++)
                    {
                        x = vectorV[j - 1];
                        var jO = j*order;
                        var jm1 = j - 1;
                        var jm1O = jm1*order;
                        var jm1Oj = jm1O + j;
                        for (var i = 0; i <= j; i++)
                        {
                            var jm1Oi = jm1O + i;
                            z = matrixH[jO + i];
                            if (i != j)
                            {
                                y = matrixH[jm1Oi];
                                matrixH[jm1Oi] = (x*y) + (matrixH[jm1O + j].Imaginary*z);
                            }
                            else
                            {
                                y = matrixH[jm1Oi].Real;
                                matrixH[jm1Oi] = new Numerics.Complex32((x.Real*y.Real) - (x.Imaginary*y.Imaginary) + (matrixH[jm1O + j].Imaginary*z.Real), matrixH[jm1Oi].Imaginary);
                            }

                            matrixH[jO + i] = (x.Conjugate()*z) - (matrixH[jm1O + j].Imaginary*y);
                        }

                        for (var i = 0; i < order; i++)
                        {
                            y = dataEv[((j - 1)*order) + i];
                            z = dataEv[(j*order) + i];
                            dataEv[jm1O + i] = (x*y) + (matrixH[jm1Oj].Imaginary*z);
                            dataEv[jO + i] = (x.Conjugate()*z) - (matrixH[jm1Oj].Imaginary*y);
                        }
                    }

                    if (s.Imaginary != 0.0f)
                    {
                        for (var i = 0; i <= n; i++)
                        {
                            matrixH[nO + i] *= s;
                        }

                        for (var i = 0; i < order; i++)
                        {
                            dataEv[nO + i] *= s;
                        }
                    }
                }
            }

            // All roots found.  
            // Backsubstitute to find vectors of upper triangular form
            norm = 0.0f;
            for (var i = 0; i < order; i++)
            {
                for (var j = i; j < order; j++)
                {
                    norm = Math.Max(norm, Math.Abs(matrixH[j*order + i].Real) + Math.Abs(matrixH[j*order + i].Imaginary));
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
                var nO = n*order;
                var nOn = nO + n;
                x = vectorV[n];
                matrixH[nOn] = 1.0f;

                for (var i = n - 1; i >= 0; i--)
                {
                    z = 0.0f;
                    for (var j = i + 1; j <= n; j++)
                    {
                        z += matrixH[j*order + i]*matrixH[nO + j];
                    }

                    y = x - vectorV[i];
                    if (y.Real == 0.0f && y.Imaginary == 0.0f)
                    {
                        y = eps*norm;
                    }

                    matrixH[nO + i] = z/y;

                    // Overflow control
                    var tr = Math.Abs(matrixH[nO + i].Real) + Math.Abs(matrixH[nO + i].Imaginary);
                    if ((eps*tr)*tr > 1)
                    {
                        for (var j = i; j <= n; j++)
                        {
                            matrixH[nO + j] = matrixH[nO + j]/tr;
                        }
                    }
                }
            }

            // Back transformation to get eigenvectors of original matrix
            for (var j = order - 1; j > 0; j--)
            {
                var jO = j*order;
                for (var i = 0; i < order; i++)
                {
                    z = Numerics.Complex32.Zero;
                    for (var k = 0; k <= j; k++)
                    {
                        z += dataEv[(k*order) + i]*matrixH[jO + k];
                    }

                    dataEv[jO + i] = z;
                }
            }
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A SVD factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</param>
        public override void Solve(Matrix<Numerics.Complex32> input, Matrix<Numerics.Complex32> result)
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
                var tmp = new Numerics.Complex32[order];

                for (var k = 0; k < order; k++)
                {
                    for (var j = 0; j < order; j++)
                    {
                        Numerics.Complex32 value = 0.0f;
                        if (j < order)
                        {
                            for (var i = 0; i < order; i++)
                            {
                                value += ((DenseMatrix) MatrixEv).Values[(j*order) + i].Conjugate()*input.At(i, k);
                            }

                            value /= (float) VectorEv[j].Real;
                        }

                        tmp[j] = value;
                    }

                    for (var j = 0; j < order; j++)
                    {
                        Numerics.Complex32 value = 0.0f;
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
        public override void Solve(Vector<Numerics.Complex32> input, Vector<Numerics.Complex32> result)
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
                // Symmetric case -> x = V * inv(λ) * VH * b;
                var order = VectorEv.Count;
                var tmp = new Numerics.Complex32[order];
                Numerics.Complex32 value;

                for (var j = 0; j < order; j++)
                {
                    value = 0;
                    if (j < order)
                    {
                        for (var i = 0; i < order; i++)
                        {
                            value += ((DenseMatrix) MatrixEv).Values[(j*order) + i].Conjugate()*input[i];
                        }

                        value /= (float) VectorEv[j].Real;
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
