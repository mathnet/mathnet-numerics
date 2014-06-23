namespace MathNet.Geometry
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class Parser
    {
        public const string DoublePattern = @"[+-]?\d+([eE][+-]\d+)?([.,]\d+)?";
        public const string SeparatorPattern = @" *[,;] *";
        public static readonly string Vector3DPattern = String.Format(@"^ *\(?(?<x>{0}){1}(?<y>{0}){1}(?<z>{0})\)? *$", DoublePattern, SeparatorPattern);
        public static readonly string Vector2DPattern = String.Format(@"^ *\(?(?<x>{0}){1}(?<y>{0})?\)? *$", DoublePattern, SeparatorPattern);
        public const string UnitValuePattern = @"^(?: *)(?<Value>[+-]?\d+([eE][+-]\d+)?([.,]\d+)?) *(?<Unit>.+) *$";
        private static string _item3DPattern = Vector3DPattern.Trim('^', '$');
        public static readonly string PlanePattern = String.Format(@"^ *p: *{{(?<p>{0})}} *v: *{{(?<v>{0})}} *$", _item3DPattern);

        internal static double[] ParseItem2D(string vectorString)
        {
            var match = Regex.Match(vectorString, Vector2DPattern);
            Group[] ss = { match.Groups["x"], match.Groups["y"] };
            double[] ds = ss.Select(ParseDouble).ToArray();
            return ds;
        }

        internal static double[] ParseItem3D(string vectorString)
        {
            var match = Regex.Match(vectorString, Vector3DPattern);
            Group[] ss = { match.Groups["x"], match.Groups["y"], match.Groups["z"] };
            double[] ds = ss.Select(x => Double.Parse(x.Value.Replace(',', '.'), CultureInfo.InvariantCulture)).ToArray();
            return ds;
        }

        public static double ParseDouble(Group @group)
        {
            if (@group.Captures.Count != 1)
            {
                throw new ArgumentException("Expected single capture");
            }
            return ParseDouble(@group.Value);
        }

        public static double ParseDouble(string s)
        {
            return Double.Parse(s.Replace(',', '.'), CultureInfo.InvariantCulture);
        }

        public static Plane ParsePlane(string s)
        {
            var match = Regex.Match(s, PlanePattern);
            var p = Point3D.Parse(match.Groups["p"].Value);
            var uv = UnitVector3D.Parse(match.Groups["v"].Value);
            return new Plane(p, uv);
        }
    }
}
