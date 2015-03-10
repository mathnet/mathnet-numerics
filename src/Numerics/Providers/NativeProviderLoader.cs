// <copyright file="NativeProviderLoader.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2015 Math.NET
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
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace MathNet.Numerics.Providers
{
    /// <summary>
    /// Helper class to load native libraries depending on the architecture of the OS and process.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]
    static internal class NativeProviderLoader
    {
        private static Lazy<Dictionary<string, string>> _platformDirectories
                            = new Lazy<Dictionary<string, string>>(
                                () => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                                {
                                    {"x86", "x86"},
                                    {"AMD64", "amd64"},
                                    {"IA64", "ia64"},
                                    {"ARM", "arm"} // Is there an ARM64?
                                },
                                true);
        /// <summary>
        /// Default directories for each platform.
        /// </summary>
        private static Dictionary<string, string> PlatformDirectories { get { return _platformDirectories.Value; } }

        private static Lazy<Dictionary<string, IntPtr>> _nativeHandles = new Lazy<Dictionary<string,IntPtr>>(true);
        /// <summary>
        /// Dictionary of handles to previously loaded libraries,
        /// </summary>
        private static Dictionary<string, IntPtr> NativeHandles { get { return _nativeHandles.Value; } }

        private static string _Platform = null;
        /// <summary>
        /// Gets a string indicating the platform of the current process.
        /// </summary>
        private static string Platform
        {
            get
            {
                if (string.IsNullOrEmpty(_Platform))
                {
                    string architecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
                    if (!Environment.Is64BitProcess && Environment.Is64BitOperatingSystem)
                    {
                        _Platform = string.Equals(architecture, "ARM", StringComparison.OrdinalIgnoreCase) ? "ARM" : "x86";
                    }
                    else
                    {
                        _Platform = architecture;
                    }
                }

                return _Platform;
            }
        }

        /// <summary>
        /// If the last native library failed to load then gets the corresponding exception which occurred or null if the library was successfully loaded.
        /// </summary>
        private static Exception LastException { get; private set; }

        private static object staticLock = new Object();
        /// <summary>
        /// Load the native library with the given filename,
        /// </summary>
        /// <param name="fileName">The file name of the library to load.</param>
        /// <returns>True if the library was successfully loaded or if it has already been loaded.</returns>
        public static bool LoadNativeLibrary(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");

            lock(staticLock)
            {
                IntPtr libraryHandle = IntPtr.Zero;
                if (NativeHandles.TryGetValue(fileName, out libraryHandle))
                    return true;

                string path = null;
                if (PlatformDirectories.TryGetValue(Platform, out path))
                {
                    path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), path, fileName);
                }
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    // Even if we haven't found the library try and load it using LoadLibrary's normal
                    // search rules by just using the filename without a path
                    path = fileName;
                }

                // If successful this will return a handle to the library
                libraryHandle = LoadLibrary(path);
                if (libraryHandle == IntPtr.Zero)
                {
                    int lastError = Marshal.GetLastWin32Error();
                    var exception = new System.ComponentModel.Win32Exception(lastError);
                    LastException = exception;
                }
                else
                {
                    LastException = null;
                    NativeHandles[fileName] = libraryHandle;
                }

                return libraryHandle != IntPtr.Zero;
            }
        }

        [DllImport("kernel32", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string fileName);
    }
}
