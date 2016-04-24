// <copyright file="DistanceTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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

using NUnit.Framework;
using System;

namespace MathNet.Numerics.UnitTests
{
    [TestFixture]
    public class DistanceTests
    {
        #region Test Data
        double[] _dp0 = new double[] { 1, 0.5 };
        double[] _dq0 = new double[] { 0.5, 1 };

        double[] _dp1 = new double[] { 4.5, 1 };
        double[] _dq1 = new double[] { 4, 2 };

        double[] _dp2 = new double[] { 0, 0, 0 };
        double[] _dq2 = new double[] { 0, 0, 0 };

        double[] _dp3 = new double[] { 1, 1, 1 };
        double[] _dq3 = new double[] { 1, 1, 1 };

        double[] _dp4 = new double[] { 2.5, 3.5, 3.0, 3.5, 2.5, 3.0 };
        double[] _dq4 = new double[] { 3.0, 3.5, 1.5, 5.0, 3.5, 3.0 };

        double[] _dp5 = new double[] { 1, 3, 5, 6, 8, 9, 6, 4, 3, 2 };
        double[] _dq5 = new double[] { 2, 5, 6, 6, 7, 7, 5, 3, 1, 1 };

        float[] _fp0 = new float[] { 1, 0.5f };
        float[] _fq0 = new float[] { 0.5f, 1 };

        float[] _fp1 = new float[] { 4.5f, 1 };
        float[] _fq1 = new float[] { 4, 2 };

        float[] _fp2 = new float[] { 0, 0, 0 };
        float[] _fq2 = new float[] { 0, 0, 0 };

        float[] _fp3 = new float[] { 1, 1, 1 };
        float[] _fq3 = new float[] { 1, 1, 1 };

        float[] _fp4 = new float[] { 2.5f, 3.5f, 3.0f, 3.5f, 2.5f, 3.0f };
        float[] _fq4 = new float[] { 3.0f, 3.5f, 1.5f, 5.0f, 3.5f, 3.0f };

        float[] _fp5 = new float[] { 1, 3, 5, 6, 8, 9, 6, 4, 3, 2 };
        float[] _fq5 = new float[] { 2, 5, 6, 6, 7, 7, 5, 3, 1, 1 };
        #endregion

        [Test]
        public void Hamming()
        {
            Assert.That(Distance.Hamming(new[] { 0.0, 0.0 }, new[] { 0.0, 0.0 }), Is.EqualTo(0.0));
            Assert.That(Distance.Hamming(new[] { 1.0, 0.0 }, new[] { 1.0, 0.0 }), Is.EqualTo(0.0));
            Assert.That(Distance.Hamming(new[] { 0.0, 0.0 }, new[] { 1.0, 0.0 }), Is.EqualTo(1.0));
            Assert.That(Distance.Hamming(new[] { 0.0, 0.0 }, new[] { 0.0, 1.0 }), Is.EqualTo(1.0));
            Assert.That(Distance.Hamming(new[] { 0.0, 0.0 }, new[] { 1.0, 1.0 }), Is.EqualTo(2.0));
            Assert.That(Distance.Hamming(new[] { 1.0, 0.0 }, new[] { 0.0, 1.0 }), Is.EqualTo(2.0));
        }

        [Test]
        public void Jaccard()
        {
            Assert.Throws<ArgumentException>(() => Distance.Jaccard(_dp0, _dq4));
            Assert.Throws<ArgumentNullException>(() => Distance.Jaccard(null, _dq4));
            Assert.Throws<ArgumentNullException>(() => Distance.Jaccard(_dp0, null));

            Assert.That(Distance.Jaccard(_dp0, _dq0), Is.EqualTo(1));
            Assert.That(Distance.Jaccard(_dp1, _dq1), Is.EqualTo(1));
            Assert.That(Distance.Jaccard(_dp2, _dq2), Is.EqualTo(Double.NaN));
            Assert.That(Distance.Jaccard(_dp3, _dq3), Is.EqualTo(0));
            Assert.That(Distance.Jaccard(_dp4, _dq4), Is.EqualTo(0.66666).Within(0.00001));
            Assert.That(Distance.Jaccard(_dp5, _dq5), Is.EqualTo(0.9).Within(0.1));

            Assert.Throws<ArgumentException>(() => Distance.Jaccard(_fp0, _fq4));
            Assert.Throws<ArgumentNullException>(() => Distance.Jaccard(null, _fq4));
            Assert.Throws<ArgumentNullException>(() => Distance.Jaccard(_fp0, null));

            Assert.That(Distance.Jaccard(_fp0, _fq0), Is.EqualTo(1));
            Assert.That(Distance.Jaccard(_fp1, _fq1), Is.EqualTo(1));
            Assert.That(Distance.Jaccard(_fp2, _fq2), Is.EqualTo(float.NaN));
            Assert.That(Distance.Jaccard(_fp3, _fq3), Is.EqualTo(0));
            Assert.That(Distance.Jaccard(_fp4, _fq4), Is.EqualTo(0.66666).Within(0.00001));
            Assert.That(Distance.Jaccard(_fp5, _fq5), Is.EqualTo(0.9).Within(0.1));
        }

        [Test]
        public void Euclidean()
        {
            Assert.Throws<ArgumentException>(() => Distance.Euclidean(_dp0, _dq4));

            Assert.That(Distance.Euclidean(_dp0, _dq0), Is.EqualTo(.70711).Within(0.00001));
            Assert.That(Distance.Euclidean(_dp1, _dq1), Is.EqualTo(1.11803).Within(0.00001));
            Assert.That(Distance.Euclidean(_dp2, _dq2), Is.EqualTo(0));
            Assert.That(Distance.Euclidean(_dp3, _dq3), Is.EqualTo(0));
            Assert.That(Distance.Euclidean(_dp4, _dq4), Is.EqualTo(2.39792).Within(0.00001));
            Assert.That(Distance.Euclidean(_dp5, _dq5), Is.EqualTo(4.24264).Within(0.00001));

            Assert.Throws<ArgumentException>(() => Distance.Euclidean(_fp0, _fq4));

            Assert.That(Distance.Euclidean(_fp0, _fq0), Is.EqualTo(.70711).Within(0.00001));
            Assert.That(Distance.Euclidean(_fp1, _fq1), Is.EqualTo(1.11803).Within(0.00001));
            Assert.That(Distance.Euclidean(_fp2, _fq2), Is.EqualTo(0));
            Assert.That(Distance.Euclidean(_fp3, _fq3), Is.EqualTo(0));
            Assert.That(Distance.Euclidean(_fp4, _fq4), Is.EqualTo(2.39792).Within(0.00001));
            Assert.That(Distance.Euclidean(_fp5, _fq5), Is.EqualTo(4.24264).Within(0.00001));
        }

        [Test]
        public void Manhattan()
        {
            Assert.Throws<ArgumentException>(() => Distance.Manhattan(_dp0, _dq4));

            Assert.That(Distance.Manhattan(_dp0, _dq0), Is.EqualTo(1));
            Assert.That(Distance.Manhattan(_dp1, _dq1), Is.EqualTo(1.5));
            Assert.That(Distance.Manhattan(_dp2, _dq2), Is.EqualTo(0));
            Assert.That(Distance.Manhattan(_dp3, _dq3), Is.EqualTo(0));
            Assert.That(Distance.Manhattan(_dp4, _dq4), Is.EqualTo(4.5));
            Assert.That(Distance.Manhattan(_dp5, _dq5), Is.EqualTo(12));

            Assert.Throws<ArgumentException>(() => Distance.Manhattan(_fp0, _fq4));

            Assert.That(Distance.Manhattan(_fp0, _fq0), Is.EqualTo(1));
            Assert.That(Distance.Manhattan(_fp1, _fq1), Is.EqualTo(1.5));
            Assert.That(Distance.Manhattan(_fp2, _fq2), Is.EqualTo(0));
            Assert.That(Distance.Manhattan(_fp3, _fq3), Is.EqualTo(0));
            Assert.That(Distance.Manhattan(_fp4, _fq4), Is.EqualTo(4.5));
            Assert.That(Distance.Manhattan(_fp5, _fq5), Is.EqualTo(12));
        }

        [Test]
        public void Cosine()
        {
            Assert.Throws<ArgumentException>(() => Distance.Cosine(_dp0, _dq4));

            Assert.That(Distance.Cosine(_dp0, _dq0), Is.EqualTo(0.2).Within(0.00001));
            Assert.That(Distance.Cosine(_dp1, _dq1), Is.EqualTo(0.029857).Within(0.00001));
            Assert.That(Distance.Cosine(_dp2, _dq2), Is.EqualTo(Double.NaN));
            Assert.That(Distance.Cosine(_dp3, _dq3), Is.EqualTo(0).Within(0.00001));
            Assert.That(Distance.Cosine(_dp4, _dq4), Is.EqualTo(0.039354).Within(0.00001));
            Assert.That(Distance.Cosine(_dp5, _dq5), Is.EqualTo(0.031026).Within(0.00001));

            Assert.Throws<ArgumentException>(() => Distance.Cosine(_fp0, _fq4));

            Assert.That(Distance.Cosine(_fp0, _fq0), Is.EqualTo(0.2).Within(0.00001));
            Assert.That(Distance.Cosine(_fp1, _fq1), Is.EqualTo(0.029857).Within(0.00001));
            Assert.That(Distance.Cosine(_fp2, _fq2), Is.EqualTo(float.NaN));
            Assert.That(Distance.Cosine(_fp3, _fq3), Is.EqualTo(0).Within(0.00001));
            Assert.That(Distance.Cosine(_fp4, _fq4), Is.EqualTo(0.039354).Within(0.00001));
            Assert.That(Distance.Cosine(_fp5, _fq5), Is.EqualTo(0.031026).Within(0.00001));
        }
    }
}
