namespace MathNet.Geometry
{
    public struct Circle3D
    {
        public readonly Point3D CenterPoint;
        public readonly UnitVector3D Axis;
        public readonly double Radius;

        public Circle3D(Point3D centerPoint, UnitVector3D axis, double radius)
        {
            CenterPoint = centerPoint;
            Axis = axis;
            Radius = radius;
        }
    }
}
