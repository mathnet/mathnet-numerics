using System;
using System.Diagnostics.Contracts;
using MathNet.Numerics.Spatial.Internal;

namespace MathNet.Numerics.Spatial.Euclidean3D
{
    /// <summary>
    /// Describes a 3 dimensional circle
    /// </summary>
    [Serializable]
    public struct Circle3D : IEquatable<Circle3D>
    {
        /// <summary>
        /// The center of the circle
        /// </summary>
        public readonly Point3D CenterPoint;

        /// <summary>
        /// the axis of the circle
        /// </summary>
        public readonly UnitVector3D Axis;

        /// <summary>
        /// the radius of the circle
        /// </summary>
        public readonly double Radius;

        /// <summary>
        /// Initializes a new instance of the <see cref="Circle3D"/> struct.
        /// Constructs a Circle3D with a given <paramref name="radius"/> at a <paramref name="centerPoint"/> orientated to the <paramref name="axis"/>
        /// </summary>
        /// <param name="centerPoint">The center of the circle</param>
        /// <param name="axis">the axis of the circle</param>
        /// <param name="radius">the radius of the circle</param>
        public Circle3D(Point3D centerPoint, UnitVector3D axis, double radius)
        {
            this.CenterPoint = centerPoint;
            this.Axis = axis;
            this.Radius = radius;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Circle3D"/> struct.
        /// Create a circle from the midpoint between two points, in a direction along a specified axis
        /// </summary>
        /// <param name="p1">First point on the circumference of the circle</param>
        /// <param name="p2">Second point on the circumference of the circle</param>
        /// <param name="axis">Direction of the plane in which the circle lies</param>
        [Obsolete("This constructor will be removed, use factory method FromPointsAndAxis. Made obsolete 2017-12-05.")]
        public Circle3D(Point3D p1, Point3D p2, UnitVector3D axis)
        {
            this = FromPointsAndAxis(p1, p2, axis);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Circle3D"/> struct.
        /// Create a circle from three points which lie along its circumference.
        /// </summary>
        /// <param name="p1">The first point on the circle</param>
        /// <param name="p2">The second point on the circle</param>
        /// <param name="p3">The third point on the circle</param>
        [Obsolete("This constructor will be removed, use factory method FromPoints. Made obsolete 2017-12-05.")]
        public Circle3D(Point3D p1, Point3D p2, Point3D p3)
        {
            this = FromPoints(p1, p2, p3);
        }

        /// <summary>
        /// Gets the diameter of the circle
        /// </summary>
        [Pure]
        public double Diameter => 2 * this.Radius;

        /// <summary>
        /// Gets the circumference of the circle
        /// </summary>
        [Pure]
        public double Circumference => 2 * Math.PI * this.Radius;

        /// <summary>
        /// Gets the area of the circle
        /// </summary>
        [Pure]
        public double Area => this.Radius * this.Radius * Math.PI;

        /// <summary>
        /// Returns a value that indicates whether each pair of elements in two specified circles is equal.
        /// </summary>
        /// <param name="left">The first circle to compare</param>
        /// <param name="right">The second circle to compare</param>
        /// <returns>True if the circles are the same; otherwise false.</returns>
        public static bool operator ==(Circle3D left, Circle3D right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates whether any pair of elements in two specified circles is not equal.
        /// </summary>
        /// <param name="left">The first circle to compare</param>
        /// <param name="right">The second circle to compare</param>
        /// <returns>True if the circles are different; otherwise false.</returns>
        public static bool operator !=(Circle3D left, Circle3D right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Circle3D"/> struct.
        /// Create a circle from three points which lie along its circumference.
        /// </summary>
        /// <param name="p1">The first point on the circle</param>
        /// <param name="p2">The second point on the circle</param>
        /// <param name="p3">The third point on the circle</param>
        /// <returns>A <see cref="Circle3D"/></returns>
        public static Circle3D FromPoints(Point3D p1, Point3D p2, Point3D p3)
        {
            // https://www.physicsforums.com/threads/equation-of-a-circle-through-3-points-in-3d-space.173847/
            //// ReSharper disable InconsistentNaming
            var p1p2 = p2 - p1;
            var p2p3 = p3 - p2;
            //// ReSharper restore InconsistentNaming

            var axis = p1p2.CrossProduct(p2p3).Normalize();
            var midPointA = p1 + (0.5 * p1p2);
            var midPointB = p2 + (0.5 * p2p3);

            var directionA = p1p2.CrossProduct(axis);
            var directionB = p2p3.CrossProduct(axis);

            var bisectorA = new Ray3D(midPointA, directionA);
            var bisectorB = Plane3D.FromPoints(midPointB, midPointB + directionB.Normalize(), midPointB + axis);

            var center = bisectorA.IntersectionWith(bisectorB);
            if (center == null)
            {
                throw new ArgumentException("A circle cannot be created from these points, are they collinear?");
            }

            return new Circle3D(center.Value, axis, center.Value.DistanceTo(p1));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Circle3D"/> struct.
        /// Create a circle from the midpoint between two points, in a direction along a specified axis
        /// </summary>
        /// <param name="p1">First point on the circumference of the circle</param>
        /// <param name="p2">Second point on the circumference of the circle</param>
        /// <param name="axis">Direction of the plane in which the circle lies</param>
        /// <returns>A <see cref="Circle3D"/></returns>
        public static Circle3D FromPointsAndAxis(Point3D p1, Point3D p2, UnitVector3D axis)
        {
            var cp = Point3D.MidPoint(p1, p2);
            return new Circle3D(cp, axis, cp.DistanceTo(p1));
        }

        /// <summary>
        /// Returns a value to indicate if a pair of circles are equal
        /// </summary>
        /// <param name="c">The circle to compare against.</param>
        /// <param name="tolerance">A tolerance (epsilon) to adjust for floating point error</param>
        /// <returns>true if the points are equal; otherwise false</returns>
        [Pure]
        public bool Equals(Circle3D c, double tolerance)
        {
            if (tolerance < 0)
            {
                throw new ArgumentException("epsilon < 0");
            }

            return Math.Abs(c.Radius - this.Radius) < tolerance
                && this.Axis.Equals(c.Axis, tolerance)
                && this.CenterPoint.Equals(c.CenterPoint, tolerance);
        }

        /// <inheritdoc />
        [Pure]
        public bool Equals(Circle3D c)
        {
            return this.CenterPoint.Equals(c.CenterPoint)
                   && this.Axis.Equals(c.Axis)
                   && this.Radius.Equals(c.Radius);
        }

        /// <inheritdoc />
        [Pure]
        public override bool Equals(object obj) => obj is Circle3D c && this.Equals(c);

        /// <inheritdoc />
        [Pure]
        public override int GetHashCode() => HashCode.Combine(this.CenterPoint, this.Axis, this.Radius);
    }
}
