using System;
using System.Diagnostics.Contracts;
using MathNet.Numerics.Spatial.Internal;

namespace MathNet.Numerics.Spatial.Euclidean2D
{
    /// <summary>
    /// Describes a standard 2 dimensional circle
    /// </summary>
    public struct Circle2D : IEquatable<Circle2D>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Circle2D"/> struct.
        /// Creates a Circle of a given radius from a center point
        /// </summary>
        /// <param name="center">The location of the center</param>
        /// <param name="radius">The radius of the circle</param>
        public Circle2D(Point2D center, double radius)
        {
            this.Center = center;
            this.Radius = radius;
        }

        /// <summary>
        /// Gets the center point of the circle
        /// </summary>
        public Point2D Center { get; }

        /// <summary>
        /// Gets the radius of the circle
        /// </summary>
        public double Radius { get; }

        /// <summary>
        /// Gets the circumference of the circle
        /// </summary>
        [Pure]
        public double Circumference => 2 * this.Radius * Math.PI;

        /// <summary>
        /// Gets the diameter of the circle
        /// </summary>
        [Pure]
        public double Diameter => 2 * this.Radius;

        /// <summary>
        /// Gets the area of the circle
        /// </summary>
        [Pure]
        public double Area => this.Radius * this.Radius * Math.PI;

        /// <summary>
        /// Returns a value that indicates whether each pair of elements in two specified circles is equal.
        /// </summary>
        /// <param name="left">The first circle to compare.</param>
        /// <param name="right">The second circle to compare.</param>
        /// <returns>True if the circles are the same; otherwise false.</returns>
        public static bool operator ==(Circle2D left, Circle2D right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates whether any pair of elements in two specified circles is not equal.
        /// </summary>
        /// <param name="left">The first circle to compare.</param>
        /// <param name="right">The second circle to compare</param>
        /// <returns>True if the circles are different; otherwise false.</returns>
        public static bool operator !=(Circle2D left, Circle2D right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Creates a <see cref="Circle2D"/> circle from three points which lie along its circumference.
        /// Points may not be collinear
        /// </summary>
        /// <param name="pointA">The first point on the circle.</param>
        /// <param name="pointB">The second point on the circle.</param>
        /// <param name="pointC">The third point on the circle.</param>
        /// <returns>A Circle which is defined by the three specified points</returns>
        /// <exception cref="ArgumentException">An exception is thrown if no possible circle can be formed from the points</exception>
        public static Circle2D FromPoints(Point2D pointA, Point2D pointB, Point2D pointC)
        {
            // ReSharper disable InconsistentNaming
            var midpointAB = Point2D.MidPoint(pointA, pointB);
            var midpointBC = Point2D.MidPoint(pointB, pointC);
            var gradientAB = (pointB.Y - pointA.Y) / (pointB.X - pointA.X);
            var gradientBC = (pointC.Y - pointB.Y) / (pointC.X - pointB.X);
            var gradientl1 = -1 / gradientAB;
            var gradientl2 = -1 / gradientBC;

            // ReSharper restore InconsistentNaming
            var denominator = gradientl2 - gradientl1;
            var nominator = midpointAB.Y - (gradientl1 * midpointAB.X) + (gradientl2 * midpointBC.X) - midpointBC.Y;
            var centerX = nominator / denominator;
            var centerY = (gradientl1 * (centerX - midpointAB.X)) + midpointAB.Y;
            var center = new Point2D(centerX, centerY);

            if (double.IsNaN(center.X) || double.IsNaN(center.Y) || double.IsInfinity(center.X) || double.IsInfinity(center.Y))
            {
                throw new ArgumentException("Points cannot form a circle, are they collinear?");
            }

            return new Circle2D(center, center.DistanceTo(pointA));
        }

        /// <summary>
        /// Returns a value to indicate if a pair of circles are equal
        /// </summary>
        /// <param name="c">The circle to compare against.</param>
        /// <param name="tolerance">A tolerance (epsilon) to adjust for floating point error</param>
        /// <returns>true if the points are equal; otherwise false</returns>
        [Pure]
        public bool Equals(Circle2D c, double tolerance)
        {
            if (tolerance < 0)
            {
                throw new ArgumentException("epsilon < 0");
            }

            return Math.Abs(c.Radius - this.Radius) < tolerance && this.Center.Equals(c.Center, tolerance);
        }

        /// <inheritdoc />
        [Pure]
        public bool Equals(Circle2D c) => this.Radius.Equals(c.Radius) && this.Center.Equals(c.Center);

        /// <inheritdoc />
        [Pure]
        public override bool Equals(object obj) => obj is Circle2D c && this.Equals(c);

        /// <inheritdoc />
        [Pure]
        public override int GetHashCode() => HashCode.Combine(this.Center, this.Radius);
    }
}
