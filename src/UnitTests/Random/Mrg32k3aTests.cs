// <copyright file="Mrg32k3aTests.cs" company="Math.NET">
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

using MathNet.Numerics.Random;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.Random
{
    /// <summary>
    /// Tests for a 32-bit combined multiple recursive generator with 2 components of order 3.
    /// </summary>
    [TestFixture, Category("Random")]
    public class Mrg32K3ATests : RandomTests
    {
        /// <summary>
        /// Initializes a new instance of the Mrg32K3ATests class.
        /// </summary>
        public Mrg32K3ATests() : base(typeof (Mrg32k3a))
        {
        }

        [Test]
        public void StaticSamplesConsistent()
        {
            Assert.That(Mrg32k3a.Doubles(1000, 1), Is.EqualTo(new Mrg32k3a(1).NextDoubles(1000)).Within(1e-12).AsCollection);
        }
    }
}
