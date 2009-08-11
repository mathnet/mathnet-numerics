// <copyright file="StatTestData.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.StatisticsTests
{
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;

    internal class StatTestData
    {
        public readonly double[] Data;
        public readonly double?[] DataWithNulls;

        public readonly double Mean;
        public readonly double StandardDeviation;

        public StatTestData(string file)
        {
            using (StreamReader reader = new StreamReader(file))
            {
                string line = reader.ReadLine().Trim();

                while (!line.StartsWith("--"))
                {
                    if (line.StartsWith("Sample Mean"))
                    {
                        Mean = GetValue(line);
                    }
                    else if (line.StartsWith("Sample Standard Deviation"))
                    {
                        StandardDeviation = GetValue(line);
                    }
                    line = reader.ReadLine().Trim();
                }

                line = reader.ReadLine();
                
                IList<double> list = new List<double>();
                while (line != null)
                {
                    line = line.Trim();
                    if (!line.Equals(string.Empty))
                    {
                        list.Add(double.Parse(line));
                    }
                    line = reader.ReadLine();
                }

                Data = list.ToArray();
            }

            DataWithNulls = new double?[Data.Length + 2];
            for (int i = 0; i < Data.Length; i++)
            {
                DataWithNulls[i + 1] = Data[i];
            }
        }

        private static double GetValue(string str)
        {
            int start = str.IndexOf(":");
            string value = str.Substring(start + 1).Trim();
            if (value.Equals("NaN"))
            {
                return 0;
            }
            return double.Parse(str.Substring(start + 1).Trim());
        }
    }
}