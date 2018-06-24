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
    /// Represents a point in 2 dimensional space
    /// </summary>
    [Serializable]
    public struct Point2D : IXmlSerializable, IEquatable<Point2D>, IFormattable
    {
        /// <summary>
        /// The x coordinate
        /// </summary>
        public readonly double X;

        /// <summary>
        /// The y coordinate
        /// </summary>
        public readonly double Y;

        /// <summary>
        /// Initializes a new instance of the <see cref="Point2D"/> struct.
        /// Creates a point for given coordinates (x, y)
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        public Point2D(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Point2D"/> struct.
        /// Creates a point r from origin rotated a counterclockwise from X-Axis
        /// </summary>
        /// <param name="r">distance from origin</param>
        /// <param name="a">the angle</param>
        [Obsolete("This constructor will be removed, use FromPolar. Made obsolete 2017-12-03.")]
        public Point2D(double r, Angle a)
            : this(r * Math.Cos(a.Radians), r * Math.Sin(a.Radians))
        {
            if (r < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(r), r, "Expected a radius greater than or equal to zero.");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Point2D"/> struct.
        /// Creates a point from a list of coordinates (x, y)
        /// </summary>
        /// <param name="data">a pair of coordinates in the order x, y</param>
        /// <exception cref="ArgumentException">Exception thrown if more than 2 coordinates are passed</exception>
        [Obsolete("This constructor will be removed. Made obsolete 2017-12-03.")]
        public Point2D(IEnumerable<double> data)
            : this(data.ToArray())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Point2D"/> struct.
        /// Creates a point from a list of coordinates (x, y)
        /// </summary>
        /// <param name="data">a pair of coordinates in the order x, y</param>
        /// <exception cref="ArgumentException">Exception thrown if more than 2 coordinates are passed</exception>
        [Obsolete("This constructor will be removed. Made obsolete 2017-12-03.")]
        public Point2D(double[] data)
            : this(data[0], data[1])
        {
            if (data.Length != 2)
            {
                throw new ArgumentException("data.Length != 2!");
            }
        }

        /// <summary>
        /// Gets a point at the origin (0,0)
        /// </summary>
        public static Point2D Origin => new Point2D(0, 0);

        /// <summary>
        /// Adds a point and a vector together
        /// </summary>
        /// <param name="point">A point</param>
        /// <param name="vector">A vector</param>
        /// <returns>A new point at the summed location</returns>
        public static Point2D operator +(Point2D point, Vector2D vector)
        {
            return new Point2D(point.X + vector.X, point.Y + vector.Y);
        }

        /// <summary>
        /// Subtracts a vector from a point
        /// </summary>
        /// <param name="left">A point</param>
        /// <param name="right">A vector</param>
        /// <returns>A new point at the difference</returns>
        public static Point2D operator -(Point2D left, Vector2D right)
        {
            return new Point2D(left.X - right.X, left.Y - right.Y);
        }

        /// <summary>
        /// Subtracts the first point from the second point
        /// </summary>
        /// <param name="left">The first point</param>
        /// <param name="right">The second point</param>
        /// <returns>A vector pointing to the difference</returns>
        public static Vector2D operator -(Point2D left, Point2D right)
        {
            return new Vector2D(left.X - right.X, left.Y - right.Y);
        }

        /// <summary>
        /// Returns a value that indicates whether each pair of elements in two specified points is equal.
        /// </summary>
        /// <param name="left">The first point to compare</param>
        /// <param name="right">The second point to compare</param>
        /// <returns>True if the points are the same; otherwise false.</returns>
        public static bool operator ==(Point2D left, Point2D right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates whether any pair of elements in two specified points is not equal.
        /// </summary>
        /// <param name="left">The first point to compare</param>
        /// <param name="right">The second point to compare</param>
        /// <returns>True if the points are different; otherwise false.</returns>
        public static bool operator !=(Point2D left, Point2D right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Point2D"/> struct.
        /// Creates a point r from origin rotated a counterclockwise from X-Axis
        /// </summary>
        /// <param name="radius">distance from origin</param>
        /// <param name="angle">the angle</param>
        /// <returns>The <see cref="Point2D"/></returns>
        public static Point2D FromPolar(double radius, Angle angle)
        {
            if (radius < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radius), radius, "Expected a radius greater than or equal to zero.");
            }

            return new Point2D(
                radius * Math.Cos(angle.Radians),
                radius * Math.Sin(angle.Radians));
        }

        /// <summary>
        /// Attempts to convert a string of the form x,y into a point
        /// </summary>
        /// <param name="text">The string to be converted</param>
        /// <param name="result">A point at the coordinates specified</param>
        /// <returns>True if <paramref name="text"/> could be parsed.</returns>
        public static bool TryParse(string text, out Point2D result)
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
        public static bool TryParse(string text, IFormatProvider formatProvider, out Point2D result)
        {
            if (Text.TryParse2D(text, formatProvider, out var x, out var y))
            {
                result = new Point2D(x, y);
                return true;
            }

            result = default(Point2D);
            return false;
        }

        /// <summary>
        /// Attempts to convert a string of the form x,y into a point
        /// </summary>
        /// <param name="value">The string to be converted</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/></param>
        /// <returns>A point at the coordinates specified</returns>
        public static Point2D Parse(string value, IFormatProvider formatProvider = null)
        {
            if (TryParse(value, formatProvider, out var p))
            {
                return p;
            }

            throw new FormatException($"Could not parse a Point2D from the string {value}");
        }

        /// <summary>
        /// Creates an <see cref="Point2D"/> from an <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="reader">An <see cref="XmlReader"/> positioned at the node to read into this <see cref="Point2D"/>.</param>
        /// <returns>An <see cref="Point2D"/> that contains the data read from the reader.</returns>
        public static Point2D ReadFrom(XmlReader reader)
        {
            return reader.ReadElementAs<Point2D>();
        }

        /// <summary>
        /// Returns the centeroid or center of mass of any set of points
        /// </summary>
        /// <param name="points">a list of points</param>
        /// <returns>the centeroid point</returns>
        public static Point2D Centroid(IEnumerable<Point2D> points)
        {
            return Centroid(points.ToArray());
        }

        /// <summary>
        /// Returns the centeroid or center of mass of any set of points
        /// </summary>
        /// <param name="points">a list of points</param>
        /// <returns>the centeroid point</returns>
        public static Point2D Centroid(params Point2D[] points)
        {
            return new Point2D(
                points.Average(point => point.X),
                points.Average(point => point.Y));
        }

        /// <summary>
        /// Returns a point midway between the provided points <paramref name="point1"/> and <paramref name="point2"/>
        /// </summary>
        /// <param name="point1">point A</param>
        /// <param name="point2">point B</param>
        /// <returns>a new point midway between the provided points</returns>
        public static Point2D MidPoint(Point2D point1, Point2D point2)
        {
            return Centroid(point1, point2);
        }

        /// <summary>
        /// Create a new Point2D from a Math.NET Numerics vector of length 2.
        /// </summary>
        /// <param name="vector"> A vector with length 2 to populate the created instance with.</param>
        /// <returns> A <see cref="Point2D"/></returns>
        public static Point2D OfVector(Vector<double> vector)
        {
            if (vector.Count != 2)
            {
                throw new ArgumentException("The vector length must be 2 in order to convert it to a Point2D");
            }

            return new Point2D(vector.At(0), vector.At(1));
        }

        /// <summary>
        /// Applies a transform matrix to the point
        /// </summary>
        /// <param name="m">A transform matrix</param>
        /// <returns>A new point</returns>
        public Point2D TransformBy(Matrix<double> m)
        {
            return OfVector(m.Multiply(this.ToVector()));
        }

        /// <summary>
        /// Gets a vector from this point to another point
        /// </summary>
        /// <param name="otherPoint">The point to which the vector should go</param>
        /// <returns>A vector pointing to the other point.</returns>
        [Pure]
        public Vector2D VectorTo(Point2D otherPoint)
        {
            return otherPoint - this;
        }

        /// <summary>
        /// Finds the straight line distance to another point
        /// </summary>
        /// <param name="otherPoint">The other point</param>
        /// <returns>a distance measure</returns>
        [Pure]
        public double DistanceTo(Point2D otherPoint)
        {
            var vector = this.VectorTo(otherPoint);
            return vector.Length;
        }

        /// <summary>
        /// Converts this point into a vector from the origin
        /// </summary>
        /// <returns>A vector equivalent to this point</returns>
        [Pure]
        public Vector2D ToVector2D()
        {
            return new Vector2D(this.X, this.Y);
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

        /// <summary>
        /// Returns a value to indicate if a pair of points are equal
        /// </summary>
        /// <param name="other">The point to compare against.</param>
        /// <param name="tolerance">A tolerance (epsilon) to adjust for floating point error</param>
        /// <returns>true if the points are equal; otherwise false</returns>
        [Pure]
        public bool Equals(Point2D other, double tolerance)
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
        public bool Equals(Point2D other) => this.X.Equals(other.X) && this.Y.Equals(other.Y);

        /// <inheritdoc />
        [Pure]
        public override bool Equals(object obj) => obj is Point2D p && this.Equals(p);

        /// <inheritdoc />
        [Pure]
        public override int GetHashCode() => HashCode.Combine(this.X, this.Y);

        /// <inheritdoc />
        XmlSchema IXmlSerializable.GetSchema() => null;

        /// <inheritdoc />
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            if (reader.TryReadAttributeAsDouble("X", out var x) &&
                reader.TryReadAttributeAsDouble("Y", out var y))
            {
                reader.Skip();
                this = new Point2D(x, y);
                return;
            }

            if (reader.TryReadChildElementsAsDoubles("X", "Y", out x, out y))
            {
                reader.Skip();
                this = new Point2D(x, y);
                return;
            }

            throw new XmlException("Could not read a Point2D");
        }

        /// <inheritdoc />
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteAttribute("X", this.X);
            writer.WriteAttribute("Y", this.Y);
        }
    }
}
