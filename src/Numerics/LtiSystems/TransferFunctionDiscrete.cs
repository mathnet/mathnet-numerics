using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using MathNet.Numerics;

namespace MathNet.Numerics.LtiSystems
{
    /// <summary> Class for LTI discrete transfer functions </summary>
    public class TransferFunctionDiscrete
    {

        private double[] _num;

        private double[] _den;

        /// <summary>  
        /// numberator (input dependent)  Polynomial coefficients as array 
        /// in order 
        /// => index high ... index low 
        /// => [n], [n-1], +..., [0] 
        /// => 1 + q^-1 + ... + q^-n 
        /// </summary>
        public double[] num
        {
            get
            {
                return _num;
            }
            set
            {
                _num = cutTrailingZeros(value);
                shiftNumDenIfPossible();
            }
        }

        /// <summary> den (state dependent)  Polynomial coefficients as array         
        /// in order 
        /// => index high ... index low 
        /// => [n], [n-1], +..., [0] 
        /// => 1 + q^-1 + ... + q^-n 
        /// </summary>
        public double[] den
        {
            get
            {
                return _den;
            }
            set
            {
                _den = cutTrailingZeros(value);
                shiftNumDenIfPossible();
            }
        }



        /// <summary>  b (input dependent)  Polynomial coefficients as array ( </summary>
        public double[] b
        {
            get
            {
                return _num;
            }
            set
            {
                _num = cutTrailingZeros(value);
                shiftNumDenIfPossible();
            }
        }

        /// <summary> a (state dependent)  Polynomial coefficients as array </summary> 
        public double[] a
        {
            get
            {
                return _den;
            }
            set
            {
                _den = cutTrailingZeros(value);
                shiftNumDenIfPossible();
            }
        }

        /// <summary> Internal FIR States -> updated in every response calculation </summary>
        public double[] z_FIR { get; set; }

        /// <summary> Internal IIR States -> updated in every response calculation </summary>
        public double[] z_IIR { get; set; }

        /// <summary> any name you want to give this transfer function </summary>
        public string Name { get; set; }

        /// <summary> sampling time of discrete transfer function (default = 1) </summary>
        public double Ts { get; set; }

        /// <summary> variable for transfer function so far all tf's are in the q^-1 (or equivalently z^-1) form. Changing this will have NO nfluence besides displaying te TF</summary>
        public string variable = "q^-1";



        /// <summary>
        /// Check if this Transfer Function is stable
        /// </summary>
        /// <param name="numTolerance">the tolerance for euclidian distance at which a pole/zero pair is considered to be canceling each other</param>
        /// <returns>false if system is unsable true if system is stable</returns>
        public bool IsStable(double numTolerance = 1e-8)
        {

            var p = GetPoles();

            var z = GetZeros();
            var z_isCompensated = new bool[z.Length];
            for (int j = 0; j < p.Length; j++)
            {
                // check if pole would lead to unstable behaviour
                if (p[j].Magnitude > 1.0)
                {

                    // init some values
                    double minDistance = Double.PositiveInfinity;
                    int idxMinDistanceZero = -1;

                    // analyze the distance between each zero and the pole now
                    for (int i = 0; i < z.Length; i++)
                    {
                        // check if pole has already been used for compensation
                        if (z_isCompensated[i])
                            continue;

                        // calculate geometrical distance between each zero and the pole now and store the closest neighbour
                        var dist = (p[j] - z[i]).Magnitude;
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            idxMinDistanceZero = i;
                        }
                    }

                    // if closest neighbour is too far away to compensate the unstable pole 
                    if (minDistance >= numTolerance)
                        return false;   // the system is unsable
                    else
                        z_isCompensated[idxMinDistanceZero] = true; // if not: mark the zero as already used for compensation

                }

            }

            return true;
            
        }



        /// <summary> constructor setting no properties at all </summary>
        public TransferFunctionDiscrete()
        {
            this.Ts = 1.0;
        }

        /// <summary>
        /// constructor setting a and b vectors as well as initializing the z_FIR and z_IIR states
        /// </summary>
        public TransferFunctionDiscrete(double b_in, double[] a_in, double Ts_in = 1.0d)
        {
            if (a_in == null)
                throw new ArgumentNullException("a_in");

            this.a = (double[])a_in.Clone();
            this.b = new double[1];
            this.b[0] = b_in;
            this.z_IIR = new double[a_in.Length];
            this.z_FIR = new double[1];
            this.Ts = Ts_in;
        }

        /// <summary>
        /// constructor setting a and b vectors as well as initializing the z_FIR and z_IIR states
        /// </summary>
        public TransferFunctionDiscrete(double[] b_in, double a_in, double Ts_in = 1.0d)
        {
            if (b_in == null)
                throw new ArgumentNullException("b_in");

            this.a = new double[1];
            this.a[0] = a_in;
            this.b = (double[])b_in.Clone();
            this.z_IIR = new double[1];
            this.z_FIR = new double[b_in.Length];
            this.Ts = Ts_in;
        }

        /// <summary>
        /// constructor setting a and b vectors as well as initializing the z_FIR and z_IIR states
        /// </summary>
        public TransferFunctionDiscrete(double b_in, double a_in, double Ts_in = 1.0d)
        {
            this.a = new double[1];
            this.a[0] = a_in;
            this.b = new double[1];
            this.b[0] = b_in;
            this.z_IIR = new double[1];
            this.z_FIR = new double[1];
            this.Ts = Ts_in;
        }

        /// <summary>
        /// constructor setting a and b vectors as well as initializing the z_FIR and z_IIR states
        /// </summary>
        public TransferFunctionDiscrete(double[] b_in, double[] a_in, double Ts_in = 1.0d)
        {
            if (b_in == null)
                throw new ArgumentNullException("b_in");

            if (a_in == null)
                throw new ArgumentNullException("a_in");

            this.a = (double[])a_in.Clone();
            this.b = (double[])b_in.Clone();
            this.z_IIR = new double[a_in.Length];
            this.z_FIR = new double[b_in.Length];
            this.Ts = Ts_in;
        }


        /// <summary>
        /// Adds delay to the numerator array (shifting the values by d steps)
        /// </summary>
        /// <param name="d">integer value of daly to add to this TF</param>
        public void AddDelay(int d)
        {
            double[] b_new = new double[b.Length + d];
            b.CopyTo(b_new, d);
            b = b_new;
        }

        #region Helpers
        /// <summary>
        /// if num and den both start at a later step (e.G highest power num = q^-3 and highest power den = q^-4), the whole tf can be shifted by n (3) steps
        /// </summary>
        private void shiftNumDenIfPossible()
        {
            var offset = 0;
            if (_num == null || _den == null)
                return;

            var n = Math.Min(_num.Length, _den.Length);
            for (int i = 0; i < n; i++)
            {
                if (num[i] == 0.0d && _den[i] == 0.0d)
                    offset = i + 1;
                else
                    break;
            }

            if (offset > 0)
            {
                double[] tmp1 = new double[_num.Length - offset];
                Array.Copy(_num, offset, tmp1, 0, tmp1.Length);
                double[] tmp2 = new double[_den.Length - offset];
                Array.Copy(_den, offset, tmp2, 0, tmp2.Length);
                _num = tmp1;
                _den = tmp2;
            }
        }

        private double[] cutTrailingZeros(double[] vIn)
        {
            int lengthNew = vIn.Length;

            for (int i = vIn.Length - 1; i >= 0; i--)
            {
                if (vIn[i] != 0)
                {
                    lengthNew = i + 1;
                    break;
                }
            }

            var v = new double[lengthNew];
            Array.Copy(vIn, v, lengthNew);

            return v;
        }

        /// <summary>
        /// checks and adjusts internal states to a and b arrays
        /// </summary>
        private void checkStateSizes()
        {
            if (this.z_IIR == null)
                this.z_IIR = new double[this.a.Length];

            if (this.z_FIR == null)
                this.z_FIR = new double[this.b.Length];

            if (this.a.Length != this.z_IIR.Length)
                this.z_IIR = new double[this.a.Length];

            if (this.b.Length != this.z_FIR.Length)
                this.z_FIR = new double[this.b.Length];

            if (this.a.Length != this.z_IIR.Length)
                this.z_IIR = new double[this.a.Length];
        }
        #endregion Helpers

        #region Operators

        /// <summary>
        /// LTI System theory division of a transfer function object by a scalar
        /// </summary>
        /// <param name="G1">transfer function</param>
        /// <param name="k">scalar for divison</param>
        /// <returns>new transfer function object divided by k</returns>
        public static TransferFunctionDiscrete operator /(TransferFunctionDiscrete G1, double k)
        {
             Polynomial A1 = new  Polynomial(G1.a);

            TransferFunctionDiscrete Gres = new TransferFunctionDiscrete(G1.b, (A1 * k).ToArray(), G1.Ts)
            {
                Name = G1.Name
            };
            return Gres;
        }

        /// <summary>
        /// LTI System theory division of a scalar by a transfer function object
        /// </summary>
        /// <param name="k">scalar value</param>
        /// <param name="G1">transfer function for division</param>
        /// <returns>new transfer function object</returns>
        public static TransferFunctionDiscrete operator /(double k, TransferFunctionDiscrete G1)
        {
             Polynomial A1 = new  Polynomial(G1.a);

            TransferFunctionDiscrete Gres = new TransferFunctionDiscrete((A1 * k).ToArray(), G1.b, G1.Ts)
            {
                Name = G1.Name
            };
            return Gres;
        }

        /// <summary>
        /// LTI System theory multiplication of a transfer function object by a scalar
        /// </summary>
        /// <param name="G1">transfer function</param>
        /// <param name="k">scalar for multiplication</param>
        /// <returns>new transfer function object</returns>
        public static TransferFunctionDiscrete operator *(TransferFunctionDiscrete G1, double k)
        {
             Polynomial B1 = new  Polynomial(G1.b);

            TransferFunctionDiscrete Gres = new TransferFunctionDiscrete((B1 * k).ToArray(), G1.a, G1.Ts)
            {
                Name = G1.Name
            };
            return Gres;

        }

        /// <summary>
        /// LTI System theory multiplication of a transfer function object by a scalar
        /// </summary>
        /// <param name="k">scalar for multiplication</param>
        /// <param name="G1">transfer function</param>
        /// <returns>new transfer function object</returns>
        public static TransferFunctionDiscrete operator *(double k, TransferFunctionDiscrete G1)
        {
             Polynomial B1 = new  Polynomial(G1.b);

            TransferFunctionDiscrete Gres = new TransferFunctionDiscrete((B1 * k).ToArray(), G1.a, G1.Ts)
            {
                Name = G1.Name
            };
            return Gres;
        }


        /// <summary>
        /// LTI System theory substraction of a transfer function with a scalar
        /// </summary>
        /// <param name="G1">transfer function</param>
        /// <param name="k">scalar</param>
        /// <returns>new transfer function object</returns>
        public static TransferFunctionDiscrete operator -(TransferFunctionDiscrete G1, double k)
        {
             Polynomial A1 = new  Polynomial(G1.a);
             Polynomial B1 = new  Polynomial(G1.b);

             Polynomial A_res = (A1);
             Polynomial B_res = B1 - (A1 * k);

            TransferFunctionDiscrete Gres = new TransferFunctionDiscrete(B_res.ToArray(), A_res.ToArray(), G1.Ts)
            {
                Name = G1.Name
            };
            return Gres;
        }

        /// <summary>
        /// LTI System theory substraction of a scalar by a transfer function
        /// </summary>
        /// <param name="k">scalar</param>
        /// <param name="G1">transfer function</param>
        /// <returns>new transfer function object</returns>
        public static TransferFunctionDiscrete operator -(double k, TransferFunctionDiscrete G1)
        {
             Polynomial A1 = new  Polynomial(G1.a);
             Polynomial B1 = new  Polynomial(G1.b);

             Polynomial A_res = (A1);
             Polynomial B_res = (A1 * k) - B1;

            TransferFunctionDiscrete Gres = new TransferFunctionDiscrete(B_res.ToArray(), A_res.ToArray(), G1.Ts)
            {
                Name = G1.Name
            };
            return Gres;
        }

        /// <summary>
        /// LTI System theory addition of a transfer function with a scalar
        /// </summary>
        /// <param name="G1">transfer function</param>
        /// <param name="k">scalar</param>
        /// <returns>new transfer function object</returns>
        public static TransferFunctionDiscrete operator +(TransferFunctionDiscrete G1, double k)
        {
             Polynomial A1 = new  Polynomial(G1.a);
             Polynomial B1 = new  Polynomial(G1.b);

             Polynomial A_res = (A1);
             Polynomial B_res = (A1 * k) + B1;

            TransferFunctionDiscrete Gres = new TransferFunctionDiscrete(B_res.ToArray(), A_res.ToArray(), G1.Ts)
            {
                Name = G1.Name
            };
            return Gres;
        }

        /// <summary>
        /// LTI System theory addition of a transfer function with a scalar
        /// </summary>
        /// <param name="k">transfer function</param>
        /// <param name="G1">scalar</param>
        /// <returns>new transfer function object</returns>
        public static TransferFunctionDiscrete operator +(double k, TransferFunctionDiscrete G1)
        {
             Polynomial A1 = new  Polynomial(G1.a);
             Polynomial B1 = new  Polynomial(G1.b);

             Polynomial A_res = (A1);
             Polynomial B_res = B1 + (A1 * k);

            TransferFunctionDiscrete Gres = new TransferFunctionDiscrete(B_res.ToArray(), A_res.ToArray(), G1.Ts)
            {
                Name = G1.Name
            };
            return Gres;
        }

        /// <summary>
        /// LTI System theory addition of two transfer functions
        /// </summary>
        /// <param name="G1">transfer function left</param>
        /// <param name="G2">transfer function right</param>
        /// <returns>new transfer function object</returns>
        public static TransferFunctionDiscrete operator +(TransferFunctionDiscrete G1, TransferFunctionDiscrete G2)
        {
            if (Math.Abs(G1.Ts - G2.Ts) > 1e-12)
                throw new ArgumentException(String.Format("The two supplied transfer functions do not have equal sampling times. G1.Ts = {0} G2.Ts = {1}", G1.Ts, G2.Ts));

             Polynomial A1 = new  Polynomial(G1.a);
             Polynomial B1 = new  Polynomial(G1.b);

             Polynomial A2 = new  Polynomial(G2.a);
             Polynomial B2 = new  Polynomial(G2.b);

             Polynomial A_res = (A1 * A2);
             Polynomial B_res = (B1 * A2) + (B2 * A1);

            return new TransferFunctionDiscrete(B_res.ToArray(), A_res.ToArray(), G1.Ts);
        }

        /// <summary>
        /// LTI System theory substraction of two transfer functions
        /// </summary>
        /// <param name="G1">transfer function left</param>
        /// <param name="G2">transfer function right</param>
        /// <returns>new transfer function object</returns>
        public static TransferFunctionDiscrete operator -(TransferFunctionDiscrete G1, TransferFunctionDiscrete G2)
        {
            if (Math.Abs(G1.Ts - G2.Ts) > 1e-12)
                throw new ArgumentException(String.Format("The two supplied transfer functions do not have equal sampling times. G1.Ts = {0} G2.Ts = {1}", G1.Ts, G2.Ts));

             Polynomial A1 = new  Polynomial(G1.a);
             Polynomial B1 = new  Polynomial(G1.b);

             Polynomial A2 = new  Polynomial(G2.a);
             Polynomial B2 = new  Polynomial(G2.b);

             Polynomial A_res = (A1 * A2);
             Polynomial B_res = (B1 * A2) - (B2 * A1);

            return new TransferFunctionDiscrete(B_res.ToArray(), A_res.ToArray(), G1.Ts);
        }

        /// <summary>
        /// LTI System theory addition of two transfer functions
        /// </summary>
        /// <param name="G1">transfer function left</param>
        /// <param name="G2">transfer function right</param>
        /// <returns>new transfer function object</returns>
        public static TransferFunctionDiscrete operator *(TransferFunctionDiscrete G1, TransferFunctionDiscrete G2)
        {
            if (Math.Abs(G1.Ts - G2.Ts) > 1e-12)
                throw new ArgumentException(String.Format("The two supplied transfer functions do not have equal sampling times. G1.Ts = {0} G2.Ts = {1}", G1.Ts, G2.Ts));

             Polynomial A1 = new  Polynomial(G1.a);
             Polynomial B1 = new  Polynomial(G1.b);

             Polynomial A2 = new  Polynomial(G2.a);
             Polynomial B2 = new  Polynomial(G2.b);

             Polynomial A_res = A1 * A2;
             Polynomial B_res = B1 * B2;

            return new TransferFunctionDiscrete(B_res.ToArray(), A_res.ToArray(), G1.Ts);
        }

        /// <summary>
        /// LTI System theory division of two transfer functions
        /// </summary>
        /// <param name="G1">transfer function left</param>
        /// <param name="G2">transfer function right</param>
        /// <returns>new transfer function object</returns>
        public static TransferFunctionDiscrete operator /(TransferFunctionDiscrete G1, TransferFunctionDiscrete G2)
        {
            if (Math.Abs(G1.Ts - G2.Ts) > 1e-12)
                throw new ArgumentException(String.Format("The two supplied transfer functions do not have equal sampling times. G1.Ts = {0} G2.Ts = {1}", G1.Ts, G2.Ts));

             Polynomial A1 = new  Polynomial(G1.a);
             Polynomial B1 = new  Polynomial(G1.b);

             Polynomial A2 = new  Polynomial(G2.a);
             Polynomial B2 = new  Polynomial(G2.b);

             Polynomial A_res = A1 * B2;
             Polynomial B_res = B1 * A2;

            return new TransferFunctionDiscrete(B_res.ToArray(), A_res.ToArray(), G1.Ts);
        }
        #endregion

        /// <summary> calculates y_k = G(q^-1) * x_k for a given x_k array </summary>
        public IEnumerable<double> CalcResponse(IEnumerable<double> x)
        {
            return this.CalcResponse(x.ToArray());
        }

        /// <summary> calculates y_k = G(q^-1) * x_k for a given x_k array </summary>
        public double[] CalcResponse(double[] x)
        {
            // this is basically a two step convolution and could be replaced by a
            // conv implementation.
            // however... this code works fine and replacing it would be more work

            double y_now = 0.0d;
            int idx_a = 0;
            int idx_b = 0;
            double[] y = new double[x.Length];

            this.checkStateSizes();

            // Loop all inputs
            for (int ii_x = 0; ii_x < x.Length; ii_x++)
            {
                y_now = 0.0d;
                idx_b = 0;

                // loop through b-matrix until end of momentary tempx-array
                for (int ii_b = 0; ii_b <= ii_x && idx_b < b.Length; ii_b++)
                {

                    z_FIR[idx_b] = x[ii_x - ii_b];
                    y_now += b[idx_b] * z_FIR[idx_b];
                    idx_b++;
                }


                // start at second position, since it's the a-matrix
                idx_a = 1;
                // loop for a-matrix
                for (int ii_a = 0; ii_a <= (ii_x - 1) && idx_a < a.Length; ii_a++)
                {
                    z_IIR[idx_a] = y[(ii_x - 1) - ii_a];
                    y_now -= a[idx_a] * z_IIR[idx_a];
                    idx_a++;
                }
                // write result
                y[ii_x] = (y_now / a[0]);
                z_IIR[0] = y[ii_x];
            }
            return (y);
        }

        // Todo: Implement FiltFilt
        /*
        /// <summary>
        /// A wrapper for the StaticFilters.FiltFilt method using the internal a and b arrays
        /// </summary>
        /// <param name="data">The data to filter</param>
        /// <param name="zi">initial state coefficients null for aotomatic generation via steady state solution</param>
        /// <param name="padlen">the number of datapoints to pad at each side use less than 0 for Math.Max(a.Length, b.Length) * 3</param>
        /// <returns>The filterd data</returns>
        /// <remarks>
        /// In order to prevent transients at the end or start of the sequence we have to pad it
        /// The padding is done by rotating the sequence by 180° at the ends and append it to the data
        /// </remarks>
        public double[] FiltFilt(double[] data, double[] zi = null, int padlen = 0)
        {
            if (this.a == null || this.a.Length == 0)
                throw new Exception("This transfer function has no a array with data");
            if (this.b == null || this.b.Length == 0)
                throw new Exception("This transfer function has no a array with data");

            return StaticFilters.FiltFilt(data, this.a, this.b, zi, padlen);
        }
        */

        #region Dynamics

        /// <summary>
        /// returns the impulse response with nSteps for the tf model 
        /// </summary>
        /// <param name="nSteps">number of steps for impulse response</param>
        /// <returns></returns>
        public double[] Impulse(int nSteps)
        {

            var Inp = new double[nSteps];
            Inp[0] = 1.0;

            var ImpulseResponse = this.CalcResponse(Inp);
            return (ImpulseResponse);
        }

        /// <summary>
        /// returns the impulse response with nSettling * 1.3 steps for the tf model 
        /// </summary>
        public double[] Impulse()
        {

            var nSteps = Convert.ToInt32((double)CalcSettlingSteps() * 1.3);
            if (nSteps <= 0)
                return null;
            var Inp = new double[nSteps];
            Inp[0] = 1.0;

            var ImpulseResponse = this.CalcResponse(Inp);
            return (ImpulseResponse);
        }


        public Complex[] Bode(int nPoints = 100)
        {
            // substituting z = exp(j * omega * Ts)
            var omega_vec = Generate.LinearSpaced(nPoints, 0, 2 * Math.PI * 1 / Ts);

            return Bode(omega_vec);
        }

        public Complex[] Bode(int nPoints, out double[] omega_vec)
        {
            // substituting z = exp(j * omega * Ts)
            omega_vec = Generate.LinearSpaced(nPoints, 0, 2 * Math.PI * 1 / Ts);


            return Bode(omega_vec);
        }

        public Complex[] Bode(double[] omega_vec)
        {

            var nPoints = omega_vec.Length;

            double omega;
            double expVal;
            Complex zVal;
            Complex denVal;
            Complex numVal;

            var bodeVal = new Complex[nPoints];

            for (int idx = 0; idx < nPoints; idx++)
            {


                omega = omega_vec[idx];

                zVal = new Complex(0.0, 0.0);

                denVal = new Complex(0.0, 0.0);
                for (int ii = 0; ii < a.Length; ii++)
                {
                    expVal = ii * omega * Ts;
                    zVal = new Complex(0.0, expVal);

                    denVal += a[ii] * zVal.Exp();
                }

                numVal = new Complex(0.0, 0.0);
                for (int ii = 0; ii < b.Length; ii++)
                {
                    expVal = ii * omega * Ts;
                    zVal = new Complex(0.0, expVal);

                    numVal += b[ii] * zVal.Exp();
                }
                bodeVal[idx] = numVal / denVal;
            }

            return bodeVal;
        }

        /// <summary> The poles resulting from the denominator  Polynomial root</summary>
        public Complex[] GetPoles()
        {
             Polynomial a_poly = new  Polynomial(a, isFlip:true);
            Complex[] r = a_poly.GetRoots();
            return r;
        }

        /// <summary> The zeros resulting from the nominator  Polynomial root </summary>
        public Complex[] GetZeros()
        {
             Polynomial b_poly = new  Polynomial(b, isFlip:true);
            Complex[] r = b_poly.GetRoots();
            return r;
        }


        /// <summary>
        /// calculate the number of steps the system will need until it can be assumed to be settled
        /// </summary>
        /// <param name="tol">tolerance in decimal percent at which to assume that the system is settled (default = 0.3)</param>
        /// <param name="n_max">maximum number of steps to simulate (default = 500000)</param>
        /// <returns>number of steps at which the system is assumed to be settled, or 0 if unstable</returns>
        public int CalcSettlingSteps(double tol = 0.03, int n_max = 500000)
        {

            // init settling time as zero for never settled
            int n_sttl = 0;

            // if the system is unstable return zero since the system will never be settled
            if (this.IsStable() == false)
                return 0;

            int n_sim = 0;

            double[] dampVals = GetDampings(out double[] EigenFrequencys);

            double dampWorst = dampVals.Min();

            //for (int ii = 1; ii < dampVals.Length; ii++)
            //    dampWorst = dampWorst * dampVals[ii];

            double t_simFull;

            // appromate a settling time based on damping
            var t_stlDamp = -Math.Log(tol) / dampWorst;

            // approximate a settling time from time constants
            var tau = new double[dampVals.Length];
            for (int ii = 0; ii < tau.Length; ii++)
                tau[ii] = 1.0 / (dampVals[ii] * EigenFrequencys[ii]);

            // approx after 5 * biggest time constant 
            var t_stlTimeConst = tau.Max() * 5;

            // choose bigger approximation
            t_simFull = Math.Max(t_stlTimeConst, t_stlDamp);

            // recalculate to number of steps
            int nStepsBase = (int)Math.Ceiling(t_simFull / Ts);


            // simulate impulse responses with n*10*nStepsBase time steps
            // incrementing n if necessary until steady state is reached
            n_sim = nStepsBase <= 0 ? 5 : nStepsBase;
            int count = 0;
            while (count < 10 && n_sttl == 0)
            {
                if (n_sim > n_max)
                    return n_sttl;

                n_sim = 10 * n_sim;

                double[] dirac_sim = new double[n_sim];
                dirac_sim[0] = 1.0;
                var tmp_outp = this.CalcResponse(dirac_sim);

                int idxPos = n_sim - 1;

                // find first step beeing bigger than tolerance
                while (idxPos > 0 && n_sttl == 0)
                {
                    if (tmp_outp[idxPos] > tol)
                        n_sttl = idxPos;

                    idxPos--;
                }
                count++;
            }
            return n_sttl;

        }

        #endregion Dynamics


        #region Dampings
        /// <summary>
        /// gets the damping coefficients from this transfer function, 
        /// since all transfer functions so far are discrete time, 
        /// these values do not directly translate to lambda. 
        /// the theoretical recalculation is:
        ///     Z = -cos(angle(log(lambda)))
        /// </summary>
        /// <returns>Array of damping values for this transfer function</returns>
        public double[] GetDampings()
        {
            return GetDampings(out double[] f);
        }

        /// <summary>
        /// gets the damping coefficients from this transfer function, 
        /// since all transfer functions so far are discrete time, 
        /// these values do not directly translate to lambda. 
        /// the theoretical recalculation is:
        ///     Z = -cos(angle(log(lambda)))
        /// </summary>
        /// <returns>Array of damping values for this transfer function</returns>
        public double[] GetDampings(out double[] wn)
        {

            var r = GetPoles().Clone() as Complex[];
            var s = new Complex[r.Length];
            var f = new double[r.Length];
            var z = new double[r.Length];

            for (int idx = 0; idx < r.Length; idx++)
            {
                s[idx] = Complex.Log(r[idx]) / Ts;
                f[idx] = s[idx].Magnitude;
                z[idx] = -s[idx].Real / f[idx];
            }

            wn = (double[])f.Clone();

            return z;

        }


        #endregion Dampings
        

        #region displaying
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string DispTF()
        {
            return (DispTF(this.b, this.a, this.Name, this.variable.Substring(0, variable.Length - 1)));
        }

        public string NumString()
        {
            var varStr = this.variable.Substring(0, variable.Length - 1);
            var num = b.Clone() as double[];
            return getFractString(num, varStr);
        }

        public string DenString()
        {
            var varStr = this.variable.Substring(0, variable.Length - 1);
            var den = a.Clone() as double[];
            return getFractString(den, varStr);
        }


        private static string getFractString(double[] num, string varStr)
        {
            string str1;
            string str2;
            string strNum = "";
            for (int item = 0; item < num.Length; item++)
            {
                if (num[item] == 0)
                    continue;

                //str2 = Math.Abs(num[item]).ToString();
                str2 = Math.Abs(num[item]).ToString("0.######");
                if (item == 0)
                {
                    if (num[item] < 0)
                        str1 = "-";
                    else
                        str1 = "";

                    strNum = String.Concat(strNum, str1, str2);
                }
                else
                {
                    if (num[item] > 0)
                        str1 = " + ";
                    else
                        str1 = " - ";
                    strNum = String.Concat(strNum, str1, str2, varStr, item.ToString());
                }
            }

            if (strNum.StartsWith("+") || strNum.StartsWith(" "))
                strNum = strNum.Substring(1);

            return strNum;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        /// <param name="den"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string DispTF(double[] num, double[] den, string name, string varStr = " q^-")
        {

            string strNum = getFractString(num, varStr);
            string strDen = getFractString(den, varStr);
            string strHead = "";

            if (String.IsNullOrEmpty(name))
                strHead = "TF = ";
            else
                strHead = name;

            int nbar = Math.Max(strDen.Length, strNum.Length);

            string strBar = new String('-', nbar);
            string strOut = String.Concat(strHead, "\n\n", strNum, '\n', strBar, '\n', strDen);

            return (strOut);
        }

        #endregion displaying

    }
}
