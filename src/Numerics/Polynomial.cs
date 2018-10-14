using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearRegression;
using MathNet.Numerics.LinearAlgebra.Factorization;

namespace MathNet.Numerics
{
    /// <summary>
    /// A single-variable polynomial with real-valued coefficients and non-negative exponents.
    /// </summary>
    public class Polynomial
    {
        /// <summary>
        /// The coefficients of the polynomial in a
        /// </summary>
        public double[] Coefficients { get; private set; }

        /// <summary>
        /// Only needed for the ToString method
        /// </summary>
        public string VarName = "x^";

        /// <summary>
        /// Degree of the polynomial, i.e. the largest monomial exponent. For example, the degree of x^2+x^5 is 5.
        /// The null-polynomial returns degree -1 because the correct degree, negative infinity, cannot be represented by integers.
        /// </summary>
        public int Degree
        {
            get
            {
                if (Coefficients == null)
                {
                    return -1;
                }

                for (int i = Coefficients.Length - 1; i >= 0; i--)
                {
                    if (Coefficients[i] != 0.0)
                    {
                        return i;
                    }
                }

                return -1;
            }
        }

        /// <summary>
        /// constructor setting a Polynomial of size n containing only zeros
        /// </summary>
        /// <param name="n">size of Polynomial</param>
        public Polynomial(int n)
        {
            if (n < 0)
            {
                throw new ArgumentOutOfRangeException("n must be postive");
            }
            Coefficients = new double[n];
        }

        /// <summary>
        /// make Polynomial: e.G 3.0 = 3.0 + 0 x^1 + 0 x^2
        /// </summary>
        /// <param name="coefficient">just the "x^0" part</param>
        public Polynomial(double coefficient)
        {
            Coefficients = new double[1];
            Coefficients[0] = coefficient;
        }

        /// <summary>
        /// make Polynomial: e.G new double[] {5, 0, 2} = "5 + 0 x^1 + 2 x^2"
        /// </summary>
        /// <param name="coefficients">Polynomial coefficients as enumerable</param>
        public Polynomial(IEnumerable<double> coefficients)
        {
            if (coefficients == null)
            {
                throw new ArgumentNullException(nameof(coefficients));
            }
            Coefficients = coefficients.ToArray();
        }

        /// <summary>
        /// make Polynomial: e.G new double[] {5, 0, 2} = "5 + 0 x^1 + 2 x^2"
        /// </summary>
        /// <param name="coefficients">Polynomial coefficients as array</param>
        public Polynomial(double[] coefficients)
        {
            if (coefficients == null)
            {
                throw new ArgumentNullException(nameof(coefficients));
            }
            Coefficients = new double[coefficients.Length];
            Array.Copy(coefficients, Coefficients, coefficients.Length);
        }

        /// <summary>
        /// remove all trailing zeros, e.G before: "3 + 2 x^1 + 0 x^2" after: "3 + 2 x^1"
        /// </summary>
        public void Trim()
        {
            if (Coefficients.Length == 1)
            {
                return;
            }

            int i = Coefficients.Length - 1;
            while (i >= 0 && Coefficients[i] == 0.0)
            {
                i--;
            }

            if (i < 0)
            {
                Coefficients = new[] { 0.0 };
            }
            else if (i == 0)
            {
                Coefficients = new[] { Coefficients[0] };
            }
            else
            {
                var hold = new double[i+1];
                Array.Copy(Coefficients, hold, i+1);
                Coefficients = hold;
            }
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

        public Polynomial Differentiate()
        {
            if (Coefficients.Length == 0)
            {
                return null;
            }

            var t = Clone() as Polynomial;
            t.Trim();
            var cNew = new double[t.Coefficients.Length - 1];
            for (int i = 1; i < t.Coefficients.Length; i++)
            {
                cNew[i-1] = t.Coefficients[i] * i;
            }

            var p = new Polynomial(cNew);
            p.Trim();
            return p;
        }

        public Polynomial Integrate()
        {
            var t = Clone() as Polynomial;
            t.Trim();
            var cNew = new double[t.Coefficients.Length + 1];
            for (int i = 1; i < cNew.Length; i++)
            {
                cNew[i] = t.Coefficients[i - 1] / i;
            }

            var p = new Polynomial(cNew);
            p.Trim();
            return p;
        }

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
        /// multiplies a Polynomial by a Polynomial using convolution [ASINCO.libs.subfun.conv(a.Coeffs, b.Coeffs)]
        /// </summary>
        /// <param name="a">Left polynomial</param>
        /// <param name="b">Right polynomial</param>
        /// <returns>resulting Polynomial</returns>
        public static Polynomial operator *(Polynomial a, Polynomial b)
        {
            var aa = a.Clone() as Polynomial;
            var bb = b.Clone() as Polynomial;
            // do not cut trailing zeros, since it may corrupt the outcom, if the array is of form 1 + x^-1 + x^-2 + x^-3
            //a.Trim();
            //b.Trim();

            double[] ret = Convolution(aa.Coefficients, bb.Coefficients);
            Polynomial result = new Polynomial(ret);

            //ret_p.Trim();

            return result;
        }

        /// <summary>
        /// multiplies a Polynomial by a scalar
        /// </summary>
        /// <param name="a">Polynomial</param>
        /// <param name="k">Scalar value</param>
        /// <returns>Resulting Polynomial</returns>
        public static Polynomial operator *(Polynomial a, double k)
        {
            var aa = a.Clone() as Polynomial;

            for (int ii = 0; ii < aa.Coefficients.Length; ii++)
                aa.Coefficients[ii] *= k;

            return aa;
        }

        /// <summary>
        /// divide Polynomial by scalar value
        /// </summary>
        /// <param name="a">Polynomial</param>
        /// <param name="k">Scalar value</param>
        /// <returns>Resulting Polynomial</returns>
        public static Polynomial operator /(Polynomial a, double k)
        {
            var aa = a.Clone() as Polynomial;

            for (int ii = 0; ii < aa.Coefficients.Length; ii++)
                aa.Coefficients[ii] /= k;

            return aa;
        }

        /// <summary>
        /// Calculates the complex roots of the Polynomial by eigenvalue decomposition
        /// </summary>
        /// <returns>a vector of complex numbers with the roots</returns>
        public Complex[] Roots()
        {
            DenseMatrix A = EigenvalueMatrix();
            Complex[] roots;

            if (A == null)
            {
                if (Coefficients.Length < 2)
                {
                    var val = Coefficients.Length == 1 ? Coefficients[0] : Double.NaN;
                    roots = new Complex[] { val };
                }
                else
                    roots = new[] { new Complex(-Coefficients[0] / Coefficients[1], 0) };
            }
            else
            {
                Evd<double> eigen = A.Evd(Symmetricity.Asymmetric);
                roots = eigen.EigenValues.ToArray();
            }

            return roots;
        }

        /// <summary>
        /// get the eigenvalue matrix A of this Polynomial such that eig(A) = roots of this Polynomial.
        /// </summary>
        /// <returns>Eigenvalue matrix A</returns>
        /// <note>this matrix is similar to the companion matrix of this polynomial, in such a way, that it's transpose is the columnflip of the companion matrix</note>
        public DenseMatrix EigenvalueMatrix()
        {
            Polynomial pLoc = new Polynomial(Coefficients);
            pLoc.Trim();

            int n = pLoc.Coefficients.Length - 1;
            if (n < 2)
            {
                return null;
            }

            double a0 = pLoc.Coefficients[n];
            double[] p = new double[n];
            for (int ii = n - 1; ii >= 0; ii--)
            {
                p[ii] = -pLoc.Coefficients[ii] / a0;
            }

            DenseMatrix A0 = DenseMatrix.CreateDiagonal(n - 1, n - 1, 1.0);
            DenseMatrix A = new DenseMatrix(n);

            A.SetSubMatrix(1, 0, A0);
            A.SetRow(0, p.Reverse().ToArray());

            return A;
        }

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

        /// <summary>
        /// Division of two polynomials returning the quotient-with-remainder of the two polynomials given
        /// </summary>
        /// <param name="a">Left polynomial</param>
        /// <param name="b">Right polynomial</param>
        /// <returns>A tuple holding quotient in first and remainder in second</returns>
        public static Tuple<Polynomial, Polynomial> DivideLong(Polynomial a, Polynomial b)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));
            if (b == null)
                throw new ArgumentNullException(nameof(b));

            if (a.Coefficients.Length <= 0)
                throw new ArgumentOutOfRangeException("a Degree must be greater than zero");
            if (b.Coefficients.Length <= 0)
                throw new ArgumentOutOfRangeException("b Degree must be greater than zero");

            if (b.Coefficients[b.Coefficients.Length - 1] == 0)
                throw new DivideByZeroException("b polynomial ends with zero");

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
            else if(n1 < n2) // denominator degree higher than nominator degree
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
                        c1[k] -= c22[k-i] * v;
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

            pRem.Trim();
            pQuo.Trim();
            return new Tuple<Polynomial, Polynomial>(pQuo, pRem);
        }

        /// <summary>
        /// Division of two polynomials returning the quotient-with-remainder of the two polynomials given
        /// </summary>
        /// <param name="b">Right polynomial</param>
        /// <returns>A tuple holding quotient in first and remainder in second</returns>
        public Tuple<Polynomial, Polynomial> DivideLong(Polynomial b)
        {
            return DivideLong(this, b);
        }

        /// <summary>
        /// "0.00 x^3 + 0.00 x^2 + 0.00 x^1 + 0.00" like display of this Polynomial
        /// </summary>
        /// <returns>string in displayed format</returns>
        public override string ToString()
        {
            return ToString(highestFirst:false);
        }

        /// <summary>
        /// "0.00 x^3 + 0.00 x^2 + 0.00 x^1 + 0.00" like display of this Polynomial
        /// </summary>
        /// <returns>string in displayed format</returns>
        public string ToString(bool highestFirst)
        {
            if (Coefficients == null)
            {
                return "null";
            }
            if (Coefficients.Length == 0)
            {
                return "";
            }

            string result = "";
            if (!highestFirst)
            {
                for (int ii = 0; ii < Coefficients.Length; ii++)
                {

                    if (ii == 0 && Coefficients.Length == 1)
                        result += String.Format("{0}", Coefficients[ii], VarName, ii);
                    else if(ii == 0)
                        result += String.Format("{0} + ", Coefficients[ii], VarName, ii);
                    else if (ii == Coefficients.Length - 1)
                        result += String.Format("{0}{1}{2}", Coefficients[ii], VarName, ii);
                    else
                        result += String.Format("{0}{1}{2} + ", Coefficients[ii], VarName, ii);
                }
            }
            else
            {
                for (int ii = Coefficients.Length - 1; ii >= 0; ii--)
                {
                    if (ii == 0)
                        result += Coefficients[ii].ToString();
                    else
                        result += String.Format("{0}{1}{2} + ", Coefficients[ii], VarName, ii);
                }
            }

            return result;
        }

        /// <summary>
        /// This method returns the coefficients of the Polynomial as an array the "IsFlipped" property,
        /// which is set during construction is taken into account automatically.
        /// </summary>
        /// <returns>the coefficients of the Polynomial as an array</returns>
        public double[] ToArray()
        {
            return Coefficients.ToArray();
        }

        /// <summary>
        /// Full convolution of two arrays
        /// </summary>
        /// <returns>convolution of a and b as vector</returns>
        static double[] Convolution(double[] a, double[] b)
        {
            double[] ret = new double[a.Length + b.Length];

            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < b.Length; j++)
                {
                    ret[i + j] += a[i] * b[j];
                }
            }
            return ret;
        }

        public object Clone()
        {
            // TODO: this assumes the constructor does a copy
            return new Polynomial(Coefficients);
        }
    }
}
