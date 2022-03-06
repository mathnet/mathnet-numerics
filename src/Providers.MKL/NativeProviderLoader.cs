// <copyright file="NativeProviderLoader.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
#if !NET5_0_OR_GREATER
using System.Security;
#endif
using System.Threading;

// ReSharper disable InconsistentNaming

namespace MathNet.Numerics.Providers.Common
{
    internal enum ProcArchitecture
    {
        X64,
        X86,
        Arm,
        Arm64
    }

    /// <summary>
    /// Helper class to load native libraries depending on the architecture of the OS and process.
    /// </summary>
    internal static class NativeProviderLoader
    {
        static readonly object StaticLock = new object();

        /// <summary>
        /// Dictionary of handles to previously loaded libraries,
        /// </summary>
        static readonly Lazy<Dictionary<string, IntPtr>> NativeHandles = new Lazy<Dictionary<string, IntPtr>>(LazyThreadSafetyMode.PublicationOnly);

#if !NET5_0_OR_GREATER
        /// <summary>
        /// If the last native library failed to load then gets the corresponding exception
        /// which occurred or null if the library was successfully loaded.
        /// </summary>
        internal static Exception LastException { get; private set; }
#endif

        static bool IsWindows { get; }
        static bool IsLinux { get; }
        static bool IsMac { get; }
        static bool IsUnix { get; }

        static ProcArchitecture ProcArchitecture { get; }
        static string Extension { get; }

        static NativeProviderLoader()
        {
#if !NET461
            IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            IsMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

            var a = RuntimeInformation.ProcessArchitecture;
            bool arm = a == Architecture.Arm || a == Architecture.Arm64;
#else
            var p = Environment.OSVersion.Platform;
            IsLinux = p == PlatformID.Unix;
            IsMac = p == PlatformID.MacOSX;

            bool arm = false;
#endif

            IsUnix = IsLinux || IsMac;
            IsWindows = !IsUnix;

            Extension = IsWindows
                ? ".dll"
                : IsLinux
                    ? ".so"
                    : ".dylib";

            ProcArchitecture = Environment.Is64BitProcess
                ? arm
                    ? ProcArchitecture.Arm64
                    : ProcArchitecture.X64
                : arm
                    ? ProcArchitecture.Arm
                    : ProcArchitecture.X86;
        }

        /// <summary>
        /// Load the native library with the given filename.
        /// </summary>
        /// <param name="fileName">The file name of the library to load.</param>
        /// <param name="hintPath">Hint path where to look for the native binaries. Can be null.</param>
        /// <returns>True if the library was successfully loaded or if it has already been loaded.</returns>
        internal static bool TryLoad(string fileName, string hintPath)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            // First just try to load it with the file name only
            if (TryLoadDirect(fileName))
            {
                return true;
            }

            // NOTE: doesn't actually work since the p/invoke is then wrong
            //if (!IsWindows && fileName.StartsWith("lib") && TryLoadDirect(fileName.Substring(3)))
            //{
            //    return true;
            //}

            if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
            {
                fileName = Path.ChangeExtension(fileName, Extension);

                // Try it also with the proper file extension
                if (TryLoadDirect(fileName))
                {
                    return true;
                }
            }

            // If we have hint path provided by the user, look there next
            if (hintPath != null && TryLoadFromDirectory(fileName, hintPath))
            {
                return true;
            }

            // If we have an overall hint path provided by the user, look there next
            if (Control.NativeProviderPath != null && Control.NativeProviderPath != hintPath && TryLoadFromDirectory(fileName, Control.NativeProviderPath))
            {
                return true;
            }

            // Look under the current AppDomain's base directory
            if (TryLoadFromDirectory(fileName, AppDomain.CurrentDomain.BaseDirectory))
            {
                return true;
            }

            // Look at this assembly's directory
            if (TryLoadFromDirectory(fileName, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)))
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
        static bool TryLoadFromDirectory(string fileName, string directory)
        {
            if (!Directory.Exists(directory))
            {
                return false;
            }

            directory = Path.GetFullPath(directory);

            if (IsWindows)
            {
                switch (ProcArchitecture)
                {
                    case ProcArchitecture.X64:
                        return TryLoadFile(directory, "runtimes/win-x64/native", fileName)
                            || TryLoadFile(directory, "win-x64/native", fileName)
                            || TryLoadFile(directory, "win-x64", fileName)
                            || TryLoadFile(directory, "x64", fileName)
                            || TryLoadFile(directory, string.Empty, fileName);
                    case ProcArchitecture.X86:
                        return TryLoadFile(directory, "runtimes/win-x86/native", fileName)
                            || TryLoadFile(directory, "win-x86/native", fileName)
                            || TryLoadFile(directory, "win-x86", fileName)
                            || TryLoadFile(directory, "x86", fileName)
                            || TryLoadFile(directory, string.Empty, fileName);
                    case ProcArchitecture.Arm64:
                        return TryLoadFile(directory, "runtimes/win-arm64/native", fileName)
                            || TryLoadFile(directory, "win-arm64/native", fileName)
                            || TryLoadFile(directory, "win-arm64", fileName)
                            || TryLoadFile(directory, "arm64", fileName)
                            || TryLoadFile(directory, string.Empty, fileName);
                    case ProcArchitecture.Arm:
                        return TryLoadFile(directory, "runtimes/win-arm/native", fileName)
                            || TryLoadFile(directory, "win-arm/native", fileName)
                            || TryLoadFile(directory, "win-arm", fileName)
                            || TryLoadFile(directory, "arm", fileName)
                            || TryLoadFile(directory, string.Empty, fileName);
                    default:
                        return TryLoadFile(directory, string.Empty, fileName);
                }
            }

            if (IsLinux)
            {
                switch (ProcArchitecture)
                {
                    case ProcArchitecture.X64:
                        return TryLoadFile(directory, "runtimes/linux-x64/native", fileName)
                            || TryLoadFile(directory, "linux-x64/native", fileName)
                            || TryLoadFile(directory, "linux-x64", fileName)
                            || TryLoadFile(directory, "x64", fileName)
                            || TryLoadFile(directory, string.Empty, fileName);
                    case ProcArchitecture.X86:
                        return TryLoadFile(directory, "runtimes/linux-x86/native", fileName)
                            || TryLoadFile(directory, "linux-x86/native", fileName)
                            || TryLoadFile(directory, "linux-x86", fileName)
                            || TryLoadFile(directory, "x86", fileName)
                            || TryLoadFile(directory, string.Empty, fileName);
                    case ProcArchitecture.Arm64:
                        return TryLoadFile(directory, "runtimes/linux-arm64/native", fileName)
                            || TryLoadFile(directory, "linux-arm64/native", fileName)
                            || TryLoadFile(directory, "linux-arm64", fileName)
                            || TryLoadFile(directory, "arm64", fileName)
                            || TryLoadFile(directory, string.Empty, fileName);
                    case ProcArchitecture.Arm:
                        return TryLoadFile(directory, "runtimes/linux-arm/native", fileName)
                            || TryLoadFile(directory, "linux-arm/native", fileName)
                            || TryLoadFile(directory, "linux-arm", fileName)
                            || TryLoadFile(directory, "arm", fileName)
                            || TryLoadFile(directory, string.Empty, fileName);
                    default:
                        return TryLoadFile(directory, string.Empty, fileName);
                }
            }

            if (IsMac)
            {
                switch (ProcArchitecture)
                {
                    case ProcArchitecture.X64:
                        return TryLoadFile(directory, "runtimes/osx-x64/native", fileName)
                            || TryLoadFile(directory, "osx-x64/native", fileName)
                            || TryLoadFile(directory, "osx-x64", fileName)
                            || TryLoadFile(directory, "x64", fileName)
                            || TryLoadFile(directory, string.Empty, fileName);
                    case ProcArchitecture.Arm64:
                        return TryLoadFile(directory, "runtimes/osx-arm64/native", fileName)
                            || TryLoadFile(directory, "osx-arm64/native", fileName)
                            || TryLoadFile(directory, "osx-arm64", fileName)
                            || TryLoadFile(directory, "arm64", fileName)
                            || TryLoadFile(directory, string.Empty, fileName);
                    default:
                        return TryLoadFile(directory, string.Empty, fileName);
                }
            }

            switch (ProcArchitecture)
            {
                case ProcArchitecture.X64:
                    return TryLoadFile(directory, "x64", fileName)
                        || TryLoadFile(directory, string.Empty, fileName);
                case ProcArchitecture.X86:
                    return TryLoadFile(directory, "x86", fileName)
                        || TryLoadFile(directory, string.Empty, fileName);
                case ProcArchitecture.Arm64:
                    return TryLoadFile(directory, "arm64", fileName)
                        || TryLoadFile(directory, string.Empty, fileName);
                case ProcArchitecture.Arm:
                    return TryLoadFile(directory, "arm", fileName)
                        || TryLoadFile(directory, string.Empty, fileName);
                default:
                    return TryLoadFile(directory, string.Empty, fileName);
            }
        }

        /// <summary>
        /// Try to load a native library by only the file name of the library.
        /// </summary>
        /// <returns>True if the library was successfully loaded or if it has already been loaded.</returns>
        static bool TryLoadDirect(string fileName)
        {
            lock (StaticLock)
            {
                if (NativeHandles.Value.TryGetValue(fileName, out IntPtr libraryHandle))
                {
                    return true;
                }

#if NET5_0_OR_GREATER
                try
                {
                    if (!NativeLibrary.TryLoad(fileName, out libraryHandle) || libraryHandle == IntPtr.Zero)
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }

                NativeHandles.Value[fileName] = libraryHandle;
                return true;

#else
                try
                {
                    // If successful this will return a handle to the library
                    libraryHandle = IsWindows ? WindowsLoader.LoadLibrary(fileName) : IsMac ? MacLoader.LoadLibrary(fileName) : LinuxLoader.LoadLibrary(fileName);
                }
                catch (Exception e)
                {
                    LastException = e;
                    return false;
                }

                if (libraryHandle == IntPtr.Zero)
                {
                    int lastError = Marshal.GetLastWin32Error();
                    var exception = new System.ComponentModel.Win32Exception(lastError);
                    LastException = exception;
                    return false;
                }

                LastException = null;
                NativeHandles.Value[fileName] = libraryHandle;
                return true;
#endif
            }
        }

        /// <summary>
        /// Try to load a native library by providing the full path including the file name of the library.
        /// </summary>
        /// <returns>True if the library was successfully loaded or if it has already been loaded.</returns>
        static bool TryLoadFile(string directory, string relativePath, string fileName)
        {
            lock (StaticLock)
            {
                if (NativeHandles.Value.TryGetValue(fileName, out IntPtr libraryHandle))
                {
                    return true;
                }

                var fullPath = Path.GetFullPath(Path.Combine(Path.Combine(directory, relativePath), fileName));
                if (!File.Exists(fullPath))
                {
                    // If the library isn't found within an architecture specific folder then return false
                    // to allow normal P/Invoke searching behavior when the library is called
                    return false;
                }

#if NET5_0_OR_GREATER
                try
                {
                    if (!NativeLibrary.TryLoad(fullPath, out libraryHandle) || libraryHandle == IntPtr.Zero)
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }

                NativeHandles.Value[fileName] = libraryHandle;
                return true;

#else
                try
                {
                    // If successful this will return a handle to the library
                    libraryHandle = IsWindows ? WindowsLoader.LoadLibrary(fullPath) : IsMac ? MacLoader.LoadLibrary(fullPath) : LinuxLoader.LoadLibrary(fullPath);
                }
                catch (Exception e)
                {
                    LastException = e;
                    return false;
                }

                if (libraryHandle == IntPtr.Zero)
                {
                    int lastError = Marshal.GetLastWin32Error();
                    var exception = new System.ComponentModel.Win32Exception(lastError);
                    LastException = exception;
                    return false;
                }

                LastException = null;
                NativeHandles.Value[fileName] = libraryHandle;
                return true;
#endif
            }
        }

#if !NET5_0_OR_GREATER
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
        static class LinuxLoader
        {
            public static IntPtr LoadLibrary(string fileName)
            {
                return dlopen(fileName, RTLD_NOW);
            }

            const int RTLD_NOW = 2;

            [DllImport("libdl.so.2", SetLastError = true)]
            static extern IntPtr dlopen(string fileName, int flags);
        }

        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        static class MacLoader
        {
            public static IntPtr LoadLibrary(string fileName)
            {
                return dlopen(fileName, RTLD_NOW);
            }

            const int RTLD_NOW = 2;

            [DllImport("libdl.dylib", SetLastError = true)]
            static extern IntPtr dlopen(string fileName, int flags);
        }
#endif
    }
}
