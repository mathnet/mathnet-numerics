using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.LinearAlgebra.Generic
{
    internal static class Common
    {
        /// <summary>
        /// Returns the maximum value.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <returns>The maximum value.</returns>
        public static float Max(float a, float b)
        {
            return Math.Max(a, b);
        }
    }
}
