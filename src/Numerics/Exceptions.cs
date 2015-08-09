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

    /// <summary>
    /// An error occured calling native provider function.
    /// </summary>
    [Serializable]
    public abstract class NativeInterfaceException : Exception
    {
        protected NativeInterfaceException()
        {
        }

        protected NativeInterfaceException(string message)
            : base(message)
        {
        }

        protected NativeInterfaceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
#if !PORTABLE
        protected NativeInterfaceException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }

    /// <summary>
    /// An error occured calling native provider function.
    /// </summary>
    [Serializable]
    public class InvalidParameterException : NativeInterfaceException
    {
        public InvalidParameterException()
            : base(Resources.InvalidParameter)
        {
        }

        public InvalidParameterException(int parameter)
            : base(string.Format(Resources.InvalidParameterWithNumber, parameter))
        {
        }

        public InvalidParameterException(int parameter, Exception innerException)
            : base(string.Format(Resources.InvalidParameterWithNumber, parameter), innerException)
        {
        }
#if !PORTABLE
        protected InvalidParameterException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }

    /// <summary>
    /// Native provider was unable to allocate sufficent memory.
    /// </summary>
    [Serializable]
    public class MemoryAllocationException : NativeInterfaceException
    {
        public MemoryAllocationException()
            : base(Resources.MemoryAllocation)
        {
        }

        public MemoryAllocationException(Exception innerException)
            : base(Resources.MemoryAllocation, innerException)
        {
        }
#if !PORTABLE
        protected MemoryAllocationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }

    /// <summary>
    /// Native provider failed LU inversion do to a singular U matrix.
    /// </summary>
    [Serializable]
    public class SingularUMatrixException : NativeInterfaceException
    {
        public SingularUMatrixException()
            : base(Resources.SingularUMatrix)
        {
        }

        public SingularUMatrixException(int element)
            : base(string.Format(Resources.SingularUMatrixWithElement, element))
        {
        }

        public SingularUMatrixException(int element, Exception innerException)
            : base(string.Format(Resources.SingularUMatrixWithElement, element), innerException)
        {
        }
#if !PORTABLE
        protected SingularUMatrixException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
