// <copyright file="IFourierTransformProvider.cs" company="Math.NET">
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

using Complex = System.Numerics.Complex;

namespace MathNet.Numerics.Providers.FourierTransform
{
    public enum FourierTransformScaling : int
    {
        NoScaling = 0,
        SymmetricScaling = 1,
        BackwardScaling = 2,
        ForwardScaling = 3
    }

    public interface IFourierTransformProvider
    {
        /// <summary>
        /// Try to find out whether the provider is available, at least in principle.
        /// Verification may still fail if available, but it will certainly fail if unavailable.
        /// </summary>
        bool IsAvailable();

        /// <summary>
        /// Initialize and verify that the provided is indeed available. If not, fall back to alternatives like the managed provider
        /// </summary>
        void InitializeVerify();

        /// <summary>
        /// Frees memory buffers, caches and handles allocated in or to the provider.
        /// Does not unload the provider itself, it is still usable afterwards.
        /// </summary>
        void FreeResources();

        void Forward(Complex32[] samples, FourierTransformScaling scaling);
        void Forward(Complex[] samples, FourierTransformScaling scaling);
        void Backward(Complex32[] spectrum, FourierTransformScaling scaling);
        void Backward(Complex[] spectrum, FourierTransformScaling scaling);

        void ForwardReal(float[] samples, int n, FourierTransformScaling scaling);
        void ForwardReal(double[] samples, int n, FourierTransformScaling scaling);
        void BackwardReal(float[] spectrum, int n, FourierTransformScaling scaling);
        void BackwardReal(double[] spectrum, int n, FourierTransformScaling scaling);

        void ForwardMultidim(Complex32[] samples, int[] dimensions, FourierTransformScaling scaling);
        void ForwardMultidim(Complex[] samples, int[] dimensions, FourierTransformScaling scaling);
        void BackwardMultidim(Complex32[] spectrum, int[] dimensions, FourierTransformScaling scaling);
        void BackwardMultidim(Complex[] spectrum, int[] dimensions, FourierTransformScaling scaling);
    }
}
