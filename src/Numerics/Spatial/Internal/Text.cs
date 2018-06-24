using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MathNet.Numerics.Spatial.Internal
{
    /// <summary>
    /// Internal text processing
    /// </summary>
    internal static class Text
    {
        /// <summary>
        /// regex pattern with period
        /// </summary>
        private const string DoublePatternPointProvider = "[+-]?\\d*(?:[.]\\d+)?(?:[eE][+-]?\\d+)?";

        /// <summary>
        /// regex pattern with comma
        /// </summary>
        private const string DoublePatternCommaProvider = "[+-]?\\d*(?:[,]\\d+)?(?:[eE][+-]?\\d+)?";

        /// <summary>
        /// Separator with period
        /// </summary>
        private const string SeparatorPatternPointProvider = " ?[,;]?( |\u00A0)?";

        /// <summary>
        /// Separator with comma
        /// </summary>
        private const string SeparatorPatternCommaProvider = " ?[;]?( |\u00A0)?";

        /// <summary>
        /// Attempts to parse a string into x, y coordinates
        /// </summary>
        /// <param name="text">a string</param>
        /// <param name="provider">a format provider</param>
        /// <param name="x">The x value</param>
        /// <param name="y">The y value</param>
        /// <returns>True if successful; otherwise false</returns>
        internal static bool TryParse2D(string text, IFormatProvider provider, out double x, out double y)
        {
            x = 0;
            y = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            if (Regex2D.TryMatch(text, provider, out var match) &&
                match.Groups.Count == 3 &&
                match.Groups[0].Captures.Count == 1 &&
                match.Groups[1].Captures.Count == 1 &&
                match.Groups[2].Captures.Count == 1)
            {
                return TryParseDouble(match.Groups["x"].Value, provider, out x) &&
                       TryParseDouble(match.Groups["y"].Value, provider, out y);
            }

            return false;
        }

        /// <summary>
        /// Attempts to parse a string into x, y, z coordinates
        /// </summary>
        /// <param name="text">A string</param>
        /// <param name="provider">A format provider</param>
        /// <param name="x">The x value</param>
        /// <param name="y">The y value</param>
        /// <param name="z">The z value</param>
        /// <returns>True if successful; otherwise false</returns>
        internal static bool TryParse3D(string text, IFormatProvider provider, out double x, out double y, out double z)
        {
            x = 0;
            y = 0;
            z = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            if (Regex3D.TryMatch(text, provider, out var match) &&
                match.Groups.Count == 4 &&
                match.Groups[0].Captures.Count == 1 &&
                match.Groups[1].Captures.Count == 1 &&
                match.Groups[2].Captures.Count == 1 &&
                match.Groups[3].Captures.Count == 1)
            {
                return TryParseDouble(match.Groups["x"].Value, provider, out x) &&
                       TryParseDouble(match.Groups["y"].Value, provider, out y) &&
                       TryParseDouble(match.Groups["z"].Value, provider, out z);
            }

            return false;
        }

        /// <summary>
        /// Attempts to parse a string into an Angle
        /// </summary>
        /// <param name="text">A string</param>
        /// <param name="provider">A format provider</param>
        /// <param name="a">An angle</param>
        /// <returns>True if successful; otherwise false</returns>
        internal static bool TryParseAngle(string text, IFormatProvider provider, out Angle a)
        {
            a = default(Angle);
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            if (RegexAngle.TryMatchDegrees(text, provider, out var value))
            {
                a = Angle.FromDegrees(value);
                return true;
            }

            if (RegexAngle.TryMatchRadians(text, provider, out value))
            {
                a = Angle.FromRadians(value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to parse a double
        /// </summary>
        /// <param name="s">A string</param>
        /// <param name="formatProvider">A format provider</param>
        /// <param name="result">A double</param>
        /// <returns>True if successful; otherwise false</returns>
        private static bool TryParseDouble(string s, IFormatProvider formatProvider, out double result)
        {
            if (formatProvider == null)
            {
                // This is for legacy reasons, we allow any culture, not nice.
                // Fixing would break
                return double.TryParse(s.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out result);
            }

            return double.TryParse(s, NumberStyles.Float, formatProvider, out result);
        }

        /// <summary>
        /// A class providing regex matching for 2D values
        /// </summary>
        private static class Regex2D
        {
            /// <summary>
            /// The pattern for this regex
            /// </summary>
            private const string Pattern2D = "^ *\\(?(?<x>{0}){1}(?<y>{0})\\)? *$";

            /// <summary>
            /// The standard options
            /// </summary>
            private const RegexOptions RegexOptions = System.Text.RegularExpressions.RegexOptions.ExplicitCapture | System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.Singleline;

            /// <summary>
            /// A regex containing a .
            /// </summary>
            private static readonly Regex Point = new Regex(
                string.Format(Pattern2D, DoublePatternPointProvider, SeparatorPatternPointProvider),
                RegexOptions);

            /// <summary>
            /// a regex containing a ,
            /// </summary>
            private static readonly Regex Comma = new Regex(
                string.Format(Pattern2D, DoublePatternCommaProvider, SeparatorPatternCommaProvider),
                RegexOptions);

            /// <summary>
            /// Attempts to match a string
            /// </summary>
            /// <param name="text">a string</param>
            /// <param name="formatProvider">a format provider</param>
            /// <param name="match">the match</param>
            /// <returns>True if successful; Otherwise false</returns>
            internal static bool TryMatch(string text, IFormatProvider formatProvider, out Match match)
            {
                if (formatProvider != null &&
                    NumberFormatInfo.GetInstance(formatProvider) is NumberFormatInfo formatInfo)
                {
                    if (formatInfo.NumberDecimalSeparator == ".")
                    {
                        match = Point.Match(text);
                        return match.Success;
                    }

                    if (formatInfo.NumberDecimalSeparator == ",")
                    {
                        match = Comma.Match(text);
                        return match.Success;
                    }
                }

                match = Point.Match(text);
                if (match.Success)
                {
                    return true;
                }

                match = Comma.Match(text);
                return match.Success;
            }
        }

        /// <summary>
        /// A class providing regex matching for 3D values
        /// </summary>
        private static class Regex3D
        {
            /// <summary>
            /// The pattern for this regex
            /// </summary>
            private const string Pattern3D = "^ *\\(?(?<x>{0}){1}(?<y>{0}){1}(?<z>{0})\\)? *$";

            /// <summary>
            /// The standard options
            /// </summary>
            private const RegexOptions RegexOptions = System.Text.RegularExpressions.RegexOptions.ExplicitCapture | System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.Singleline;

            /// <summary>
            /// A regex containing a .
            /// </summary>
            private static readonly Regex Point = new Regex(
                string.Format(Pattern3D, DoublePatternPointProvider, SeparatorPatternPointProvider),
                RegexOptions);

            /// <summary>
            /// A regex containing a ,
            /// </summary>
            private static readonly Regex Comma = new Regex(
                string.Format(Pattern3D, DoublePatternCommaProvider, SeparatorPatternCommaProvider),
                RegexOptions);

            /// <summary>
            /// Attempts to match a string
            /// </summary>
            /// <param name="text">a string</param>
            /// <param name="formatProvider">a format provider</param>
            /// <param name="match">the match</param>
            /// <returns>True if successful; Otherwise false</returns>
            internal static bool TryMatch(string text, IFormatProvider formatProvider, out Match match)
            {
                if (formatProvider != null &&
                    NumberFormatInfo.GetInstance(formatProvider) is NumberFormatInfo formatInfo)
                {
                    if (formatInfo.NumberDecimalSeparator == ".")
                    {
                        match = Point.Match(text);
                        return match.Success;
                    }

                    if (formatInfo.NumberDecimalSeparator == ",")
                    {
                        match = Comma.Match(text);
                        return match.Success;
                    }
                }

                match = Point.Match(text);
                if (match.Success)
                {
                    return true;
                }

                match = Comma.Match(text);
                return match.Success;
            }
        }

        /// <summary>
        /// A class providing regex matching for angle values
        /// </summary>
        private static class RegexAngle
        {
            /// <summary>
            /// A regex for radians angles
            /// </summary>
            private const string RadiansPattern = "^(?<value>{0})( |\u00A0)?(°|rad|radians) *$";

            /// <summary>
            /// A regex for degrees angles
            /// </summary>
            private const string DegreesPattern = "^(?<value>{0})( |\u00A0)?(°|deg|degrees) *$";

            /// <summary>
            /// Standard regex options
            /// </summary>
            private const RegexOptions RegexOptions = System.Text.RegularExpressions.RegexOptions.ExplicitCapture | System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase;

            /// <summary>
            /// Radians with a point
            /// </summary>
            private static readonly Regex RadiansPoint = new Regex(
                string.Format(RadiansPattern, DoublePatternPointProvider),
                RegexOptions);

            /// <summary>
            /// Radians with a comma
            /// </summary>
            private static readonly Regex RadiansComma = new Regex(
                string.Format(RadiansPattern, DoublePatternCommaProvider),
                RegexOptions);

            /// <summary>
            /// Degrees with a point
            /// </summary>
            private static readonly Regex DegreesPoint = new Regex(
                string.Format(DegreesPattern, DoublePatternPointProvider),
                RegexOptions);

            /// <summary>
            /// Degrees with a comma
            /// </summary>
            private static readonly Regex DegreesComma = new Regex(
                string.Format(DegreesPattern, DoublePatternCommaProvider),
                RegexOptions);

            /// <summary>
            /// Attempts to match Degrees
            /// </summary>
            /// <param name="text">a string</param>
            /// <param name="provider">a format provider</param>
            /// <param name="value">a double</param>
            /// <returns>True if successful; otherwise false</returns>
            internal static bool TryMatchDegrees(string text, IFormatProvider provider, out double value)
            {
                if (TryMatchDegrees(text, provider, out Match match))
                {
                    return TryParseDouble(match.Groups["value"].Value, provider, out value);
                }

                value = 0;
                return false;
            }

            /// <summary>
            /// Attempts to match Radians
            /// </summary>
            /// <param name="text">a string</param>
            /// <param name="provider">a format provider</param>
            /// <param name="value">a double</param>
            /// <returns>True if successful; otherwise false</returns>
            internal static bool TryMatchRadians(string text, IFormatProvider provider, out double value)
            {
                if (TryMatchRadians(text, provider, out Match match))
                {
                    return TryParseDouble(match.Groups["value"].Value, provider, out value);
                }

                value = 0;
                return false;
            }

            /// <summary>
            /// Attempts to match Radians with either . or , separators
            /// </summary>
            /// <param name="text">a string</param>
            /// <param name="formatProvider">a format provider</param>
            /// <param name="match">a list of matches</param>
            /// <returns>True if successful; otherwise false</returns>
            private static bool TryMatchRadians(string text, IFormatProvider formatProvider, out Match match)
            {
                if (formatProvider != null &&
                    NumberFormatInfo.GetInstance(formatProvider) is NumberFormatInfo formatInfo)
                {
                    if (formatInfo.NumberDecimalSeparator == ".")
                    {
                        match = RadiansPoint.Match(text);
                        return match.Success;
                    }

                    if (formatInfo.NumberDecimalSeparator == ",")
                    {
                        match = RadiansComma.Match(text);
                        return match.Success;
                    }
                }

                match = RadiansPoint.Match(text);
                if (match.Success)
                {
                    return true;
                }

                match = RadiansComma.Match(text);
                return match.Success;
            }

            /// <summary>
            /// Attempts to match Degrees with either . or , separators
            /// </summary>
            /// <param name="text">a string</param>
            /// <param name="provider">a format provider</param>
            /// <param name="match">a list of matches</param>
            /// <returns>True if successful; otherwise false</returns>
            private static bool TryMatchDegrees(string text, IFormatProvider provider, out Match match)
            {
                if (provider != null && NumberFormatInfo.GetInstance(provider) is NumberFormatInfo formatInfo)
                {
                    if (formatInfo.NumberDecimalSeparator == ".")
                    {
                        match = DegreesPoint.Match(text);
                        return match.Success;
                    }

                    if (formatInfo.NumberDecimalSeparator == ",")
                    {
                        match = DegreesComma.Match(text);
                        return match.Success;
                    }
                }

                match = DegreesPoint.Match(text);
                if (match.Success)
                {
                    return true;
                }

                match = DegreesComma.Match(text);
                return match.Success;
            }
        }
    }
}
