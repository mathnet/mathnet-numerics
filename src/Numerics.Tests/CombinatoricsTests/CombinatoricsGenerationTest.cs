// <copyright file="CombinatoricsGenerationTest.cs" company="Math.NET">
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
using System.Numerics;
namespace MathNet.Numerics.UnitTests.CombinatoricsTests
{

    /// <summary>
    /// Test if combinatoric generation functions work correctly.
    /// </summary>
    [TestFixture]
    public class CombinatoricsGenerationTest
    {
        private void CGBIV(BigInteger n,int k)
        {
            BigInteger[] selection = Combinatorics.GenerateVariation(n, k);
            Assert.AreEqual(k, selection.Length);
            for (int i = 0; i < k; i++)
            {
                Assert.GreaterOrEqual(selection[i], 0);
                Assert.Less(selection[i], n);
                System.Console.Write(selection[i] + " ");
            }
        }
        static readonly BigInteger TestBI = new BigInteger(ulong.MaxValue) * 6;
        /// <summary>
        /// BigInteger isn't const so we can't simply set TestCase attributes. <br/>
        /// You need to check if the generated variation is statistically likely to be correct. Auto assertion won't find such error.
        /// </summary>
        /// <param name="n">N parameter.</param>
        /// <param name="k">K parameter.</param>
        /// <param name="expected">Expected value.</param>
        [Test]
        public void CanGenerateBigIntegerVariations()
        {
            //Main case
            CGBIV(TestBI, 6);
            //Border cases
            CGBIV(0, 0);
            CGBIV(6, 0);
            CGBIV(6, 6);
        }
    }
}
