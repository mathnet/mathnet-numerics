namespace MathNet.Geometry.Units
{
    using System;
    using System.ComponentModel;

    [Serializable, EditorBrowsable(EditorBrowsableState.Never)]
    public struct Degrees : IAngleUnit
    {
        private const double Conv = Math.PI / 180.0;
        internal const string Name = "°";

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

        public static Angle operator *(double left, Degrees right)
        {
            return new Angle(left, right);
        }
    }
}