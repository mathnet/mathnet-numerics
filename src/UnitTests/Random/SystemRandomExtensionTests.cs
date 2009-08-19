// <copyright file="SystemRandomExtensionTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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

namespace MathNet.Numerics.UnitTests.DistributionTests
{
    using MbUnit.Framework;
    using MathNet.Numerics.Random;

    [TestFixture]
    public class SystemRandomExtensionTests
    {
        [Test]
        public void CanSampleInt64()
        {
            var rnd = new System.Random();
            long l = rnd.NextInt64();
        }

        [Test]
        public void CanSampleFullRangeInt32()
        {
            var rnd = new System.Random();
            int l = rnd.NextFullRangeInt32();
        }

        [Test]
        public void CanSampleFullRangeInt64()
        {
            var rnd = new System.Random();
            long l = rnd.NextFullRangeInt64();
        }

        [Test]
        public void CanSampleDecimal()
        {
            var rnd = new System.Random();
            decimal l = rnd.NextDecimal();
        }
    }
}
