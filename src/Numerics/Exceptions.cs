﻿using System;

namespace MathNet.Numerics
{
    /// <summary>
    /// An algorithm failed to converge.
    /// </summary>
    [Serializable]
    public class NonConvergenceException : Exception
    {
        public NonConvergenceException() : base("An algorithm failed to converge.")
        {
        }

        public NonConvergenceException(string message) : base(message)
        {
        }

        public NonConvergenceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// An algorithm failed to converge due to a numerical breakdown.
    /// </summary>
    [Serializable]
    public class NumericalBreakdownException : NonConvergenceException
    {
        public NumericalBreakdownException()
            : base("Algorithm experience a numerical break down.")
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
    }

    /// <summary>
    /// An error occurred calling native provider function.
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
    }

    /// <summary>
    /// An error occurred calling native provider function.
    /// </summary>
    [Serializable]
    public class InvalidParameterException : NativeInterfaceException
    {
        public InvalidParameterException()
            : base("An invalid parameter was passed to a native method.")
        {
        }

        public InvalidParameterException(int parameter)
            : base($"An invalid parameter was passed to a native method, parameter number : {parameter}")
        {
        }

        public InvalidParameterException(int parameter, Exception innerException)
            : base($"An invalid parameter was passed to a native method, parameter number : {parameter}", innerException)
        {
        }
    }

    /// <summary>
    /// Native provider was unable to allocate sufficient memory.
    /// </summary>
    [Serializable]
    public class MemoryAllocationException : NativeInterfaceException
    {
        public MemoryAllocationException()
            : base("Unable to allocate native memory.")
        {
        }

        public MemoryAllocationException(Exception innerException)
            : base("Unable to allocate native memory.", innerException)
        {
        }
    }

    /// <summary>
    /// Native provider failed LU inversion do to a singular U matrix.
    /// </summary>
    [Serializable]
    public class SingularUMatrixException : NativeInterfaceException
    {
        public SingularUMatrixException()
            : base("U is singular, and the inversion could not be completed.")
        {
        }

        public SingularUMatrixException(int element)
            : base($"U is singular, and the inversion could not be completed. The {element}-th diagonal element of the factor U is zero.")
        {
        }

        public SingularUMatrixException(int element, Exception innerException)
            : base($"U is singular, and the inversion could not be completed. The {element}-th diagonal element of the factor U is zero.", innerException)
        {
        }
    }

    /// <summary>
    /// Distance between point 't' and reference points is too large (can lead to inaccurate interpolation).
    /// </summary>
    [Serializable]
    public class InterpolatingDistanceException : Exception

    {
        public InterpolatingDistanceException()
            : base("Distace from 't' to the two closest points exceeds MaxDeltaT.")
        {
        }

        public InterpolatingDistanceException(string message)
            : base(message)
        {
        }

        public InterpolatingDistanceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected InterpolatingDistanceException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
