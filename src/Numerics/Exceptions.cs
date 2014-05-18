using System;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics
{
    /// <summary>
    /// An algorithm failed to converge.
    /// </summary>
    [Serializable]
    public class NonConvergenceException : Exception
    {
        public NonConvergenceException() : base(Resources.ConvergenceFailed)
        {
        }

        public NonConvergenceException(string message) : base(message)
        {
        }

        public NonConvergenceException(string message, Exception innerException) : base(message, innerException)
        {
        }
#if !PORTABLE
        protected NonConvergenceException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }

    /// <summary>
    /// An algorithm failed to converge due to a numerical breakdown.
    /// </summary>
    [Serializable]
    public class NumericalBreakdownException : NonConvergenceException
    {
        public NumericalBreakdownException()
            : base(Resources.NumericalBreakdown)
        {
        }

        public NumericalBreakdownException(string message)
            : base(message)
        {
        }

        public NumericalBreakdownException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
#if !PORTABLE
        protected NumericalBreakdownException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
