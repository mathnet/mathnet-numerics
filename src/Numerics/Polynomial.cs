using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Linq;
using Complex = System.Numerics.Complex;
using System.Text;
#if NET5_0_OR_GREATER
using System.Text.Json.Serialization;
#endif
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearRegression;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System.Runtime;

namespace MathNet.Numerics
{
    /// <summary>
    /// A single-variable polynomial with real-valued coefficients and non-negative exponents.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "urn:MathNet/Numerics")]
    public class Polynomial : IFormattable, IEquatable<Polynomial>, ICloneable
    {
        /// <summary>
        /// The coefficients of the polynomial in a
        /// </summary>
        [DataMember(Order = 1)]
#if NET5_0_OR_GREATER
        [JsonInclude]
#endif
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
        /// An array of length N can support polynomials of a degree of at most N-1.
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
            Coefficients = Array.Empty<double>();
        }

        /// <summary>
        /// Create a constant polynomial.
        /// Example: 3.0 -> "p : x -> 3.0"
        /// </summary>
        /// <param name="coefficient">The coefficient of the "x^0" monomial.</param>
        public Polynomial(double coefficient)
        {
            if (coefficient == 0.0)
            {
                Coefficients = Array.Empty<double>();
            }
            else
            {
                Coefficients = new[] { coefficient };
            }
        }

        /// <summary>
        /// Create a polynomial with the provided coefficients (in ascending order, where the index matches the exponent).
        /// Example: {5, 0, 2} -> "p : x -> 5 + 0 x^1 + 2 x^2".
        /// </summary>
        /// <param name="coefficients">Polynomial coefficients as array</param>
        public Polynomial(params double[] coefficients)
        {
            Coefficients = coefficients;
        }

        /// <summary>
        /// Create a polynomial with the provided coefficients (in ascending order, where the index matches the exponent).
        /// Example: {5, 0, 2} -> "p : x -> 5 + 0 x^1 + 2 x^2".
        /// </summary>
        /// <param name="coefficients">Polynomial coefficients as enumerable</param>
        public Polynomial(IEnumerable<double> coefficients) : this(coefficients.ToArray())
        {
        }

        public static Polynomial Zero => new Polynomial();

        /// <summary>
        /// Least-Squares fitting the points (x,y) to a k-order polynomial y : x -> p0 + p1*x + p2*x^2 + ... + pk*x^k
        /// </summary>
        public static Polynomial Fit(double[] x, double[] y, int order, DirectRegressionMethod method = DirectRegressionMethod.QR)
        {
            var coefficients = Numerics.Fit.Polynomial(x, y, order, method);
            return new Polynomial(coefficients);
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

        #region Evaluation

        /// <summary>
        /// Evaluate a polynomial at point x.
        /// Coefficients are ordered ascending by power with power k at index k.
        /// Example: coefficients [3,-1,2] represent y=2x^2-x+3.
        /// </summary>
        /// <param name="z">The location where to evaluate the polynomial at.</param>
        /// <param name="coefficients">The coefficients of the polynomial, coefficient for power k at index k.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="coefficients"/> is a null reference.
        /// </exception>
        public static double Evaluate(double z, params double[] coefficients)
        {

            // 2020-10-07 jbialogrodzki #730 Since this is public API we should probably
            // handle null arguments? It doesn't seem to have been done consistently in this class though.
            if (coefficients == null)
            {
                throw new ArgumentNullException(nameof(coefficients));
            }

            // 2020-10-07 jbialogrodzki #730 Zero polynomials need explicit handling.
            // Without this check, we attempted to peek coefficients at negative indices!
            int n = coefficients.Length;
            if (n == 0)
            {
                return 0;
            }

            double sum = coefficients[n - 1];
            for (int i = n - 2; i >= 0; --i)
            {
                sum *= z;
                sum += coefficients[i];
            }

            return sum;

        }

        /// <summary>
        /// Evaluate a polynomial at point x.
        /// Coefficients are ordered ascending by power with power k at index k.
        /// Example: coefficients [3,-1,2] represent y=2x^2-x+3.
        /// </summary>
        /// <param name="z">The location where to evaluate the polynomial at.</param>
        /// <param name="coefficients">The coefficients of the polynomial, coefficient for power k at index k.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="coefficients"/> is a null reference.
        /// </exception>
        public static Complex Evaluate(Complex z, params double[] coefficients)
        {

            // 2020-10-07 jbialogrodzki #730 Since this is a public API we should probably
            // handle null arguments? It doesn't seem to have been done consistently in this class though.
            if (coefficients == null)
            {
                throw new ArgumentNullException(nameof(coefficients));
            }

            // 2020-10-07 jbialogrodzki #730 Zero polynomials need explicit handling.
            // Without this check, we attempted to peek coefficients at negative indices!
            int n = coefficients.Length;
            if (n == 0)
            {
                return 0;
            }

            Complex sum = coefficients[n - 1];
            for (int i = n - 2; i >= 0; --i)
            {
                sum *= z;
                sum += coefficients[i];
            }

            return sum;

        }

        /// <summary>
        /// Evaluate a polynomial at point x.
        /// Coefficients are ordered ascending by power with power k at index k.
        /// Example: coefficients [3,-1,2] represent y=2x^2-x+3.
        /// </summary>
        /// <param name="z">The location where to evaluate the polynomial at.</param>
        /// <param name="coefficients">The coefficients of the polynomial, coefficient for power k at index k.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="coefficients"/> is a null reference.
        /// </exception>
        public static Complex Evaluate(Complex z, params Complex[] coefficients)
        {

            // 2020-10-07 jbialogrodzki #730 Since this is a public API we should probably
            // handle null arguments? It doesn't seem to have been done consistently in this class though.
            if (coefficients == null)
            {
                throw new ArgumentNullException(nameof(coefficients));
            }

            // 2020-10-07 jbialogrodzki #730 Zero polynomials need explicit handling.
            // Without this check, we attempted to peek coefficients at negative indices!
            int n = coefficients.Length;
            if (n == 0)
            {
                return 0;
            }

            Complex sum = coefficients[n - 1];
            for (int i = n - 2; i >= 0; --i)
            {
                sum *= z;
                sum += coefficients[i];
            }

            return sum;

        }

        /// <summary>
        /// Evaluate a polynomial at point x.
        /// </summary>
        /// <param name="z">The location where to evaluate the polynomial at.</param>
        public double Evaluate(double z)
        {
            return Evaluate(z, Coefficients);
        }

        /// <summary>
        /// Evaluate a polynomial at point x.
        /// </summary>
        /// <param name="z">The location where to evaluate the polynomial at.</param>
        public Complex Evaluate(Complex z)
        {
            return Evaluate(z, Coefficients);
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
                return Zero;
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
                    return Array.Empty<Complex>();
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
            for (int i = n - 1; i >= 0; i--)
            {
                p[i] = -Coefficients[i] / aN;
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
        /// <exception cref="ArgumentNullException">
        /// <paramref name="a"/> or <paramref name="b"/> is a null reference.
        /// </exception>
        public static Polynomial Multiply(Polynomial a, Polynomial b)
        {

            // 2020-10-07 jbialogrodzki #730 Since this is a public API we should probably
            // handle null arguments? It doesn't seem to have been done consistently in this class though.
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            var ad = a.Degree;
            var bd = b.Degree;

            // 2020-10-07 jbialogrodzki #730 Zero polynomials need explicit handling.
            // Without this check, we attempted to create arrays of negative lengths!
            if (ad < 0 || bd < 0)
            {
                return Polynomial.Zero;
            }

            double[] ac = a.Coefficients;
            double[] bc = b.Coefficients;

            var degree = ad + bd;
            double[] result = new double[degree + 1];

            for (int i = 0; i <= ad; i++)
            {
                for (int j = 0; j <= bd; j++)
                {
                    result[i + j] += ac[i] * bc[j];
                }
            }

            return new Polynomial(result);

        }

        /// <summary>
        /// Scales a polynomial by a scalar
        /// </summary>
        /// <param name="a">Polynomial</param>
        /// <param name="k">Scalar value</param>
        /// <returns>Resulting Polynomial</returns>
        public static Polynomial Multiply(Polynomial a, double k)
        {
            var ac = a.Coefficients;

            var result = new double[a.Degree + 1];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = ac[i] * k;
            }

            return new Polynomial(result);
        }

        /// <summary>
        /// Scales a polynomial by division by a scalar
        /// </summary>
        /// <param name="a">Polynomial</param>
        /// <param name="k">Scalar value</param>
        /// <returns>Resulting Polynomial</returns>
        public static Polynomial Divide(Polynomial a, double k)
        {
            var ac = a.Coefficients;

            var result = new double[a.Degree + 1];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = ac[i] / k;
            }

            return new Polynomial(result);
        }

        /// <summary>
        /// Euclidean long division of two polynomials, returning the quotient q and remainder r of the two polynomials a and b such that a = q*b + r
        /// </summary>
        /// <param name="a">Left polynomial</param>
        /// <param name="b">Right polynomial</param>
        /// <returns>A tuple holding quotient in first and remainder in second</returns>
        public static (Polynomial, Polynomial) DivideRemainder(Polynomial a, Polynomial b)
        {
            var bDegree = b.Degree;
            if (bDegree < 0)
            {
                throw new DivideByZeroException("b polynomial ends with zero");
            }

            var aDegree = a.Degree;
            if (aDegree < 0)
            {
                // zero divided by non-zero is zero without remainder
                return (a, a);
            }

            if (bDegree == 0)
            {
                // division by scalar
                return (Divide(a, b.Coefficients[0]), Zero);
            }

            if (aDegree < bDegree)
            {
                // denominator degree higher than nominator degree
                // quotient always be 0 and return c1 as remainder
                return (Zero, a);
            }

            var c1 = a.Coefficients.ToArray();
            var c2 = b.Coefficients.ToArray();

            var scl = c2[bDegree];
            var c22 = new double[bDegree];
            for (int ii = 0; ii < c22.Length; ii++)
            {
                c22[ii] = c2[ii] / scl;
            }

            int i = aDegree - bDegree;
            int j = aDegree;
            while (i >= 0)
            {
                var v = c1[j];
                for (int k = i; k < j; k++)
                {
                    c1[k] -= c22[k - i] * v;
                }

                i--;
                j--;
            }

            var j1 = j + 1;
            var l1 = aDegree - j;

            var quo = new double[l1];
            for (int k = 0; k < l1; k++)
            {
                quo[k] = c1[k + j1] / scl;
            }

            var rem = new double[j1];
            for (int k = 0; k < j1; k++)
            {
                rem[k] = c1[k];
            }

            return (new Polynomial(quo), new Polynomial(rem));
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
        public (Polynomial, Polynomial) DivideRemainder(Polynomial b)
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

        #region Clone

        public Polynomial Clone()
        {
            int degree = EvaluateDegree(Coefficients);
            var coefficients = new double[degree + 1];
            Array.Copy(Coefficients, coefficients, coefficients.Length);
            return new Polynomial(coefficients);
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion
    }
}
