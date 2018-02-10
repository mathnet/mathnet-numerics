using MathNet.Numerics;
using MathNet.Numerics.Providers.Common.Mkl;

namespace Benchmark
{
    public enum Provider : int
    {
        Managed = 0,
        NativeMKLAutoHigh = 1,
        NativeMKLAutoLow = 2,
        NativeMKLAvx2High = 3,
        NativeMKLAvx2Low = 4,
        NativeOpenBLAS = 5
    }

    public static class Providers
    {
        public static void ForceProvider(Provider provider)
        {
            //Control.NativeProviderPath = @"..\..\..\..\out\MKL\Windows\";

            switch (provider)
            {
                case Provider.Managed:
                    Control.UseManaged();
                    break;
                case Provider.NativeMKLAutoHigh:
                    Control.UseNativeMKL(MklConsistency.Auto, MklPrecision.Double, MklAccuracy.High);
                    break;
                case Provider.NativeMKLAutoLow:
                    Control.UseNativeMKL(MklConsistency.Auto, MklPrecision.Double, MklAccuracy.Low);
                    break;
                case Provider.NativeMKLAvx2High:
                    Control.UseNativeMKL(MklConsistency.AVX2, MklPrecision.Double, MklAccuracy.High);
                    break;
                case Provider.NativeMKLAvx2Low:
                    Control.UseNativeMKL(MklConsistency.AVX2, MklPrecision.Double, MklAccuracy.Low);
                    break;
                case Provider.NativeOpenBLAS:
                    Control.UseNativeOpenBLAS();
                    break;
            }
        }
    }
}
