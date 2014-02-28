#include "wrapper_common.h"

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
		case 64: return 4;	// revision

		// LINEAR ALGEBRA
		case 128: return 1;	// basic dense linear algebra

		// OPTIMIZATION
		case 256: return 0; // basic optimization

		// FFT
		case 384: return 0; // basic FFT

		default: return 0; // unknown or not supported

		}
	}

#ifdef __cplusplus
}
#endif /* __cplusplus */
