using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Spatial.Internal;

namespace MathNet.Numerics.Spatial.Euclidean3D
{
    /// <summary>
    /// A unit vector, this is used to describe a direction in 3D
    /// </summary>
    [Serializable]
    public struct UnitVector3D : IXmlSerializable, IEquatable<UnitVector3D>, IEquatable<Vector3D>, IFormattable
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
        /// Initializes a new instance of the <see cref="UnitVector3D"/> struct.
        /// The provided values are scaled to L2 norm == 1
        /// </summary>
        /// <param name="x">The x component.</param>
        /// <param name="y">The y component.</param>
        /// <param name="z">The z component.</param>
        [Obsolete("This constructor will be made private, prefer the factory method Create. Made obsolete 2017-12-05.")]
        public UnitVector3D(double x, double y, double z)
        {
            if (double.IsNaN(x) || double.IsInfinity(x))
            {
                throw new ArgumentOutOfRangeException(nameof(x), x, "Invalid value.");
            }

            if (double.IsNaN(y) || double.IsInfinity(y))
            {
                throw new ArgumentOutOfRangeException(nameof(y), y, "Invalid value.");
            }

            if (double.IsNaN(z) || double.IsInfinity(z))
            {
                throw new ArgumentOutOfRangeException(nameof(z), z, "Invalid value.");
            }

            var norm = Math.Sqrt((x * x) + (y * y) + (z * z));
            if (norm < float.Epsilon)
            {
                throw new ArgumentException("l < float.Epsilon");
            }

            this.X = x / norm;
            this.Y = y / norm;
            this.Z = z / norm;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitVector3D"/> struct.
        /// </summary>
        /// <param name="data">A list of 3 points</param>
        [Obsolete("This constructor will be removed. Made obsolete 2017-12-05.")]
        public UnitVector3D(IEnumerable<double> data)
            : this(data.ToArray())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitVector3D"/> struct.
        /// </summary>
        /// <param name="data">A list of 3 points</param>
        [Obsolete("This constructor will be removed. Made obsolete 2017-12-05.")]
        public UnitVector3D(double[] data)
            : this(data[0], data[1], data[2])
        {
            if (data.Length != 3)
            {
                throw new ArgumentException("Size must be 3");
            }
        }

        /// <summary>
        /// Gets the X axis
        /// </summary>
        public static UnitVector3D XAxis { get; } = Create(1, 0, 0);

        /// <summary>
        /// Gets the Y axis
        /// </summary>
        public static UnitVector3D YAxis { get; } = Create(0, 1, 0);

        /// <summary>
        /// Gets the z Axis
        /// </summary>
        public static UnitVector3D ZAxis { get; } = Create(0, 0, 1);

        /// <summary>
        /// Gets a vector orthogonal to this
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
        /// Gets the length of the vector not the count of elements
        /// </summary>
        [Pure]
        public double Length => 1;

        /// <summary>
        /// Gets the cross product matrix
        /// </summary>
        [Pure]
        internal Matrix<double> CrossProductMatrix => Matrix<double>.Build.Dense(3, 3, new[] { 0d, this.Z, -this.Y, -this.Z, 0d, this.X, this.Y, -this.X, 0d });

        /// <summary>
        /// Returns a value that indicates whether each pair of elements in two specified vectors is equal.
        /// </summary>
        /// <param name="left">The first vector to compare</param>
        /// <param name="right">The second vector to compare</param>
        /// <returns>True if the vectors are the same; otherwise false.</returns>
        public static bool operator ==(UnitVector3D left, UnitVector3D right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates whether each pair of elements in two specified vectors is equal.
        /// </summary>
        /// <param name="left">The first vector to compare</param>
        /// <param name="right">The second vector to compare</param>
        /// <returns>True if the vectors are the same; otherwise false.</returns>
        public static bool operator ==(Vector3D left, UnitVector3D right)
        {
            return left.Equals((object)right);
        }

        /// <summary>
        /// Returns a value that indicates whether each pair of elements in two specified vectors is equal.
        /// </summary>
        /// <param name="left">The first vector to compare</param>
        /// <param name="right">The second vector to compare</param>
        /// <returns>True if the vectors are the same; otherwise false.</returns>
        public static bool operator ==(UnitVector3D left, Vector3D right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates whether any pair of elements in two specified vectors is not equal.
        /// </summary>
        /// <param name="left">The first vector to compare.</param>
        /// <param name="right">The second vector to compare.</param>
        /// <returns>True if the vectors are different; otherwise false.</returns>
        public static bool operator !=(UnitVector3D left, UnitVector3D right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates whether any pair of elements in two specified vectors is not equal.
        /// </summary>
        /// <param name="left">The first vector to compare.</param>
        /// <param name="right">The second vector to compare.</param>
        /// <returns>True if the vectors are different; otherwise false.</returns>
        public static bool operator !=(Vector3D left, UnitVector3D right)
        {
            return !left.Equals((object)right);
        }

        /// <summary>
        /// Returns a value that indicates whether any pair of elements in two specified vectors is not equal.
        /// </summary>
        /// <param name="left">The first vector to compare.</param>
        /// <param name="right">The second vector to compare.</param>
        /// <returns>True if the vectors are different; otherwise false.</returns>
        public static bool operator !=(UnitVector3D left, Vector3D right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Adds two vectors
        /// </summary>
        /// <param name="v1">The first vector</param>
        /// <param name="v2">The second vector</param>
        /// <returns>A new summed vector</returns>
        public static Vector3D operator +(UnitVector3D v1, UnitVector3D v2)
        {
            return new Vector3D(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        /// <summary>
        /// Adds two vectors
        /// </summary>
        /// <param name="v1">The first vector</param>
        /// <param name="v2">The second vector</param>
        /// <returns>A new summed vector</returns>
        public static Vector3D operator +(Vector3D v1, UnitVector3D v2)
        {
            return new Vector3D(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        /// <summary>
        /// Adds two vectors
        /// </summary>
        /// <param name="v1">The first vector</param>
        /// <param name="v2">The second vector</param>
        /// <returns>A new summed vector</returns>
        public static Vector3D operator +(UnitVector3D v1, Vector3D v2)
        {
            return new Vector3D(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        /// <summary>
        /// Subtracts two vectors
        /// </summary>
        /// <param name="v1">The first vector</param>
        /// <param name="v2">The second vector</param>
        /// <returns>A new difference vector</returns>
        public static Vector3D operator -(UnitVector3D v1, UnitVector3D v2)
        {
            return new Vector3D(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        /// <summary>
        /// Subtracts two vectors
        /// </summary>
        /// <param name="v1">The first vector</param>
        /// <param name="v2">The second vector</param>
        /// <returns>A new difference vector</returns>
        public static Vector3D operator -(Vector3D v1, UnitVector3D v2)
        {
            return new Vector3D(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        /// <summary>
        /// Subtracts two vectors
        /// </summary>
        /// <param name="v1">The first vector</param>
        /// <param name="v2">The second vector</param>
        /// <returns>A new difference vector</returns>
        public static Vector3D operator -(UnitVector3D v1, Vector3D v2)
        {
            return new Vector3D(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        /// <summary>
        /// Negates the vector
        /// </summary>
        /// <param name="v">A vector to negate</param>
        /// <returns>A new negated vector</returns>
        public static Vector3D operator -(UnitVector3D v)
        {
            return new Vector3D(-1 * v.X, -1 * v.Y, -1 * v.Z);
        }

        /// <summary>
        /// Multiplies a vector by a scalar
        /// </summary>
        /// <param name="d">A scalar</param>
        /// <param name="v">A vector</param>
        /// <returns>A scaled vector</returns>
        public static Vector3D operator *(double d, UnitVector3D v)
        {
            return new Vector3D(d * v.X, d * v.Y, d * v.Z);
        }

        /// <summary>
        /// Divides a vector by a scalar
        /// </summary>
        /// <param name="v">A vector</param>
        /// <param name="d">A scalar</param>
        /// <returns>A scaled vector</returns>
        public static Vector3D operator /(UnitVector3D v, double d)
        {
            return new Vector3D(v.X / d, v.Y / d, v.Z / d);
        }

        /// <summary>
        /// Multiplies a Matrix by a Vector
        /// </summary>
        /// <param name="left">A Matrix</param>
        /// <param name="right">A Vector</param>
        /// <returns>A new vector</returns>
        [Obsolete("Not sure this is nice")]
        public static Vector<double> operator *(Matrix<double> left, UnitVector3D right)
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
        public static Vector<double> operator *(UnitVector3D left, Matrix<double> right)
        {
            return left.ToVector() * right;
        }

        /// <summary>
        /// Returns the dot product of two vectors
        /// </summary>
        /// <param name="left">The first vector</param>
        /// <param name="right">The second vector</param>
        /// <returns>A scalar result</returns>
        public static double operator *(UnitVector3D left, UnitVector3D right)
        {
            return left.DotProduct(right);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitVector3D"/> struct.
        /// The provided values are scaled to L2 norm == 1
        /// </summary>
        /// <param name="x">The x component.</param>
        /// <param name="y">The y component.</param>
        /// <param name="z">The z component.</param>
        /// <param name="tolerance">The allowed deviation from 1 for the L2-norm of x,y,z</param>
        /// <returns>The <see cref="UnitVector3D"/></returns>
        public static UnitVector3D Create(double x, double y, double z, double tolerance = double.PositiveInfinity)
        {
            var norm = Math.Sqrt((x * x) + (y * y) + (z * z));
            if (norm < float.Epsilon)
            {
                throw new InvalidOperationException("The Euclidean norm of x, y, z is less than float.Epsilon");
            }

            if (Math.Abs(norm - 1) > tolerance)
            {
                throw new InvalidOperationException("The Euclidean norm of x, y, z differs more than tolerance from 1");
            }
#pragma warning disable 618
            return new UnitVector3D(x / norm, y / norm, z / norm);
#pragma warning restore 618
        }

        /// <summary>
        /// Create a new <see cref="UnitVector3D"/> from a Math.NET Numerics vector of length 3.
        /// </summary>
        /// <param name="vector"> A vector with length 2 to populate the created instance with.</param>
        /// <returns> A <see cref="UnitVector3D"/></returns>
        public static UnitVector3D OfVector(Vector<double> vector)
        {
            if (vector.Count != 3)
            {
                throw new ArgumentException("The vector length must be 3 in order to convert it to a Vector3D");
            }

            return Create(vector.At(0), vector.At(1), vector.At(2));
        }

        /// <summary>
        /// Attempts to convert a string of the form x,y,z into a vector
        /// </summary>
        /// <param name="text">The string to be converted</param>
        /// <param name="result">A vector with the coordinates specified</param>
        /// <param name="tolerance">The tolerance for how big deviation from Length = 1 is accepted</param>
        /// <returns>True if <paramref name="text"/> could be parsed.</returns>
        public static bool TryParse(string text, out UnitVector3D result, double tolerance = 0.1)
        {
            return TryParse(text, null, out result, tolerance);
        }

        /// <summary>
        /// Attempts to convert a string of the form x,y,z into a unit vector
        /// First it is parsed to a vector then the length of the vector is compared to the tolerance and normalized if within.
        /// </summary>
        /// <param name="text">The string to be converted</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/></param>
        /// <param name="result">A point at the coordinates specified</param>
        /// <param name="tolerance">The tolerance for how big deviation from Length = 1 is accepted</param>
        /// <returns>True if <paramref name="text"/> could be parsed.</returns>
        public static bool TryParse(string text, IFormatProvider formatProvider, out UnitVector3D result, double tolerance = 0.1)
        {
            if (Text.TryParse3D(text, formatProvider, out var x, out var y, out var z))
            {
                var temp = new Vector3D(x, y, z);
                if (Math.Abs(temp.Length - 1) < tolerance)
                {
                    result = temp.Normalize();
                    return true;
                }
            }

            result = default(UnitVector3D);
            return false;
        }

        /// <summary>
        /// Attempts to convert a string of the form x,y,z into a vector
        /// </summary>
        /// <param name="value">The string to be converted</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/></param>
        /// <param name="tolerance">The tolerance for how big deviation from Length = 1 is accepted</param>
        /// <returns>A point at the coordinates specified</returns>
        public static UnitVector3D Parse(string value, IFormatProvider formatProvider = null, double tolerance = 0.1)
        {
            if (TryParse(value, formatProvider, out var p, tolerance))
            {
                return p;
            }

            throw new FormatException($"Could not parse a UnitVector3D from the string {value}");
        }

        /// <summary>
        /// Creates an <see cref="UnitVector3D"/> from an <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="reader">An <see cref="XmlReader"/> positioned at the node to read into this <see cref="UnitVector3D"/>.</param>
        /// <returns>An <see cref="UnitVector3D"/> that contains the data read from the reader.</returns>
        public static UnitVector3D ReadFrom(XmlReader reader)
        {
            return reader.ReadElementAs<UnitVector3D>();
        }

        /// <summary>
        /// Scale this instance by <paramref name="factor"/>
        /// </summary>
        /// <param name="factor">The plane to project on.</param>
        /// <returns>The projected <see cref="Ray3D"/></returns>
        [Pure]
        public Vector3D ScaleBy(double factor)
        {
            return factor * this;
        }

        /// <summary>
        /// Project this instance onto the plane
        /// </summary>
        /// <param name="plane">The plane to project on.</param>
        /// <returns>The projected <see cref="Ray3D"/></returns>
        [Pure]
        public Ray3D ProjectOn(Plane3D plane)
        {
            return plane.Project(this.ToVector3D());
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
            return pd * this;
        }

        /// <summary>
        /// Computes whether or not this vector is parallel to another vector using the dot product method and comparing it
        /// to within a specified tolerance.
        /// </summary>
        /// <param name="othervector">The other <see cref="Vector3D"/></param>
        /// <param name="tolerance">A tolerance value for the dot product method.  Values below 2*Precision.DoublePrecision may cause issues.</param>
        /// <returns>true if the vector dot product is within the given tolerance of unity, false if it is not</returns>
        [Pure]
        public bool IsParallelTo(Vector3D othervector, double tolerance = 1e-10)
        {
            var other = othervector.Normalize();
            return this.IsParallelTo(other, tolerance);
        }

        /// <summary>
        /// Computes whether or not this vector is parallel to a unit vector using the dot product method and comparing it
        /// to within a specified tolerance.
        /// </summary>
        /// <param name="othervector">The other <see cref="UnitVector3D"/></param>
        /// <param name="tolerance">A tolerance value for the dot product method.  Values below 2*Precision.DoublePrecision may cause issues.</param>
        /// <returns>true if the vector dot product is within the given tolerance of unity, false if not</returns>
        [Pure]
        public bool IsParallelTo(UnitVector3D othervector, double tolerance = 1e-10)
        {
            // This is the master method for all Vector3D and UnitVector3D IsParallelTo comparisons.  Everything else
            // ends up here sooner or later.
            var dp = Math.Abs(this.DotProduct(othervector));
            return Math.Abs(1 - dp) <= tolerance;
        }

        /// <summary>
        /// Determine whether or not this vector is parallel to another vector within a given angle tolerance.
        /// </summary>
        /// <param name="othervector">The other <see cref="Vector3D"/></param>
        /// <param name="angleTolerance">The tolerance for when the vectors are considered parallel.</param>
        /// <returns>true if the vectors are parallel within the angle tolerance, false if they are not</returns>
        [Pure]
        public bool IsParallelTo(UnitVector3D othervector, Angle angleTolerance)
        {
            // Compute the angle between these vectors
            var angle = this.AngleTo(othervector);

            // Compute the 180° opposite of the angle
            var opposite = Angle.FromDegrees(180) - angle;

            // Check against the smaller of the two
            return ((angle < opposite) ? angle : opposite) < angleTolerance;
        }

        /// <summary>
        /// Determine whether or not this vector is parallel to a unit vector within a given angle tolerance.
        /// </summary>
        /// <param name="othervector">The other <see cref="UnitVector3D"/></param>
        /// <param name="angleTolerance">The tolerance for when the vectors are considered parallel.</param>
        /// <returns>true if the vectors are parallel within the angle tolerance, false if they are not</returns>
        [Pure]
        public bool IsParallelTo(Vector3D othervector, Angle angleTolerance)
        {
            var other = othervector.Normalize();
            return this.IsParallelTo(other, angleTolerance);
        }

        /// <summary>
        /// Computes whether or not this vector is perpendicular to another vector using the dot product method and
        /// comparing it to within a specified tolerance
        /// </summary>
        /// <param name="othervector">The other <see cref="Vector3D"/></param>
        /// <param name="tolerance">A tolerance value for the dot product method.  Values below 2*Precision.DoublePrecision may cause issues.</param>
        /// <returns>true if the vector dot product is within the given tolerance of zero, false if not</returns>
        [Pure]
        public bool IsPerpendicularTo(Vector3D othervector, double tolerance = 1e-10)
        {
            var other = othervector.Normalize();
            return Math.Abs(this.DotProduct(other)) < tolerance;
        }

        /// <summary>
        /// Computes whether or not this vector is perpendicular to another vector using the dot product method and
        /// comparing it to within a specified tolerance
        /// </summary>
        /// <param name="othervector">The other <see cref="UnitVector3D"/></param>
        /// <param name="tolerance">A tolerance value for the dot product method.  Values below 2*Precision.DoublePrecision may cause issues.</param>
        /// <returns>true if the vector dot product is within the given tolerance of zero, false if not</returns>
        [Pure]
        public bool IsPerpendicularTo(UnitVector3D othervector, double tolerance = 1e-10)
        {
            return Math.Abs(this.DotProduct(othervector)) < tolerance;
        }

        /// <summary>
        /// Inverses the direction of the vector, equivalent to multiplying by -1
        /// </summary>
        /// <returns>A <see cref="Vector3D"/> pointing in the opposite direction.</returns>
        [Pure]
        public UnitVector3D Negate()
        {
            return UnitVector3D.Create(-1 * this.X, -1 * this.Y, -1 * this.Z);
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
            var dp = (this.X * v.X) + (this.Y * v.Y) + (this.Z * v.Z);
            return Math.Max(-1, Math.Min(dp, 1));
        }

        /// <summary>
        /// Subtracts a vector from this
        /// </summary>
        /// <param name="v">a vector to subtract</param>
        /// <returns>A new vector</returns>
        [Obsolete("Use - instead")]
        public Vector3D Subtract(UnitVector3D v)
        {
            return new Vector3D(this.X - v.X, this.Y - v.Y, this.Z - v.Z);
        }

        /// <summary>
        /// Adds a vector to this
        /// </summary>
        /// <param name="v">a vector to add</param>
        /// <returns>A new vector</returns>
        [Obsolete("Use + instead")]
        public Vector3D Add(UnitVector3D v)
        {
            return new Vector3D(this.X + v.X, this.Y + v.Y, this.Z + v.Z);
        }

        /// <summary>
        /// Returns the cross product of this vector and a vector
        /// </summary>
        /// <param name="other">A vector</param>
        /// <returns>A new vector with the cross product result</returns>
        [Pure]
        public UnitVector3D CrossProduct(UnitVector3D other)
        {
            var x = (this.Y * other.Z) - (this.Z * other.Y);
            var y = (this.Z * other.X) - (this.X * other.Z);
            var z = (this.X * other.Y) - (this.Y * other.X);
            var v = Create(x, y, z);
            return v;
        }

        /// <summary>
        /// Returns the cross product of this vector and a unit vector
        /// </summary>
        /// <param name="inVector3D">A vector</param>
        /// <returns>A new vector with the cross product result</returns>
        [Pure]
        public Vector3D CrossProduct(Vector3D inVector3D)
        {
            var x = (this.Y * inVector3D.Z) - (this.Z * inVector3D.Y);
            var y = (this.Z * inVector3D.X) - (this.X * inVector3D.Z);
            var z = (this.X * inVector3D.Y) - (this.Y * inVector3D.X);
            var v = new Vector3D(x, y, z);
            return v;
        }

        /// <summary>
        /// Returns a dense Matrix with the unit tensor product
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
            return this.SignedAngleTo(v.Normalize(), about);
        }

        /// <summary>
        /// Returns signed angle
        /// </summary>
        /// <param name="v">The vector to calculate the signed angle to </param>
        /// <param name="about">The vector around which to rotate to get the correct sign</param>
        /// <returns>A signed Angle</returns>
        [Pure]
        public Angle SignedAngleTo(UnitVector3D v, UnitVector3D about)
        {
            if (this.IsParallelTo(about))
            {
                throw new ArgumentException("FromVector parallel to aboutVector");
            }

            if (v.IsParallelTo(about))
            {
                throw new ArgumentException("FromVector parallel to aboutVector");
            }

            var rp = new Plane3D(new Point3D(0, 0, 0), about);
            var pfv = this.ProjectOn(rp).Direction;
            var ptv = v.ProjectOn(rp).Direction;
            var dp = pfv.DotProduct(ptv);
            if (Math.Abs(dp - 1) < 1E-15)
            {
                return Angle.FromRadians(0);
            }

            if (Math.Abs(dp + 1) < 1E-15)
            {
                return Angle.FromRadians(Math.PI);
            }

            var angle = Math.Acos(dp);
            var cpv = pfv.CrossProduct(ptv);
            var sign = cpv.DotProduct(rp.Normal);
            var signedAngle = sign * angle;
            return Angle.FromRadians(signedAngle);
        }

        /// <summary>
        /// The nearest angle between the vectors
        /// </summary>
        /// <param name="v">The other vector</param>
        /// <returns>The angle</returns>
        [Pure]
        public Angle AngleTo(Vector3D v)
        {
            return this.AngleTo(v.Normalize());
        }

        /// <summary>
        /// Compute the angle between this vector and a unit vector using the arccosine of the dot product.
        /// </summary>
        /// <param name="v">The other vector</param>
        /// <returns>The angle between the vectors, with a range between 0° and 180°</returns>
        [Pure]
        public Angle AngleTo(UnitVector3D v)
        {
            var dp = this.DotProduct(v);
            var angle = Math.Acos(dp);
            return Angle.FromRadians(angle);
        }

        /// <summary>
        /// Returns a vector that is this vector rotated the signed angle around the about vector
        /// </summary>
        /// <param name="about">The vector to rotate around.</param>
        /// <param name="angle">The angle positive according to right hand rule.</param>
        /// <returns>A rotated vector.</returns>
        [Pure]
        public UnitVector3D Rotate(UnitVector3D about, Angle angle)
        {
            var cs = CoordinateSystem3D.Rotation(angle, about);
            return cs.Transform(this).Normalize();
        }

        /// <summary>
        /// Returns a point equivalent to the vector
        /// </summary>
        /// <returns>A point</returns>
        [Pure]
        public Point3D ToPoint3D()
        {
            return new Point3D(this.X, this.Y, this.Z);
        }

        /// <summary>
        /// Returns a Vector3D equivalent to this unit vector
        /// </summary>
        /// <returns>A vector</returns>
        [Pure]
        public Vector3D ToVector3D()
        {
            return new Vector3D(this.X, this.Y, this.Z);
        }

        /// <summary>
        /// Transforms the vector by a coordinate system and returns the transformed.
        /// </summary>
        /// <param name="coordinateSystem">A coordinate system</param>
        /// <returns>A new transformed vector</returns>
        [Pure]
        public Vector3D TransformBy(CoordinateSystem3D coordinateSystem)
        {
            return coordinateSystem.Transform(this.ToVector3D());
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

        /// <inheritdoc/>
        [Pure]
        public string ToString(string format, IFormatProvider provider = null)
        {
            var numberFormatInfo = provider != null ? NumberFormatInfo.GetInstance(provider) : CultureInfo.InvariantCulture.NumberFormat;
            var separator = numberFormatInfo.NumberDecimalSeparator == "," ? ";" : ",";
            return string.Format("({0}{1} {2}{1} {3})", this.X.ToString(format, numberFormatInfo), separator, this.Y.ToString(format, numberFormatInfo), this.Z.ToString(format, numberFormatInfo));
        }

        /// <summary>
        /// Returns a value to indicate if a pair of vectors are equal
        /// </summary>
        /// <param name="other">The vector to compare against.</param>
        /// <param name="tolerance">A tolerance (epsilon) to adjust for floating point error</param>
        /// <returns>True if the vectors are equal; otherwise false</returns>
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

        /// <summary>
        /// Returns a value to indicate if a pair of vectors are equal
        /// </summary>
        /// <param name="other">The vector to compare against.</param>
        /// <param name="tolerance">A tolerance (epsilon) to adjust for floating point error</param>
        /// <returns>True if the vectors are equal; otherwise false</returns>
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

        /// <inheritdoc/>
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
            bool TryGetUnitVector(double xv, double yv, double zv, out UnitVector3D result)
            {
                var temp = new Vector3D(xv, yv, zv);
                if (Math.Abs(temp.Length - 1) < 1E-3)
                {
                    result = temp.Normalize();
                    return true;
                }

                result = default(UnitVector3D);
                return false;
            }

            if (reader.TryReadAttributeAsDouble("X", out var x) &&
                reader.TryReadAttributeAsDouble("Y", out var y) &&
                reader.TryReadAttributeAsDouble("Z", out var z) &&
                TryGetUnitVector(x, y, z, out var uv))
            {
                reader.Skip();
                this = uv;
                return;
            }

            if (reader.TryReadChildElementsAsDoubles("X", "Y", "Z", out x, out y, out z) &&
                TryGetUnitVector(x, y, z, out uv))
            {
                reader.Skip();
                this = uv;
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

        /// <summary>
        /// Returns the dot product of this vector with a second
        /// </summary>
        /// <param name="v">a second vector</param>
        /// <returns>The dot product</returns>
        [Pure]
        internal double DotProduct(Point3D v)
        {
            return (this.X * v.X) + (this.Y * v.Y) + (this.Z * v.Z);
        }
    }
}
