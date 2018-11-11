using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics
{
    public static partial class SpecialFunctions
    {
        public enum Scale
        {
            /// <summary>
            /// For Bessel-related functions, no scaling factor is applied.
            /// </summary>
            Unity = 0,

            /// <summary>
            /// For Bessel-related functions, exponential scaling is applied.
            /// </summary>
            Exponential = 1
        }
    }
}
