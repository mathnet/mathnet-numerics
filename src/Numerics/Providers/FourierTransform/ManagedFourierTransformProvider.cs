using System.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace MathNet.Numerics.Providers.FourierTransform
{
    public class ManagedFourierTransformProvider : IFourierTransformProvider
    {
        public virtual void InitializeVerify()
        {
        }

        public void ForwardInplace(Complex[] complex)
        {
            Fourier.BluesteinForward(complex, FourierOptions.Default);
        }

        public void BackwardInplace(Complex[] complex)
        {
            Fourier.BluesteinInverse(complex, FourierOptions.Default);
        }

        public Complex[] Forward(Complex[] complexTimeSpace)
        {
            Complex[] work = new Complex[complexTimeSpace.Length];
            complexTimeSpace.Copy(work);
            ForwardInplace(work);
            return work;
        }

        public Complex[] Backward(Complex[] complexFrequenceSpace)
        {
            Complex[] work = new Complex[complexFrequenceSpace.Length];
            complexFrequenceSpace.Copy(work);
            BackwardInplace(work);
            return work;
        }
    }
}
