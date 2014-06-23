namespace MathNet.Geometry.Units
{
    using System;
    using System.ComponentModel;

    [Serializable, EditorBrowsable(EditorBrowsableState.Never)]
    public struct Radians : IAngleUnit
    {
        private const double Conv = 1.0;
        internal const string Name = "rad";

        public double Conversionfactor
        {
            get
            {
                return Conv;
            }
        }

        public string ShortName
        {
            get
            {
                return Name;
            }
        }

        public static Angle operator *(double left, Radians right)
        {
            return new Angle(left, right);
        }
    }
}