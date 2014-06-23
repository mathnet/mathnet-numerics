namespace MathNet.Geometry
{
    using System;
    using MathNet.Numerics.LinearAlgebra;
    using MathNet.Numerics.LinearAlgebra.Double;
    using Units;

    public static class Matrix3D
    {
        public static DenseMatrix RotationAroundXAxis(Angle angle)
        {
            var rotationMatrix = new DenseMatrix(3, 3);
            rotationMatrix[0, 0] = 1;
            rotationMatrix[1, 1] = Math.Cos(angle.Radians);
            rotationMatrix[1, 2] = -Math.Sin(angle.Radians);
            rotationMatrix[2, 1] = Math.Sin(angle.Radians);
            rotationMatrix[2, 2] = Math.Cos(angle.Radians);
            return rotationMatrix;
        }

        public static DenseMatrix RotationAroundYAxis(Angle angle)
        {
            var rotationMatrix = new DenseMatrix(3, 3);
            rotationMatrix[0, 0] = Math.Cos(angle.Radians);
            rotationMatrix[0, 2] = Math.Sin(angle.Radians);
            rotationMatrix[1, 1] = 1;
            rotationMatrix[2, 0] = -Math.Sin(angle.Radians);
            rotationMatrix[2, 2] = Math.Cos(angle.Radians);
            return rotationMatrix;
        }

        public static Matrix<double> RotationAroundZAxis(Angle angle)
        {
            var rotationMatrix = new DenseMatrix(3, 3);
            rotationMatrix[0, 0] = Math.Cos(angle.Radians);
            rotationMatrix[0, 1] = -Math.Sin(angle.Radians);
            rotationMatrix[1, 0] = Math.Sin(angle.Radians);
            rotationMatrix[1, 1] = Math.Cos(angle.Radians);
            rotationMatrix[2, 2] = 1;
            return rotationMatrix;
        }

        /// <summary>
        /// Sets to the matrix of rotation that would align the 'from' vector with the 'to' vector. 
        /// The optional Axis argument may be used when the two vectors are perpendicular and in opposite directions to specify a specific solution, but is otherwise ignored.
        /// </summary>
        /// <param name="fromVector">Input Vector object to align from.</param>
        /// <param name="toVector">Input Vector object to align to.</param>
        /// <param name="axis">Input Vector object. </param>
        public static Matrix<double> RotationTo(
            Vector3D fromVector,
            Vector3D toVector,
            UnitVector3D? axis = null)
        {
            return RotationTo(fromVector.Normalize(), toVector.Normalize(), axis);
        }
        
        /// <summary>
        /// Sets to the matrix of rotation that would align the 'from' vector with the 'to' vector. 
        /// The optional Axis argument may be used when the two vectors are perpendicular and in opposite directions to specify a specific solution, but is otherwise ignored.
        /// </summary>
        /// <param name="fromVector">Input Vector object to align from.</param>
        /// <param name="toVector">Input Vector object to align to.</param>
        /// <param name="axis">Input Vector object. </param>
        public static Matrix<double> RotationTo(UnitVector3D fromVector, UnitVector3D toVector, UnitVector3D? axis = null)
        {
            if (fromVector == toVector)
                return DenseMatrix.CreateIdentity(3);
            if (fromVector.IsParallelTo(toVector))
            {
                if (axis == null)
                {
                    axis = fromVector.Orthogonal;
                }
            }
            else
            {
                axis = fromVector.CrossProduct(toVector);
            }
            var signedAngleTo = fromVector.SignedAngleTo(toVector, axis.Value);
            return RotationAroundArbitraryVector(axis.Value, signedAngleTo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aboutVector"></param>
        /// <param name="angle">Angle in degrees</param>
        /// <param name="angleUnit"></param>
        /// <returns></returns>
        public static Matrix<double> RotationAroundArbitraryVector<T>(UnitVector3D aboutVector, double angle, T angleUnit) where T : IAngleUnit
        {
            return RotationAroundArbitraryVector(aboutVector, Angle.From(angle, angleUnit));
        }

        public static Matrix<double> RotationAroundArbitraryVector(UnitVector3D aboutVector, Angle angle)
        {
            //http://en.wikipedia.org/wiki/Rotation_matrix
            var unitTensorProduct = aboutVector.GetUnitTensorProduct();
            var crossproductMatrix = aboutVector.CrossProductMatrix; //aboutVector.Clone().CrossProduct(aboutVector.Clone());

            var r1 = DenseMatrix.CreateIdentity(3).Multiply(Math.Cos(angle.Radians));
            var r2 = crossproductMatrix.Multiply(Math.Sin(angle.Radians));
            var r3 = unitTensorProduct.Multiply(1 - Math.Cos(angle.Radians));
            var totalR = r1.Add(r2).Add(r3);
            return totalR;
        }
    }
}