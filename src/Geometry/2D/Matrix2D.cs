namespace MathNet.Geometry
{
    using System;
    using Numerics.LinearAlgebra.Double;
    using Units;

    /// <summary>
    /// Helper class for creating matrices for manipulating 2D-elements
    /// </summary>
    public static class Matrix2D
    {
        /// <summary>
        /// Creates a rotation about the z-axis
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static DenseMatrix Rotation(Angle rotation)
        {
            double c = Math.Cos(rotation.Radians);
            double s = Math.Sin(rotation.Radians);
            return Create(c, -s, s, c);
        }

        /// <summary>
        /// Creates an arbitrary 2D transform
        /// </summary>
        /// <param name="m11"></param>
        /// <param name="m12"></param>
        /// <param name="m21"></param>
        /// <param name="m22"></param>
        /// <returns></returns>
        public static DenseMatrix Create(double m11, double m12, double m21, double m22)
        {
            return DenseMatrix.OfColumnMajor(2, 2, new[] { m11, m21, m12, m22 });
        }
    }
}
