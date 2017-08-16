using MathNet.Numerics;

namespace Benchmark
{
    public static class Providers
    {
        public static void ForceNativeMKL()
        {
            //Control.NativeProviderPath = @"C:\Triage\NATIVE-Win\";
            Control.NativeProviderPath = @"..\..\..\..\out\MKL\Windows\";
            Control.UseNativeMKL();
        }

        public static void ForceOpenBLAS()
        {
            //Control.NativeProviderPath = @"C:\Triage\NATIVE-Win\";
            Control.NativeProviderPath = @"..\..\..\..\out\OpenBLAS\Windows\";
            Control.UseNativeOpenBLAS();
        }
    }
}
