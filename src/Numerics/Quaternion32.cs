// <copyright file="Quaternion.cs" company="Math.NET">
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

namespace MathNet.Numerics
{
    using System;
    using Distributions;

    /// <summary>
    /// 32-bit single precision Quaternion number class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The class <c>Quaternion32</c> provides all basic operations on Quaternion numbers. 
    /// All the operators <c>+</c>, <c>-</c>,
    /// <c>*</c>, <c>/</c>, <c>==</c>, <c>!=</c> are defined in the
    /// canonical way. 
    /// </para>
    /// <para>
    /// Class is based on http://web.cs.iastate.edu/~cs577/handouts/quaternion.pdf and http://mathworld.wolfram.com/Quaternion.html
    /// </para>
    /// </remarks>
    public struct Quaternion32 : IFormattable, IEquatable<Quaternion32>
    {
        private float a1, a2, a3, a4;
        #region constructors
        public Quaternion32(float a1, float a2, float a3, float a4)
        {
            this.a1 = a1;
            this.a2 = a2;
            this.a3 = a3;
            this.a4 = a4;
        }

        public Quaternion32(Complex32 a, Complex32 b)
        {
            a1 = a.Real;
            a2 = a.Imaginary;
            a3 = b.Real;
            a4 = b.Imaginary;
        }
        #endregion
        #region operators
        public static Quaternion32 operator +(Quaternion32 q1, Quaternion32 q2)
        {
            return new Quaternion32(q1.a1 + q2.a1, q1.a2 + q2.a2, q1.a3 + q2.a3, q1.a4 + q2.a4);
        }

        public static Quaternion32 operator +(Quaternion32 q1, float f)
        {
            return new Quaternion32(q1.a1 + f, q1.a2, q1.a3, q1.a4);
        }

        public static Quaternion32 operator +(float f, Quaternion32 q1)
        {
            return q1 + f;
        }
        public static Quaternion32 operator -(Quaternion32 q1, Quaternion32 q2)
        {
            return new Quaternion32(q1.a1 - q2.a1, q1.a2 - q2.a2, q1.a3 - q2.a3, q1.a4 - q2.a4);
        }

        public static Quaternion32 operator -(Quaternion32 q1, float f)
        {
            return new Quaternion32(q1.a1 - f, q1.a2, q1.a3, q1.a4);
        }

        public static Quaternion32 operator -(float f, Quaternion32 q1)
        {
            return new Quaternion32(f - q1.a1, q1.a2, q1.a3, q1.a4);
        }
        public static Quaternion32 operator *(Quaternion32 q1, Quaternion32 q2)
        {
            return new Quaternion32(
                 q1.a1 * q2.a1 - q1.a2 * q2.a2 - q1.a3 * q2.a3 - q1.a4 * q2.a4,
                q1.a1 * q2.a2 + q1.a2 * q2.a1 + q1.a3 * q2.a4 - q1.a4 * q2.a3,
                q1.a1 * q2.a3 - q1.a2 * q2.a4 + q1.a3 * q2.a1 + q1.a4 * q2.a2,
                q1.a1 * q2.a4 + q1.a2 * q2.a3 - q1.a3 * q2.a2 + q1.a4 * q2.a1
                );
        }
        public static Quaternion32 operator *(Quaternion32 q1, float f)
        {
            return new Quaternion32(q1.a1 * f, q1.a2 * f, q1.a3 * f, q1.a4 * f);
        }
        public static Quaternion32 operator *(float f, Quaternion32 q1)
        {
            return q1 * f;
        }

        public static Quaternion32 operator /(Quaternion32 q1, float f)
        {
            return new Quaternion32(q1.a1 / f, q1.a2 / f, q1.a3 / f, q1.a4 / f);
        }
        public static Quaternion32 operator /(float f, Quaternion32 q1)
        {
            //TODO : More robust divisionFdo
            return f * q1.Inverse();
        }
        public static bool operator ==(Quaternion32 q1, Quaternion32 q2)
        {
            return q1.Equals(q2);
        }

        public static bool operator !=(Quaternion32 q1, Quaternion32 q2)
        {
            return !(q1 == q2);
        }
        #endregion 
        #region operations

        public float NormSquared
        {
            get { return a1 * a1 + a2 * a2 + a3 * a3 + a4 * a4; }
        }

        public float Norm
        {
            //TODO : create more robust magnitude
            get { return (float)Math.Sqrt(NormSquared); }
        }

        public static float DotProduct(Quaternion32 q1, Quaternion32 q2)
        {
            return q1.a2 * q2.a2 + q1.a3 * q2.a3 + q1.a4 * q2.a4 + q1.a1 * q2.a1;
        }
        public Quaternion32 Inverse()
        {
            return Conjugate() / NormSquared;
        }
        public Quaternion32 Normalize()
        {
            var norm = Norm;
            return new Quaternion32(a1 / norm, a2 / norm, a3 / norm, a4 / norm);
        }
        public static Quaternion32 Normalize(Quaternion32 quat)
        {
            return quat.Normalize();
        }
        public Quaternion32 Conjugate()
        {
            return new Quaternion32(a1, -a2, -a3, -a4);
        }
        public static Quaternion32 Conjugate(Quaternion32 quat)
        {
            return quat.Conjugate();
        }
        #endregion

        public bool IsNan
        {
            get { return float.IsNaN(a1) || float.IsNaN(a2) || float.IsNaN(a3) || float.IsNaN(a4); }
        }

        public bool IsInfinity
        {
            get { return float.IsInfinity(a1) || float.IsInfinity(a2) || float.IsInfinity(a3) || float.IsInfinity(a4); }
        }

        public static readonly Quaternion32 One = new Quaternion32(1, 0, 0, 0);
        public static readonly Quaternion32 Zero = new Quaternion32(0, 0, 0, 0);
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "({0}, {1}, {2}, {3})",
                a1.ToString(format, formatProvider),
                a2.ToString(format, formatProvider),
                a3.ToString(format, formatProvider),
                a4.ToString(format, formatProvider));
        }

        public bool Equals(Quaternion32 other)
        {
            return a1.AlmostEqual(other.a1) && a2.AlmostEqual(other.a2) && a3.AlmostEqual(other.a3) && a4.AlmostEqual(other.a4);
        }
    }
}
