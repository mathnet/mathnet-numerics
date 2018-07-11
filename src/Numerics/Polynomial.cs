using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.LinearRegression;
using MathNet.Numerics.LinearAlgebra.Factorization;

namespace MathNet.Numerics
{
    /// <summary>
    /// a class handlin REAL VALUED Polynomials, complex coefficients can not be handled (yet)
    /// </summary>
    public class Polynomial
    {

        public double[] Coeffs { get; set; }

        /// <summary>
        /// indicator if Polynomial was flipped
        /// </summary>
        public bool IsFlipped { get; }

        /// <summary>
        /// Only needed for the ToString method
        /// </summary>
        public string VarName = "x^";

        /// <summary>
        /// Length of Polynomial (max element + 1) e.G x^5 highest element, will give Length = 6
        /// </summary>
        public int Degree
        {
            get
            {
                return (Coeffs == null ? 0 : Coeffs.Length);
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
            Coeffs = new double[n];
        }


        /// <summary>
        /// make Polynomial: e.G 3.0 = 3.0 + 0 x^1 + 0 x^2
        /// </summary>
        /// <param name="coeff">just the "x^0" part</param>
        public Polynomial(double coeff)
        {
            IsFlipped = false;
            this.Coeffs = new double[1];
            Coeffs[0] = coeff;
        }

        /// <summary>
        /// make Polynomial: e.G new double[] {5, 0, 2} = "5 + 0 x^1 + 2 x^2"
        /// </summary>
        /// <param name="coeffs"> Polynomial coefficiens as array</param>
        public Polynomial(double[] coeffs)
        {
            if (coeffs == null)
            {
                throw new ArgumentNullException("coeffs");
            }
            this.Coeffs = new double[coeffs.Length];
            Array.Copy(coeffs, this.Coeffs, coeffs.Length);
        }


        /// <summary>
        /// constructor setting Polynomial coefficiens and flipping them if necessary.
        /// 
        /// e.G: 
        /// var x = new double[] {5, 4, 3, 0, 2};
        /// var xP1 = new Polynomial(x, isFlip:true);
        /// var xP2 = new Polynomial(x, isFlip:false);
        /// 
        /// xP1:  5 x^3 + 4 x^2 + 3 x^2 + 0 x^1 + 2
        /// xP2:  2 x^3 + 0 x^2 + 3 x^2 + 4 x^1 + 5
        /// 
        /// </summary>
        /// <param name="coeffs"> Polynomial coefficiens as array</param>
        /// <param name="isFlip">use true for flipping</param>
        public Polynomial(double[] coeffs, bool isFlip)
        {
            if (coeffs == null)
            {
                throw new ArgumentNullException("coeffs");
            }
            this.Coeffs = new double[coeffs.Length];
            Array.Copy(coeffs, Coeffs, coeffs.Length);

            if (isFlip)
            {
                Coeffs = Coeffs.Reverse().ToArray();
                IsFlipped = true;
            }
        }

        /// <summary>
        /// remove all trailing zeros, e.G before: "0.00 x^2 + 1.0 x^1 + 1.00" after: "1.0 x^1 + 1.00"
        /// </summary>
        public void CutTrailZeros()
        {
            int count = 0;
            for (int ii = Degree - 1; ii >= 0; ii--)
            {
                if (Coeffs[ii] == 0.0)
                {
                    count++;
                }
                else
                {
                    double[] CoeffsHold = new double[Coeffs.Length];
                    Coeffs.CopyTo(CoeffsHold, 0);
                    Array.Resize(ref CoeffsHold, Degree - count);
                    Coeffs = new double[Degree - count];
                    CoeffsHold.CopyTo(Coeffs, 0);
                    return;
                }
            }
        }

        #region Data Interaction

        /// <summary>
        /// Least-Squares fitting the points (x,y) to a k-order polynomial y : x -> p0 + p1*x + p2*x^2 + ... + pk*x^k,
        /// returning its best fitting parameters as [p0, p1, p2, ..., pk] array, compatible with Evaluate.Polynomial.
        /// A polynomial with order/degree k has (k+1) coefficients and thus requires at least (k+1) samples.
        /// </summary>
        public static Polynomial Fit(double[] x, double[] y, int order, DirectRegressionMethod method = DirectRegressionMethod.QR)
        {
            var pArr = MathNet.Numerics.Fit.Polynomial(x, y, order, method);
            return new Polynomial(pArr);
        }

        /// <summary>
        /// Evaluate a polynomial at point x.
        /// </summary>
        /// <param name="z">The location where to evaluate the polynomial at.</param>
        public double Evaluate(double z)
        {
            return MathNet.Numerics.Evaluate.Polynomial(z, Coeffs);
        }

        /// <summary>
        /// Evaluate a polynomial at points z.
        /// </summary>
        /// <param name="z">The locations where to evaluate the polynomial at.</param>
        public IEnumerable<double> Evaluate(IEnumerable<double> z)
        {
            var Lst = new List<double>();
            foreach (var item in z)
            {
                Lst.Add(Evaluate(item));
            }
            return Lst;
        }

        #endregion

        #region diff/int
        public Polynomial Differentiate()
        {
            
            if (Coeffs.Length == 0)
            {
                return null;
            }

            var t = this.Clone() as Polynomial;
            t.CutTrailZeros();
            var cNew = new double[t.Coeffs.Length - 1];
            for (int i = 1; i < t.Coeffs.Length; i++)
            {
                cNew[i-1] = t.Coeffs[i] * i;
            }
            var p = new Polynomial(cNew, isFlip: IsFlipped);
            p.CutTrailZeros();
            return p;
        }

        public Polynomial Integrate()
        {
            var t = this.Clone() as Polynomial;
            t.CutTrailZeros();
            var cNew = new double[t.Coeffs.Length + 1];
            for (int i = 1; i < cNew.Length; i++)
            {
                cNew[i] = t.Coeffs[i-1] / i;
            }
            var p = new Polynomial(cNew, isFlip: IsFlipped);
            p.CutTrailZeros();
            return p;
        }

        #endregion

        #region Operators


        /// <summary>
        /// multiplies a Polynomial by a Polynomial using convolution [ASINCO.libs.subfun.conv(a.Coeffs, b.Coeffs)]
        /// </summary>
        /// <param name="a">left Polynomial</param>
        /// <param name="b">right Polynomial</param>
        /// <returns>resulting Polynomial</returns>
        public static Polynomial operator *( Polynomial a, Polynomial b)
        {
            // do not cut trailing zeros, since it may corrupt the outcom, if the array is of form 1 + x^-1 + x^-2 + x^-3
            //a.CutTrailZeros();
            //b.CutTrailZeros();

            double[] ret = conv(a.Coeffs, b.Coeffs);
            Polynomial ret_p = new Polynomial(ret);

            //ret_p.CutTrailZeros();

            return (ret_p);

        }

        /// <summary>
        /// multiplies a Polynomial by a scalar
        /// </summary>
        /// <param name="a">left Polynomial</param>
        /// <param name="k">scalar value</param>
        /// <returns>resulting Polynomial</returns>
        public static Polynomial operator *( Polynomial a, double k)
        {
            for (int ii = 0; ii < a.Coeffs.Length; ii++)
                a.Coeffs[ii] *= k;

            return a;
        }

        /// <summary>
        /// adds a scalar to a Polynomial (to the x^0 element)
        /// </summary>
        /// <param name="a">left Polynomial</param>
        /// <param name="k">scalar value</param>
        /// <returns>resulting Polynomial</returns>
        public static Polynomial operator +( Polynomial a, double k)
        {
            a.Coeffs[0] += k;
            return a;
        }

        /// <summary>
        /// substracs a scalar from a Polynomial (from the x^0 element)
        /// </summary>
        /// <param name="a">left Polynomial</param>
        /// <param name="k">scalar value</param>
        /// <returns>resulting Polynomial</returns>
        public static Polynomial operator -( Polynomial a, double k)
        {

            a.Coeffs[0] -= k;
            return a;
        }

        /// <summary>
        /// divide Polynomial by scalar value
        /// </summary>
        /// <param name="a">left Polynomial</param>
        /// <param name="k">scalar value</param>
        /// <returns>resulting Polynomial</returns>
        public static Polynomial operator /( Polynomial a, double k)
        {
            for (int ii = 0; ii < a.Coeffs.Length; ii++)
                a.Coeffs[ii] /= k;

            return a;
        }

        /// <summary>
        /// Addition of two Polynomials (piecewise)
        /// </summary>
        /// <param name="a">left Polynomial</param>
        /// <param name="b">right Polynomial</param>
        /// <returns>resulting Polynomial</returns>
        public static Polynomial operator +( Polynomial a, Polynomial b)
        {
            return Add(a, b);
        }

        /// <summary>
        /// substraction of two Polynomials (piecewise)
        /// </summary>
        /// <param name="a">left Polynomial</param>
        /// <param name="b">right Polynomial</param>
        /// <returns>resulting Polynomial</returns>
        public static Polynomial operator -( Polynomial a, Polynomial b)
        {
            return Substract(a, b);
        }
        
        /// <summary>
        /// Calculates the complex roots of the Polynomial in the same way as matlab does
        /// </summary>
        /// <returns>a vector of complex numbers with the roots</returns>
        public Complex[] GetRoots()
        {
            DenseMatrix A = this.GetEigValMatrix();
            Complex[] c_vec;

            if (A == null)
            {
                if (Coeffs.Length < 2)
                {
                    var val = Coeffs.Length == 1 ? Coeffs[0] : Double.NaN;
                    c_vec = new Complex[1] { val };
                }
                else
                    c_vec = new Complex[1] { new Complex(-Coeffs[0] / Coeffs[1], 0) };
            }
            else
            {
                Evd<double> eigen = A.Evd(Symmetricity.Asymmetric);
                c_vec = eigen.EigenValues.ToArray();
            }
            return c_vec;
        }

        /// <summary>
        /// get the eigenvalue matrix A of this Polynomial such that eig(A) = roots of this Polynomial
        /// </summary>
        /// <returns>Eigenvalue matrix A</returns>
        public DenseMatrix GetEigValMatrix()
        {
            Polynomial pLoc = new Polynomial(this.Coeffs);
            pLoc.CutTrailZeros();

            int n = pLoc.Coeffs.Length - 1;
            if (n < 2)
                return null;

            double[] p = new double[n];

            double a0 = pLoc.Coeffs[n];

            for (int ii = n - 1; ii >= 0; ii--)
                p[ii] = -pLoc.Coeffs[ii] / a0;

            DenseMatrix A0 = DenseMatrix.CreateDiagonal(n - 1, n - 1, 1.0);
            DenseMatrix A = new DenseMatrix(n);

            A.SetSubMatrix(1, 0, A0);

            A.SetRow(0, p.Reverse().ToArray());
            return A;
        }

        /// <summary>
        /// pointwise division of two Polynomials
        /// </summary>
        /// <param name="a">left Polynomial</param>
        /// <param name="b">right Polynomial</param>
        /// <returns>resulting Polynomial</returns>
        public static Polynomial DividePointwise( Polynomial a, Polynomial b)
        {
            if (a.Coeffs.Length != b.Coeffs.Length)
                mkSameLength(ref a, ref b);

            int n = a.Coeffs.Length;
            double[] res = new double[a.Coeffs.Length];


            for (int ii = 0; ii < n; ii++)
            {
                res[ii] = a.Coeffs[ii] / b.Coeffs[ii];
            }
            Polynomial res_poly = new Polynomial(res);
            return (res_poly);
        }

        /// <summary>
        /// pointwise multiplication of two Polynomials
        /// </summary>
        /// <param name="a">left Polynomial</param>
        /// <param name="b">right Polynomial</param>
        /// <returns>resulting Polynomial</returns>
        public static Polynomial MultiplyPointwise( Polynomial a, Polynomial b)
        {
            if (a.Coeffs.Length != b.Coeffs.Length)
                mkSameLength(ref a, ref b);

            int n = a.Coeffs.Length;
            double[] res = new double[a.Coeffs.Length];


            for (int ii = 0; ii < n; ii++)
            {
                res[ii] = a.Coeffs[ii] * b.Coeffs[ii];
            }
            Polynomial res_poly = new Polynomial(res);
            return (res_poly);
        }

        /// <summary>
        /// Addition of two Polynomials (piecewise)
        /// </summary>
        /// <param name="a">left Polynomial</param>
        /// <param name="b">right Polynomial</param>
        /// <returns>resulting Polynomial</returns>
        public static Polynomial Add( Polynomial a, Polynomial b)
        {

            if (a.Degree != b.Degree)
                mkSameLength(ref a, ref b);

            int n = a.Degree;
            double[] res = new double[n];


            for (int ii = 0; ii < n; ii++)
            {
                res[ii] = a.Coeffs[ii] + b.Coeffs[ii];
            }
            Polynomial res_poly = new Polynomial(res);
            return (res_poly);
        }

        /// <summary>
        /// substraction of two Polynomials (piecewise)
        /// </summary>
        /// <param name="a">left Polynomial</param>
        /// <param name="b">right Polynomial</param>
        /// <returns>resulting Polynomial</returns>
        public static Polynomial Substract( Polynomial a, Polynomial b)
        {

            if (a.Degree != b.Degree)
                mkSameLength(ref a, ref b);

            int n = a.Degree;
            double[] res = new double[n];


            for (int ii = 0; ii < n; ii++)
            {
                res[ii] = a.Coeffs[ii] - b.Coeffs[ii];
            }
            Polynomial res_poly = new Polynomial(res);
            return (res_poly);
        }

        #endregion

        #region Displaying
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
            string strLoc = "";
            if (this.Coeffs == null)
            {
                return "null";
            }
            if (this.Coeffs.Length == 0)
            {
                return "";
            }

            if (!highestFirst)
            {
                for (int ii = 0; ii < Coeffs.Length; ii++)
                {

                    if (ii == 0)
                        strLoc += String.Format("{0} + ", this.Coeffs[ii], VarName, ii);
                    else if (ii == Coeffs.Length - 1)
                        strLoc += String.Format("{0}{1}{2}", this.Coeffs[ii], VarName, ii);
                    else
                        strLoc += String.Format("{0}{1}{2} + ", this.Coeffs[ii], VarName, ii);
                }
            }
            else
            {
                for (int ii = Coeffs.Length - 1; ii >= 0; ii--)
                {
                    if (ii == 0)
                        strLoc += this.Coeffs[ii].ToString();
                    else
                        strLoc += String.Format("{0}{1}{2} + ", this.Coeffs[ii], VarName, ii);
                }
            }

            return strLoc;
        }


        #endregion

        #region Interfacing

        /// <summary>
        /// This method returns the coefficcients of the Polynomial as an array the "IsFlipped" property, 
        /// which is set during construction is taken into account automatically.
        /// </summary>
        /// <returns>the coefficcients of the Polynomial as an array</returns>
        public double[] ToArray()
        {
            if (IsFlipped == true)
                return (Coeffs.Reverse().ToArray());
            else
                return (Coeffs);
        }

        #endregion

        #region Helpers

        private static void mkSameLength(ref Polynomial a, ref Polynomial b)
        {
            double[] aHold = new double[a.Coeffs.Length];
            double[] bHold = new double[b.Coeffs.Length];
            Array.Copy(a.Coeffs, aHold, a.Coeffs.Length);
            Array.Copy(b.Coeffs, bHold, b.Coeffs.Length);

            if (a.Coeffs.Length < b.Coeffs.Length)
            {
                a.Coeffs = new double[b.Coeffs.Length];
                b.Coeffs = new double[b.Coeffs.Length];
                Array.Copy(aHold, a.Coeffs, aHold.Length);
                Array.Copy(bHold, b.Coeffs, bHold.Length);
            }
            else
            {
                a.Coeffs = new double[a.Coeffs.Length];
                b.Coeffs = new double[a.Coeffs.Length];
                Array.Copy(aHold, a.Coeffs, aHold.Length);
                Array.Copy(bHold, b.Coeffs, bHold.Length);
            }

        }

        /// <summary>
        /// (full) convolution of two arrays 
        /// </summary>
        /// <param name="a">left vector</param>
        /// <param name="b">right vector</param>
        /// <returns>convolution of a and b as vector</returns>
        private static double[] conv(double[] a, double[] b)
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
            var p = new double[this.Coeffs.Length];
            Array.Copy(Coeffs, p, Coeffs.Length);
            return new Polynomial(p, isFlip: IsFlipped);
        }
        #endregion

    }

}
