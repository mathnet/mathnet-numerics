// <copyright file="Combinatorics.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using MbUnit.Framework;
    using MathNet.Numerics;

    [TestFixture]
    public class ErfTests
    {
        private List<double []> mLargePrecisionVals;

        [SetUp]
        public void ReadLargePrecisionValues()
        {
            var sr = new StreamReader(@"..\..\data\erf.txt");
            mLargePrecisionVals = new List<double[]>();

            while(!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var vals = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                mLargePrecisionVals.Add(new double[] { Double.Parse(vals[0]), Double.Parse(vals[1]) });
            }
            sr.Close();
        }

        [Test, MultipleAsserts]
        public void CanMatchLargePrecision()
        {
            foreach (var xf in mLargePrecisionVals)
            {
                double x = xf[0];
                double f = xf[1];
                AssertEx.AreEqual<double>(f, SpecialFunctions.Erf(x));
            }
        }
    }
}
