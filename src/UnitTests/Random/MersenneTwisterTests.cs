// <copyright file="MersenneTwisterTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.UnitTests.Random
{
    using Numerics.Random;
    using NUnit.Framework;

    /// <summary>
    /// Tests for random number generator using Mersenne Twister 19937 algorithm.
    /// </summary>
    [TestFixture]
    public class MersenneTwisterTests : RandomTests
    {
        /// <summary>
        /// Initializes a new instance of the MersenneTwisterTests class.
        /// </summary>
        public MersenneTwisterTests() : base(typeof(MersenneTwister))
        {
        }

        /// <summary>
        /// Sample known values.
        /// </summary>
        [Test]
        public void SampleKnownValues()
        {
            var mt = new MersenneTwister(0);
            Assert.AreEqual(mt.NextDouble(), 0.5488135023042560);
            Assert.AreEqual(mt.NextDouble(), 0.5928446163889021);
            Assert.AreEqual(mt.NextDouble(), 0.7151893649715930);
            Assert.AreEqual(mt.NextDouble(), 0.8442657440900803);
        }
    }
}
