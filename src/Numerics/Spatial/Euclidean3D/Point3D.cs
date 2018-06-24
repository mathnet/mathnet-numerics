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
    /// Represents a point in 3 dimensional space
    /// </summary>
    [Serializable]
    public struct Point3D : IXmlSerializable, IEquatable<Point3D>, IFormattable
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
        /// Initializes a new instance of the <see cref="Point3D"/> struct.
        /// </summary>
        /// <param name="x">The x component.</param>
        /// <param name="y">The y component.</param>
        /// <param name="z">The z component.</param>
        public Point3D(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// Gets a point at the origin
        /// </summary>
        public static Point3D Origin { get; } = new Point3D(0, 0, 0);

        /// <summary>
        /// Gets a point where all values are NAN
        /// </summary>
        public static Point3D NaN { get; } = new Point3D(double.NaN, double.NaN, double.NaN);

        /// <summary>
        /// Multiplies a matrix and a vector representation of the point together
        /// </summary>
        /// <param name="left">A matrix</param>
        /// <param name="right">A point</param>
        /// <returns>A Mathnet.Numerics vector</returns>
        [Obsolete("Not sure this is nice")]
        public static Vector<double> operator *(Matrix<double> left, Point3D right)
        {
            return left * right.ToVector();
        }

        /// <summary>
        /// Multiplies a matrix and a vector representation of the point together
        /// </summary>
        /// <param name="left">A point</param>
        /// <param name="right">A matrix</param>
        /// <returns>A Mathnet.Numerics vector</returns>
        [Obsolete("Not sure this is nice")]
        public static Vector<double> operator *(Point3D left, Matrix<double> right)
        {
            return left.ToVector() * right;
        }

        /// <summary>
        /// Adds a point and a vector together
        /// </summary>
        /// <param name="point">A point</param>
        /// <param name="vector">A vector</param>
        /// <returns>A new point at the summed location</returns>
        public static Point3D operator +(Point3D point, Vector3D vector)
        {
            return new Point3D(point.X + vector.X, point.Y + vector.Y, point.Z + vector.Z);
        }

        /// <summary>
        /// Adds a point and a vector together
        /// </summary>
        /// <param name="point">A point</param>
        /// <param name="vector">A vector</param>
        /// <returns>A new point at the summed location</returns>
        public static Point3D operator +(Point3D point, UnitVector3D vector)
        {
            return new Point3D(point.X + vector.X, point.Y + vector.Y, point.Z + vector.Z);
        }

        /// <summary>
        /// Subtracts a vector from a point
        /// </summary>
        /// <param name="point">A point</param>
        /// <param name="vector">A vector</param>
        /// <returns>A new point at the difference</returns>
        public static Point3D operator -(Point3D point, Vector3D vector)
        {
            return new Point3D(point.X - vector.X, point.Y - vector.Y, point.Z - vector.Z);
        }

        /// <summary>
        /// Subtracts a vector from a point
        /// </summary>
        /// <param name="point">A point</param>
        /// <param name="vector">A vector</param>
        /// <returns>A new point at the difference</returns>
        public static Point3D operator -(Point3D point, UnitVector3D vector)
        {
            return new Point3D(point.X - vector.X, point.Y - vector.Y, point.Z - vector.Z);
        }

        /// <summary>
        /// Subtracts the first point from the second point
        /// </summary>
        /// <param name="left">The first point</param>
        /// <param name="right">The second point</param>
        /// <returns>A vector pointing to the difference</returns>
        public static Vector3D operator -(Point3D left, Point3D right)
        {
            return new Vector3D(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        /// <summary>
        /// Returns a value that indicates whether each pair of elements in two specified points is equal.
        /// </summary>
        /// <param name="left">The first point to compare</param>
        /// <param name="right">The second point to compare</param>
        /// <returns>True if the points are the same; otherwise false.</returns>
        public static bool operator ==(Point3D left, Point3D right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates whether any pair of elements in two specified points is not equal.
        /// </summary>
        /// <param name="left">The first point to compare</param>
        /// <param name="right">The second point to compare</param>
        /// <returns>True if the points are different; otherwise false.</returns>
        public static bool operator !=(Point3D left, Point3D right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Attempts to convert a string of the form x,y,z into a point
        /// </summary>
        /// <param name="text">The string to be converted</param>
        /// <param name="result">A point with the coordinates specified</param>
        /// <returns>True if <paramref name="text"/> could be parsed.</returns>
        public static bool TryParse(string text, out Point3D result)
        {
            return TryParse(text, null, out result);
        }

        /// <summary>
        /// Attempts to convert a string of the form x,y,z into a point
        /// </summary>
        /// <param name="text">The string to be converted</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/></param>
        /// <param name="result">A point at the coordinates specified</param>
        /// <returns>True if <paramref name="text"/> could be parsed.</returns>
        public static bool TryParse(string text, IFormatProvider formatProvider, out Point3D result)
        {
            if (Text.TryParse3D(text, formatProvider, out var x, out var y, out var z))
            {
                result = new Point3D(x, y, z);
                return true;
            }

            result = default(Point3D);
            return false;
        }

        /// <summary>
        /// Attempts to convert a string of the form x,y,z into a point
        /// </summary>
        /// <param name="value">The string to be converted</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/></param>
        /// <returns>A point at the coordinates specified</returns>
        public static Point3D Parse(string value, IFormatProvider formatProvider = null)
        {
            if (TryParse(value, formatProvider, out var p))
            {
                return p;
            }

            throw new FormatException($"Could not parse a Point3D from the string {value}");
        }

        /// <summary>
        /// Create a new <see cref="Point3D"/> from a Math.NET Numerics vector of length 3.
        /// </summary>
        /// <param name="vector"> A vector with length 2 to populate the created instance with.</param>
        /// <returns> A <see cref="Point3D"/></returns>
        public static Point3D OfVector(Vector<double> vector)
        {
            if (vector.Count != 3)
            {
                throw new ArgumentException("The vector length must be 3 in order to convert it to a Point3D");
            }

            return new Point3D(vector.At(0), vector.At(1), vector.At(2));
        }

        /// <summary>
        /// Creates an <see cref="Point3D"/> from an <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="reader">An <see cref="XmlReader"/> positioned at the node to read into this <see cref="Point3D"/>.</param>
        /// <returns>An <see cref="Point3D"/> that contains the data read from the reader.</returns>
        public static Point3D ReadFrom(XmlReader reader)
        {
            return reader.ReadElementAs<Point3D>();
        }

        /// <summary>
        /// Returns the centroid of an arbitrary collection of points
        /// </summary>
        /// <param name="points">a list of points</param>
        /// <returns>The centroid of the points</returns>
        public static Point3D Centroid(IEnumerable<Point3D> points)
        {
            return Centroid(points.ToArray());
        }

        /// <summary>
        /// Returns the centroid of an arbitrary collection of points
        /// </summary>
        /// <param name="points">a list of points</param>
        /// <returns>The centroid of the points</returns>
        public static Point3D Centroid(params Point3D[] points)
        {
            return new Point3D(
                points.Average(point => point.X),
                points.Average(point => point.Y),
                points.Average(point => point.Z));
        }

        /// <summary>
        /// Returns the midpoint of two points
        /// </summary>
        /// <param name="p1">The first point</param>
        /// <param name="p2">The second point</param>
        /// <returns>The midpoint of the points</returns>
        public static Point3D MidPoint(Point3D p1, Point3D p2)
        {
            return Centroid(p1, p2);
        }

        /// <summary>
        /// Returns the point at which three planes intersect
        /// </summary>
        /// <param name="plane1">The first plane</param>
        /// <param name="plane2">The second plane</param>
        /// <param name="plane3">The third plane</param>
        /// <returns>The point of intersection</returns>
        public static Point3D IntersectionOf(Plane3D plane1, Plane3D plane2, Plane3D plane3)
        {
            var ray = plane1.IntersectionWith(plane2);
            return plane3.IntersectionWith(ray);
        }

        /// <summary>
        /// Returns the point of intersection between a plane and a ray
        /// </summary>
        /// <param name="plane">A geometric plane</param>
        /// <param name="ray">a ray</param>
        /// <returns>The point of intersection</returns>
        public static Point3D IntersectionOf(Plane3D plane, Ray3D ray)
        {
            return plane.IntersectionWith(ray);
        }

        /// <summary>
        /// Returns the mirror point of this point across a plane
        /// </summary>
        /// <param name="plane">A plane</param>
        /// <returns>The mirrored point</returns>
        [Pure]
        public Point3D MirrorAbout(Plane3D plane)
        {
            return plane.MirrorAbout(this);
        }

        /// <summary>
        /// Projects a point onto a plane
        /// </summary>
        /// <param name="plane">a plane</param>
        /// <returns>The projected point</returns>
        [Pure]
        public Point3D ProjectOn(Plane3D plane)
        {
            return plane.Project(this);
        }

        /// <summary>
        /// Rotates the point about a given vector
        /// </summary>
        /// <param name="aboutVector">A vector</param>
        /// <param name="angle">The angle to rotate</param>
        /// <returns>The rotated point</returns>
        [Pure]
        public Point3D Rotate(Vector3D aboutVector, Angle angle)
        {
            return this.Rotate(aboutVector.Normalize(), angle);
        }

        /// <summary>
        /// Rotates the point about a given vector
        /// </summary>
        /// <param name="aboutVector">A vector</param>
        /// <param name="angle">The angle to rotate</param>
        /// <returns>The rotated point</returns>
        [Pure]
        public Point3D Rotate(UnitVector3D aboutVector, Angle angle)
        {
            var cs = CoordinateSystem3D.Rotation(angle, aboutVector);
            return cs.Transform(this);
        }

        /// <summary>
        /// Gets a vector from this point to another point
        /// </summary>
        /// <param name="p">The point to which the vector should go</param>
        /// <returns>A vector pointing to the other point.</returns>
        [Pure]
        public Vector3D VectorTo(Point3D p)
        {
            return p - this;
        }

        /// <summary>
        /// Finds the straight line distance to another point
        /// </summary>
        /// <param name="p">The other point</param>
        /// <returns>a distance measure</returns>
        [Pure]
        public double DistanceTo(Point3D p)
        {
            var vector = this.VectorTo(p);
            return vector.Length;
        }

        /// <summary>
        /// Converts this point into a vector from the origin
        /// </summary>
        /// <returns>A vector equivalent to this point</returns>
        [Pure]
        public Vector3D ToVector3D()
        {
            return new Vector3D(this.X, this.Y, this.Z);
        }

        /// <summary>
        /// Applies a transform coordinate system to the point
        /// </summary>
        /// <param name="cs">A coordinate system</param>
        /// <returns>A new 3D point</returns>
        [Pure]
        public Point3D TransformBy(CoordinateSystem3D cs)
        {
            return cs.Transform(this);
        }

        /// <summary>
        /// Applies a transform matrix to the point
        /// </summary>
        /// <param name="m">A transform matrix</param>
        /// <returns>A new point</returns>
        [Pure]
        public Point3D TransformBy(Matrix<double> m)
        {
            return OfVector(m.Multiply(this.ToVector()));
        }

        /// <summary>
        /// Convert to a Math.NET Numerics dense vector of length 3.
        /// </summary>
        /// <returns>A Math.Net Numerics vector</returns>
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
        [Pure]
        public string ToString(string format, IFormatProvider provider = null)
        {
            var numberFormatInfo = provider != null ? NumberFormatInfo.GetInstance(provider) : CultureInfo.InvariantCulture.NumberFormat;
            var separator = numberFormatInfo.NumberDecimalSeparator == "," ? ";" : ",";
            return string.Format("({0}{1} {2}{1} {3})", this.X.ToString(format, numberFormatInfo), separator, this.Y.ToString(format, numberFormatInfo), this.Z.ToString(format, numberFormatInfo));
        }

        /// <summary>
        /// Returns a value to indicate if a pair of points are equal
        /// </summary>
        /// <param name="other">The point to compare against.</param>
        /// <param name="tolerance">A tolerance (epsilon) to adjust for floating point error</param>
        /// <returns>True if the points are equal; otherwise false</returns>
        [Pure]
        public bool Equals(Point3D other, double tolerance)
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
        public bool Equals(Point3D other)
        {
            return this.X.Equals(other.X) && this.Y.Equals(other.Y) && this.Z.Equals(other.Z);
        }

        /// <inheritdoc />
        [Pure]
        public override bool Equals(object obj) => obj is Point3D p && this.Equals(p);

        /// <inheritdoc />
        [Pure]
        public override int GetHashCode() => HashCode.Combine(this.X, this.Y, this.Z);

        /// <inheritdoc />
        XmlSchema IXmlSerializable.GetSchema() => null;

        /// <inheritdoc />
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            if (reader.TryReadAttributeAsDouble("X", out var x) &&
                reader.TryReadAttributeAsDouble("Y", out var y) &&
                reader.TryReadAttributeAsDouble("Z", out var z))
            {
                reader.Skip();
                this = new Point3D(x, y, z);
                return;
            }

            if (reader.TryReadChildElementsAsDoubles("X", "Y", "Z", out x, out y, out z))
            {
                reader.Skip();
                this = new Point3D(x, y, z);
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
