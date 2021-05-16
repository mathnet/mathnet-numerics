// <copyright file="MklFourierTransformControl.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// https://numerics.mathdotnet.com
// https://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2021 Math.NET
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
using MathNet.Numerics.Providers.FourierTransform;

namespace MathNet.Numerics.Providers.MKL.FourierTransform
{
    public class MklFourierTransformControl : IProviderCreator<IFourierTransformProvider>
    {
        const string EnvVarFFTProviderPath = "MathNetNumericsFFTProviderPath";

        /// <summary>
        /// Optional path to try to load native provider binaries from.
        /// If not set, Numerics will fall back to the environment variable
        /// `MathNetNumericsMKLProviderPath` or the default probing paths.
        /// </summary>
        public static string HintPath { get; set; }

        public static IFourierTransformProvider CreateNativeMKL()
        {
            return new MklFourierTransformProvider(GetCombinedHintPath());
        }

        public static void UseNativeMKL()
        {
            FourierTransformControl.Provider = CreateNativeMKL();
        }

        public static bool TryUseNativeMKL()
        {
            return FourierTransformControl.TryUse(CreateNativeMKL());
        }

        static string GetCombinedHintPath()
        {
            if (!String.IsNullOrEmpty(HintPath))
            {
                return HintPath;
            }

            if (!String.IsNullOrEmpty(FourierTransformControl.HintPath))
            {
                return FourierTransformControl.HintPath;
            }

            var value = Environment.GetEnvironmentVariable(MklControl.EnvVarMKLProviderPath);
            if (!String.IsNullOrEmpty(value))
            {
                return value;
            }

            value = Environment.GetEnvironmentVariable(EnvVarFFTProviderPath);
            if (!String.IsNullOrEmpty(value))
            {
                return value;
            }

            return null;
        }

        public IFourierTransformProvider CreateProvider()
        {
            return CreateNativeMKL();
        }
    }
}
