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
    /// a class handling REAL VALUED Polynomials, complex coefficients can not be handled (yet)
    /// </summary>
    public class Polynomial
    {
        /// <summary>
        /// The coefficients of the polynomial in a
        /// </summary>
        public double[] Coeffs { get; set; }

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
            this.Coeffs = new double[1];
            Coeffs[0] = coeff;
        }

        /// <summary>
        /// make Polynomial: e.G new double[] {5, 0, 2} = "5 + 0 x^1 + 2 x^2"
        /// </summary>
        /// <param name="coeffs"> Polynomial coefficiens as enumerable</param>
        public Polynomial(IEnumerable<double> coeffs)
        {
            if (coeffs == null)
            {
                throw new ArgumentNullException("coeffs");
            }
            this.Coeffs = coeffs.ToArray();
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
        /// remove all trailing zeros, e.G before: "3 + 2 x^1 + 0 x^2" after: "3 + 2 x^1"
        /// </summary>
        public void Trim()
        {
            if (Degree == 1)
                return;

            int i = Degree - 1;
            while (i >= 0 && Coeffs[i] == 0.0)
                i--;
            
            if (i < 0)
                Coeffs = new double[1] { 0.0 };
            else if (i == 0)
                Coeffs = new double[1] { Coeffs[0] };
            else
            {
                var hold = new double[i+1];
                Array.Copy(Coeffs, hold, i+1);
                Coeffs = hold;
            }
        }
        

        #region Data Interaction

        /// <summary>
        /// Least-Squares fitting the points (x,y) to a k-order polynomial y : x -> p0 + p1*x + p2*x^2 + ... + pk*x^k
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
            t.Trim();
            var cNew = new double[t.Coeffs.Length - 1];
            for (int i = 1; i < t.Coeffs.Length; i++)
            {
                cNew[i-1] = t.Coeffs[i] * i;
            }
            var p = new Polynomial(cNew);
            p.Trim();
            return p;
        }

        public Polynomial Integrate()
        {
            var t = this.Clone() as Polynomial;
            t.Trim();
            var cNew = new double[t.Coeffs.Length + 1];
            for (int i = 1; i < cNew.Length; i++)
            {
                cNew[i] = t.Coeffs[i-1] / i;
            }
            var p = new Polynomial(cNew);
            p.Trim();
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
            var aa = a.Clone() as Polynomial;
            var bb = b.Clone() as Polynomial;
            // do not cut trailing zeros, since it may corrupt the outcom, if the array is of form 1 + x^-1 + x^-2 + x^-3
            //a.Trim();
            //b.Trim();

            double[] ret = conv(aa.Coeffs, bb.Coeffs);
            Polynomial ret_p = new Polynomial(ret);

            //ret_p.Trim();

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
            var aa = a.Clone() as Polynomial;
            

            for (int ii = 0; ii < aa.Coeffs.Length; ii++)
                aa.Coeffs[ii] *= k;

            return aa;
        }

        /// <summary>
        /// adds a scalar to a Polynomial (to the x^0 element)
        /// </summary>
        /// <param name="a">left Polynomial</param>
        /// <param name="k">scalar value</param>
        /// <returns>resulting Polynomial</returns>
        public static Polynomial operator +( Polynomial a, double k)
        {
            var aa = a.Clone() as Polynomial;

            aa.Coeffs[0] += k;
            return aa;
        }

        /// <summary>
        /// substracs a scalar from a Polynomial (from the x^0 element)
        /// </summary>
        /// <param name="a">left Polynomial</param>
        /// <param name="k">scalar value</param>
        /// <returns>resulting Polynomial</returns>
        public static Polynomial operator -( Polynomial a, double k)
        {
            var aa = a.Clone() as Polynomial;

            a.Coeffs[0] -= k;
            return aa;
        }

        /// <summary>
        /// divide Polynomial by scalar value
        /// </summary>
        /// <param name="a">left Polynomial</param>
        /// <param name="k">scalar value</param>
        /// <returns>resulting Polynomial</returns>
        public static Polynomial operator /( Polynomial a, double k)
        {
            var aa = a.Clone() as Polynomial;
            
            for (int ii = 0; ii < aa.Coeffs.Length; ii++)
                aa.Coeffs[ii] /= k;

            return aa;
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
        /// Calculates the complex roots of the Polynomial by eigenvalue decomposition
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
        /// get the eigenvalue matrix A of this Polynomial such that eig(A) = roots of this Polynomial.
        /// </summary>
        /// <returns>Eigenvalue matrix A</returns>
        /// <note>this matrix is similar to the companion matrix of this polynomial, in such a way, that it's transpose is the columnflip of the companion matrix</note>
        public DenseMatrix GetEigValMatrix()
        {
            Polynomial pLoc = new Polynomial(this.Coeffs);
            pLoc.Trim();

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
            var aa = a.Clone() as Polynomial;
            var bb = b.Clone() as Polynomial;

            if (aa.Coeffs.Length != bb.Coeffs.Length)
                mkSameLength(ref aa, ref bb);

            int n = aa.Coeffs.Length;
            double[] res = new double[aa.Coeffs.Length];


            for (int ii = 0; ii < n; ii++)
            {
                res[ii] = aa.Coeffs[ii] / bb.Coeffs[ii];
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
            var aa = a.Clone() as Polynomial;
            var bb = b.Clone() as Polynomial;

            if (aa.Coeffs.Length != bb.Coeffs.Length)
                mkSameLength(ref aa, ref bb);

            int n = aa.Coeffs.Length;
            double[] res = new double[aa.Coeffs.Length];


            for (int ii = 0; ii < n; ii++)
            {
                res[ii] = aa.Coeffs[ii] * bb.Coeffs[ii];
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
            var aa = a.Clone() as Polynomial;
            var bb = b.Clone() as Polynomial;

            if (aa.Degree != bb.Degree)
                mkSameLength(ref aa, ref bb);

            int n = aa.Degree;
            double[] res = new double[n];


            for (int ii = 0; ii < n; ii++)
            {
                res[ii] = aa.Coeffs[ii] + bb.Coeffs[ii];
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
            var aa = a.Clone() as Polynomial;
            var bb = b.Clone() as Polynomial;

            if (aa.Degree != bb.Degree)
                mkSameLength(ref aa, ref bb);

            int n = aa.Degree;
            double[] res = new double[n];


            for (int ii = 0; ii < n; ii++)
            {
                res[ii] = aa.Coeffs[ii] - bb.Coeffs[ii];
            }
            Polynomial res_poly = new Polynomial(res);
            return (res_poly);
        }

        /// <summary>
        /// Division of two polynomials returning the quotient-with-remainder of the two polynomials given
        /// </summary>
        /// <param name="a">left polynomial</param>
        /// <param name="b">right polynomial</param>
        /// <returns>a tuple holding quotient in first and remainder in second</returns>
        public static Tuple<Polynomial, Polynomial> DivideLong(Polynomial a, Polynomial b)
        {
            if (a == null)
                throw new ArgumentNullException("a");
            if (b == null)
                throw new ArgumentNullException("b");

            if (a.Degree <= 0)
                throw new ArgumentOutOfRangeException("a Degree must be greater than zero");
            if (b.Degree <= 0)
                throw new ArgumentOutOfRangeException("b Degree must be greater than zero");

            if (b.Coeffs[b.Degree-1] == 0)
                throw new DivideByZeroException("b polynomial ends with zero");
            
            var c1 = a.Coeffs.ToArray();
            var c2 = b.Coeffs.ToArray();

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
                    c22[ii] = c2[ii] / scl;

                int i = dn;
                int j = n1 - 1;
                while (i >= 0)
                {
                    var v = c1[j];
                    var vals = new double[j - i];
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
                    quo[k] = c1[k + j1] / scl;

                for (int k = 0; k < j1; k++)
                    rem[k] = c1[k];
                
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
        /// <param name="b">right polynomial</param>
        /// <returns>a tuple holding quotient in first and remainder in second</returns>
        public Tuple<Polynomial, Polynomial> DivideLong(Polynomial b)
        {
            return DivideLong(this, b);
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

                    if (ii == 0 && Coeffs.Length == 1)
                        strLoc += String.Format("{0}", this.Coeffs[ii], VarName, ii);
                    else if(ii == 0)
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
            return (Coeffs.ToArray());
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
            return new Polynomial(this.Coeffs);
        }
        #endregion

    }

}
