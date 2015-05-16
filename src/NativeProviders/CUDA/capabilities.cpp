#include "wrapper_cuda.h"
#include "cuda_runtime.h"
#include "cublas_v2.h"
#include "cusolverDn.h"

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
		int count;
		int device;
		cudaDeviceProp prop;

		if (cudaGetDeviceCount(&count))
			return 0;

		if (count == 0)
			return 0;

		if (cudaGetDevice(&device))
			return 0;

		if (cudaGetDeviceProperties(&prop, device))
			return 0;



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
		case 64: 
			return prop.major;

			// LINEAR ALGEBRA
		case 128:
			return prop.major >= 2;

			// OPTIMIZATION
		case 256: return 0; // basic optimization

			// FFT
		case 384: return 0; // basic FFT

		default: return 0; // unknown or not supported

		}
	}

	DLLEXPORT CudaResults createBLASHandle(cublasHandle_t *blasHandle){
		CudaResults ret;
		ret.blasStatus = cublasCreate(blasHandle);
		return ret;
	}

	DLLEXPORT CudaResults destroyBLASHandle(cublasHandle_t blasHandle){
		CudaResults ret;
		ret.blasStatus = cublasDestroy(blasHandle);
		return ret;
	}

	DLLEXPORT CudaResults createSolverHandle(cusolverDnHandle_t *solverHandle){
		CudaResults ret;
		ret.solverStatus = cusolverDnCreate(solverHandle);
		return ret;
	}

	DLLEXPORT CudaResults destroySolverHandle(cusolverDnHandle_t solverHandle){
		CudaResults ret;
		ret.solverStatus = cusolverDnDestroy(solverHandle);
		return ret;
	}

#ifdef __cplusplus
}
#endif /* __cplusplus */
