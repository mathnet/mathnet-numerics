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

#if NATIVE

namespace MathNet.Numerics.Providers
{
    /// <summary>
    /// Helper class to load native libraries depending on the architecture of the OS and process.
    /// </summary>
    internal static class NativeProviderLoader
    {
        static Lazy<Dictionary<string, string>> _ArchitectureDirectories
            = new Lazy<Dictionary<string, string>>(
                () => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "x86", "x86" },
                    { "AMD64", "x64" },
                    { "IA64", "ia64" },
                    { "ARM", "arm" }
                },
                true);

        /// <summary>
        /// Default directories for each architecture.
        /// </summary>
        static Dictionary<string, string> ArchitectureDirectories
        {
            get { return _ArchitectureDirectories.Value; }
        }

        static Lazy<Dictionary<string, IntPtr>> _nativeHandles = new Lazy<Dictionary<string, IntPtr>>(true);

        /// <summary>
        /// Dictionary of handles to previously loaded libraries,
        /// </summary>
        static Dictionary<string, IntPtr> NativeHandles
        {
            get { return _nativeHandles.Value; }
        }

        static string _ArchDirectory = null;

        /// <summary>
        /// Gets a string indicating the architecture and bitness of the current process.
        /// </summary>
        static string ArchDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_ArchDirectory))
                {
                    string architecture;
                    if (IsUnix)
                    {
                        // Only support x86 and amd64 on Unix as there isn't a reliable way to detect the architecture
                        architecture = Environment.Is64BitProcess ? "AMD64" : "x86";
                    }
                    else
                    {
                        architecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
                        if (!Environment.Is64BitProcess && string.Equals(architecture, "AMD64", StringComparison.OrdinalIgnoreCase))
                        {
                            architecture = "x86";
                        }
                    }

                    // Fallback to using the architecture name if unknown
                    if (!ArchitectureDirectories.TryGetValue(architecture, out _ArchDirectory))
                        _ArchDirectory = architecture;
                }

                return _ArchDirectory;
            }
        }

        static bool IsUnix
        {
            get
            {
                var p = Environment.OSVersion.Platform;
                return p == PlatformID.Unix || p == PlatformID.MacOSX;
            }
        }

        /// <summary>
        /// If the last native library failed to load then gets the corresponding exception
        /// which occurred or null if the library was successfully loaded.
        /// </summary>
        public static Exception LastException { get; private set; }

        static readonly object StaticLock = new Object();

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
            var userDir = Control.NativeProviderPath;
            if (userDir != null && userDir.Exists && TryLoad(fileName, userDir))
            {
                return true;
            }

            // Look under the current AppDomain's base directory
            if (TryLoad(fileName, new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)))
            {
                return true;
            }

            // Look at this assembly's directory
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(assemblyDir) && TryLoad(fileName, new DirectoryInfo(assemblyDir)))
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
        public static bool TryLoad(string fileName, DirectoryInfo directory)
        {
            if (!directory.Exists)
            {
                return false;
            }

            // If we have a know architecture, try the matching subdirectory first
            var architecture = ArchDirectory;
            if (!string.IsNullOrEmpty(architecture) && TryLoadFile(new FileInfo(Path.Combine(directory.FullName, architecture, fileName))))
            {
                return true;
            }

            // Otherwise try to load directly from the provided directory
            return TryLoadFile(new FileInfo(Path.Combine(directory.FullName, fileName)));
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
                if (NativeHandles.TryGetValue(file.Name, out libraryHandle))
                {
                    return true;
                }

                if (!file.Exists)
                {
                    // If the library isn't found within an architecture specific folder then return false
                    // to allow normal P/Invoke searching behaviour when the library is called
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
                    NativeHandles[file.Name] = libraryHandle;
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

            [DllImport("kernel32", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto, SetLastError = true)]
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

            [DllImport("libdl.so", CharSet = CharSet.Auto, SetLastError = true)]
            static extern IntPtr dlopen(String fileName, int flags);
        }
    }
}

#endif