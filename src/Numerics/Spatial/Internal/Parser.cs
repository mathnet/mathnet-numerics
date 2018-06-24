using System;
using System.Globalization;
using System.Text.RegularExpressions;
using MathNet.Numerics.Spatial.Euclidean3D;

#pragma warning disable SA1600 // Elements must be documented
namespace MathNet.Numerics.Spatial.Internal
{
    [Obsolete("This should not have been public, will be removed in a future version. Made obsolete 2017-12-03")]
    internal static class Parser
    {
        public const string SeparatorPattern = @" *[,;] *";
        public static readonly string DoublePattern = @"[+-]?\d*(?:[.,]\d+)?(?:[eE][+-]?\d+)?";
        public static readonly string Vector3DPattern = string.Format(@"^ *\(?(?<x>{0}){1}(?<y>{0}){1}(?<z>{0})\)? *$", DoublePattern, SeparatorPattern);
        public static readonly string Vector2DPattern = string.Format(@"^ *\(?(?<x>{0}){1}(?<y>{0})?\)? *$", DoublePattern, SeparatorPattern);
        public static readonly string Item3DPattern = Vector3DPattern.Trim('^', '$');
        public static readonly string PlanePointVectorPattern = string.Format(@"^ *p: *{{(?<p>{0})}} *v: *{{(?<v>{0})}} *$", Item3DPattern);
        public static readonly string PlaneAbcdPattern = string.Format(@"^ *\(?(?<a>{0}){1}(?<b>{0}){1}(?<c>{0}){1}(?<d>{0})\)? *$", DoublePattern, SeparatorPattern);

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
            return double.Parse(s.Replace(',', '.'), CultureInfo.InvariantCulture);
        }

        public static Plane3D ParsePlane(string s)
        {
            var match = Regex.Match(s, PlanePointVectorPattern);
            if (match.Success)
            {
                var p = Point3D.Parse(match.Groups["p"].Value);
                var uv = UnitVector3D.Parse(match.Groups["v"].Value);
                return new Plane3D(p, uv);
            }

            match = Regex.Match(s, PlaneAbcdPattern);
            {
                var a = ParseDouble(match.Groups["a"]);
                var b = ParseDouble(match.Groups["b"]);
                var c = ParseDouble(match.Groups["c"]);
                var d = ParseDouble(match.Groups["d"]);
                return new Plane3D(a, b, c, d);
            }
        }

        public static Ray3D ParseRay3D(string s)
        {
            var match = Regex.Match(s, PlanePointVectorPattern);
            var p = Point3D.Parse(match.Groups["p"].Value);
            var uv = UnitVector3D.Parse(match.Groups["v"].Value);
            return new Ray3D(p, uv);
        }
    }
}
