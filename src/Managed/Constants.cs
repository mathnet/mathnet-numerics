// <copyright file="Constants.cs" company="Math.NET">
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

namespace MathNet.Numerics
{
    /// <summary>
    /// A collection of frequently used mathematical constants.
    /// </summary>
    /// <seealso cref="SiConstants"/>
    /// <seealso cref="SiPrefixes"/>
    public static class Constants
    {
        /// <summary>The number e</summary>
        public const double E = 2.7182818284590452353602874713526624977572470937000d;

        /// <summary>The number log[2](e)</summary>
        public const double Log2E = 1.4426950408889634073599246810018921374266459541530d;

        /// <summary>The number log[10](e)</summary>
        public const double Log10E = 0.43429448190325182765112891891660508229439700580366d;

        /// <summary>The number log[e](2)</summary>
        public const double Ln2 = 0.69314718055994530941723212145817656807550013436026d;

        /// <summary>The number log[e](10)</summary>
        public const double Ln10 = 2.3025850929940456840179914546843642076011014886288d;

        /// <summary>The number log[e](pi)</summary>
        public const double LnPi = 1.1447298858494001741434273513530587116472948129153d;

        /// <summary>The number log[e](2*pi)/2</summary>
        public const double Ln2PiOver2 = 0.91893853320467274178032973640561763986139747363780d;

        /// <summary>The number 1/e</summary>
        public const double InvE = 0.36787944117144232159552377016146086744581113103176d;

        /// <summary>The number sqrt(e)</summary>
        public const double SqrtE = 1.6487212707001281468486507878141635716537761007101d;

        /// <summary>The number sqrt(2)</summary>
        public const double Sqrt2 = 1.4142135623730950488016887242096980785696718753769d;

        /// <summary>The number sqrt(1/2) = 1/sqrt(2) = sqrt(2)/2</summary>
        public const double Sqrt1Over2 = 0.70710678118654752440084436210484903928483593768845d;

        /// <summary>The number sqrt(3)/2</summary>
        public const double HalfSqrt3 = 0.86602540378443864676372317075293618347140262690520d;

        /// <summary>The number pi</summary>
        public const double Pi = 3.1415926535897932384626433832795028841971693993751d;

        /// <summary>The number 1/pi</summary>
        public const double OneOverPi = 0.31830988618379067153776752674502872406891929148091d;

        /// <summary>The number pi/2</summary>
        public const double PiOver2 = 1.5707963267948966192313216916397514420985846996876d;

        /// <summary>The number pi/4</summary>
        public const double PiOver4 = 0.78539816339744830961566084581987572104929234984378d;

        /// <summary>The number sqrt(pi)</summary>
        public const double SqrtPi = 1.7724538509055160272981674833411451827975494561224d;

        /// <summary>The number sqrt(2pi)</summary>
        public const double Sqrt2Pi = 2.5066282746310005024157652848110452530069867406099d;

        /// <summary>The number sqrt(2*pi*e)</summary>
        public const double Sqrt2PiE = 4.1327313541224929384693918842998526494455219169913d;

        /// <summary>The number log(sqrt(2*pi))</summary>
        public const double LogSqrt2Pi = 0.91893853320467274178032973640561763986139747363778;

        /// <summary>The number log(sqrt(2*pi*e))</summary>
        public const double LogSqrt2PiE = 1.4189385332046727417803297364056176398613974736378d;

        /// <summary>The number 1/pi</summary>
        public const double InvPi = 0.31830988618379067153776752674502872406891929148091d;

        /// <summary>The number 2/pi</summary>
        public const double TwoInvPi = 0.63661977236758134307553505349005744813783858296182d;

        /// <summary>The number 1/sqrt(pi)</summary>
        public const double InvSqrtPi = 0.56418958354775628694807945156077258584405062932899d;

        /// <summary>The number 1/sqrt(2pi)</summary>
        public const double InvSqrt2Pi = 0.39894228040143267793994605993438186847585863116492d;

        /// <summary>The number 2/sqrt(pi)</summary>
        public const double TwoInvSqrtPi = 1.1283791670955125738961589031215451716881012586580d;

        /// <summary>The number (pi)/180 - factor to convert from Degree (deg) to Radians (rad).</summary>
        /// <seealso cref="Trig.DegreeToRadian"/>
        /// <seealso cref="Trig.RadianToDegree"/>
        public const double Degree = 0.017453292519943295769236907684886127134428718885417d;

        /// <summary>The number (pi)/200 - factor to convert from NewGrad (grad) to Radians (rad).</summary>
        /// <seealso cref="Trig.GradToRadian"/>
        /// <seealso cref="Trig.RadianToGrad"/>
        public const double Grad = 0.015707963267948966192313216916397514420985846996876d;

        /// <summary>The number ln(10)/20 - factor to convert from Power Decibel (dB) to Neper (Np). Use this version when the Decibel represent a power gain but the compared values are not powers (e.g. amplitude, current, voltage).</summary>
        /// <seealso cref="Ratios.RatioToPowerDecibel(double)"/>
        /// <seealso cref="Ratios.PowerDecibelToRatio"/>
        /// <seealso cref="Ratios.PowerDecibelToValue"/>
        public const double PowerDecibel = 0.11512925464970228420089957273421821038005507443144d;

        /// <summary>The number ln(10)/10 - factor to convert from Neutral Decibel (dB) to Neper (Np). Use this version when either both or neither of the Decibel and the compared values represent powers.</summary>
        /// <seealso cref="Ratios.RatioToDecibel(double)"/>
        /// <seealso cref="Ratios.DecibelToRatio"/>
        /// <seealso cref="Ratios.DecibelToValue"/>
        public const double NeutralDecibel = 0.23025850929940456840179914546843642076011014886288d;

        /// <summary>The Catalan constant</summary>
        /// <remarks>Sum(k=0 -> inf){ (-1)^k/(2*k + 1)2 }</remarks>
        public const double Catalan = 0.9159655941772190150546035149323841107741493742816721342664981196217630197762547694794d;

        /// <summary>The Euler-Mascheroni constant</summary>
        /// <remarks>lim(n -> inf){ Sum(k=1 -> n) { 1/k - log(n) } }</remarks>
        public const double EulerMascheroni = 0.5772156649015328606065120900824024310421593359399235988057672348849d;

        /// <summary>The number (1+sqrt(5))/2, also known as the golden ratio</summary>
        public const double GoldenRatio = 1.6180339887498948482045868343656381177203091798057628621354486227052604628189024497072d;

        /// <summary>The Glaisher constant</summary>
        /// <remarks>e^(1/12 - Zeta(-1))</remarks>
        public const double Glaisher = 1.2824271291006226368753425688697917277676889273250011920637400217404063088588264611297d;

        /// <summary>The Khinchin constant</summary>
        /// <remarks>prod(k=1 -> inf){1+1/(k*(k+2))^log(k,2)}</remarks>
        public const double Khinchin = 2.6854520010653064453097148354817956938203822939944629530511523455572188595371520028011d;
    }
}