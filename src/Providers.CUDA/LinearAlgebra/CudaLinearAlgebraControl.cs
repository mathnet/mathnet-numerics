// <copyright file="CudaLinearAlgebraControl.cs" company="Math.NET">
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
using MathNet.Numerics.Providers.LinearAlgebra;

namespace MathNet.Numerics.Providers.CUDA.LinearAlgebra
{
    public class CudaLinearAlgebraControl : IProviderCreator<ILinearAlgebraProvider>
    {
        public static ILinearAlgebraProvider CreateNativeCUDA() => new CudaLinearAlgebraProvider(GetCombinedHintPath());
        public static void UseNativeCUDA() => LinearAlgebraControl.Provider = CreateNativeCUDA();
        public static bool TryUseNativeCUDA() => LinearAlgebraControl.TryUse(CreateNativeCUDA());

        static string GetCombinedHintPath()
        {
            if (!string.IsNullOrEmpty(CudaControl.HintPath))
            {
                return CudaControl.HintPath;
            }

            if (!string.IsNullOrEmpty(LinearAlgebraControl.HintPath))
            {
                return LinearAlgebraControl.HintPath;
            }

            var value = Environment.GetEnvironmentVariable(CudaControl.EnvVarCUDAProviderPath);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }

            return null;
        }

        public ILinearAlgebraProvider CreateProvider() => CreateNativeCUDA();
    }
}
