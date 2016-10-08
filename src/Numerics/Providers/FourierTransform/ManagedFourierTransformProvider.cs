// <copyright file="ManagedFourierTransformProvider.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
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

using MathNet.Numerics.IntegralTransforms;

namespace MathNet.Numerics.Providers.FourierTransform
{

#if !NOSYSNUMERICS
    using Complex = System.Numerics.Complex;
#endif

    public class ManagedFourierTransformProvider : IFourierTransformProvider
    {
        public virtual void InitializeVerify()
        {
        }

        public virtual void ForwardInplace(Complex[] complex, FourierTransformScaling scaling)
        {
            Fourier.BluesteinForward(complex, Options(scaling));
        }

        public virtual void BackwardInplace(Complex[] complex, FourierTransformScaling scaling)
        {
            Fourier.BluesteinInverse(complex, Options(scaling));
        }

        public virtual Complex[] Forward(Complex[] complexTimeSpace, FourierTransformScaling scaling)
        {
            Complex[] work = new Complex[complexTimeSpace.Length];
            complexTimeSpace.Copy(work);
            ForwardInplace(work, scaling);
            return work;
        }

        public virtual Complex[] Backward(Complex[] complexFrequenceSpace, FourierTransformScaling scaling)
        {
            Complex[] work = new Complex[complexFrequenceSpace.Length];
            complexFrequenceSpace.Copy(work);
            BackwardInplace(work, scaling);
            return work;
        }

        private FourierOptions Options(FourierTransformScaling scaling)
        {
            switch (scaling)
            {
                case FourierTransformScaling.NoScaling:
                    return FourierOptions.NoScaling;
                case FourierTransformScaling.AsymmetricScaling:
                    return FourierOptions.AsymmetricScaling;
                case FourierTransformScaling.SymmetricScaling:
                default:
                    return FourierOptions.Default;
            }
        }
    }
}
