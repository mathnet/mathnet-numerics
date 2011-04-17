// <copyright file="SafeNativeMethods.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
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

using System.Numerics;
using System.Runtime.InteropServices;
using System.Security;

namespace MathNet.Numerics.Algorithms.LinearAlgebra.GotoBlas
{
    /// <summary>
    /// P/Invoke methods to the native math libraries.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]
    internal static class SafeNativeMethods
    {
        /// <summary>
        /// Name of the native DLL.
        /// </summary>
        private const string DllName = "MathNET.Numerics.GotoBLAS2.dll";

        #region BLAS
        
        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void s_axpy(int n, float alpha, float[] x, [In, Out] float[] y);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void d_axpy(int n, double alpha, double[] x, [In, Out] double[] y);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void c_axpy(int n, Complex32 alpha, Complex32[] x, [In, Out] Complex32[] y);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void z_axpy(int n, Complex alpha, Complex[] x, [In, Out] Complex[] y);
        
        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void s_scale(int n, float alpha, [Out] float[] x);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void d_scale(int n, double alpha, [Out] double[] x);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void c_scale(int n, Complex32 alpha, [In, Out] Complex32[] x);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void z_scale(int n, Complex alpha, [In, Out] Complex[] x);
        
        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float s_dot_product(int n, float[] x, float[] y);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern double d_dot_product(int n, double[] x, double[] y);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern Complex32 c_dot_product(int n, Complex32[] x, Complex32[] y);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern Complex z_dot_product(int n, Complex[] x, Complex[] y);
        
        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void s_matrix_multiply(Transpose transA, Transpose transB, int m, int n, int k, float alpha, float[] x, float[] y, float beta, [In, Out]float[] c);
        
        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void d_matrix_multiply(Transpose transA, Transpose transB, int m, int n, int k, double alpha, double[] x, double[] y, double beta, [In, Out]double[] c);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void c_matrix_multiply(Transpose transA, Transpose transB, int m, int n, int k, Complex32 alpha, Complex32[] x, Complex32[] y, Complex32 beta, [In, Out]Complex32[] c);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void z_matrix_multiply(Transpose transA, Transpose transB, int m, int n, int k, Complex alpha, Complex[] x, Complex[] y, Complex beta, [In, Out]Complex[] c);

        #endregion BLAS
        
        #region LAPACK

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float s_matrix_norm(byte norm, int rows, int columns, [In] float[] a, [In, Out] float[] work);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float d_matrix_norm(byte norm, int rows, int columns, [In] double[] a, [In, Out] double[] work);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float c_matrix_norm(byte norm, int rows, int columns, [In] Complex32[] a, [In, Out] float[] work);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern double z_matrix_norm(byte norm, int rows, int columns, [In] Complex[] a, [In, Out] double[] work);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int s_cholesky_factor(int n, [In, Out] float[] a);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int d_cholesky_factor(int n, [In, Out] double[] a);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int c_cholesky_factor(int n, [In, Out] Complex32[] a);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int z_cholesky_factor(int n, [In, Out] Complex[] a);
        
        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int s_lu_factor(int n, [In, Out] float[] a, [In, Out] int[] ipiv);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int d_lu_factor(int n, [In, Out] double[] a, [In, Out] int[] ipiv);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int c_lu_factor(int n, [In, Out] Complex32[] a, [In, Out] int[] ipiv);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int z_lu_factor(int n, [In, Out] Complex[] a, [In, Out] int[] ipiv);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int s_lu_inverse(int n, [In, Out] float[] a, [In, Out] float[] work, int lwork);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int d_lu_inverse(int n, [In, Out] double[] a, [In, Out] double[] work, int lwork);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int c_lu_inverse(int n, [In, Out] Complex32[] a, [In, Out] Complex32[] work, int lwork);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int z_lu_inverse(int n, [In, Out] Complex[] a, [In, Out] Complex[] work, int lwork);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int s_lu_inverse_factored(int n, [In, Out] float[] a, [In, Out] int[] ipiv, [In, Out] float[] work, int lwork);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int d_lu_inverse_factored(int n, [In, Out] double[] a, [In, Out] int[] ipiv, [In, Out] double[] work, int lwork);
    
        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int c_lu_inverse_factored(int n, [In, Out] Complex32[] a, [In, Out] int[] ipiv, [In, Out] Complex32[] work, int lwork);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int z_lu_inverse_factored(int n, [In, Out] Complex[] a, [In, Out] int[] ipiv, [In, Out] Complex[] work, int lwork);
        
        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int s_lu_solve_factored(int n, int nrhs, float[] a, [In, Out]int[] ipiv, [In, Out] float[] b);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int d_lu_solve_factored(int n, int nrhs, double[] a, [In, Out] int[] ipiv, [In, Out] double[] b);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int c_lu_solve_factored(int n, int nrhs, Complex32[] a, [In, Out] int[] ipiv, [In, Out] Complex32[] b);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int z_lu_solve_factored(int n, int nrhs, Complex[] a, [In, Out]int[] ipiv, [In, Out] Complex[] b);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int s_lu_solve(int n, int nrhs, float[] a, [In, Out] float[] b);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int d_lu_solve(int n, int nrhs, double[] a, [In, Out] double[] b);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int c_lu_solve(int n, int nrhs, Complex32[] a, [In, Out] Complex32[] b);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int z_lu_solve(int n, int nrhs, Complex[] a,  [In, Out] Complex[] b);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int s_cholesky_solve(int n, int nrhs, float[] a, [In, Out] float[] b);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int d_cholesky_solve(int n, int nrhs, double[] a, [In, Out] double[] b);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int c_cholesky_solve(int n, int nrhs, Complex32[] a, [In, Out] Complex32[] b);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int z_cholesky_solve(int n, int nrhs, Complex[] a, [In, Out] Complex[] b);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int s_cholesky_solve_factored(int n, int nrhs, float[] a, [In, Out] float[] b);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int d_cholesky_solve_factored(int n, int nrhs, double[] a, [In, Out] double[] b);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int c_cholesky_solve_factored(int n, int nrhs, Complex32[] a, [In, Out] Complex32[] b);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int z_cholesky_solve_factored(int n, int nrhs, Complex[] a, [In, Out] Complex[] b);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int s_qr_factor(int m, int n, [In, Out] float[] r, [In, Out] float[] tau, [In, Out] float[] q, [In, Out] float[] work, int len);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int d_qr_factor(int m, int n, [In, Out] double[] r, [In, Out] double[] tau, [In, Out] double[] q, [In, Out] double[] work, int len);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int c_qr_factor(int m, int n, [In, Out] Complex32[] r, [In, Out] Complex32[] tau, [In, Out] Complex32[] q, [In, Out] Complex32[] work, int len);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int z_qr_factor(int m, int n, [In, Out] Complex[] r, [In, Out] Complex[] tau, [In, Out] Complex[] q, [In, Out] Complex[] work, int len);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int s_qr_solve(int m, int n, int bn, float[] r, float[] b, [In, Out] float[] x, [In, Out] float[] work, int len);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int d_qr_solve(int m, int n, int bn, double[] r, double[] b, [In, Out] double[] x, [In, Out] double[] work, int len);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int c_qr_solve(int m, int n, int bn, Complex32[] r, Complex32[] b, [In, Out] Complex32[] x, [In, Out] Complex32[] work, int len);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int z_qr_solve(int m, int n, int bn, Complex[] r, Complex[] b, [In, Out] Complex[] x, [In, Out] Complex[] work, int len);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int s_qr_solve_factored(int m, int n, int bn, float[] r, float[] b, float[] tau, [In, Out] float[] x, [In, Out] float[] work, int len);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int d_qr_solve_factored(int m, int n, int bn, double[] r, double[] b, double[] tau, [In, Out] double[] x, [In, Out] double[] work, int len);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int c_qr_solve_factored(int m, int n, int bn, Complex32[] r, Complex32[] b, Complex32[] tau, [In, Out] Complex32[] x, [In, Out] Complex32[] work, int len);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int z_qr_solve_factored(int m, int n, int bn, Complex[] r, Complex[] b, Complex[] tau, [In, Out] Complex[] x, [In, Out] Complex[] work, int len);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int s_svd_factor(bool computeVectors, int m, int n, [In, Out] float[] a, [In, Out] float[] s, [In, Out] float[] u, [In, Out] float[] v, [In, Out] float[] work, int len);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int d_svd_factor(bool computeVectors, int m, int n, [In, Out] double[] a, [In, Out] double[] s, [In, Out] double[] u, [In, Out] double[] v, [In, Out] double[] work, int len);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int c_svd_factor(bool computeVectors, int m, int n, [In, Out] Complex32[] a, [In, Out] Complex32[] s, [In, Out] Complex32[] u, [In, Out] Complex32[] v, [In, Out] Complex32[] work, int len);

        [DllImport(DllName, ExactSpelling = true, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int z_svd_factor(bool computeVectors, int m, int n, [In, Out] Complex[] a, [In, Out] Complex[] s, [In, Out] Complex[] u, [In, Out] Complex[] v, [In, Out] Complex[] work, int len);

        #endregion LAPACK
    }
}