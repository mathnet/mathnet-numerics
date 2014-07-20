// <copyright file="Adler32.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
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

/* This code is a port and simplification of
  adler32.c -- compute the Adler-32 checksum of a data stream
  Copyright (C) 1995-2011 Mark Adler
  zlib license:
  Copyright (C) 1995-2012 Jean-loup Gailly and Mark Adler

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jean-loup Gailly        Mark Adler
  jloup@gzip.org          madler@alumni.caltech.edu

*/

namespace MathNet.Numerics.Data.Matlab
{
    internal static class Adler32
    {
        /* largest prime smaller than 65536 */
        const int Base = 65521;
        /* NMAX is the largest n such that 255n(n+1)/2 + (n+1)(BASE-1) <= 2^32-1 */
        const int Nmax = 5552;

        /// <summary>
        /// Computes the Adler-32 checksum of the given data.
        /// </summary>
        /// <param name="data">The data to create the checksum.</param>
        /// <returns>The checksum</returns>
        public static uint Compute(byte[] data)
        {
            uint adler = 1;
            uint sum2 = 0;
            var len = data.Length;
            var offset = 0;
            while (len > 0)
            {
                var tlen = len < Nmax ? len : Nmax;
                len -= tlen;

                do
                {
                    adler += data[offset++];
                    sum2 += adler;
                }
                while (--tlen > 0);

                adler %= Base;
                sum2 %= Base;
            }

            return adler | (sum2 << 16);
        }
    }
}
