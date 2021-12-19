// <copyright file="AppSwitches.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2020 Math.NET
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

namespace MathNet.Numerics
{
    /// <summary>
    /// AppContext based switches to disable functionality, controllable through also in the
    /// host application through AppContext or by configuration with AppContextSwitchOverride.
    /// https://docs.microsoft.com/en-us/dotnet/framework/configure-apps/file-schema/runtime/appcontextswitchoverrides-element
    /// </summary>
    /// <remarks>
    /// Since AppContext is not supported on .NET Framework 4.0, a local implementation is used there instead,
    /// which cannot be controlled though configuration or through AppContext.
    /// </remarks>
    public static class AppSwitches
    {
        const string AppSwitchDisableNativeProviderProbing = "Switch.MathNet.Numerics.Providers.DisableNativeProviderProbing";
        const string AppSwitchDisableNativeProviders = "Switch.MathNet.Numerics.Providers.DisableNativeProviders";
        const string AppSwitchDisableMklNativeProvider = "Switch.MathNet.Numerics.Providers.DisableMklNativeProvider";
        const string AppSwitchDisableCudaNativeProvider = "Switch.MathNet.Numerics.Providers.DisableCudaNativeProvider";
        const string AppSwitchDisableOpenBlasNativeProvider = "Switch.MathNet.Numerics.Providers.DisableOpenBlasNativeProvider";

        static void SetSwitch(string switchName, bool isEnabled)
        {
            System.AppContext.SetSwitch(switchName, isEnabled);
        }

        static bool IsEnabled(string switchName)
        {
            return System.AppContext.TryGetSwitch(switchName, out bool isEnabled) && isEnabled;
        }

        public static bool DisableNativeProviderProbing
        {
            get => IsEnabled(AppSwitchDisableNativeProviderProbing);
            set => SetSwitch(AppSwitchDisableNativeProviderProbing, value);
        }

        public static bool DisableNativeProviders
        {
            get => IsEnabled(AppSwitchDisableNativeProviders);
            set => SetSwitch(AppSwitchDisableNativeProviders, value);
        }

        public static bool DisableMklNativeProvider
        {
            get => IsEnabled(AppSwitchDisableMklNativeProvider);
            set => SetSwitch(AppSwitchDisableMklNativeProvider, value);
        }

        public static bool DisableCudaNativeProvider
        {
            get => IsEnabled(AppSwitchDisableCudaNativeProvider);
            set => SetSwitch(AppSwitchDisableCudaNativeProvider, value);
        }

        public static bool DisableOpenBlasNativeProvider
        {
            get => IsEnabled(AppSwitchDisableOpenBlasNativeProvider);
            set => SetSwitch(AppSwitchDisableOpenBlasNativeProvider, value);
        }
    }
}
