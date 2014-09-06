using System;

namespace MathNet.Numerics.Providers.ExperimentalLinearAlgebra
{
    [Obsolete("Experimental with breaking changes expected between minor version. Do not use until properly released.")]
    public partial class ReferenceExperimentalLinearAlgebraProvider : IExperimentalLinearAlgebraProvider
    {
        /// <summary>
        /// Initialize and verify that the provided is indeed available. If not, fall back to alternatives like the managed provider
        /// </summary>
        public virtual void InitializeVerify()
        {
        }

        public override string ToString()
        {
            return "Reference";
        }
    }
}
