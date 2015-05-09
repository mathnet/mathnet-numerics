#include "wrapper_common.h"
#include "mkl.h"

#if __cplusplus
extern "C" {
#endif

DLLEXPORT void free_buffers(void) {
	mkl_free_buffers();
}

DLLEXPORT void thread_free_buffers(void) {
	mkl_thread_free_buffers();
}

DLLEXPORT int disable_fast_mm(void) {
	return mkl_disable_fast_mm();
}

DLLEXPORT MKL_INT64 mem_stat(int* AllocatedBuffers) {
	return mkl_mem_stat(AllocatedBuffers);
}

DLLEXPORT MKL_INT64 peak_mem_usage(int mode) {
	return mkl_peak_mem_usage(mode);
}

#if __cplusplus
}
#endif
