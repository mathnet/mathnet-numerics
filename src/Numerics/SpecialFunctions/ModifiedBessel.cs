// <copyright file="ModifiedBessel.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2012 Math.NET
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

// <contribution>
//    CERN - European Laboratory for Particle Physics
//        http://www.docjar.com/html/api/cern/jet/math/Bessel.java.html
//        Copyright 1999 CERN - European Laboratory for Particle Physics.
//        Permission to use, copy, modify, distribute and sell this software and its documentation for any purpose 
//        is hereby granted without fee, provided that the above copyright notice appear in all copies and 
//        that both that copyright notice and this permission notice appear in supporting documentation. 
//        CERN makes no representations about the suitability of this software for any purpose. 
//        It is provided "as is" without expressed or implied warranty.
//    TOMS757 - Uncommon Special Functions (Fortran77) by Allan McLeod
//        http://people.sc.fsu.edu/~jburkardt/f77_src/toms757/toms757.html
//    Wei Wu
//    Cephes Math Library, Stephen L. Moshier
//    ALGLIB 2.0.1, Sergey Bochkanov
// </contribution>

// ReSharper disable CheckNamespace
namespace MathNet.Numerics
// ReSharper restore CheckNamespace
{
    using System;

    /// <summary>
    /// This partial implementation of the SpecialFunctions class contains all methods related to the modified bessel function.
    /// </summary>
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// **************************************
        /// COEFFICIENTS FOR METHODS bessi0      *
        /// **************************************
        /// </summary>
        /// <summary> Chebyshev coefficients for exp(-x) I0(x)
        /// in the interval [0, 8].
        /// 
        /// lim(x->0){ exp(-x) I0(x) } = 1.
        /// </summary>
        private static readonly double[] BesselI0A = new[] { -4.41534164647933937950e-18, 3.33079451882223809783e-17, -2.43127984654795469359e-16, 1.71539128555513303061e-15, -1.16853328779934516808e-14, 7.67618549860493561688e-14, -4.85644678311192946090e-13, 2.95505266312963983461e-12, -1.72682629144155570723e-11, 9.67580903537323691224e-11, -5.18979560163526290666e-10, 2.65982372468238665035e-9, -1.30002500998624804212e-8, 6.04699502254191894932e-8, -2.67079385394061173391e-7, 1.11738753912010371815e-6, -4.41673835845875056359e-6, 1.64484480707288970893e-5, -5.75419501008210370398e-5, 1.88502885095841655729e-4, -5.76375574538582365885e-4, 1.63947561694133579842e-3, -4.32430999505057594430e-3, 1.05464603945949983183e-2, -2.37374148058994688156e-2, 4.93052842396707084878e-2, -9.49010970480476444210e-2, 1.71620901522208775349e-1, -3.04682672343198398683e-1, 6.76795274409476084995e-1 };

        /// <summary> Chebyshev coefficients for exp(-x) sqrt(x) I0(x)
        /// in the inverted interval [8, infinity].
        /// 
        /// lim(x->inf){ exp(-x) sqrt(x) I0(x) } = 1/sqrt(2pi).
        /// </summary>
        private static readonly double[] BesselI0B = new[] { -7.23318048787475395456e-18, -4.83050448594418207126e-18, 4.46562142029675999901e-17, 3.46122286769746109310e-17, -2.82762398051658348494e-16, -3.42548561967721913462e-16, 1.77256013305652638360e-15, 3.81168066935262242075e-15, -9.55484669882830764870e-15, -4.15056934728722208663e-14, 1.54008621752140982691e-14, 3.85277838274214270114e-13, 7.18012445138366623367e-13, -1.79417853150680611778e-12, -1.32158118404477131188e-11, -3.14991652796324136454e-11, 1.18891471078464383424e-11, 4.94060238822496958910e-10, 3.39623202570838634515e-9, 2.26666899049817806459e-8, 2.04891858946906374183e-7, 2.89137052083475648297e-6, 6.88975834691682398426e-5, 3.36911647825569408990e-3, 8.04490411014108831608e-1 };

        /// <summary>
        /// **************************************
        /// COEFFICIENTS FOR METHODS bessi1      *
        /// **************************************
        /// </summary>
        /// <summary> Chebyshev coefficients for exp(-x) I1(x) / x
        /// in the interval [0, 8].
        /// 
        /// lim(x->0){ exp(-x) I1(x) / x } = 1/2.
        /// </summary>
        private static readonly double[] BesselI1A = new[] { 2.77791411276104639959e-18, -2.11142121435816608115e-17, 1.55363195773620046921e-16, -1.10559694773538630805e-15, 7.60068429473540693410e-15, -5.04218550472791168711e-14, 3.22379336594557470981e-13, -1.98397439776494371520e-12, 1.17361862988909016308e-11, -6.66348972350202774223e-11, 3.62559028155211703701e-10, -1.88724975172282928790e-9, 9.38153738649577178388e-9, -4.44505912879632808065e-8, 2.00329475355213526229e-7, -8.56872026469545474066e-7, 3.47025130813767847674e-6, -1.32731636560394358279e-5, 4.78156510755005422638e-5, -1.61760815825896745588e-4, 5.12285956168575772895e-4, -1.51357245063125314899e-3, 4.15642294431288815669e-3, -1.05640848946261981558e-2, 2.47264490306265168283e-2, -5.29459812080949914269e-2, 1.02643658689847095384e-1, -1.76416518357834055153e-1, 2.52587186443633654823e-1 };

        /// <summary> Chebyshev coefficients for exp(-x) sqrt(x) I1(x)
        /// in the inverted interval [8, infinity].
        ///
        /// lim(x->inf){ exp(-x) sqrt(x) I1(x) } = 1/sqrt(2pi).
        /// </summary>
        private static readonly double[] BesselI1B = new[] { 7.51729631084210481353e-18, 4.41434832307170791151e-18, -4.65030536848935832153e-17, -3.20952592199342395980e-17, 2.96262899764595013876e-16, 3.30820231092092828324e-16, -1.88035477551078244854e-15, -3.81440307243700780478e-15, 1.04202769841288027642e-14, 4.27244001671195135429e-14, -2.10154184277266431302e-14, -4.08355111109219731823e-13, -7.19855177624590851209e-13, 2.03562854414708950722e-12, 1.41258074366137813316e-11, 3.25260358301548823856e-11, -1.89749581235054123450e-11, -5.58974346219658380687e-10, -3.83538038596423702205e-9, -2.63146884688951950684e-8, -2.51223623787020892529e-7, -3.88256480887769039346e-6, -1.10588938762623716291e-4, -9.76109749136146840777e-3, 7.78576235018280120474e-1 };

        /// <summary>
        /// **************************************
        /// COEFFICIENTS FOR METHODS bessk0, bessk0e *
        /// **************************************
        /// </summary>
        /// <summary> Chebyshev coefficients for K0(x) + log(x/2) I0(x)
        /// in the interval [0, 2].  The odd order coefficients are all
        /// zero; only the even order coefficients are listed.
        /// 
        /// lim(x->0){ K0(x) + log(x/2) I0(x) } = -EUL.
        /// </summary>
        private static readonly double[] BesselK0A = new[] { 1.37446543561352307156e-16, 4.25981614279661018399e-14, 1.03496952576338420167e-11, 1.90451637722020886025e-9, 2.53479107902614945675e-7, 2.28621210311945178607e-5, 1.26461541144692592338e-3, 3.59799365153615016266e-2, 3.44289899924628486886e-1, -5.35327393233902768720e-1 };

        /// <summary> Chebyshev coefficients for exp(x) sqrt(x) K0(x)
        /// in the inverted interval [2, infinity].
        /// 
        /// lim(x->inf){ exp(x) sqrt(x) K0(x) } = sqrt(pi/2).
        /// </summary>
        private static readonly double[] BesselK0B = new[] { 5.30043377268626276149e-18, -1.64758043015242134646e-17, 5.21039150503902756861e-17, -1.67823109680541210385e-16, 5.51205597852431940784e-16, -1.84859337734377901440e-15, 6.34007647740507060557e-15, -2.22751332699166985548e-14, 8.03289077536357521100e-14, -2.98009692317273043925e-13, 1.14034058820847496303e-12, -4.51459788337394416547e-12, 1.85594911495471785253e-11, -7.95748924447710747776e-11, 3.57739728140030116597e-10, -1.69753450938905987466e-9, 8.57403401741422608519e-9, -4.66048989768794782956e-8, 2.76681363944501510342e-7, -1.83175552271911948767e-6, 1.39498137188764993662e-5, -1.28495495816278026384e-4, 1.56988388573005337491e-3, -3.14481013119645005427e-2, 2.44030308206595545468e0 };

        /// <summary>
        /// **************************************
        /// COEFFICIENTS FOR METHODS bessk1, bessk1e *
        /// **************************************
        /// </summary>
        /// <summary> Chebyshev coefficients for x(K1(x) - log(x/2) I1(x))
        /// in the interval [0, 2].
        /// 
        /// lim(x->0){ x(K1(x) - log(x/2) I1(x)) } = 1.
        /// </summary>
        private static readonly double[] BesselK1A = new[] { -7.02386347938628759343e-18, -2.42744985051936593393e-15, -6.66690169419932900609e-13, -1.41148839263352776110e-10, -2.21338763073472585583e-8, -2.43340614156596823496e-6, -1.73028895751305206302e-4, -6.97572385963986435018e-3, -1.22611180822657148235e-1, -3.53155960776544875667e-1, 1.52530022733894777053e0 };

        /// <summary> Chebyshev coefficients for exp(x) sqrt(x) K1(x)
        /// in the interval [2, infinity].
        ///
        /// lim(x->inf){ exp(x) sqrt(x) K1(x) } = sqrt(pi/2).
        /// </summary>
        private static readonly double[] BesselK1B = new[] { -5.75674448366501715755e-18, 1.79405087314755922667e-17, -5.68946255844285935196e-17, 1.83809354436663880070e-16, -6.05704724837331885336e-16, 2.03870316562433424052e-15, -7.01983709041831346144e-15, 2.47715442448130437068e-14, -8.97670518232499435011e-14, 3.34841966607842919884e-13, -1.28917396095102890680e-12, 5.13963967348173025100e-12, -2.12996783842756842877e-11, 9.21831518760500529508e-11, -4.19035475934189648750e-10, 2.01504975519703286596e-9, -1.03457624656780970260e-8, 5.74108412545004946722e-8, -3.50196060308781257119e-7, 2.40648494783721712015e-6, -1.93619797416608296024e-5, 1.95215518471351631108e-4, -2.85781685962277938680e-3, 1.03923736576817238437e-1, 2.72062619048444266945e0 };

        /// <summary>Returns the modified Bessel function of first kind, order 0 of the argument.
        /// <p/>
        /// The function is defined as <tt>i0(x) = j0( ix )</tt>.
        /// <p/>
        /// The range is partitioned into the two intervals [0, 8] and
        /// (8, infinity). Chebyshev polynomial expansions are employed
        /// in each interval.
        /// </summary>
        /// <param name="x">The value to compute the bessel function of.
        /// </param>
        public static double BesselI0(double x)
        {
            if (x < 0)
            {
                x = -x;
            }
            if (x <= 8.0)
            {
                double y = (x / 2.0) - 2.0;
                return (Math.Exp(x) * Evaluate.ChebyshevA(BesselI0A, y));
            }

            double x1 = 32.0 / x - 2.0;
            return (Math.Exp(x) * Evaluate.ChebyshevA(BesselI0B, x1) / Math.Sqrt(x));
        }

        /// <summary>Returns the modified Bessel function of first kind,
        /// order 1 of the argument.
        /// <p/>
        /// The function is defined as <tt>i1(x) = -i j1( ix )</tt>.
        /// <p/>
        /// The range is partitioned into the two intervals [0, 8] and
        /// (8, infinity). Chebyshev polynomial expansions are employed
        /// in each interval.
        /// </summary>
        /// <param name="x">The value to compute the bessel function of.
        /// </param>
        public static double BesselI1(double x)
        {
            double z = Math.Abs(x);
            if (z <= 8.0)
            {
                double y = (z / 2.0) - 2.0;
                z = Evaluate.ChebyshevA(BesselI1A, y) * z * Math.Exp(z);
            }
            else
            {
                double x1 = 32.0 / z - 2.0;
                z = Math.Exp(z) * Evaluate.ChebyshevA(BesselI1B, x1) / Math.Sqrt(z);
            }
            if (x < 0.0)
            {
                z = -z;
            }
            return z;
        }

        /// <summary> Returns the modified Bessel function of the second kind
        /// of order 0 of the argument.
        /// <p/>
        /// The range is partitioned into the two intervals [0, 8] and
        /// (8, infinity). Chebyshev polynomial expansions are employed
        /// in each interval.
        /// </summary>
        /// <param name="x">The value to compute the bessel function of.
        /// </param>
        public static double BesselK0(double x)
        {
            if (x <= 0.0)
            {
                throw new ArithmeticException();
            }
            if (x <= 2.0)
            {
                double y = x * x - 2.0;
                return Evaluate.ChebyshevA(BesselK0A, y) - Math.Log(0.5 * x) * BesselI0(x);
            }

            double z = 8.0 / x - 2.0;
            return Math.Exp(-x) * Evaluate.ChebyshevA(BesselK0B, z) / Math.Sqrt(x);
        }

        /// <summary>Returns the exponentially scaled modified Bessel function
        /// of the second kind of order 0 of the argument.
        /// </summary>
        /// <param name="x">The value to compute the bessel function of.
        /// </param>
        public static double BesselK0e(double x)
        {
            if (x <= 0.0)
            {
                throw new ArithmeticException();
            }
            if (x <= 2.0)
            {
                double y = x * x - 2.0;
                return Evaluate.ChebyshevA(BesselK0A, y) - Math.Log(0.5 * x) * BesselI0(x) * Math.Exp(x);
            }

            double x1 = 8.0 / x - 2.0;
            return Evaluate.ChebyshevA(BesselK0B, x1) / Math.Sqrt(x);
        }

        /// <summary> Returns the modified Bessel function of the second kind
        /// of order 1 of the argument.
        /// <p/>
        /// The range is partitioned into the two intervals [0, 2] and
        /// (2, infinity). Chebyshev polynomial expansions are employed
        /// in each interval.
        /// </summary>
        /// <param name="x">The value to compute the bessel function of.
        /// </param>
        public static double BesselK1(double x)
        {
            double z = 0.5 * x;
            if (z <= 0.0)
            {
                throw new ArithmeticException();
            }
            if (x <= 2.0)
            {
                double y = x * x - 2.0;
                return Math.Log(z) * BesselI1(x) + Evaluate.ChebyshevA(BesselK1A, y) / x;
            }

            double x1 = 8.0 / x - 2.0;
            return Math.Exp(-x) * Evaluate.ChebyshevA(BesselK1B, x1) / Math.Sqrt(x);
        }

        /// <summary> Returns the exponentially scaled modified Bessel function
        /// of the second kind of order 1 of the argument.
        /// <p/>
        /// <tt>k1e(x) = exp(x) * k1(x)</tt>.
        /// </summary>
        /// <param name="x">The value to compute the bessel function of.
        /// </param>
        public static double BesselK1e(double x)
        {
            if (x <= 0.0)
            {
                throw new ArithmeticException();
            }
            if (x <= 2.0)
            {
                double y = x * x - 2.0;
                return Math.Log(0.5 * x) * BesselI1(x) + Evaluate.ChebyshevA(BesselK1A, y) / x * Math.Exp(x);
            }

            double x1 = 8.0 / x - 2.0;
            return Evaluate.ChebyshevA(BesselK1B, x1) / Math.Sqrt(x);
        }
    }
}
