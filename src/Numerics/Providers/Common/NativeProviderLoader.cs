// <copyright file="NativeProviderLoader.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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

#if NATIVE

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace MathNet.Numerics.Providers.Common
{
    /// <summary>
    /// Helper class to load native libraries depending on the architecture of the OS and process.
    /// </summary>
    internal static class NativeProviderLoader
    {
        static readonly object StaticLock = new Object();

        const string X86 = "x86";
        const string X64 = "x64";
        const string IA64 = "ia64";
        const string ARM = "arm";
        const string ARM64 = "arm64";

        /// <summary>
        /// Dictionary of handles to previously loaded libraries,
        /// </summary>
        static readonly Lazy<Dictionary<string, IntPtr>> NativeHandles = new Lazy<Dictionary<string, IntPtr>>(LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Gets a string indicating the architecture and bitness of the current process.
        /// </summary>
        static readonly Lazy<string> ArchitectureKey = new Lazy<string>(EvaluateArchitectureKey, LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// If the last native library failed to load then gets the corresponding exception
        /// which occurred or null if the library was successfully loaded.
        /// </summary>
        public static Exception LastException { get; private set; }

        static bool IsUnix
        {
            get
            {
                var p = Environment.OSVersion.Platform;
                return p == PlatformID.Unix || p == PlatformID.MacOSX;
            }
        }

        static string EvaluateArchitectureKey()
        {
            if (IsUnix)
            {
                // Only support x86 and amd64 on Unix as there isn't a reliable way to detect the architecture
                return Environment.Is64BitProcess ? X64 : X86;
            }

            var architecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");

            if (string.Equals(architecture, "x86", StringComparison.OrdinalIgnoreCase))
            {
                return X86;
            }

            if (string.Equals(architecture, "amd64", StringComparison.OrdinalIgnoreCase)
                || string.Equals(architecture, "x64", StringComparison.OrdinalIgnoreCase))
            {
                return Environment.Is64BitProcess ? X64 : X86;
            }

            if (string.Equals(architecture, "ia64", StringComparison.OrdinalIgnoreCase))
            {
                return IA64;
            }

            if (string.Equals(architecture, "arm", StringComparison.OrdinalIgnoreCase))
            {
                return Environment.Is64BitProcess ? ARM64 : ARM;
            }

            // Fallback if unknown
            return architecture;
        }

        /// <summary>
        /// Load the native library with the given filename.
        /// </summary>
        /// <param name="fileName">The file name of the library to load.</param>
        /// <returns>True if the library was successfully loaded or if it has already been loaded.</returns>
        public static bool TryLoad(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            // If we have an extra path provided by the user, look there first
            if (TryLoad(fileName, Control.NativeProviderPath))
            {
                return true;
            }

            // Look under the current AppDomain's base directory
            if (TryLoad(fileName, AppDomain.CurrentDomain.BaseDirectory))
            {
                return true;
            }

            // Look at this assembly's directory
            if (TryLoad(fileName, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to load a native library by providing its name and a directory.
        /// Tries to load an implementation suitable for the current CPU architecture
        /// and process mode if there is a matching subfolder.
        /// </summary>
        /// <returns>True if the library was successfully loaded or if it has already been loaded.</returns>
        public static bool TryLoad(string fileName, string directory)
        {
            if (!Directory.Exists(directory))
            {
                return false;
            }

            directory = Path.GetFullPath(directory);

            // If we have a know architecture, try the matching subdirectory first
            var architecture = ArchitectureKey.Value;
            if (!string.IsNullOrEmpty(architecture) && TryLoadFile(new FileInfo(Path.Combine(directory, architecture, fileName))))
            {
                return true;
            }

            // Otherwise try to load directly from the provided directory
            return TryLoadFile(new FileInfo(Path.Combine(directory, fileName)));
        }

        /// <summary>
        /// Try to load a native library by providing the full path including the file name of the library.
        /// </summary>
        /// <returns>True if the library was successfully loaded or if it has already been loaded.</returns>
        public static bool TryLoadFile(FileInfo file)
        {
            lock (StaticLock)
            {
                IntPtr libraryHandle;
                if (NativeHandles.Value.TryGetValue(file.Name, out libraryHandle))
                {
                    return true;
                }

                if (!file.Exists)
                {
                    // If the library isn't found within an architecture specific folder then return false
                    // to allow normal P/Invoke searching behavior when the library is called
                    return false;
                }

                // If successful this will return a handle to the library
                libraryHandle = IsUnix ? UnixLoader.LoadLibrary(file.FullName) : WindowsLoader.LoadLibrary(file.FullName);
                if (libraryHandle == IntPtr.Zero)
                {
                    int lastError = Marshal.GetLastWin32Error();
                    var exception = new System.ComponentModel.Win32Exception(lastError);
                    LastException = exception;
                }
                else
                {
                    LastException = null;
                    NativeHandles.Value[file.Name] = libraryHandle;
                }

                return libraryHandle != IntPtr.Zero;
            }
        }

        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        static class WindowsLoader
        {
            public static IntPtr LoadLibrary(string fileName)
            {
                return LoadLibraryEx(fileName, IntPtr.Zero, LOAD_WITH_ALTERED_SEARCH_PATH);
            }

            // Search for dependencies in the library's directory rather than the calling process's directory
            const uint LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008;

            [DllImport("kernel32", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, SetLastError = true)]
            static extern IntPtr LoadLibraryEx(string fileName, IntPtr reservedNull, uint flags);
        }

        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        static class UnixLoader
        {
            public static IntPtr LoadLibrary(string fileName)
            {
                return dlopen(fileName, RTLD_NOW);
            }

            const int RTLD_NOW = 2;

            [DllImport("libdl.so", SetLastError = true)]
            static extern IntPtr dlopen(String fileName, int flags);
        }
    }
}

#endif
