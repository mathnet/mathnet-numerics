#include "wrapper_common.h"
#include "mkl.h"

#ifdef __cplusplus
extern "C" {
#endif /* __cplusplus */

	/*
		Capability is supported if >0

		Actual number can be increased over time to indicate
		extensions/revisions (that do not break compatibility)
	*/
	DLLEXPORT int query_capability(const int capability)
	{
		switch (capability)
		{

		// SANITY CHECKS
		case 0:	return 0;
		case 1:	return -1;

		case 4: return sizeof(size_t);    // 4 (x86), 8 (x64)
		case 5: return sizeof(MKL_INT);   // 4 (both)
		case 6: return sizeof(MKL_LONG);  // 4 (both)
		case 7: return sizeof(MKL_INT64); // 8 (both)

		// PLATFORM
		case 8:
#ifdef _M_IX86
			return 1;
#else
			return 0;
#endif
		case 9:
#ifdef _M_X64
			return 1;
#else
			return 0;
#endif
		case 10:
#ifdef _M_IA64
			return 1;
#else
			return 0;
#endif

		// MKL VERSION
		case 32: // major version
			{
				MKLVersion Version;
				mkl_get_version(&Version);
				return Version.MajorVersion;
			}
		case 33: // minor version
			{
				MKLVersion Version;
				mkl_get_version(&Version);
				return Version.MinorVersion;
			}
		case 34: // update version
			{
				MKLVersion Version;
				mkl_get_version(&Version);
				return Version.UpdateVersion;
			}

		// COMMON/SHARED
		case 64: return 11; // revision
		case 65: return 1; // numerical consistency, precision and accuracy modes
		case 66: return 1; // threading control
		case 67: return 1; // memory management

		// LINEAR ALGEBRA
		case 128: return 2;	// basic dense linear algebra (major - breaking)
		case 129: return 0;	// basic dense linear algebra (minor - non-breaking)
		case 130: return 0;	// vector functions (major - breaking)
		case 131: return 1;	// vector functions (minor - non-breaking)

		// OPTIMIZATION
		case 256: return 0; // basic optimization

		// FFT
		case 384: return 1; // basic FFT (major - breaking)
		case 385: return 0; // basic FFT (minor - non-breaking)

		default: return 0; // unknown or not supported

		}
	}

	DLLEXPORT void set_consistency_mode(const MKL_INT mode)
	{
		mkl_cbwr_set(mode);
	}

	DLLEXPORT void set_vml_mode(const MKL_UINT mode)
	{
		vmlSetMode(mode);
	}

	DLLEXPORT void set_max_threads(const MKL_INT num_threads)
	{
		mkl_set_num_threads(num_threads);
	}

	/* Obsolete, will be dropped in the next revision */
	DLLEXPORT void SetImprovedConsistency(void)
	{
		// set improved consistency for MKL and vector functions
		mkl_cbwr_set(MKL_CBWR_COMPATIBLE);
		vmlSetMode(VML_HA | VML_DOUBLE_CONSISTENT);
	}

#ifdef __cplusplus
}
#endif /* __cplusplus */
