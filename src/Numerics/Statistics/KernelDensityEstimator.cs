// <copyright file="KernelDensityEstimator.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2013 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.Statistics
{
  /// <summary>
  /// An enum of the methods the <see cref="MathNet.Numerics.Statistics.KernelDensityEstimator"/> supports
  /// for automatic bandwidth selection.
  /// </summary>
  public enum KDEBandwidthSelectionMethod
  {
    /// <summary>
    /// TBD
    /// </summary>
    SilvermansRuleOfThumb,
    /// <summary>
    /// TBD
    /// </summary>
    SolveTheEquation
  }

  /// <summary>
  /// The <see cref="MathNet.Numerics.Statistics.KernelDensityEstimator"/> supports several predefined Kernels.
  /// Note that you can set your own custom kernel by setting <see cref="MathNet.Numerics.Statistics.KernelDensityEstimator.Kernel"/>
  /// </summary>
  public enum KDEKernelType
  {
    /// <summary>
    /// A Gaussian kernel (PDF of Normal distribution with mean 0 and variance 1).
    /// This kernel is the default.
    /// </summary>
    Gaussian,

    /// <summary>
    /// Epanechnikov Kernel
    /// x => Math.Abs(x) <= 1.0 ? 3.0/4.0(1.0-x^2) : 0.0
    /// </summary>
    Epanechnikov,

    /// <summary>
    /// Uniform Kernel
    /// x => Math.Abs(x) <= 1.0 ? 1.0/2.0 : 0.0
    /// </summary>
    Uniform,

    /// <summary>
    /// Triangular Kernel
    /// x => Math.Abs(x) <= 1.0 ? (1.0-Math.Abs(x)) : 0.0
    /// </summary>
    Triangular,

    /// <summary>
    /// A custom kernel can be set by property <see cref="MathNet.Numerics.Statistics.KernelDensityEstimator.Kernel"/>
    /// </summary>
    Custom
  }

  /// <summary>
  /// 
  /// </summary>
  public class KernelDensityEstimator
  {
    public KernelDensityEstimator(IList<double> samples)
    {
      _samples = samples;
      KernelType = KDEKernelType.Gaussian;
    }

    public double EstimateDensity(double x)
    {
      var n = Samples.Count;
      var estimate = CommonParallel.Aggregate(0, n,
        i =>
        {
          var s = Samples[i];
          return Kernel((x - s) / Bandwidth);
        },
        (a, b) => a + b,
        0d) / (n * Bandwidth);

      return estimate;
    }

    private readonly IList<double> _samples;
    public IList<double> Samples
    {
      get { return _samples; }
    }

    private double _bandwidth = 1;
    public double Bandwidth
    {
      get
      {
        return _bandwidth;
      }
      set
      {
        if (value <= 0)
        {
          throw new ArgumentException("The bandwidth must be a positive number!");
        }
        _bandwidth = value;
      }
    }

    private KDEKernelType _kernelType;
    public KDEKernelType KernelType
    {
      get { return _kernelType; }
      set
      {
        switch (value)
        {
          case KDEKernelType.Gaussian:
            {
              Kernel = x => Normal.PDF(0.0, 1.0, x);
              _kernelType = KDEKernelType.Gaussian;
            }
            break;
          case KDEKernelType.Epanechnikov:
            {
              Kernel = x => Math.Abs(x) <= 1.0 ? 0.75 * (1 - x * x) : 0.0;
              _kernelType = KDEKernelType.Epanechnikov;
            }
            break;
          case KDEKernelType.Uniform:
            {
              Kernel = x => ContinuousUniform.PDF(-1.0, 1.0, x);
              _kernelType = KDEKernelType.Uniform;
            }
            break;
          case KDEKernelType.Triangular:
            {
              Kernel = x => Triangular.PDF(-1.0, 1.0, 0.0, x);
              _kernelType = KDEKernelType.Triangular;
            }
            break;
          case KDEKernelType.Custom:
            throw new ArgumentException("In order to set a custom Kernel, property Kernel must be set directly.");
        }
      }
    }

    private Func<double, double> _kernel;
    /// <summary>
    /// Sets or Gets the Kernel used for the density estimate.
    /// Setting the Kernel changes the <see cref="KernelDensityEstimator.KernelType"/> to <see cref="KDEKernelType.Custom"/>
    /// A Kernel is a real function with Integral 1. Typically, it is also positive and symmetric about 0.
    /// Note that none of these properties are checked.
    /// </summary>
    public Func<double, double> Kernel
    {
      get { return _kernel; }
      set
      {
        _kernel = value;
        _kernelType = KDEKernelType.Custom;
      }
    }

    public double SelectBandwidth(KDEBandwidthSelectionMethod bandwidthSelectionMethod)
    {
      throw new NotImplementedException();
    }
  }
}
