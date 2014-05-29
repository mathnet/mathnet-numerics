using MathNet.Numerics.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.Interpolation
{
    public class TransformedInterpolation : IInterpolation
    {
        private IInterpolation _baseInterpolation;
        private Func<double, double> _transformer;

        public TransformedInterpolation(IInterpolation baseInterp, Func<double, double> transformer)
        {
            _baseInterpolation = baseInterp;
            _transformer = transformer;
        }

        public static TransformedInterpolation Interpolate(
            Func<double, double> transformer, Func<double, double> transformerInverse,
            IEnumerable<double> x, IEnumerable<double> y)
        {
            var xx = (x as double[]) ?? x.ToArray();
            var yy = (y as double[]) ?? y.ToArray();

            if (xx.Length != yy.Length)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            for (int i = 0; i < yy.Length; i++)
                yy[i] = transformerInverse(yy[i]);

            var baseInterp = LinearSpline.Interpolate(xx, yy);
            var interp = new TransformedInterpolation(baseInterp, transformer);

            return interp;
        }

        public bool SupportsDifferentiation
        {
            get { return false; }
        }

        public bool SupportsIntegration
        {
            get { return false; }
        }

        public double Differentiate(double t)
        {
            throw new NotImplementedException();
        }

        public double Differentiate2(double t)
        {
            throw new NotImplementedException();
        }

        public double Integrate(double t)
        {
            throw new NotImplementedException();
        }

        public double Integrate(double a, double b)
        {
            throw new NotImplementedException();
        }

        public double Interpolate(double t)
        {
            return _transformer(_baseInterpolation.Interpolate(t));
        }
    }
}
