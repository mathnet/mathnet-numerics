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

namespace MathNet.Numerics.Spatial.Euclidean2D
{
    /// <summary>
    /// A struct representing a vector in 2D space
    /// </summary>
    [Serializable]
    public struct Vector2D : IXmlSerializable, IEquatable<Vector2D>, IFormattable
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
        /// Initializes a new instance of the <see cref="Vector2D"/> struct.
        /// </summary>
        /// <param name="x">The x component.</param>
        /// <param name="y">The y component.</param>
        public Vector2D(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2D"/> struct.
        /// Creates a vector with length r rotated a counterclockwise from X-Axis
        /// </summary>
        /// <param name="r">The radius</param>
        /// <param name="a">The angle</param>
        [Obsolete("This constructor will be removed, use FromPolar. Made obsolete 2017-12-03.")]
        //// ReSharper disable once UnusedMember.Global
        public Vector2D(double r, Angle a)
            : this(r * Math.Cos(a.Radians), r * Math.Sin(a.Radians))
        {
            if (r < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(r), r, "Expected a radius greater than or equal to zero.");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2D"/> struct.
        /// </summary>
        /// <param name="data">A list of 2 doubles</param>
        [Obsolete("This constructor will be removed. Made obsolete 2017-12-03.")]
        //// ReSharper disable once UnusedMember.Global
        public Vector2D(IEnumerable<double> data)
            : this(data.ToArray())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2D"/> struct.
        /// </summary>
        /// <param name="data">A list of 2 doubles</param>
        [Obsolete("This constructor will be removed. Made obsolete 2017-12-03.")]
        public Vector2D(double[] data)
            : this(data[0], data[1])
        {
            if (data.Length != 2)
            {
                throw new ArgumentException("data.Length != 2!");
            }
        }

        /// <summary>
        /// Gets a vector representing the X Axis
        /// </summary>
        public static Vector2D XAxis { get; } = new Vector2D(1, 0);

        /// <summary>
        /// Gets a vector representing the Y Axis
        /// </summary>
        public static Vector2D YAxis { get; } = new Vector2D(0, 1);

        /// <summary>
        /// Gets the length of the vector
        /// </summary>
        [Pure]
        public double Length => Math.Sqrt((this.X * this.X) + (this.Y * this.Y));

        /// <summary>
        /// Returns a value that indicates whether each pair of elements in two specified vectors is equal.
        /// </summary>
        /// <param name="left">The first vector to compare.</param>
        /// <param name="right">The second vector to compare.</param>
        /// <returns>True if the vectors are the same; otherwise false.</returns>
        public static bool operator ==(Vector2D left, Vector2D right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates whether any pair of elements in two specified vectors is not equal.
        /// </summary>
        /// <param name="left">The first vector to compare.</param>
        /// <param name="right">The second vector to compare.</param>
        /// <returns>True if the vectors are different; otherwise false.</returns>
        public static bool operator !=(Vector2D left, Vector2D right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Adds two vectors
        /// </summary>
        /// <param name="left">The first vector</param>
        /// <param name="right">The second vector</param>
        /// <returns>A new summed vector</returns>
        public static Vector2D operator +(Vector2D left, Vector2D right)
        {
            return left.Add(right);
        }

        /// <summary>
        /// Subtracts two vectors
        /// </summary>
        /// <param name="left">The first vector</param>
        /// <param name="right">The second vector</param>
        /// <returns>A new difference vector</returns>
        public static Vector2D operator -(Vector2D left, Vector2D right)
        {
            return left.Subtract(right);
        }

        /// <summary>
        /// Negates the vector
        /// </summary>
        /// <param name="v">A vector to negate</param>
        /// <returns>A new negated vector</returns>
        public static Vector2D operator -(Vector2D v)
        {
            return v.Negate();
        }

        /// <summary>
        /// Multiplies a vector by a scalar
        /// </summary>
        /// <param name="d">A scalar</param>
        /// <param name="v">A vector</param>
        /// <returns>A scaled vector</returns>
        public static Vector2D operator *(double d, Vector2D v)
        {
            return new Vector2D(d * v.X, d * v.Y);
        }

        /// <summary>
        /// Multiplies a vector by a scalar
        /// </summary>
        /// <param name="v">A vector</param>
        /// <param name="d">A scalar</param>
        /// <returns>A scaled vector</returns>
        public static Vector2D operator *(Vector2D v, double d)
        {
            return d * v;
        }

        /// <summary>
        /// Divides a vector by a scalar
        /// </summary>
        /// <param name="v">A vector</param>
        /// <param name="d">A scalar</param>
        /// <returns>A scaled vector</returns>
        public static Vector2D operator /(Vector2D v, double d)
        {
            return new Vector2D(v.X / d, v.Y / d);
        }

        /// <summary>
        /// Creates a Vector from Polar coordinates
        /// </summary>
        /// <param name="radius">The distance of the point from the origin</param>
        /// <param name="angle">The angle of the point as measured from the X Axis</param>
        /// <returns>A vector.</returns>
        public static Vector2D FromPolar(double radius, Angle angle)
        {
            if (radius < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radius), radius, "Expected a radius greater than or equal to zero.");
            }

            return new Vector2D(
                radius * Math.Cos(angle.Radians),
                radius * Math.Sin(angle.Radians));
        }

        /// <summary>
        /// Attempts to convert a string of the form x,y into a point
        /// </summary>
        /// <param name="text">The string to be converted</param>
        /// <param name="result">A point at the coordinates specified</param>
        /// <returns>True if <paramref name="text"/> could be parsed.</returns>
        public static bool TryParse(string text, out Vector2D result)
        {
            return TryParse(text, null, out result);
        }

        /// <summary>
        /// Attempts to convert a string of the form x,y into a point
        /// </summary>
        /// <param name="text">The string to be converted</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/></param>
        /// <param name="result">A point at the coordinates specified</param>
        /// <returns>True if <paramref name="text"/> could be parsed.</returns>
        public static bool TryParse(string text, IFormatProvider formatProvider, out Vector2D result)
        {
            if (Text.TryParse2D(text, formatProvider, out var x, out var y))
            {
                result = new Vector2D(x, y);
                return true;
            }

            result = default(Vector2D);
            return false;
        }

        /// <summary>
        /// Attempts to convert a string of the form x,y into a point
        /// </summary>
        /// <param name="value">The string to be converted</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/></param>
        /// <returns>A point at the coordinates specified</returns>
        public static Vector2D Parse(string value, IFormatProvider formatProvider = null)
        {
            if (TryParse(value, formatProvider, out var p))
            {
                return p;
            }

            throw new FormatException($"Could not parse a Vector2D from the string {value}");
        }

        /// <summary>
        /// Creates an <see cref="Vector2D"/> from an <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="reader">An <see cref="XmlReader"/> positioned at the node to read into this <see cref="Vector2D"/>.</param>
        /// <returns>An <see cref="Vector2D"/> that contains the data read from the reader.</returns>
        public static Vector2D ReadFrom(XmlReader reader)
        {
            return reader.ReadElementAs<Vector2D>();
        }

        /// <summary>
        /// Create a new <see cref="Vector2D"/> from a Math.NET Numerics vector of length 2.
        /// </summary>
        /// <param name="vector"> A vector with length 2 to populate the created instance with.</param>
        /// <returns> A <see cref="Vector2D"/></returns>
        [Pure]
        public static Vector2D OfVector(Vector<double> vector)
        {
            if (vector.Count != 2)
            {
                throw new ArgumentException("The vector length must be 2 in order to convert it to a Vector2D");
            }

            return new Vector2D(vector.At(0), vector.At(1));
        }

        /// <summary>
        /// Computes whether or not this vector is perpendicular to <paramref name="other"/> vector by:
        /// 1. Normalizing both
        /// 2. Computing the dot product.
        /// 3. Comparing 1- Math.Abs(dot product) to <paramref name="tolerance"/>
        /// </summary>
        /// <param name="other">The other <see cref="Vector2D"/></param>
        /// <param name="tolerance">The tolerance for when vectors are said to be parallel</param>
        /// <returns>True if the vector dot product is within the given double tolerance of unity, false if not</returns>
        [Pure]
        public bool IsParallelTo(Vector2D other, double tolerance = 1e-10)
        {
            var dp = Math.Abs(this.Normalize().DotProduct(other.Normalize()));
            return Math.Abs(1 - dp) <= tolerance;
        }

        /// <summary>
        /// Computes whether or not this vector is parallel to another vector within a given angle tolerance.
        /// </summary>
        /// <param name="other">The other <see cref="Vector2D"/></param>
        /// <param name="tolerance">The tolerance for when vectors are said to be parallel</param>
        /// <returns>True if the vectors are parallel within the angle tolerance, false if they are not</returns>
        [Pure]
        public bool IsParallelTo(Vector2D other, Angle tolerance)
        {
            // Compute the angle between these vectors
            var angle = this.AngleTo(other);
            if (angle < tolerance)
            {
                return true;
            }

            // Compute the 180° opposite of the angle
            return Angle.FromRadians(Math.PI) - angle < tolerance;
        }

        /// <summary>
        /// Computes whether or not this vector is perpendicular to <paramref name="other"/> vector by:
        /// 1. Normalizing both
        /// 2. Computing the dot product.
        /// 3. Comparing Math.Abs(dot product) to <paramref name="tolerance"/>
        /// </summary>
        /// <param name="other">The other <see cref="Vector2D"/></param>
        /// <param name="tolerance">The tolerance for when vectors are said to be parallel</param>
        /// <returns>True if the vector dot product is within the given double tolerance of unity, false if not</returns>
        [Pure]
        public bool IsPerpendicularTo(Vector2D other, double tolerance = 1e-10)
        {
            return Math.Abs(this.Normalize().DotProduct(other.Normalize())) < tolerance;
        }

        /// <summary>
        /// Computes whether or not this vector is parallel to another vector within a given angle tolerance.
        /// </summary>
        /// <param name="other">The other <see cref="Vector2D"/></param>
        /// <param name="tolerance">The tolerance for when vectors are said to be parallel</param>
        /// <returns>True if the vectors are parallel within the angle tolerance, false if they are not</returns>
        [Pure]
        public bool IsPerpendicularTo(Vector2D other, Angle tolerance)
        {
            var angle = this.AngleTo(other);
            const double Perpendicular = Math.PI / 2;
            return Math.Abs(angle.Radians - Perpendicular) < tolerance.Radians;
        }

        /// <summary>
        /// Compute the signed angle to another vector.
        /// </summary>
        /// <param name="other">The other <see cref="Vector2D"/></param>
        /// <param name="clockWise">Positive in clockwise direction</param>
        /// <param name="returnNegative">When true and the result is > 180° a negative value is returned</param>
        /// <returns>The angle between the vectors.</returns>
        [Pure]
        public Angle SignedAngleTo(Vector2D other, bool clockWise = false, bool returnNegative = false)
        {
            var sign = clockWise ? -1 : 1;
            var a1 = Math.Atan2(this.Y, this.X);
            if (a1 < 0)
            {
                a1 += 2 * Math.PI;
            }

            var a2 = Math.Atan2(other.Y, other.X);
            if (a2 < 0)
            {
                a2 += 2 * Math.PI;
            }

            var a = sign * (a2 - a1);
            if (a < 0 && !returnNegative)
            {
                a += 2 * Math.PI;
            }

            if (a > Math.PI && returnNegative)
            {
                a -= 2 * Math.PI;
            }

            return Angle.FromRadians(a);
        }

        /// <summary>
        /// Compute the angle between this vector and another using the arccosine of the dot product.
        /// </summary>
        /// <param name="other">The other <see cref="Vector2D"/></param>
        /// <returns>The angle between vectors, with a range between 0° and 180°</returns>
        [Pure]
        public Angle AngleTo(Vector2D other)
        {
            return Angle.FromRadians(
                Math.Abs(
                    Math.Atan2(
                        (this.X * other.Y) - (other.X * this.Y),
                        (this.X * other.X) + (this.Y * other.Y))));
        }

        /// <summary>
        /// Rotates a Vector by an angle
        /// </summary>
        /// <param name="angle">The angle.</param>
        /// <returns>A new rotated vector.</returns>
        [Pure]
        public Vector2D Rotate(Angle angle)
        {
            var cs = Math.Cos(angle.Radians);
            var sn = Math.Sin(angle.Radians);
            var x = (this.X * cs) - (this.Y * sn);
            var y = (this.X * sn) + (this.Y * cs);
            return new Vector2D(x, y);
        }

        /// <summary>
        /// Perform the dot product on a pair of vectors
        /// </summary>
        /// <param name="other">The second vector</param>
        /// <returns>The result of the dot product.</returns>
        [Pure]
        public double DotProduct(Vector2D other)
        {
            return (this.X * other.X) + (this.Y * other.Y);
        }

        /// <summary>
        /// Performs the 2D 'cross product' as if the 2D vectors were really 3D vectors in the z=0 plane, returning
        /// the scalar magnitude and direction of the resulting z value.
        /// Formula: (this.X * other.Y) - (this.Y * other.X)
        /// </summary>
        /// <param name="other">The other <see cref="Vector2D"/></param>
        /// <returns>(this.X * other.Y) - (this.Y * other.X)</returns>
        [Pure]
        public double CrossProduct(Vector2D other)
        {
            // Though the cross product is undefined in 2D space, this is a useful mathematical operation to
            // determine angular direction and to compute the area of 2D shapes
            return (this.X * other.Y) - (this.Y * other.X);
        }

        /// <summary>
        /// Projects this vector onto another vector
        /// </summary>
        /// <param name="other">The other <see cref="Vector2D"/></param>
        /// <returns>A <see cref="Vector2D"/> representing this vector projected on <paramref name="other"/></returns>
        [Pure]
        public Vector2D ProjectOn(Vector2D other)
        {
            return other * (this.DotProduct(other) / other.DotProduct(other));
        }

        /// <summary>
        /// Creates a new unit vector from the existing vector.
        /// </summary>
        /// <returns>A new unit vector in the same direction as the original vector</returns>
        [Pure]
        public Vector2D Normalize()
        {
            var l = this.Length;
            return new Vector2D(this.X / l, this.Y / l);
        }

        /// <summary>
        /// Scales the vector by the provided value
        /// </summary>
        /// <param name="d">a scaling factor</param>
        /// <returns>A new scale adjusted vector</returns>
        [Pure]
        public Vector2D ScaleBy(double d)
        {
            return new Vector2D(d * this.X, d * this.Y);
        }

        /// <summary>
        /// Returns the negative of the vector
        /// </summary>
        /// <returns>A new negated vector.</returns>
        [Pure]
        public Vector2D Negate()
        {
            return new Vector2D(-1 * this.X, -1 * this.Y);
        }

        /// <summary>
        /// Subtracts a vector from this vector.
        /// </summary>
        /// <param name="v">A vector to subtract</param>
        /// <returns>A new vector which is the difference of the current vector and the provided vector</returns>
        [Pure]
        public Vector2D Subtract(Vector2D v)
        {
            return new Vector2D(this.X - v.X, this.Y - v.Y);
        }

        /// <summary>
        /// Adds a vector to this vector
        /// </summary>
        /// <param name="v">A vector to add</param>
        /// <returns>A new vector which is the sum of the existing vector and the provided vector</returns>
        [Pure]
        public Vector2D Add(Vector2D v)
        {
            return new Vector2D(this.X + v.X, this.Y + v.Y);
        }

        /// <summary>
        /// Transforms a vector by multiplying it against a provided matrix
        /// </summary>
        /// <param name="m">The matrix to multiply</param>
        /// <returns>A new transformed vector</returns>
        [Pure]
        public Vector2D TransformBy(Matrix<double> m)
        {
            var transformed = m.Multiply(this.ToVector());
            return new Vector2D(transformed.At(0), transformed.At(1));
        }

        /// <summary>
        /// Convert to a Math.NET Numerics dense vector of length 2.
        /// </summary>
        /// <returns> A <see cref="Vector{Double}"/> with the x and y values from this instance.</returns>
        [Pure]
        public Vector<double> ToVector()
        {
            return Vector<double>.Build.Dense(new[] { this.X, this.Y });
        }

        /// <summary>
        /// Compare this instance with <paramref name="other"/>
        /// </summary>
        /// <param name="other">The other <see cref="Vector2D"/></param>
        /// <param name="tolerance">The tolerance when comparing the x and y components</param>
        /// <returns>True if found to be equal.</returns>
        [Pure]
        public bool Equals(Vector2D other, double tolerance)
        {
            if (tolerance < 0)
            {
                throw new ArgumentException("epsilon < 0");
            }

            return Math.Abs(other.X - this.X) < tolerance &&
                   Math.Abs(other.Y - this.Y) < tolerance;
        }

        /// <inheritdoc />
        [Pure]
        public bool Equals(Vector2D other) => this.X.Equals(other.X) && this.Y.Equals(other.Y);

        /// <inheritdoc />
        [Pure]
        public override bool Equals(object obj) => obj is Vector2D v && this.Equals(v);

        /// <inheritdoc />
        [Pure]
        public override int GetHashCode() => HashCode.Combine(this.X, this.Y);

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
        [Pure]
        public string ToString(string format, IFormatProvider provider = null)
        {
            var numberFormatInfo = provider != null ? NumberFormatInfo.GetInstance(provider) : CultureInfo.InvariantCulture.NumberFormat;
            var separator = numberFormatInfo.NumberDecimalSeparator == "," ? ";" : ",";
            return $"({this.X.ToString(format, numberFormatInfo)}{separator}\u00A0{this.Y.ToString(format, numberFormatInfo)})";
        }

        /// <inheritdoc />
        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        /// <inheritdoc />
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            if (reader.TryReadAttributeAsDouble("X", out var x) &&
                reader.TryReadAttributeAsDouble("Y", out var y))
            {
                reader.Skip();
                this = new Vector2D(x, y);
                return;
            }

            if (reader.TryReadChildElementsAsDoubles("X", "Y", out x, out y))
            {
                reader.Skip();
                this = new Vector2D(x, y);
                return;
            }

            throw new XmlException("Could not read a Vector2D");
        }

        /// <inheritdoc />
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteAttribute("X", this.X);
            writer.WriteAttribute("Y", this.Y);
        }
    }
}
