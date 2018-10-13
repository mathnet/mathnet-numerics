//Copyright (c) 2018 Giulio Occhionero

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

namespace MathNet.Numerics.Equity.BlackScholes

open MathNet.Numerics

///Functions to compute the theoretical price and greek parameters of European call options on any time-frame
///p = underlying price
///k = strike price
///v = logarithmic volatility of a single event (events may be days, months, years or whatever...)
///n = number of events to expiration (events may be days, months, years or whatever...)
///r = expected return per event, usually assumed equal to risk-free zero-coupon interest rate (events may be days, months, years or whatever...)
module Call =

    ///Computes the theoretical price of the option
    let Price p k v n r =
        (exp((n * v ** 2.0) / 2.0) * p * (1.0 + SpecialFunctions.Erf((n * (r + v ** 2.0) - log(k) + log(p)) / (sqrt(2.0 * n) * v))) + 
            (k * (-2.0 + SpecialFunctions.Erfc((n * r - log(k) + log(p)) / (sqrt(2.0 * n) * v))))/ exp(n * r)) / 2.0
    
    ///Computes the partial derivative of the theoretical price with respect to the underlying price
    let Delta p k v n r =
        (exp((n * v ** 2.0) / 2.0) * (1.0 + SpecialFunctions.Erf((n * (r + v ** 2.0) - log(k) + log(p)) / (sqrt(2.0 * n) * v))))/2.0

    ///Computes the second partial derivative of the theoretical price with respect to the underlying price
    let Gamma p k v n r =
        exp((n * v ** 2.0) / 2.0 - (n * (r + v ** 2.0) - log(k) + log(p)) ** 2.0 / (2.0 * n * v ** 2.0)) / (sqrt(2.0 * n * Constants.Pi) * v * p)

    ///Computes the partial derivative of the theoretical price with respect to the number of events remaining
    let Theta p k v n r =
        ((2.0 * k * r) / exp(n * r) + (k ** (1.0 + r / v ** 2.0 + log(p) / (n * v ** 2.0)) * sqrt(2.0 / Constants.Pi) * v) /
            (exp((n ** 2.0 * r * (r + 2.0 * v ** 2.0) + log(k) ** 2.0 + log(p) ** 2.0) / (2.0 * n * v ** 2.0)) * sqrt(n) * p ** (r / v ** 2.0))
            + exp((n * v ** 2.0) / 2.0) * p * v ** 2.0 + (2.0 * k * r * SpecialFunctions.Erf((n * r - log(k) + log(p)) / (Constants.Sqrt2 * sqrt(n) * v))) / exp(n * r) + 
            exp((n * v ** 2.0) / 2.0) * p * v ** 2.0 * SpecialFunctions.Erf((n * (r + v ** 2.0) - log(k) + log(p)) / (Constants.Sqrt2 * sqrt(n) * v))) / 4.0

    ///Computes the partial derivative of the theoretical price with respect to the expected return (interest rate)
    let Rho p k v n r =
        (k * n * (1.0 + SpecialFunctions.Erf((n * r - log(k) + log(p)) / (Constants.Sqrt2 * sqrt(n) * v)))) / (2.0 * exp(n * r))

    ///Computes the partial derivative of the theoretical price with respect to the volatility
    let Vega p k v n r =
        (k ** (1.0 + r / v ** 2.0 + log(p) / (n * v ** 2.0)) * sqrt(n)) /
        (exp((n ** 2.0 * r * (r + 2.0 * v ** 2.0) + log(k) ** 2.0 + log(p) ** 2.0) / (2.0 * n * v ** 2.0)) * p ** (r / v ** 2.0) * sqrt(2.0 * Constants.Pi))
        + (exp((n * v ** 2.0) / 2.0) * n * p * v) / 2.0 + (exp((n * v ** 2.0) / 2.0) * n * p * v * SpecialFunctions.Erf((n * (r + v ** 2.0) - log(k) + log(p)) / (Constants.Sqrt2 * sqrt(n) * v))) / 2.0

    ///Computes the leverage factor of the option with respect to the underlying
    let Leverage p k v n r = p * (Delta p k v n r) / (Price p k v n r)


///Functions to compute the theoretical price and greek parameters of European put options on any time-frame
///p = underlying price
///k = strike price
///v = logarithmic volatility of a single event (events may be days, months, years or whatever...)
///n = number of events to expiration (events may be days, months, years or whatever...)
///r = expected return per event, usually assumed equal to risk-free zero-coupon interest rate (events may be days, months, years or whatever...)
module Put =

    ///Computes the theoretical price of the option
    let Price p k v n r =
        ((k * SpecialFunctions.Erfc((n * r - log(k) + log(p)) / (Constants.Sqrt2 * sqrt(n) * v))) / exp(n * r) - 
            exp((n * v ** 2.0) / 2.0) * p * SpecialFunctions.Erfc((n * (r + v ** 2.0) - log(k) + log(p)) / (Constants.Sqrt2 * sqrt(n) * v))) / 2.0

    ///Computes the partial derivative of the theoretical price with respect to the underlying price
    let Delta p k v n r =
        -(exp((n * v ** 2.0) / 2.0) * SpecialFunctions.Erfc((n * (r + v ** 2.0) - log(k) + log(p)) / (Constants.Sqrt2 * sqrt(n) * v))) / 2.0

    ///Computes the second partial derivative of the theoretical price with respect to the underlying price
    let Gamma p k v n r =
        exp((n * v ** 2.0) / 2.0 - (n * (r + v ** 2.0) - log(k) + log(p)) ** 2.0 / (2.0 * n * v ** 2.0)) / (sqrt(n) * p * sqrt(2.0 * Constants.Pi) * v)

    ///Computes the partial derivative of the theoretical price with respect to the number of events remaining
    let Theta p k v n r =
        ((-2.0 * k * r * SpecialFunctions.Erfc((n * r - log(k) + log(p)) / (Constants.Sqrt2 * sqrt(n) * v))) / 
            exp(n * r) + v * ((k ** (1.0 + r / v ** 2.0 + log(p) / (n * v ** 2.0)) * sqrt(2.0 / Constants.Pi)) / 
                (exp((n ** 2.0 * r * (r + 2.0 * v ** 2.0) + log(k) ** 2.0 + log(p) ** 2.0) /
                    (2.0 * n * v ** 2.0)) * sqrt(n) * p ** (r / v ** 2.0)) - exp((n * v ** 2.0) / 2.0) * p * v *
                        SpecialFunctions.Erfc((n * (r + v ** 2.0) - log(k) + log(p)) / (Constants.Sqrt2 * sqrt(n) * v)))) / 4.0

    ///Computes the partial derivative of the theoretical price with respect to the expected return (interest rate)
    let Rho p k v n r =
        -(k * n * SpecialFunctions.Erfc((n * r - log(k) + log(p)) / (Constants.Sqrt2 * sqrt(n) * v))) / (2.0 * exp(n * r))

    ///Computes the partial derivative of the theoretical price with respect to the volatility
    let Vega p k v n r =
        (sqrt(n) * ((k ** (1.0 + r / v ** 2.0 + log(p) / (n * v ** 2.0)) * sqrt(2.0 / Constants.Pi)) /
          (exp((log(k) ** 2.0 + log(p) ** 2.0) / (2.0 * n * v ** 2.0)) * p ** (r / v ** 2.0)) - 
            exp((n * (r + v ** 2.0) ** 2.0) / (2.0 * v ** 2.0)) * sqrt(n) * p * v *
             SpecialFunctions.Erfc((n * (r + v ** 2.0) - log(k) + log(p)) / (Constants.Sqrt2 * sqrt(n) * v)))) /
                (2.0 * exp((n * r * (r + 2.0 * v ** 2.0)) / (2.0 * v ** 2.0)))

    ///Computes the leverage factor of the option with respect to the underlying
    let Leverage p k v n r = p * (Delta p k v n r) / (Price p k v n r)

