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
            var cholNorm = (XtX - cholL * cholL.ConjugateTranspose()).L2Norm();   // 0.0 so A is now LL'
            var boolTest = (XtX.Equals(cholL * cholL.ConjugateTranspose()));      // A is LL' elementwise as well

        }
    }
}
