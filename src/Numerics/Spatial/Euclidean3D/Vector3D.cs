using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Spatial.Internal;

namespace MathNet.Numerics.Spatial.Euclidean3D
{
    /// <summary>
    /// A struct representing a vector in 3D space
    /// </summary>
    [Serializable]
    public struct Vector3D : IXmlSerializable, IEquatable<Vector3D>, IEquatable<UnitVector3D>, IFormattable
    {
        /// <summary>
        /// The x component.
        /// </summary>
        public readonly double X;

        /// <summary>
        /// The y component.
        /// </summary>
        public readonly double Y;

        /// <summary>
        /// The z component.
        /// </summary>
        public readonly double Z;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3D"/> struct.
        /// </summary>
        /// <param name="x">The x component.</param>
        /// <param name="y">The y component.</param>
        /// <param name="z">The z component.</param>
        public Vector3D(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// Gets an invalid vector with no values
        /// </summary>
        public static Vector3D NaN => new Vector3D(double.NaN, double.NaN, double.NaN);

        /// <summary>
        /// Gets the Euclidean Norm.
        /// </summary>
        [Pure]
        public double Length => Math.Sqrt((this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z));

        /// <summary>
        /// Gets a unit vector orthogonal to this
        /// </summary>
        [Pure]
        public UnitVector3D Orthogonal
        {
            get
            {
                if (-this.X - this.Y > 0.1)
                {
                    return UnitVector3D.Create(this.Z, this.Z, -this.X - this.Y);
                }

                return UnitVector3D.Create(-this.Y - this.Z, this.X, this.X);
            }
        }

        /// <summary>
        /// Gets a dense matrix containing the cross product of this vector
        /// </summary>
        [Pure]
        internal Matrix<double> CrossProductMatrix => Matrix<double>.Build.Dense(3, 3, new[] { 0d, this.Z, -this.Y, -this.Z, 0d, this.X, this.Y, -this.X, 0d });

        /// <summary>
        /// Returns a value that indicates whether each pair of elements in two specified vectors is equal.
        /// </summary>
        /// <param name="left">The first vector to compare.</param>
        /// <param name="right">The second vector to compare.</param>
        /// <returns>True if the vectors are the same; otherwise false.</returns>
        public static bool operator ==(Vector3D left, Vector3D right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates whether any pair of elements in two specified vectors is not equal.
        /// </summary>
        /// <param name="left">The first vector to compare.</param>
        /// <param name="right">The second vector to compare.</param>
        /// <returns>True if the vectors are different; otherwise false.</returns>
        public static bool operator !=(Vector3D left, Vector3D right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Multiplies a Matrix by a Vector
        /// </summary>
        /// <param name="left">A Matrix</param>
        /// <param name="right">A Vector</param>
        /// <returns>A new vector</returns>
        [Obsolete("Not sure this is nice")]
        public static Vector<double> operator *(Matrix<double> left, Vector3D right)
        {
            return left * right.ToVector();
        }

        /// <summary>
        /// Multiplies a vector by a matrix
        /// </summary>
        /// <param name="left">A Vector</param>
        /// <param name="right">A Matrix</param>
        /// <returns>A new vector</returns>
        [Obsolete("Not sure this is nice")]
        public static Vector<double> operator *(Vector3D left, Matrix<double> right)
        {
            return left.ToVector() * right;
        }

        /// <summary>
        /// Returns the dot product of two vectors
        /// </summary>
        /// <param name="left">The first vector</param>
        /// <param name="right">The second vector</param>
        /// <returns>A scalar result</returns>
        public static double operator *(Vector3D left, Vector3D right)
        {
            return left.DotProduct(right);
        }

        /// <summary>
        /// Adds two vectors
        /// </summary>
        /// <param name="left">The first vector</param>
        /// <param name="right">The second vector</param>
        /// <returns>A new summed vector</returns>
        public static Vector3D operator +(Vector3D left, Vector3D right)
        {
            return new Vector3D(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        /// <summary>
        /// Subtracts two vectors
        /// </summary>
        /// <param name="left">The first vector</param>
        /// <param name="right">The second vector</param>
        /// <returns>A new difference vector</returns>
        public static Vector3D operator -(Vector3D left, Vector3D right)
        {
            return new Vector3D(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        /// <summary>
        /// Negates the vector
        /// </summary>
        /// <param name="v">A vector to negate</param>
        /// <returns>A new negated vector</returns>
        public static Vector3D operator -(Vector3D v)
        {
            return v.Negate();
        }

        /// <summary>
        /// Multiplies a vector by a scalar
        /// </summary>
        /// <param name="d">A scalar</param>
        /// <param name="v">A vector</param>
        /// <returns>A scaled vector</returns>
        public static Vector3D operator *(double d, Vector3D v)
        {
            return new Vector3D(d * v.X, d * v.Y, d * v.Z);
        }

        /// <summary>
        /// Divides a vector by a scalar
        /// </summary>
        /// <param name="v">A vector</param>
        /// <param name="d">A scalar</param>
        /// <returns>A scaled vector</returns>
        public static Vector3D operator /(Vector3D v, double d)
        {
            return new Vector3D(v.X / d, v.Y / d, v.Z / d);
        }

        /// <summary>
        /// Attempts to convert a string of the form x,y,z into a vector
        /// </summary>
        /// <param name="text">The string to be converted</param>
        /// <param name="result">A vector with the coordinates specified</param>
        /// <returns>True if <paramref name="text"/> could be parsed.</returns>
        public static bool TryParse(string text, out Vector3D result)
        {
            return TryParse(text, null, out result);
        }

        /// <summary>
        /// Attempts to convert a string of the form x,y,z into a vector
        /// </summary>
        /// <param name="text">The string to be converted</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/></param>
        /// <param name="result">A point at the coordinates specified</param>
        /// <returns>True if <paramref name="text"/> could be parsed.</returns>
        public static bool TryParse(string text, IFormatProvider formatProvider, out Vector3D result)
        {
            if (Text.TryParse3D(text, formatProvider, out var x, out var y, out var z))
            {
                result = new Vector3D(x, y, z);
                return true;
            }

            result = default(Vector3D);
            return false;
        }

        /// <summary>
        /// Attempts to convert a string of the form x,y,z into a vector
        /// </summary>
        /// <param name="value">The string to be converted</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/></param>
        /// <returns>A point at the coordinates specified</returns>
        public static Vector3D Parse(string value, IFormatProvider formatProvider = null)
        {
            if (TryParse(value, formatProvider, out var p))
            {
                return p;
            }

            throw new FormatException($"Could not parse a Vector3D from the string {value}");
        }

        /// <summary>
        /// Create a new <see cref="Vector3D"/> from a Math.NET Numerics vector of length 3.
        /// </summary>
        /// <param name="vector"> A vector with length 2 to populate the created instance with.</param>
        /// <returns> A <see cref="Vector3D"/></returns>
        public static Vector3D OfVector(Vector<double> vector)
        {
            if (vector.Count != 3)
            {
                throw new ArgumentException("The vector length must be 3 in order to convert it to a Vector3D");
            }

            return new Vector3D(vector.At(0), vector.At(1), vector.At(2));
        }

        /// <summary>
        /// Creates an <see cref="Vector3D"/> from an <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="reader">An <see cref="XmlReader"/> positioned at the node to read into this <see cref="Vector3D"/>.</param>
        /// <returns>An <see cref="Vector3D"/> that contains the data read from the reader.</returns>
        public static Vector3D ReadFrom(XmlReader reader)
        {
            return reader.ReadElementAs<Vector3D>();
        }

        ////public static explicit operator Vector3D(System.Windows.Media.Media3D.Vector3D v)
        ////{
        ////    return new Vector3D(v.X, v.Y, v.Z);
        ////}

        ////public static explicit operator System.Windows.Media.Media3D.Vector3D(Vector3D p)
        ////{
        ////    return new System.Windows.Media.Media3D.Vector3D(p.X, p.Y, p.Z);
        ////}

        /// <summary>
        /// Compute and return a unit vector from this vector
        /// </summary>
        /// <returns>a normalized unit vector</returns>
        [Pure]
        public UnitVector3D Normalize()
        {
            return UnitVector3D.Create(this.X, this.Y, this.Z);
        }

        /// <summary>
        /// Multiplies the current vector by a scalar
        /// </summary>
        /// <param name="scaleFactor">a scalar</param>
        /// <returns>A new scaled vector</returns>
        [Pure]
        public Vector3D ScaleBy(double scaleFactor)
        {
            return scaleFactor * this;
        }

        /// <summary>
        /// Projects the vector onto a plane
        /// </summary>
        /// <param name="planeToProjectOn">A geometric plane</param>
        /// <returns>A ray</returns>
        [Pure]
        public Ray3D ProjectOn(Plane3D planeToProjectOn)
        {
            return planeToProjectOn.Project(this);
        }

        /// <summary>
        /// Returns the Dot product of the current vector and a unit vector
        /// </summary>
        /// <param name="uv">A unit vector</param>
        /// <returns>Returns a new vector</returns>
        [Pure]
        public Vector3D ProjectOn(UnitVector3D uv)
        {
            var pd = this.DotProduct(uv);
            return pd * uv;
        }

        /// <summary>
        /// Computes whether or not this vector is parallel to another vector using the dot product method and comparing it
        /// to within a specified tolerance.
        /// </summary>
        /// <param name="other">The other <see cref="Vector3D"/></param>
        /// <param name="tolerance">A tolerance value for the dot product method.  Values below 2*Precision.DoublePrecision may cause issues.</param>
        /// <returns>true if the vector dot product is within the given tolerance of unity, false if it is not</returns>
        [Pure]
        public bool IsParallelTo(Vector3D other, double tolerance = 1e-10)
        {
            var @this = this.Normalize();
            return @this.IsParallelTo(other, tolerance);
        }

        /// <summary>
        /// Computes whether or not this vector is parallel to a unit vector using the dot product method and comparing it
        /// to within a specified tolerance.
        /// </summary>
        /// <param name="other">The other <see cref="UnitVector3D"/></param>
        /// <param name="tolerance">A tolerance value for the dot product method.  Values below 2*Precision.DoublePrecision may cause issues.</param>
        /// <returns>true if the vector dot product is within the given tolerance of unity, false if not</returns>
        [Pure]
        public bool IsParallelTo(UnitVector3D other, double tolerance = 1e-10)
        {
            return this.Normalize().IsParallelTo(other, tolerance);
        }

        /// <summary>
        /// Determine whether or not this vector is parallel to another vector within a given angle tolerance.
        /// </summary>
        /// <param name="other">The other <see cref="Vector3D"/></param>
        /// <param name="tolerance">The tolerance for when the vectors are considered parallel.</param>
        /// <returns>true if the vectors are parallel within the angle tolerance, false if they are not</returns>
        [Pure]
        public bool IsParallelTo(Vector3D other, Angle tolerance)
        {
            return this.Normalize().IsParallelTo(other, tolerance);
        }

        /// <summary>
        /// Determine whether or not this vector is parallel to a unit vector within a given angle tolerance.
        /// </summary>
        /// <param name="other">The other <see cref="UnitVector3D"/></param>
        /// <param name="tolerance">The tolerance for when the vectors are considered parallel.</param>
        /// <returns>true if the vectors are parallel within the angle tolerance, false if they are not</returns>
        [Pure]
        public bool IsParallelTo(UnitVector3D other, Angle tolerance)
        {
            var @this = this.Normalize();
            return @this.IsParallelTo(other, tolerance);
        }

        /// <summary>
        /// Computes whether or not this vector is perpendicular to another vector using the dot product method and
        /// comparing it to within a specified tolerance
        /// </summary>
        /// <param name="other">The other <see cref="Vector3D"/></param>
        /// <param name="tolerance">A tolerance value for the dot product method.  Values below 2*Precision.DoublePrecision may cause issues.</param>
        /// <returns>true if the vector dot product is within the given tolerance of zero, false if not</returns>
        [Pure]
        public bool IsPerpendicularTo(Vector3D other, double tolerance = 1e-6)
        {
            return Math.Abs(this.Normalize().DotProduct(other.Normalize())) < tolerance;
        }

        /// <summary>
        /// Computes whether or not this vector is perpendicular to another vector using the dot product method and
        /// comparing it to within a specified tolerance
        /// </summary>
        /// <param name="other">The other <see cref="UnitVector3D"/></param>
        /// <param name="tolerance">A tolerance value for the dot product method.  Values below 2*Precision.DoublePrecision may cause issues.</param>
        /// <returns>true if the vector dot product is within the given tolerance of zero, false if not</returns>
        [Pure]
        public bool IsPerpendicularTo(UnitVector3D other, double tolerance = 1e-6)
        {
            return Math.Abs(this.Normalize().DotProduct(other)) < tolerance;
        }

        /// <summary>
        /// Inverses the direction of the vector, equivalent to multiplying by -1
        /// </summary>
        /// <returns>A <see cref="Vector3D"/> pointing in the opposite direction.</returns>
        [Pure]
        public Vector3D Negate()
        {
            return new Vector3D(-1 * this.X, -1 * this.Y, -1 * this.Z);
        }

        /// <summary>
        /// Returns the dot product of two vectors.
        /// </summary>
        /// <param name="v">The second vector.</param>
        /// <returns>The dot product.</returns>
        [Pure]
        public double DotProduct(Vector3D v)
        {
            return (this.X * v.X) + (this.Y * v.Y) + (this.Z * v.Z);
        }

        /// <summary>
        /// Returns the dot product of two vectors.
        /// </summary>
        /// <param name="v">The second vector.</param>
        /// <returns>The dot product.</returns>
        [Pure]
        public double DotProduct(UnitVector3D v)
        {
            return (this.X * v.X) + (this.Y * v.Y) + (this.Z * v.Z);
        }

        /// <summary>
        /// Subtracts a vector from this
        /// </summary>
        /// <param name="v">a vector to subtract</param>
        /// <returns>A new vector</returns>
        [Pure]
        public Vector3D Subtract(Vector3D v)
        {
            return new Vector3D(this.X - v.X, this.Y - v.Y, this.Z - v.Z);
        }

        /// <summary>
        /// Adds a vector to this
        /// </summary>
        /// <param name="v">a vector to add</param>
        /// <returns>A new vector</returns>
        [Pure]
        public Vector3D Add(Vector3D v)
        {
            return new Vector3D(this.X + v.X, this.Y + v.Y, this.Z + v.Z);
        }

        /// <summary>
        /// Returns the cross product of this vector and <paramref name="other"/>
        /// </summary>
        /// <param name="other">A vector</param>
        /// <returns>A new vector with the cross product result</returns>
        [Pure]
        public Vector3D CrossProduct(Vector3D other)
        {
            var x = (this.Y * other.Z) - (this.Z * other.Y);
            var y = (this.Z * other.X) - (this.X * other.Z);
            var z = (this.X * other.Y) - (this.Y * other.X);
            var v = new Vector3D(x, y, z);
            return v;
        }

        /// <summary>
        /// Returns the cross product of this vector and <paramref name="other"/>
        /// </summary>
        /// <param name="other">A vector</param>
        /// <returns>A new vector with the cross product result</returns>
        [Pure]
        public Vector3D CrossProduct(UnitVector3D other)
        {
            var x = (this.Y * other.Z) - (this.Z * other.Y);
            var y = (this.Z * other.X) - (this.X * other.Z);
            var z = (this.X * other.Y) - (this.Y * other.X);
            var v = new Vector3D(x, y, z);
            return v;
        }

        /// <summary>
        /// Returns a dense Matrix with the unit tensor product
        /// [ux^2,  ux*uy, ux*uz],
        /// [ux*uy, uy^2,  uy*uz],
        /// [ux*uz, uy*uz, uz^2]
        /// </summary>
        /// <returns>a dense matrix</returns>
        [Pure]
        public Matrix<double> GetUnitTensorProduct()
        {
            // unitTensorProduct:matrix([ux^2,ux*uy,ux*uz],[ux*uy,uy^2,uy*uz],[ux*uz,uy*uz,uz^2]),
            var xy = this.X * this.Y;
            var xz = this.X * this.Z;
            var yz = this.Y * this.Z;
            return Matrix<double>.Build.Dense(3, 3, new[] { this.X * this.X, xy, xz, xy, this.Y * this.Y, yz, xz, yz, this.Z * this.Z });
        }

        /// <summary>
        /// Returns signed angle
        /// </summary>
        /// <param name="v">The vector to calculate the signed angle to </param>
        /// <param name="about">The vector around which to rotate to get the correct sign</param>
        /// <returns>A signed Angle</returns>
        [Pure]
        public Angle SignedAngleTo(Vector3D v, UnitVector3D about)
        {
            return this.Normalize().SignedAngleTo(v.Normalize(), about);
        }

        /// <summary>
        /// Returns signed angle
        /// </summary>
        /// <param name="v">The vector to calculate the signed angle to </param>
        /// <param name="about">The vector around which to rotate to get the correct sign</param>
        /// <returns>A signed angle</returns>
        [Pure]
        public Angle SignedAngleTo(UnitVector3D v, UnitVector3D about)
        {
            return this.Normalize().SignedAngleTo(v, about);
        }

        /// <summary>
        /// Compute the angle between this vector and another using the arccosine of the dot product.
        /// </summary>
        /// <param name="v">The other vector</param>
        /// <returns>The angle between the vectors, with a range between 0° and 180°</returns>
        [Pure]
        public Angle AngleTo(Vector3D v)
        {
            var uv1 = this.Normalize();
            var uv2 = v.Normalize();
            return uv1.AngleTo(uv2);
        }

        /// <summary>
        /// Compute the angle between this vector and a unit vector using the arccosine of the dot product.
        /// </summary>
        /// <param name="v">The other vector</param>
        /// <returns>The angle between the vectors, with a range between 0° and 180°</returns>
        [Pure]
        public Angle AngleTo(UnitVector3D v)
        {
            var uv = this.Normalize();
            return uv.AngleTo(v);
        }

        /// <summary>
        /// Returns a vector that is this vector rotated the signed angle around the about vector
        /// </summary>
        /// <param name="about">A vector to rotate about</param>
        /// <param name="angle">A signed angle</param>
        /// <returns>A rotated vector.</returns>
        [Pure]
        public Vector3D Rotate(Vector3D about, Angle angle)
        {
            return this.Rotate(about.Normalize(), angle);
        }

        /// <summary>
        /// Returns a vector that is this vector rotated the signed angle around the about vector
        /// </summary>
        /// <param name="about">A unit vector to rotate about</param>
        /// <param name="angle">A signed angle</param>
        /// <returns>A rotated vector.</returns>
        [Pure]
        public Vector3D Rotate(UnitVector3D about, Angle angle)
        {
            var cs = CoordinateSystem3D.Rotation(angle, about);
            return cs.Transform(this);
        }

        /// <summary>
        /// Returns a point equivalent to the vector
        /// </summary>
        /// <returns>A point</returns>
        public Point3D ToPoint3D()
        {
            return new Point3D(this.X, this.Y, this.Z);
        }

        /// <summary>
        /// Transforms the vector by a coordinate system and returns the transformed.
        /// </summary>
        /// <param name="coordinateSystem">A coordinate system</param>
        /// <returns>A new transformed vector</returns>
        [Pure]
        public Vector3D TransformBy(CoordinateSystem3D coordinateSystem)
        {
            return coordinateSystem.Transform(this);
        }

        /// <summary>
        /// Transforms a vector by multiplying it against a provided matrix
        /// </summary>
        /// <param name="m">The matrix to multiply</param>
        /// <returns>A new transformed vector</returns>
        [Pure]
        public Vector3D TransformBy(Matrix<double> m)
        {
            return Vector3D.OfVector(m.Multiply(this.ToVector()));
        }

        /// <summary>
        /// Convert to a Math.NET Numerics dense vector of length 3.
        /// </summary>
        /// <returns>A dense vector</returns>
        [Pure]
        public Vector<double> ToVector()
        {
            return Vector<double>.Build.Dense(new[] { this.X, this.Y, this.Z });
        }

        /// <inheritdoc />
        [Pure]
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns a string representation of this instance using the provided <see cref="IFormatProvider"/>
        /// </summary>
        /// <param name="provider">A <see cref="IFormatProvider"/></param>
        /// <returns>The string representation of this instance.</returns>
        [Pure]
        public string ToString(IFormatProvider provider)
        {
            return this.ToString(null, provider);
        }

        /// <inheritdoc />
        public string ToString(string format, IFormatProvider provider = null)
        {
            var numberFormatInfo = provider != null ? NumberFormatInfo.GetInstance(provider) : CultureInfo.InvariantCulture.NumberFormat;
            var separator = numberFormatInfo.NumberDecimalSeparator == "," ? ";" : ",";
            return string.Format(
                "({1}{0} {2}{0} {3})",
                separator,
                this.X.ToString(format, numberFormatInfo),
                this.Y.ToString(format, numberFormatInfo),
                this.Z.ToString(format, numberFormatInfo));
        }

        /// <summary>
        /// Returns a value to indicate if a pair of vectors are equal
        /// </summary>
        /// <param name="other">The vector to compare against.</param>
        /// <param name="tolerance">A tolerance (epsilon) to adjust for floating point error</param>
        /// <returns>true if the vectors are equal; otherwise false</returns>
        [Pure]
        public bool Equals(Vector3D other, double tolerance)
        {
            if (tolerance < 0)
            {
                throw new ArgumentException("epsilon < 0");
            }

            return Math.Abs(other.X - this.X) < tolerance &&
                   Math.Abs(other.Y - this.Y) < tolerance &&
                   Math.Abs(other.Z - this.Z) < tolerance;
        }

        /// <summary>
        /// Returns a value to indicate if this vector is equivalent to a given unit vector
        /// </summary>
        /// <param name="other">The unit vector to compare against.</param>
        /// <param name="tolerance">A tolerance (epsilon) to adjust for floating point error</param>
        /// <returns>true if the vectors are equal; otherwise false</returns>
        [Pure]
        public bool Equals(UnitVector3D other, double tolerance)
        {
            if (tolerance < 0)
            {
                throw new ArgumentException("epsilon < 0");
            }

            return Math.Abs(other.X - this.X) < tolerance &&
                   Math.Abs(other.Y - this.Y) < tolerance &&
                   Math.Abs(other.Z - this.Z) < tolerance;
        }

        /// <inheritdoc />
        [Pure]
        public bool Equals(Vector3D other)
        {
            return this.X.Equals(other.X) && this.Y.Equals(other.Y) && this.Z.Equals(other.Z);
        }

        /// <inheritdoc />
        [Pure]
        public bool Equals(UnitVector3D other)
        {
            return this.X.Equals(other.X) && this.Y.Equals(other.Y) && this.Z.Equals(other.Z);
        }

        /// <inheritdoc />
        [Pure]
        public override bool Equals(object obj)
        {
            return (obj is UnitVector3D u && this.Equals(u)) || (obj is Vector3D v && this.Equals(v));
        }

        /// <inheritdoc />
        [Pure]
        public override int GetHashCode() => HashCode.Combine(this.X, this.Y, this.Z);

        /// <inheritdoc />
        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        /// <inheritdoc />
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            if (reader.TryReadAttributeAsDouble("X", out var x) &&
                reader.TryReadAttributeAsDouble("Y", out var y) &&
                reader.TryReadAttributeAsDouble("Z", out var z))
            {
                reader.Skip();
                this = new Vector3D(x, y, z);
                return;
            }

            if (reader.TryReadChildElementsAsDoubles("X", "Y", "Z", out x, out y, out z))
            {
                reader.Skip();
                this = new Vector3D(x, y, z);
                return;
            }

            throw new XmlException($"Could not read a {this.GetType()}");
        }

        /// <inheritdoc />
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteAttribute("X", this.X);
            writer.WriteAttribute("Y", this.Y);
            writer.WriteAttribute("Z", this.Z);
        }
    }
}
