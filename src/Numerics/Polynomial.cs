using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Linq;
using System.Numerics;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearRegression;
using MathNet.Numerics.LinearAlgebra.Factorization;

#if !NETSTANDARD1_3
using System.Runtime;
#endif

namespace MathNet.Numerics
{
    /// <summary>
    /// A single-variable polynomial with real-valued coefficients and non-negative exponents.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "urn:MathNet/Numerics")]
    public class Polynomial : IFormattable, IEquatable<Polynomial>
    {
        /// <summary>
        /// The coefficients of the polynomial in a
        /// </summary>
        [DataMember(Order = 1)]
        public double[] Coefficients { get; private set; }

        /// <summary>
        /// Only needed for the ToString method
        /// </summary>
        [DataMember(Order = 2)]
        public string VariableName = "x";

        /// <summary>
        /// Degree of the polynomial, i.e. the largest monomial exponent. For example, the degree of y=x^2+x^5 is 5, for y=3 it is 0.
        /// The null-polynomial returns degree -1 because the correct degree, negative infinity, cannot be represented by integers.
        /// </summary>
        public int Degree => EvaluateDegree(Coefficients);

        /// <summary>
        /// Create a zero-polynomial with a coefficient array of the given length.
        /// </summary>
        /// <param name="n">Length of the coefficient array</param>
        public Polynomial(int n)
        {
            if (n < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(n), "n must be non-negative");
            }

            Coefficients = new double[n];
        }

        /// <summary>
        /// Create a zero-polynomial
        /// </summary>
        public Polynomial()
        {
            Coefficients = new double[0];
        }

        /// <summary>
        /// Create a constant polynomial.
        /// Example: 3.0 -> "p : x -> 3.0"
        /// </summary>
        /// <param name="coefficient">just the "x^0" part</param>
        public Polynomial(double coefficient)
        {
            Coefficients = coefficient == 0.0 ? new double[0] : new[] { coefficient };
        }

        /// <summary>
        /// Create a polynomial with the provided coefficients (in ascending order, where the index matches the exponent).
        /// Example: {5, 0, 2} -> "p : x -> 5 + 0 x^1 + 2 x^2".
        /// </summary>
        /// <param name="coefficients">Polynomial coefficients as array</param>
        public Polynomial(double[] coefficients)
        {
            int degree = EvaluateDegree(coefficients);
            Coefficients = new double[degree + 1];
            Array.Copy(coefficients, Coefficients, Coefficients.Length);
        }

        /// <summary>
        /// Create a polynomial with the provided coefficients (in ascending order, where the index matches the exponent).
        /// Example: {5, 0, 2} -> "p : x -> 5 + 0 x^1 + 2 x^2".
        /// </summary>
        /// <param name="coefficients">Polynomial coefficients as enumerable</param>
        public Polynomial(IEnumerable<double> coefficients) : this(coefficients.ToArray())
        {
        }

        static int EvaluateDegree(double[] coefficients)
        {
            for (int i = coefficients.Length - 1; i >= 0; i--)
            {
                if (coefficients[i] != 0.0)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to a k-order polynomial y : x -> p0 + p1*x + p2*x^2 + ... + pk*x^k
        /// </summary>
        public static Polynomial Fit(double[] x, double[] y, int order, DirectRegressionMethod method = DirectRegressionMethod.QR)
        {
            var coefficients = Numerics.Fit.Polynomial(x, y, order, method);
            return new Polynomial(coefficients);
        }

        /// <summary>
        /// This method returns the coefficients of the Polynomial as an array the "IsFlipped" property,
        /// which is set during construction is taken into account automatically.
        /// </summary>
        /// <returns>The coefficients of the polynomial as an array</returns>
        public double[] ToArray()
        {
            return Coefficients.ToArray();
        }

        public object Clone()
        {
            // TODO: this assumes the constructor does a copy
            return new Polynomial(Coefficients);
        }

        #region Evaluation
        /// <summary>
        /// Evaluate a polynomial at point x.
        /// </summary>
        /// <param name="z">The location where to evaluate the polynomial at.</param>
        public double Evaluate(double z)
        {
            return Numerics.Evaluate.Polynomial(z, Coefficients);
        }

        /// <summary>
        /// Evaluate a polynomial at point x.
        /// </summary>
        /// <param name="z">The location where to evaluate the polynomial at.</param>
        public Complex Evaluate(Complex z)
        {
            return Numerics.Evaluate.Polynomial(z, Coefficients);
        }

        /// <summary>
        /// Evaluate a polynomial at points z.
        /// </summary>
        /// <param name="z">The locations where to evaluate the polynomial at.</param>
        public IEnumerable<double> Evaluate(IEnumerable<double> z)
        {
            return z.Select(Evaluate);
        }

        /// <summary>
        /// Evaluate a polynomial at points z.
        /// </summary>
        /// <param name="z">The locations where to evaluate the polynomial at.</param>
        public IEnumerable<Complex> Evaluate(IEnumerable<Complex> z)
        {
            return z.Select(Evaluate);
        }
        #endregion

        #region Calculus
        public Polynomial Differentiate()
        {
            int n = Degree;
            if (n < 0)
            {
                return this;
            }
            if (n == 0)
            {
                // Zero
                return new Polynomial();
            }

            var c = new double[n];
            for (int i = 0; i < c.Length; i++)
            {
                c[i] = Coefficients[i + 1] * (i + 1);
            }

            return new Polynomial(c);
        }

        public Polynomial Integrate()
        {
            int n = Degree;
            if (n < 0)
            {
                return this;
            }

            var c = new double[n + 2];
            for (int i = 1; i < c.Length; i++)
            {
                c[i] = Coefficients[i - 1] / i;
            }

            return new Polynomial(c);
        }
        #endregion

        #region Linear Algebra
        /// <summary>
        /// Calculates the complex roots of the Polynomial by eigenvalue decomposition
        /// </summary>
        /// <returns>a vector of complex numbers with the roots</returns>
        public Complex[] Roots()
        {
            switch (Degree)
            {
                case -1: // Zero-polynomial
                case 0: // Non-zero constant: y = a0
                    return new Complex[0];
                case 1: // Linear: y = a0 + a1*x
                    return new[] { new Complex(-Coefficients[0] / Coefficients[1], 0) };
            }

            DenseMatrix A = EigenvalueMatrix();
            Evd<double> eigen = A.Evd(Symmetricity.Asymmetric);
            return eigen.EigenValues.AsArray();
        }

        /// <summary>
        /// Get the eigenvalue matrix A of this polynomial such that eig(A) = roots of this polynomial.
        /// </summary>
        /// <returns>Eigenvalue matrix A</returns>
        /// <note>This matrix is similar to the companion matrix of this polynomial, in such a way, that it's transpose is the columnflip of the companion matrix</note>
        public DenseMatrix EigenvalueMatrix()
        {
            int n = Degree;
            if (n < 2)
            {
                return null;
            }

            // Negate, and normalize (scale such that the polynomial becomes monic)
            double aN = Coefficients[n];
            double[] p = new double[n];
            for (int ii = n - 1; ii >= 0; ii--)
            {
                p[ii] = -Coefficients[ii] / aN;
            }

            DenseMatrix A0 = DenseMatrix.CreateDiagonal(n - 1, n - 1, 1.0);
            DenseMatrix A = new DenseMatrix(n);

            A.SetSubMatrix(1, 0, A0);
            A.SetRow(0, p.Reverse().ToArray());

            return A;
        }
        #endregion

        #region Arithmetic Operations
        /// <summary>
        /// Addition of two Polynomials (point-wise).
        /// </summary>
        /// <param name="a">Left Polynomial</param>
        /// <param name="b">Right Polynomial</param>
        /// <returns>Resulting Polynomial</returns>
        public static Polynomial Add(Polynomial a, Polynomial b)
        {
            var ac = a.Coefficients;
            var bc = b.Coefficients;

            var degree = Math.Max(a.Degree, b.Degree);
            var result = new double[degree + 1];

            var commonLength = Math.Min(Math.Min(ac.Length, bc.Length), result.Length);
            for (int i = 0; i < commonLength; i++)
            {
                result[i] = ac[i] + bc[i];
            }

            int acLength = Math.Min(ac.Length, result.Length);
            for (int i = commonLength; i < acLength; i++)
            {
                // no need to add since only one of both applies
                result[i] = ac[i];
            }

            int bcLength = Math.Min(bc.Length, result.Length);
            for (int i = commonLength; i < bcLength; i++)
            {
                // no need to add since only one of both applies
                result[i] = bc[i];
            }

            return new Polynomial(result);
        }

        /// <summary>
        /// Addition of a polynomial and a scalar.
        /// </summary>
        public static Polynomial Add(Polynomial a, double b)
        {
            var ac = a.Coefficients;

            var degree = Math.Max(a.Degree, 0);
            var result = new double[degree + 1];

            var commonLength = Math.Min(ac.Length, result.Length);
            for (int i = 0; i < commonLength; i++)
            {
                result[i] = ac[i];
            }

            result[0] += b;

            return new Polynomial(result);
        }

        /// <summary>
        /// Subtraction of two Polynomials (point-wise).
        /// </summary>
        /// <param name="a">Left Polynomial</param>
        /// <param name="b">Right Polynomial</param>
        /// <returns>Resulting Polynomial</returns>
        public static Polynomial Subtract(Polynomial a, Polynomial b)
        {
            var ac = a.Coefficients;
            var bc = b.Coefficients;

            var degree = Math.Max(a.Degree, b.Degree);
            var result = new double[degree + 1];

            var commonLength = Math.Min(Math.Min(ac.Length, bc.Length), result.Length);
            for (int i = 0; i < commonLength; i++)
            {
                result[i] = ac[i] - bc[i];
            }

            int acLength = Math.Min(ac.Length, result.Length);
            for (int i = commonLength; i < acLength; i++)
            {
                // no need to add since only one of both applies
                result[i] = ac[i];
            }

            int bcLength = Math.Min(bc.Length, result.Length);
            for (int i = commonLength; i < bcLength; i++)
            {
                // no need to add since only one of both applies
                result[i] = -bc[i];
            }

            return new Polynomial(result);
        }

        /// <summary>
        /// Addition of a scalar from a polynomial.
        /// </summary>
        public static Polynomial Subtract(Polynomial a, double b)
        {
            return Add(a, -b);
        }

        /// <summary>
        /// Addition of a polynomial from a scalar.
        /// </summary>
        public static Polynomial Subtract(double b, Polynomial a)
        {
            var ac = a.Coefficients;

            var degree = Math.Max(a.Degree, 0);
            var result = new double[degree + 1];

            var commonLength = Math.Min(ac.Length, result.Length);
            for (int i = 0; i < commonLength; i++)
            {
                result[i] = -ac[i];
            }

            result[0] += b;

            return new Polynomial(result);
        }

        /// <summary>
        /// Negation of a polynomial.
        /// </summary>
        public static Polynomial Negate(Polynomial a)
        {
            var ac = a.Coefficients;

            var degree = a.Degree;
            var result = new double[degree + 1];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = -ac[i];
            }

            return new Polynomial(result);
        }

        /// <summary>
        /// Multiplies a polynomial by a polynomial (convolution)
        /// </summary>
        /// <param name="a">Left polynomial</param>
        /// <param name="b">Right polynomial</param>
        /// <returns>Resulting Polynomial</returns>
        public static Polynomial Multiply(Polynomial a, Polynomial b)
        {
            var aa = a.Clone() as Polynomial;
            var bb = b.Clone() as Polynomial;
            // do not cut trailing zeros, since it may corrupt the outcom, if the array is of form 1 + x^-1 + x^-2 + x^-3
            //a.Trim();
            //b.Trim();

            double[] a1 = aa.Coefficients;
            double[] b1 = bb.Coefficients;
            double[] ret = new double[a1.Length + b1.Length];

            for (int i = 0; i < a1.Length; i++)
            {
                for (int j = 0; j < b1.Length; j++)
                {
                    ret[i + j] += a1[i] * b1[j];
                }
            }

            Polynomial result = new Polynomial(ret);

            //ret_p.Trim();

            return result;
        }

        /// <summary>
        /// Scales a polynomial by a scalar
        /// </summary>
        /// <param name="a">Polynomial</param>
        /// <param name="k">Scalar value</param>
        /// <returns>Resulting Polynomial</returns>
        public static Polynomial Multiply(Polynomial a, double k)
        {
            var aa = a.Clone() as Polynomial;

            for (int ii = 0; ii < aa.Coefficients.Length; ii++)
                aa.Coefficients[ii] *= k;

            return aa;
        }

        /// <summary>
        /// Scales a polynomial by division by a scalar
        /// </summary>
        /// <param name="a">Polynomial</param>
        /// <param name="k">Scalar value</param>
        /// <returns>Resulting Polynomial</returns>
        public static Polynomial Divide(Polynomial a, double k)
        {
            var aa = a.Clone() as Polynomial;

            for (int ii = 0; ii < aa.Coefficients.Length; ii++)
                aa.Coefficients[ii] /= k;

            return aa;
        }

        /// <summary>
        /// Euclidean long division of two polynomials, returning the quotient q and remainder r of the two polynomials a and b such that a = q*b + r
        /// </summary>
        /// <param name="a">Left polynomial</param>
        /// <param name="b">Right polynomial</param>
        /// <returns>A tuple holding quotient in first and remainder in second</returns>
        public static Tuple<Polynomial, Polynomial> DivideRemainder(Polynomial a, Polynomial b)
        {
            if (b.Degree < 0)
                throw new DivideByZeroException("b polynomial ends with zero");

            if (a.Degree < 0)
            {
                // zero divided by non-zero is zero without remainder
                return Tuple.Create(a, a);
            }

            var c1 = a.Coefficients.ToArray();
            var c2 = b.Coefficients.ToArray();

            var n1 = c1.Length;
            var n2 = c2.Length;

            double[] quo = null;
            double[] rem = null;

            if (n2 == 1) // division by scalar
            {
                var fact = c2[0];
                quo = new double[n1];
                for (int i = 0; i < n1; i++)
                    quo[i] = c1[i] / fact;
                rem = new double[] { 0 };
            }
            else if (n1 < n2) // denominator degree higher than nominator degree
            {
                // quotient always be 0 and return c1 as remainder
                quo = new double[] { 0 };
                rem = c1.ToArray();
            }
            else
            {
                var dn = n1 - n2;
                var scl = c2[n2 - 1];
                var c22 = new double[n2 - 1];
                for (int ii = 0; ii < c22.Length; ii++)
                {
                    c22[ii] = c2[ii] / scl;
                }

                int i = dn;
                int j = n1 - 1;
                while (i >= 0)
                {
                    var v = c1[j];
                    for (int k = i; k < j; k++)
                        c1[k] -= c22[k - i] * v;
                    i--;
                    j--;
                }

                var j1 = j + 1;
                var l1 = n1 - j1;

                rem = new double[j1];
                quo = new double[l1];

                for (int k = 0; k < l1; k++)
                {
                    quo[k] = c1[k + j1] / scl;
                }

                for (int k = 0; k < j1; k++)
                {
                    rem[k] = c1[k];
                }

            }

            if (rem == null)
                throw new NullReferenceException("resulting remainder was null");

            if (quo == null)
                throw new NullReferenceException("resulting quotient was null");

            // output mapping
            var pQuo = new Polynomial(quo);
            var pRem = new Polynomial(rem);

            return new Tuple<Polynomial, Polynomial>(pQuo, pRem);
        }
        #endregion

        #region Arithmetic Pointwise Operations
        /// <summary>
        /// Point-wise division of two Polynomials
        /// </summary>
        /// <param name="a">Left Polynomial</param>
        /// <param name="b">Right Polynomial</param>
        /// <returns>Resulting Polynomial</returns>
        public static Polynomial PointwiseDivide(Polynomial a, Polynomial b)
        {
            var ac = a.Coefficients;
            var bc = b.Coefficients;

            var degree = a.Degree;
            var result = new double[degree + 1];

            var commonLength = Math.Min(Math.Min(ac.Length, bc.Length), result.Length);
            for (int i = 0; i < commonLength; i++)
            {
                result[i] = ac[i] / bc[i];
            }

            for (int i = commonLength; i < result.Length; i++)
            {
                result[i] = ac[i] / 0.0;
            }

            return new Polynomial(result);
        }

        /// <summary>
        /// Point-wise multiplication of two Polynomials
        /// </summary>
        /// <param name="a">Left Polynomial</param>
        /// <param name="b">Right Polynomial</param>
        /// <returns>Resulting Polynomial</returns>
        public static Polynomial PointwiseMultiply(Polynomial a, Polynomial b)
        {
            var ac = a.Coefficients;
            var bc = b.Coefficients;

            var degree = Math.Min(a.Degree, b.Degree);
            var result = new double[degree + 1];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = ac[i] * bc[i];
            }

            return new Polynomial(result);
        }
        #endregion

        #region Arithmetic Instance Methods (forwarders)
        /// <summary>
        /// Division of two polynomials returning the quotient-with-remainder of the two polynomials given
        /// </summary>
        /// <param name="b">Right polynomial</param>
        /// <returns>A tuple holding quotient in first and remainder in second</returns>
        public Tuple<Polynomial, Polynomial> DivideRemainder(Polynomial b)
        {
            return DivideRemainder(this, b);
        }
        #endregion

        #region Arithmetic Operator Overloads (forwarders)
        /// <summary>
        /// Addition of two Polynomials (piecewise)
        /// </summary>
        /// <param name="a">Left polynomial</param>
        /// <param name="b">Right polynomial</param>
        /// <returns>Resulting Polynomial</returns>
        public static Polynomial operator +(Polynomial a, Polynomial b)
        {
            return Add(a, b);
        }

        /// <summary>
        /// adds a scalar to a polynomial.
        /// </summary>
        /// <param name="a">Polynomial</param>
        /// <param name="k">Scalar value</param>
        /// <returns>Resulting Polynomial</returns>
        public static Polynomial operator +(Polynomial a, double k)
        {
            return Add(a, k);
        }

        /// <summary>
        /// adds a scalar to a polynomial.
        /// </summary>
        /// <param name="k">Scalar value</param>
        /// <param name="a">Polynomial</param>
        /// <returns>Resulting Polynomial</returns>
        public static Polynomial operator +(double k, Polynomial a)
        {
            return Add(a, k);
        }

        /// <summary>
        /// Subtraction of two polynomial.
        /// </summary>
        /// <param name="a">Left polynomial</param>
        /// <param name="b">Right polynomial</param>
        /// <returns>Resulting Polynomial</returns>
        public static Polynomial operator -(Polynomial a, Polynomial b)
        {
            return Subtract(a, b);
        }

        /// <summary>
        /// Subtracts a scalar from a polynomial.
        /// </summary>
        /// <param name="a">Polynomial</param>
        /// <param name="k">Scalar value</param>
        /// <returns>Resulting Polynomial</returns>
        public static Polynomial operator -(Polynomial a, double k)
        {
            return Subtract(a, k);
        }

        /// <summary>
        /// Subtracts a polynomial from a scalar.
        /// </summary>
        /// <param name="k">Scalar value</param>
        /// <param name="a">Polynomial</param>
        /// <returns>Resulting Polynomial</returns>
        public static Polynomial operator -(double k, Polynomial a)
        {
            return Subtract(k, a);
        }

        /// <summary>
        /// Negates a polynomial.
        /// </summary>
        /// <param name="a">Polynomial</param>
        /// <returns>Resulting Polynomial</returns>
        public static Polynomial operator -(Polynomial a)
        {
            return Negate(a);
        }

        /// <summary>
        /// Multiplies a polynomial by a polynomial (convolution).
        /// </summary>
        /// <param name="a">Left polynomial</param>
        /// <param name="b">Right polynomial</param>
        /// <returns>resulting Polynomial</returns>
        public static Polynomial operator *(Polynomial a, Polynomial b)
        {
            return Multiply(a, b);
        }

        /// <summary>
        /// Multiplies a polynomial by a scalar.
        /// </summary>
        /// <param name="a">Polynomial</param>
        /// <param name="k">Scalar value</param>
        /// <returns>Resulting Polynomial</returns>
        public static Polynomial operator *(Polynomial a, double k)
        {
            return Multiply(a, k);
        }

        /// <summary>
        /// Multiplies a polynomial by a scalar.
        /// </summary>
        /// <param name="k">Scalar value</param>
        /// <param name="a">Polynomial</param>
        /// <returns>Resulting Polynomial</returns>
        public static Polynomial operator *(double k, Polynomial a)
        {
            return Multiply(a, k);
        }

        /// <summary>
        /// Divides a polynomial by scalar value.
        /// </summary>
        /// <param name="a">Polynomial</param>
        /// <param name="k">Scalar value</param>
        /// <returns>Resulting Polynomial</returns>
        public static Polynomial operator /(Polynomial a, double k)
        {
            return Divide(a, k);
        }
        #endregion

        #region ToString
        /// <summary>
        /// Format the polynomial in ascending order, e.g. "4.3 + 2.0x^2 - x^3".
        /// </summary>
        public override string ToString()
        {
            return ToString("G", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Format the polynomial in descending order, e.g. "x^3 + 2.0x^2 - 4.3".
        /// </summary>
        public string ToStringDescending()
        {
            return ToStringDescending("G", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Format the polynomial in ascending order, e.g. "4.3 + 2.0x^2 - x^3".
        /// </summary>
        public string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Format the polynomial in descending order, e.g. "x^3 + 2.0x^2 - 4.3".
        /// </summary>
        public string ToStringDescending(string format)
        {
            return ToStringDescending(format, CultureInfo.CurrentCulture);
        }
        /// <summary>
        /// Format the polynomial in ascending order, e.g. "4.3 + 2.0x^2 - x^3".
        /// </summary>
        public string ToString(IFormatProvider formatProvider)
        {
            return ToString("G", formatProvider);
        }

        /// <summary>
        /// Format the polynomial in descending order, e.g. "x^3 + 2.0x^2 - 4.3".
        /// </summary>
        public string ToStringDescending(IFormatProvider formatProvider)
        {
            return ToStringDescending("G", formatProvider);
        }

        /// <summary>
        /// Format the polynomial in ascending order, e.g. "4.3 + 2.0x^2 - x^3".
        /// </summary>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (Degree < 0)
            {
                return "0";
            }

            var sb = new StringBuilder();
            bool first = true;
            for (int i = 0; i < Coefficients.Length; i++)
            {
                double c = Coefficients[i];
                if (c == 0.0)
                {
                    continue;
                }

                if (first)
                {
                    sb.Append(c.ToString(format, formatProvider));
                    if (i > 0)
                    {
                        sb.Append(VariableName);
                    }
                    if (i > 1)
                    {
                        sb.Append("^");
                        sb.Append(i);
                    }

                    first = false;
                }
                else
                {
                    if (c < 0.0)
                    {
                        sb.Append(" - ");
                        sb.Append((-c).ToString(format, formatProvider));
                    }
                    else
                    {
                        sb.Append(" + ");
                        sb.Append(c.ToString(format, formatProvider));
                    }
                    if (i > 0)
                    {
                        sb.Append(VariableName);
                    }
                    if (i > 1)
                    {
                        sb.Append("^");
                        sb.Append(i);
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Format the polynomial in descending order, e.g. "x^3 + 2.0x^2 - 4.3".
        /// </summary>
        public string ToStringDescending(string format, IFormatProvider formatProvider)
        {
            if (Degree < 0)
            {
                return "0";
            }

            var sb = new StringBuilder();
            bool first = true;
            for (int i = Coefficients.Length - 1; i >= 0; i--)
            {
                double c = Coefficients[i];
                if (c == 0.0)
                {
                    continue;
                }

                if (first)
                {
                    sb.Append(c.ToString(format, formatProvider));
                    if (i > 0)
                    {
                        sb.Append(VariableName);
                    }
                    if (i > 1)
                    {
                        sb.Append("^");
                        sb.Append(i);
                    }

                    first = false;
                }
                else
                {
                    if (c < 0.0)
                    {
                        sb.Append(" - ");
                        sb.Append((-c).ToString(format, formatProvider));
                    }
                    else
                    {
                        sb.Append(" + ");
                        sb.Append(c.ToString(format, formatProvider));
                    }
                    if (i > 0)
                    {
                        sb.Append(VariableName);
                    }
                    if (i > 1)
                    {
                        sb.Append("^");
                        sb.Append(i);
                    }
                }
            }

            return sb.ToString();
        }
        #endregion

        #region Equality
        public bool Equals(Polynomial other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            int n = Degree;
            if (n != other.Degree)
            {
                return false;
            }

            for (var i = 0; i <= n; i++)
            {
                if (!Coefficients[i].Equals(other.Coefficients[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Polynomial)) return false;
            return Equals((Polynomial)obj);
        }

        public override int GetHashCode()
        {
            var hashNum = Math.Min(Degree + 1, 25);
            int hash = 17;
            unchecked
            {
                for (var i = 0; i < hashNum; i++)
                {
                    hash = hash * 31 + Coefficients[i].GetHashCode();
                }
            }
            return hash;
        }
        #endregion
    }
}
