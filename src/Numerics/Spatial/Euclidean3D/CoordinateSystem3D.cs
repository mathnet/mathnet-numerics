using System;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Spatial.Internal;

namespace MathNet.Numerics.Spatial.Euclidean3D
{
    /// <summary>
    /// A coordinate system
    /// </summary>
    [Serializable]
    public class CoordinateSystem3D : LinearAlgebra.Double.DenseMatrix, IEquatable<CoordinateSystem3D>, IXmlSerializable
    {
        /// <summary>
        /// A local regex pattern for 3D items
        /// </summary>
        private static readonly string Item3DPattern = Parser.Vector3DPattern.Trim('^', '$');

        /// <summary>
        /// A local regex pattern for a coordinate system
        /// </summary>
        private static readonly string CsPattern = string.Format(@"^ *o: *{{(?<op>{0})}} *x: *{{(?<xv>{0})}} *y: *{{(?<yv>{0})}} *z: *{{(?<zv>{0})}} *$", Item3DPattern);

        /// <summary>
        /// Initializes a new instance of the <see cref="CoordinateSystem3D"/> class.
        /// </summary>
        public CoordinateSystem3D()
            : this(new Point3D(0, 0, 0), UnitVector3D.XAxis.ToVector3D(), UnitVector3D.YAxis.ToVector3D(), UnitVector3D.ZAxis.ToVector3D())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoordinateSystem3D"/> class.
        /// </summary>
        /// <param name="xAxis">The x axis</param>
        /// <param name="yAxis">The y axis</param>
        /// <param name="zAxis">The z axis</param>
        /// <param name="origin">The origin</param>
        public CoordinateSystem3D(Vector3D xAxis, Vector3D yAxis, Vector3D zAxis, Point3D origin)
            : this(origin, xAxis, yAxis, zAxis)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoordinateSystem3D"/> class.
        /// </summary>
        /// <param name="origin">The origin</param>
        /// <param name="xAxis">The x axis</param>
        /// <param name="yAxis">The y axis</param>
        /// <param name="zAxis">The z axis</param>
        public CoordinateSystem3D(Point3D origin, UnitVector3D xAxis, UnitVector3D yAxis, UnitVector3D zAxis)
            : this(origin, xAxis.ToVector3D(), yAxis.ToVector3D(), zAxis.ToVector3D())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoordinateSystem3D"/> class.
        /// </summary>
        /// <param name="origin">The origin</param>
        /// <param name="xAxis">The x axis</param>
        /// <param name="yAxis">The y axis</param>
        /// <param name="zAxis">The z axis</param>
        public CoordinateSystem3D(Point3D origin, Vector3D xAxis, Vector3D yAxis, Vector3D zAxis)
            : base(4)
        {
            this.SetColumn(0, new[] { xAxis.X, xAxis.Y, xAxis.Z, 0 });
            this.SetColumn(1, new[] { yAxis.X, yAxis.Y, yAxis.Z, 0 });
            this.SetColumn(2, new[] { zAxis.X, zAxis.Y, zAxis.Z, 0 });
            this.SetColumn(3, new[] { origin.X, origin.Y, origin.Z, 1 });
        }

        ////public CoordinateSystem(Vector3D x, Vector3D y, Vector3D z, Vector3D offsetToBase)
        ////    : this(x, y, z, offsetToBase.ToPoint3D())
        ////{
        ////}

        /// <summary>
        /// Initializes a new instance of the <see cref="CoordinateSystem3D"/> class.
        /// </summary>
        /// <param name="matrix">A matrix</param>
        public CoordinateSystem3D(Matrix<double> matrix)
            : base(4, 4, matrix.ToColumnMajorArray())
        {
            if (matrix.RowCount != 4)
            {
                throw new ArgumentException("RowCount must be 4");
            }

            if (matrix.ColumnCount != 4)
            {
                throw new ArgumentException("ColumnCount must be 4");
            }
        }

        /// <summary>
        /// Gets the X Axis
        /// </summary>
        public Vector3D XAxis
        {
            get
            {
                var row = this.SubMatrix(0, 3, 0, 1).ToRowMajorArray();
                return new Vector3D(row[0], row[1], row[2]);
            }
        }

        /// <summary>
        /// Gets the Y Axis
        /// </summary>
        public Vector3D YAxis
        {
            get
            {
                var row = this.SubMatrix(0, 3, 1, 1).ToRowMajorArray();
                return new Vector3D(row[0], row[1], row[2]);
            }
        }

        /// <summary>
        /// Gets the z Axis
        /// </summary>
        public Vector3D ZAxis
        {
            get
            {
                var row = this.SubMatrix(0, 3, 2, 1).ToRowMajorArray();
                return new Vector3D(row[0], row[1], row[2]);
            }
        }

        /// <summary>
        /// Gets the point of origin
        /// </summary>
        public Point3D Origin
        {
            get
            {
                var row = this.SubMatrix(0, 3, 3, 1).ToRowMajorArray();
                return new Point3D(row[0], row[1], row[2]);
            }
        }

        /// <summary>
        /// Gets the offset to origin
        /// </summary>
        public Vector3D OffsetToBase
        {
            get { return this.Origin.ToVector3D(); }
        }

        /// <summary>
        /// Gets the base change matrix
        /// </summary>
        public CoordinateSystem3D BaseChangeMatrix
        {
            get
            {
                var matrix = Build.DenseOfColumnVectors(this.XAxis.ToVector(), this.YAxis.ToVector(), this.ZAxis.ToVector());
                var cs = new CoordinateSystem3D(this);
                cs.SetRotationSubMatrix(matrix.Transpose());
                return cs;
            }
        }

        /// <summary>
        /// Returns a value that indicates whether each pair of elements in two specified coordinate system is equal.
        /// </summary>
        /// <param name="left">The first coordinate system to compare</param>
        /// <param name="right">The second coordinate system to compare</param>
        /// <returns>True if the coordinate system are the same; otherwise false.</returns>
        public static bool operator ==(CoordinateSystem3D left, CoordinateSystem3D right)
        {
            return CoordinateSystem3D.Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether any pair of elements in two specified coordinate system is not equal.
        /// </summary>
        /// <param name="left">The first coordinate system to compare</param>
        /// <param name="right">The second coordinate system to compare</param>
        /// <returns>True if the coordinate systems are different; otherwise false.</returns>
        public static bool operator !=(CoordinateSystem3D left, CoordinateSystem3D right)
        {
            return !CoordinateSystem3D.Equals(left, right);
        }

        /// <summary>
        /// Creates a coordinate system from a string
        /// </summary>
        /// <param name="s">The string</param>
        /// <returns>A coordinate system</returns>
        public static CoordinateSystem3D Parse(string s)
        {
            var match = Regex.Match(s, CsPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline);
            var o = Point3D.Parse(match.Groups["op"].Value);
            var x = Vector3D.Parse(match.Groups["xv"].Value);
            var y = Vector3D.Parse(match.Groups["yv"].Value);
            var z = Vector3D.Parse(match.Groups["zv"].Value);
            return new CoordinateSystem3D(o, x, y, z);
        }

        /// <summary>
        /// Sets to the matrix of rotation that aligns the 'from' vector with the 'to' vector.
        /// The optional Axis argument may be used when the two vectors are perpendicular and in opposite directions to specify a specific solution, but is otherwise ignored.
        /// </summary>
        /// <param name="fromVector3D">Input Vector object to align from.</param>
        /// <param name="toVector3D">Input Vector object to align to.</param>
        /// <param name="axis">Input Vector object. </param>
        /// <returns>A rotated coordinate system </returns>
        public static CoordinateSystem3D RotateTo(UnitVector3D fromVector3D, UnitVector3D toVector3D, UnitVector3D? axis = null)
        {
            var r = Matrix3D.RotationTo(fromVector3D, toVector3D, axis);
            var coordinateSystem = new CoordinateSystem3D();
            var cs = SetRotationSubMatrix(r, coordinateSystem);
            return cs;
        }

        /// <summary>
        /// Creates a coordinate system that rotates
        /// </summary>
        /// <param name="angle">Angle to rotate</param>
        /// <param name="v">Vector to rotate about</param>
        /// <returns>A rotating coordinate system</returns>
        public static CoordinateSystem3D Rotation(Angle angle, UnitVector3D v)
        {
            var m = Build.Dense(4, 4);
            m.SetSubMatrix(0, 3, 0, 3, Matrix3D.RotationAroundArbitraryVector(v, angle));
            m[3, 3] = 1;
            return new CoordinateSystem3D(m);
        }

        /// <summary>
        /// Creates a coordinate system that rotates
        /// </summary>
        /// <param name="angle">Angle to rotate</param>
        /// <param name="v">Vector to rotate about</param>
        /// <returns>A rotated coordinate system</returns>
        public static CoordinateSystem3D Rotation(Angle angle, Vector3D v)
        {
            return Rotation(angle, v.Normalize());
        }

        /// <summary>
        /// Rotation around Z (yaw) then around Y (pitch) and then around X (roll)
        /// http://en.wikipedia.org/wiki/Aircraft_principal_axes
        /// </summary>
        /// <param name="yaw">Rotates around Z</param>
        /// <param name="pitch">Rotates around Y</param>
        /// <param name="roll">Rotates around X</param>
        /// <returns>A rotated coordinate system</returns>
        public static CoordinateSystem3D Rotation(Angle yaw, Angle pitch, Angle roll)
        {
            var cs = new CoordinateSystem3D();
            var yt = Yaw(yaw);
            var pt = Pitch(pitch);
            var rt = Roll(roll);
            return rt.Transform(pt.Transform(yt.Transform(cs)));
        }

        /// <summary>
        /// Rotates around Z
        /// </summary>
        /// <param name="av">An angle</param>
        /// <returns>A rotated coordinate system</returns>
        public static CoordinateSystem3D Yaw(Angle av)
        {
            return Rotation(av, UnitVector3D.ZAxis);
        }

        /// <summary>
        /// Rotates around Y
        /// </summary>
        /// <param name="av">An angle</param>
        /// <returns>A rotated coordinate system</returns>
        public static CoordinateSystem3D Pitch(Angle av)
        {
            return Rotation(av, UnitVector3D.YAxis);
        }

        /// <summary>
        /// Rotates around X
        /// </summary>
        /// <param name="av">An angle</param>
        /// <returns>A rotated coordinate system</returns>
        public static CoordinateSystem3D Roll(Angle av)
        {
            return Rotation(av, UnitVector3D.XAxis);
        }

        /// <summary>
        /// Creates a coordinate system that maps from the 'from' coordinate system to the 'to' coordinate system.
        /// </summary>
        /// <param name="fromCs">The from coordinate system</param>
        /// <param name="toCs">The to coordinate system</param>
        /// <returns>A mapping coordinate system</returns>
        public static CoordinateSystem3D CreateMappingCoordinateSystem(CoordinateSystem3D fromCs, CoordinateSystem3D toCs)
        {
            var m = toCs.Multiply(fromCs.Inverse());
            m[3, 3] = 1;
            return new CoordinateSystem3D(m);
        }

        /// <summary>
        /// Sets this matrix to be the matrix that maps from the 'from' coordinate system to the 'to' coordinate system.
        /// </summary>
        /// <param name="fromOrigin">Input Point3D that defines the origin to map the coordinate system from.</param>
        /// <param name="fromXAxis">Input Vector3D object that defines the X-axis to map the coordinate system from.</param>
        /// <param name="fromYAxis">Input Vector3D object that defines the Y-axis to map the coordinate system from.</param>
        /// <param name="fromZAxis">Input Vector3D object that defines the Z-axis to map the coordinate system from.</param>
        /// <param name="toOrigin">Input Point3D object that defines the origin to map the coordinate system to.</param>
        /// <param name="toXAxis">Input Vector3D object that defines the X-axis to map the coordinate system to.</param>
        /// <param name="toYAxis">Input Vector3D object that defines the Y-axis to map the coordinate system to.</param>
        /// <param name="toZAxis">Input Vector3D object that defines the Z-axis to map the coordinate system to.</param>
        /// <returns>A mapping coordinate system</returns>
        public static CoordinateSystem3D SetToAlignCoordinateSystems(Point3D fromOrigin, Vector3D fromXAxis, Vector3D fromYAxis, Vector3D fromZAxis, Point3D toOrigin, Vector3D toXAxis, Vector3D toYAxis, Vector3D toZAxis)
        {
            var cs1 = new CoordinateSystem3D(fromOrigin, fromXAxis, fromYAxis, fromZAxis);
            var cs2 = new CoordinateSystem3D(toOrigin, toXAxis, toYAxis, toZAxis);
            var mcs = CreateMappingCoordinateSystem(cs1, cs2);
            return mcs;
        }

        /// <summary>
        /// Creates a translation
        /// </summary>
        /// <param name="translation">A translation vector</param>
        /// <returns>A translated coordinate system</returns>
        public static CoordinateSystem3D Translation(Vector3D translation)
        {
            return new CoordinateSystem3D(translation.ToPoint3D(), UnitVector3D.XAxis, UnitVector3D.YAxis, UnitVector3D.ZAxis);
        }

        /// <summary>
        /// Creates a rotating coordinate system
        /// </summary>
        /// <param name="r">A 3×3 matrix with the rotation portion</param>
        /// <param name="coordinateSystem">A rotated coordinate system</param>
        /// <returns>A rotating coordinate system</returns>
        public static CoordinateSystem3D SetRotationSubMatrix(Matrix<double> r, CoordinateSystem3D coordinateSystem)
        {
            if (r.RowCount != 3 || r.ColumnCount != 3)
            {
                throw new ArgumentOutOfRangeException();
            }

            var cs = new CoordinateSystem3D(coordinateSystem.Origin, coordinateSystem.XAxis, coordinateSystem.YAxis, coordinateSystem.ZAxis);
            cs.SetSubMatrix(0, r.RowCount, 0, r.ColumnCount, r);
            return cs;
        }

        /// <summary>
        /// Gets a rotation submatrix from a coordinate system
        /// </summary>
        /// <param name="coordinateSystem">a coordinate system</param>
        /// <returns>A rotation matrix</returns>
        public static Matrix<double> GetRotationSubMatrix(CoordinateSystem3D coordinateSystem)
        {
            return coordinateSystem.SubMatrix(0, 3, 0, 3);
        }

        ////public CoordinateSystem SetCoordinateSystem(Matrix<double> matrix)
        ////{
        ////    if (matrix.ColumnCount != 4 || matrix.RowCount != 4)
        ////        throw new ArgumentException("Not a 4x4 matrix!");
        ////    return new CoordinateSystem(matrix);
        ////}

        /// <summary>
        /// Resets rotations preserves scales
        /// </summary>
        /// <returns>A coordinate system with reset rotation</returns>
        public CoordinateSystem3D ResetRotations()
        {
            var x = this.XAxis.Length * UnitVector3D.XAxis;
            var y = this.YAxis.Length * UnitVector3D.YAxis;
            var z = this.ZAxis.Length * UnitVector3D.ZAxis;
            return new CoordinateSystem3D(x, y, z, this.Origin);
        }

        /// <summary>
        /// Rotates a coordinate system around a vector
        /// </summary>
        /// <param name="about">The vector</param>
        /// <param name="angle">An angle</param>
        /// <returns>A rotated coordinate system</returns>
        public CoordinateSystem3D RotateCoordSysAroundVector(UnitVector3D about, Angle angle)
        {
            var rcs = Rotation(angle, about);
            return rcs.Transform(this);
        }

        /// <summary>
        /// Rotate without Reset
        /// </summary>
        /// <param name="yaw">The yaw</param>
        /// <param name="pitch">The pitch</param>
        /// <param name="roll">The roll</param>
        /// <returns>A rotated coordinate system</returns>
        public CoordinateSystem3D RotateNoReset(Angle yaw, Angle pitch, Angle roll)
        {
            var rcs = Rotation(yaw, pitch, roll);
            return rcs.Transform(this);
        }

        /// <summary>
        /// Translates a coordinate system
        /// </summary>
        /// <param name="v">a translation vector</param>
        /// <returns>A translated coordinate system</returns>
        public CoordinateSystem3D OffsetBy(Vector3D v)
        {
            return new CoordinateSystem3D(this.Origin + v, this.XAxis, this.YAxis, this.ZAxis);
        }

        /// <summary>
        /// Translates a coordinate system
        /// </summary>
        /// <param name="v">a translation vector</param>
        /// <returns>A translated coordinate system</returns>
        public CoordinateSystem3D OffsetBy(UnitVector3D v)
        {
            return new CoordinateSystem3D(this.Origin + v, this.XAxis, this.YAxis, this.ZAxis);
        }

        /// <summary>
        /// Transforms a ray according to this change matrix
        /// </summary>
        /// <param name="r">a ray</param>
        /// <returns>a transformed ray</returns>
        public Ray3D TransformToCoordSys(Ray3D r)
        {
            var p = r.ThroughPoint;
            var uv = r.Direction;

            // The position and the vector are transformed
            var baseChangeMatrix = this.BaseChangeMatrix;
            var point = baseChangeMatrix.Transform(p) + this.OffsetToBase;
            var direction = uv.TransformBy((Matrix<double>)baseChangeMatrix);
            return new Ray3D(point, direction);
        }

        /// <summary>
        /// Transforms a point according to this change matrix
        /// </summary>
        /// <param name="p">a point</param>
        /// <returns>a transformed point</returns>
        public Point3D TransformToCoordSys(Point3D p)
        {
            var baseChangeMatrix = this.BaseChangeMatrix;
            var point = baseChangeMatrix.Transform(p) + this.OffsetToBase;
            return point;
        }

        /// <summary>
        /// Transforms a ray according to the inverse of this change matrix
        /// </summary>
        /// <param name="r">a ray</param>
        /// <returns>a transformed ray</returns>
        public Ray3D TransformFromCoordSys(Ray3D r)
        {
            var p = r.ThroughPoint;
            var uv = r.Direction;

            // The position and the vector are transformed
            var point = this.BaseChangeMatrix.Invert().Transform(p) + this.OffsetToBase;
            var direction = this.BaseChangeMatrix.Invert().Transform(uv);
            return new Ray3D(point, direction);
        }

        /// <summary>
        /// Transforms a point according to the inverse of this change matrix
        /// </summary>
        /// <param name="p">a point</param>
        /// <returns>a transformed point</returns>
        public Point3D TransformFromCoordSys(Point3D p)
        {
            var point = this.BaseChangeMatrix.Invert().Transform(p) + this.OffsetToBase;
            return point;
        }

        /// <summary>
        /// Creates a rotation submatrix
        /// </summary>
        /// <param name="r">a matrix</param>
        /// <returns>a coordinate system</returns>
        public CoordinateSystem3D SetRotationSubMatrix(Matrix<double> r)
        {
            return SetRotationSubMatrix(r, this);
        }

        /// <summary>
        /// Returns a translation coordinate system
        /// </summary>
        /// <param name="v">a vector</param>
        /// <returns>a coordinate system</returns>
        public CoordinateSystem3D SetTranslation(Vector3D v)
        {
            return new CoordinateSystem3D(v.ToPoint3D(), this.XAxis, this.YAxis, this.ZAxis);
        }

        /// <summary>
        /// Returns a rotation sub matrix
        /// </summary>
        /// <returns>a rotation sub matrix</returns>
        public Matrix<double> GetRotationSubMatrix()
        {
            return GetRotationSubMatrix(this);
        }

        /// <summary>
        /// Transforms a vector and returns the transformed vector
        /// </summary>
        /// <param name="v">A vector</param>
        /// <returns>A transformed vector</returns>
        public Vector3D Transform(Vector3D v)
        {
            var v3 = Vector<double>.Build.Dense(new[] { v.X, v.Y, v.Z });
            this.GetRotationSubMatrix().Multiply(v3, v3);
            return new Vector3D(v3[0], v3[1], v3[2]);
        }

        /// <summary>
        /// Transforms a vector and returns the transformed vector
        /// </summary>
        /// <param name="v">a unit vector</param>
        /// <returns>A transformed vector</returns>
        public Vector3D Transform(UnitVector3D v)
        {
            var v3 = Vector<double>.Build.Dense(new[] { v.X, v.Y, v.Z });
            this.GetRotationSubMatrix().Multiply(v3, v3);
            return new Vector3D(v3[0], v3[1], v3[2]);
        }

        /// <summary>
        /// Transforms a point and returns the transformed point
        /// </summary>
        /// <param name="p">a point</param>
        /// <returns>A transformed point</returns>
        public Point3D Transform(Point3D p)
        {
            var v4 = Vector<double>.Build.Dense(new[] { p.X, p.Y, p.Z, 1 });
            this.Multiply(v4, v4);
            return new Point3D(v4[0], v4[1], v4[2]);
        }

        /// <summary>
        /// Transforms a coordinate system and returns the transformed
        /// </summary>
        /// <param name="cs">a coordinate system</param>
        /// <returns>A transformed coordinate system</returns>
        public CoordinateSystem3D Transform(CoordinateSystem3D cs)
        {
            return new CoordinateSystem3D(this.Multiply(cs));
        }

        /// <summary>
        /// Transforms a line and returns the transformed.
        /// </summary>
        /// <param name="l">A line</param>
        /// <returns>A transformed line</returns>
        [Obsolete("Use LineSegment3D, Obsolete from 2017-12-10")]
        public Line3D Transform(Line3D l)
        {
            return new Line3D(this.Transform(l.StartPoint), this.Transform(l.EndPoint));
        }

        /// <summary>
        /// Transforms a line segment.
        /// </summary>
        /// <param name="l">A line segment</param>
        /// <returns>The transformed line segment</returns>
        public LineSegment3D Transform(LineSegment3D l)
        {
            return new LineSegment3D(this.Transform(l.StartPoint), this.Transform(l.EndPoint));
        }

        /// <summary>
        /// Transforms a ray and returns the transformed.
        /// </summary>
        /// <param name="ray">A ray</param>
        /// <returns>A transformed ray</returns>
        public Ray3D Transform(Ray3D ray)
        {
            return new Ray3D(this.Transform(ray.ThroughPoint), this.Transform(ray.Direction));
        }

        /// <summary>
        /// Transforms a coordinate system
        /// </summary>
        /// <param name="matrix">a matrix</param>
        /// <returns>A transformed coordinate system</returns>
        public CoordinateSystem3D TransformBy(Matrix<double> matrix)
        {
            return new CoordinateSystem3D(matrix.Multiply(this));
        }

        /// <summary>
        /// Transforms this by the coordinate system and returns the transformed.
        /// </summary>
        /// <param name="cs">a coordinate system</param>
        /// <returns>a transformed coordinate system</returns>
        public CoordinateSystem3D TransformBy(CoordinateSystem3D cs)
        {
            return cs.Transform(this);
        }

        /// <summary>
        /// Inverts this coordinate system
        /// </summary>
        /// <returns>An inverted coordinate system</returns>
        public CoordinateSystem3D Invert()
        {
            return new CoordinateSystem3D(this.Inverse());
        }

        /// <summary>
        /// Returns a value to indicate if this CoordinateSystem is equivalent to a another CoordinateSystem
        /// </summary>
        /// <param name="other">The CoordinateSystem to compare against.</param>
        /// <param name="tolerance">A tolerance (epsilon) to adjust for floating point error</param>
        /// <returns>true if the CoordinateSystems are equal; otherwise false</returns>
        [Pure]
        public bool Equals(CoordinateSystem3D other, double tolerance)
        {
            if (this.Values.Length != other?.Values.Length)
            {
                return false;
            }

            for (var i = 0; i < this.Values.Length; i++)
            {
                if (Math.Abs(this.Values[i] - other.Values[i]) > tolerance)
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        [Pure]
        public bool Equals(CoordinateSystem3D other)
        {
            if (this.Values.Length != other?.Values.Length)
            {
                return false;
            }

            for (var i = 0; i < this.Values.Length; i++)
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (this.Values[i] != other.Values[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        [Pure]
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return obj is CoordinateSystem3D cs && this.Equals(cs);
        }

        /// <inheritdoc />
        [Pure]
        public override int GetHashCode() => HashCode.CombineMany(this.Values);

        /// <summary>
        /// Returns a string representation of the coordinate system
        /// </summary>
        /// <returns>a string</returns>
        public new string ToString()
        {
            return $"Origin: {this.Origin}, XAxis: {this.XAxis}, YAxis: {this.YAxis}, ZAxis: {this.ZAxis}";
        }

        /// <inheritdoc />
        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        /// <inheritdoc />
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            var e = (XElement)XNode.ReadFrom(reader);

            var xAxis = Vector3D.ReadFrom(e.SingleElementReader("XAxis"));
            this.SetColumn(0, new[] { xAxis.X, xAxis.Y, xAxis.Z, 0 });

            var yAxis = Vector3D.ReadFrom(e.SingleElementReader("YAxis"));
            this.SetColumn(1, new[] { yAxis.X, yAxis.Y, yAxis.Z, 0 });

            var zAxis = Vector3D.ReadFrom(e.SingleElementReader("ZAxis"));
            this.SetColumn(2, new[] { zAxis.X, zAxis.Y, zAxis.Z, 0 });

            var origin = Point3D.ReadFrom(e.SingleElementReader("Origin"));
            this.SetColumn(3, new[] { origin.X, origin.Y, origin.Z, 1 });
        }

        /// <inheritdoc />
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteElement("Origin", this.Origin);
            writer.WriteElement("XAxis", this.XAxis);
            writer.WriteElement("YAxis", this.YAxis);
            writer.WriteElement("ZAxis", this.ZAxis);
        }
    }
}
