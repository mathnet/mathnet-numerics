using System;
using System.IO;
using MathNet.Numerics.Providers.SparseSolver;

namespace MathNet.Numerics.Providers.MKL.SparseSolver
{
    /// <summary>
    /// Intel's Math Kernel Library (MKL) sparse solver provider.
    /// </summary>
    internal sealed partial class MklSparseSolverProvider : ISparseSolverProvider, IDisposable
    {
        const int MinimumCompatibleRevision = 14;

        readonly string _hintPath;

        int sparseSolverMajor;
        int sparseSolverMinor;

        /// <param name="hintPath">Hint path where to look for the native binaries</param>
        internal MklSparseSolverProvider(string hintPath)
        {
            _hintPath = hintPath != null ? Path.GetFullPath(hintPath) : null;
        }

        /// <summary>
        /// Try to find out whether the provider is available, at least in principle.
        /// Verification may still fail if available, but it will certainly fail if unavailable.
        /// </summary>
        public bool IsAvailable()
        {
            return MklProvider.IsAvailable(hintPath: _hintPath);
        }

        /// <summary>
        /// Initialize and verify that the provided is indeed available.
        /// If calling this method fails, consider to fall back to alternatives like the managed provider.
        /// </summary>
        public void InitializeVerify()
        {
            int revision = MklProvider.Load(_hintPath);
            if (revision < MinimumCompatibleRevision)
            {
                throw new NotSupportedException($"MKL Native Provider revision r{revision} is too old. Consider upgrading to a newer version. Revision r{MinimumCompatibleRevision} and newer are supported.");
            }

            sparseSolverMajor = SafeNativeMethods.query_capability((int)ProviderCapability.SparseSolverMajor);
            sparseSolverMinor = SafeNativeMethods.query_capability((int)ProviderCapability.SparseSolverMinor);
            if (!(sparseSolverMajor == 1 && sparseSolverMinor >= 0))
            {
                throw new NotSupportedException(string.Format("MKL Native Provider not compatible. Expecting sparse solver v1 but provider implements v{0}.", sparseSolverMajor));
            }
        }

        /// <summary>
        /// Frees memory buffers, caches and handles allocated in or to the provider.
        /// Does not unload the provider itself, it is still usable afterwards.
        /// </summary>
        public void FreeResources()
        {
            MklProvider.FreeResources();
        }

        public override string ToString()
        {
            return MklProvider.Describe();
        }

        public void Dispose()
        {
            FreeResources();
        }
    }
}
