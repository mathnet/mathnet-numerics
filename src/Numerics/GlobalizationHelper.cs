// <copyright file="GlobalizationHelper.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    /// Globalized String Handling Helpers
    /// </summary>
    internal static class GlobalizationHelper
    {
        /// <summary>
        /// Tries to get a <see cref="CultureInfo"/> from the format provider,
        /// returning the current culture if it fails.
        /// </summary>
        /// <param name="formatProvider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific
        /// formatting information.
        /// </param>
        /// <returns>A <see cref="CultureInfo"/> instance.</returns>
        internal static CultureInfo GetCultureInfo(this IFormatProvider formatProvider)
        {
            if (formatProvider == null)
            {
                return CultureInfo.CurrentCulture;
            }

            return (formatProvider as CultureInfo)
                   ?? (formatProvider.GetFormat(typeof(CultureInfo)) as CultureInfo)
                      ?? CultureInfo.CurrentCulture;
        }

        /// <summary>
        /// Tries to get a <see cref="NumberFormatInfo"/> from the format
        /// provider, returning the current culture if it fails.
        /// </summary>
        /// <param name="formatProvider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific
        /// formatting information.
        /// </param>
        /// <returns>A <see cref="NumberFormatInfo"/> instance.</returns>
        internal static NumberFormatInfo GetNumberFormatInfo(this IFormatProvider formatProvider)
        {
            return NumberFormatInfo.GetInstance(formatProvider);
        }

        /// <summary>
        /// Tries to get a <see cref="TextInfo"/> from the format provider, returning the current culture if it fails.
        /// </summary>
        /// <param name="formatProvider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific
        /// formatting information.
        /// </param>
        /// <returns>A <see cref="TextInfo"/> instance.</returns>
        internal static TextInfo GetTextInfo(this IFormatProvider formatProvider)
        {
            return (formatProvider as TextInfo)
                   ?? GetCultureInfo(formatProvider).TextInfo;
        }

        /// <summary>
        /// Globalized Parsing: Tokenize a node by splitting it into several nodes.
        /// </summary>
        /// <param name="node">Node that contains the trimmed string to be tokenized.</param>
        /// <param name="keywords">List of keywords to tokenize by.</param>
        /// <param name="skip">keywords to skip looking for (because they've already been handled).</param>
        internal static void Tokenize(LinkedListNode<string> node, string[] keywords, int skip)
        {
            for (int i = skip; i < keywords.Length; i++)
            {
                var keyword = keywords[i];
                int indexOfKeyword;
                while ((indexOfKeyword = node.Value.IndexOf(keyword)) >= 0)
                {
                    if (indexOfKeyword != 0)
                    {
                        // separate part before the token, process recursively
                        string partBeforeKeyword = node.Value.Substring(0, indexOfKeyword).Trim();
                        Tokenize(node.List.AddBefore(node, partBeforeKeyword), keywords, i + 1);

                        // continue processing the rest iteratively
                        node.Value = node.Value.Substring(indexOfKeyword);
                    }

                    if (keyword.Length == node.Value.Length)
                    {
                        return;
                    }

                    // separate the token, done
                    string partAfterKeyword = node.Value.Substring(keyword.Length).Trim();
                    node.List.AddBefore(node, keyword);

                    // continue processing the rest on the right iteratively
                    node.Value = partAfterKeyword;
                }
            }
        }

#if PORTABLE
        /// <summary>
        /// Globalized Parsing: Parse a double number
        /// </summary>
        /// <param name="token">First token of the number.</param>
        /// <returns>The parsed double number using the current culture information.</returns>
        /// <exception cref="FormatException" />
        internal static double ParseDouble(ref LinkedListNode<string> token)
        {
            // in case the + and - in scientific notation are separated, join them back together.
            if (token.Value.EndsWith("e", StringComparison.CurrentCultureIgnoreCase))
            {
                if (token.Next == null || token.Next.Next == null)
                {
                    throw new FormatException();
                }

                token.Value = token.Value + token.Next.Value + token.Next.Next.Value;

                var list = token.List;
                list.Remove(token.Next.Next);
                list.Remove(token.Next);
            }

            double value;
            if (!Double.TryParse(token.Value, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
            {
                throw new FormatException();
            }

            token = token.Next;
            return value;
        }

        /// <summary>
        /// Globalized Parsing: Parse a float number
        /// </summary>
        /// <param name="token">First token of the number.</param>
        /// <returns>The parsed float number using the current culture information.</returns>
        /// <exception cref="FormatException" />
        internal static float ParseSingle(ref LinkedListNode<string> token)
        {
            // in case the + and - in scientific notation are separated, join them back together.
            if (token.Value.EndsWith("e", StringComparison.CurrentCultureIgnoreCase))
            {
                if (token.Next == null || token.Next.Next == null)
                {
                    throw new FormatException();
                }

                token.Value = token.Value + token.Next.Value + token.Next.Next.Value;

                var list = token.List;
                list.Remove(token.Next.Next);
                list.Remove(token.Next);
            }

            float value;
            if (!Single.TryParse(token.Value, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
            {
                throw new FormatException();
            }

            token = token.Next;
            return value;
        }

#else
        /// <summary>
        /// Globalized Parsing: Parse a double number
        /// </summary>
        /// <param name="token">First token of the number.</param>
        /// <param name="culture">Culture Info.</param>
        /// <returns>The parsed double number using the given culture information.</returns>
        /// <exception cref="FormatException" />
        internal static double ParseDouble(ref LinkedListNode<string> token, CultureInfo culture)
        {
            // in case the + and - in scientific notation are separated, join them back together.
            if (token.Value.EndsWith("e", true, culture))
            {
                if (token.Next == null || token.Next.Next == null)
                {
                    throw new FormatException();
                }

                token.Value = token.Value + token.Next.Value + token.Next.Next.Value;

                var list = token.List;
                list.Remove(token.Next.Next);
                list.Remove(token.Next);
            }

            double value;
            if (!Double.TryParse(token.Value, NumberStyles.Any, culture, out value))
            {
                throw new FormatException();
            }

            token = token.Next;
            return value;
        }

        /// <summary>
        /// Globalized Parsing: Parse a float number
        /// </summary>
        /// <param name="token">First token of the number.</param>
        /// <param name="culture">Culture Info.</param>
        /// <returns>The parsed float number using the given culture information.</returns>
        /// <exception cref="FormatException" />
        internal static float ParseSingle(ref LinkedListNode<string> token, CultureInfo culture)
        {
            // in case the + and - in scientific notation are separated, join them back together.
            if (token.Value.EndsWith("e", true, culture))
            {
                if (token.Next == null || token.Next.Next == null)
                {
                    throw new FormatException();
                }

                token.Value = token.Value + token.Next.Value + token.Next.Next.Value;

                var list = token.List;
                list.Remove(token.Next.Next);
                list.Remove(token.Next);
            }

            float value;
            if (!Single.TryParse(token.Value, NumberStyles.Any, culture, out value))
            {
                throw new FormatException();
            }

            token = token.Next;
            return value;
        }
#endif
    }
}