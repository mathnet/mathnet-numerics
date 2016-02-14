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
    /// </remarks>
    public struct Quaternion32 : IFormattable, IEquatable<Quaternion32>
    {
        private float w, x, y, z;
        #region constructors
        public Quaternion32(float w, float x, float y, float z)
        {
            this.w = w;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Quaternion32(Complex32 a, Complex32 b)
        {
            w = a.Real;
            x = a.Imaginary;
            y = b.Real;
            z = b.Imaginary;
        }
        #endregion
        #region operators
        public static Quaternion32 operator +(Quaternion32 q1, Quaternion32 q2)
        {
            return new Quaternion32(q1.w + q2.w, q1.x + q2.x, q1.y + q2.y, q1.z + q2.z);
        }
        public static Quaternion32 operator -(Quaternion32 q1, Quaternion32 q2)
        {
            return new Quaternion32(q1.w - q2.w, q1.x - q2.x, q1.y - q2.y, q1.z - q2.z);
        }

        public static Quaternion32 operator *(Quaternion32 q1, Quaternion32 q2)
        {
            return new Quaternion32(
                 q1.w * q2.w - q1.x * q2.x - q1.y * q2.y - q1.z * q2.z,
                q1.w * q2.x + q1.x * q2.w - q1.y * q2.z + q1.z * q2.y,
                q1.w * q2.y + q1.x * q2.z + q1.y * q2.w - q1.z * q2.x,
                q1.w * q2.z - q1.x * q2.y + q1.y * q2.x + q1.z * q2.w
                );
        }

        public static Quaternion32 operator /(Quaternion32 q1, float f)
        {
            return new Quaternion32(q1.w / f, q1.x / f, q1.y / f, q1.z / f);
        }
        #endregion

        #region others

        public float MagnitudeSquared
        {
            get { return w * w + x * x + y * y + z * z; }
        }

        public float Magnitude
        {
            //TODO : create more robust magnitude
            get { return (float)Math.Sqrt(MagnitudeSquared); }
        }

        public float DotProduct(Quaternion32 q1, Quaternion32 q2)
        {
            return q1.x * q2.x + q1.y * q2.y + q1.z * q2.z + q1.w * q2.w;
        }
        public Quaternion32 Inverse()
        {
            return Conjugate() / MagnitudeSquared;
        }
        public Quaternion32 Normalize()
        {
            var magnitude = Magnitude;
            return new Quaternion32(w / magnitude, x / magnitude, y / magnitude, z / magnitude);
        }
        public static Quaternion32 Normalize(Quaternion32 quat)
        {
            return quat.Normalize();
        }
        public Quaternion32 Conjugate()
        {
            return new Quaternion32(w, -x, -y, -z);
        }
        public static Quaternion32 Conjugate(Quaternion32 quat)
        {
            return quat.Conjugate();
        }
        #endregion
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "({0}, {1}, {2}, {3})",
                w.ToString(format, formatProvider),
                x.ToString(format, formatProvider),
                y.ToString(format, formatProvider),
                z.ToString(format, formatProvider));
        }

        public bool Equals(Quaternion32 other)
        {
            return w.AlmostEqual(other.w) && x.AlmostEqual(other.x) && y.AlmostEqual(other.y) && z.AlmostEqual(other.z);
        }
    }
}
