using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Issue552
{
    class Program
    {

        static void log(string what, int i, int j)
        {
            Console.Write("  " + what + "[" + i + "," + j + "]");
        }

        // Re-arranging the Cholesky factor code as suggested by diluculo
        public static void CholeskyFactor_Rearranged(double[] a, int order)
        {
            if (a == null)
            {
                throw new ArgumentNullException("a");
            }

            double[] t = new double[order]; // tmp space

            for (int ij = 0; ij < order; ij++)
            {
                t[ij] = 0.0;
                if (ij > 0)
                {
                    for (int k = 0; k < ij; k++)
                    {
                        t[ij] += a[k * order + ij] * a[k * order + ij];
                    }
                }

                a[ij * order + ij] = Math.Sqrt(a[ij * order + ij] - t[ij]);
                for (int i = ij + 1; i < order; i++)
                {
                    t[i] = 0.0;
                    for (int k = 0; k < ij; k++)
                    {
                        t[i] += a[k * order + i] * a[k * order + ij];
                    }

                    a[ij * order + i] = (a[ij * order + i] - t[i]) / a[ij * order + ij];
                    a[i * order + ij] = 0.0;
                }
            }

        }


        static void Main(string[] args)
        {
            // independent variable x
            var x = Enumerable.Range(0, 21).Select(Convert.ToDouble).ToArray();
            // polynomial order
            int order = 5;
            // number of points
            int Ns = x.Length; // 21
                               // degree of freedom
            int DF = Ns - (order + 1); // 21 - 6 = 15

            // design Matrix X
            Matrix<double> X = Matrix<double>.Build.Dense(Ns, order + 1, (i, j) => Math.Pow(x[i], j));
            // Transposed matrix of X
            Matrix<double> Xt = X.Transpose();
            // XtX = X'X is a symmetric square matrix
            Matrix<double> XtX = Xt * X;

            // Let's do Cholesky factorization
            var chol = XtX.Cholesky();
            var cholL = chol.Factor;
            var cholNorm = (XtX - cholL * cholL.ConjugateTranspose()).L2Norm(); // 0.00390625
                                                                                // A is not LL'. Something wrong !!!

            var tmp_rrg = XtX.ToColumnMajorArray();
            CholeskyFactor_Rearranged(tmp_rrg, XtX.RowCount);
            var cholL_rrg = Matrix<double>.Build.Dense(XtX.RowCount, XtX.ColumnCount, tmp_rrg);
            var cholNorm_rrg = (XtX - cholL_rrg * cholL_rrg.ConjugateTranspose()).L2Norm(); // rearranged, it is 0


            // Let's see how DoCholesky method works? 
            // (1) 1st column
            var l11 = Math.Sqrt(XtX[0, 0]);
            var l21 = XtX[1, 0] / l11;
            var l31 = XtX[2, 0] / l11; // and so on 
                                       // (1a) doCholeskyStep
            var l22 = XtX[1, 1] - l21 * l21;
            var l32 = XtX[2, 1] - l21 * l31;
            var l33 = XtX[2, 2] - l31 * l31; // and so on
                                             // (2) 2nd column
            l22 = Math.Sqrt(l22);
            l32 = l32 / l22; // and so on
                             // (2a) doCholeskyStep
            l33 = l33 - l32 * l32; // and so on

            // so far all calculated values are exactly same to my reference values.

            // (3) 3rd column
            l33 = Math.Sqrt(l33); // 149.77538738613421,
                                  // but l33 should be     149.775387386134.
            var l33_rrg = cholL_rrg[2, 2];  // rearranged is 149.775387386134
                                  //                                       ^
                                  // This small errors propagates through next steps.
            var l66 = cholL[5, 5]; // 21011.779008808317
                                   // it should be           21011.779009604055
                                   //                                   ^
            var l66_rrg = cholL_rrg[5, 5]; // rearranged in 21011.779009325193
                                   //                                   ^  

            // I think the error is related with the fact the followings give different values!!!
            var l33a = Math.Sqrt(XtX[2, 2] - l31 * l31 - l32 * l32); // 149.77538738613421
            var l33b = Math.Sqrt(XtX[2, 2] - (l31 * l31 + l32 * l32)); // 149.775387386134        }
        }
    }
}
