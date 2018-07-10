using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public int Length
        {
            get
            {
                return (Coeffs.Length);
            }
        }

        /// <summary>
        /// constructor setting a Polynomial of size n containing only zeros
        /// </summary>
        /// <param name="n">size of Polynomial</param>
        public Polynomial(int n)
        {
            Coeffs = new double[n];
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
        /// WARNING cut all trailing zeros before, since they would result in zeros at the end
        /// </summary>
        /// <param name="coeffs"> Polynomial coefficiens as array</param>
        /// <param name="isFlip">use true for flipping</param>
        public Polynomial(double[] coeffs, bool isFlip = false)
        {
            this.Coeffs = new double[Coeffs.Length];
            Array.Copy(coeffs, Coeffs, coeffs.Length);

            if (isFlip)
            {
                Coeffs = Coeffs.Reverse().ToArray();
                IsFlipped = true;
            }          
        }

        /// <summary>
        /// constructor setting Polynomial coefficiens
        /// </summary>
        /// <param name="coeff">just the x^0 part</param>
        public Polynomial(double coeff)
        {
            IsFlipped = false;
            this.Coeffs = new double[1];
            Coeffs[0] = coeff;
        }

        /// <summary>
        /// constructor setting Polynomial coefficiens
        /// </summary>
        /// <param name="Coeffs"> Polynomial coefficiens as array</param>
        public Polynomial(double[] Coeffs)
        {
            this.Coeffs = new double[Coeffs.Length];
            Array.Copy(Coeffs, this.Coeffs, Coeffs.Length);
        }

        /// <summary>
        /// remove all trailing zeros, e.G before: "0.00 x^2 + 1.0 x^1 + 1.00" after: "1.0 x^1 + 1.00"
        /// </summary>
        public void CutTrailZeros()
        {
            int count = 0;
            for (int ii = Length - 1; ii >= 0; ii--)
            {
                if (Coeffs[ii] == 0.0)
                {
                    count++;
                }
                else
                {
                    double[] CoeffsHold = new double[Length];
                    Coeffs.CopyTo(CoeffsHold, 0);
                    Array.Resize(ref CoeffsHold, Length - count);
                    Coeffs = new double[Length - count];
                    CoeffsHold.CopyTo(Coeffs, 0);
                    return;
                }
            }
        }

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
            for (int ii = 0; ii < a.Length; ii++)
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
            for (int ii = 0; ii < a.Length; ii++)
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
            return add(a, b);
        }

        /// <summary>
        /// substraction of two Polynomials (piecewise)
        /// </summary>
        /// <param name="a">left Polynomial</param>
        /// <param name="b">right Polynomial</param>
        /// <returns>resulting Polynomial</returns>
        public static Polynomial operator -( Polynomial a, Polynomial b)
        {
            return substract(a, b);
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

            int n = pLoc.Length - 1;
            if (n < 2)
                return null;

            double[] p = new double[n];

            double a0 = pLoc.Coeffs[p.Length];

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
            if (a.Length != b.Length)
                mkSameLength(ref a, ref b);

            int n = a.Length;
            double[] res = new double[a.Length];


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
            if (a.Length != b.Length)
                mkSameLength(ref a, ref b);

            int n = a.Length;
            double[] res = new double[a.Length];


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
        public static Polynomial add( Polynomial a, Polynomial b)
        {

            if (a.Length != b.Length)
                mkSameLength(ref a, ref b);

            int n = a.Length;
            double[] res = new double[a.Length];


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
        public static Polynomial substract( Polynomial a, Polynomial b)
        {

            if (a.Length != b.Length)
                mkSameLength(ref a, ref b);

            int n = a.Length;
            double[] res = new double[a.Length];


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
            string strLoc = "";

            for (int ii = Length - 1; ii >= 0; ii--)
            {

                if (ii == 0)
                    strLoc = String.Concat(strLoc, this.Coeffs[ii].ToString());
                else
                    strLoc = String.Concat(strLoc, this.Coeffs[ii].ToString(), VarName, ii.ToString(), " + ");
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
            double[] aHold = new double[a.Length];
            double[] bHold = new double[b.Length];
            Array.Copy(a.Coeffs, aHold, a.Length);
            Array.Copy(b.Coeffs, bHold, b.Length);

            if (a.Length < b.Length)
            {
                a.Coeffs = new double[b.Length];
                b.Coeffs = new double[b.Length];
                Array.Copy(aHold, a.Coeffs, aHold.Length);
                Array.Copy(bHold, b.Coeffs, bHold.Length);
            }
            else
            {
                a.Coeffs = new double[a.Length];
                b.Coeffs = new double[a.Length];
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
        #endregion

    }

}
