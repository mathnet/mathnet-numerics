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

		// COMMON/SHARED
		case 64: return 6; // revision
		case 65: return 1; // numerical consistency, precision and accuracy modes
		case 66: return 1; // threading control

		// LINEAR ALGEBRA
		case 128: return 1;	// basic dense linear algebra

		// OPTIMIZATION
		case 256: return 0; // basic optimization

		// FFT
		case 384: return 0; // basic FFT

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
