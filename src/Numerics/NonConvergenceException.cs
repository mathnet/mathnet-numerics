using System;
using System.Runtime.Serialization;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics
{
    /// <summary>
    /// An algorithm failed to converge.
    /// </summary>
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

        protected NonConvergenceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
