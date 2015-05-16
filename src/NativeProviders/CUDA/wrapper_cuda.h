#ifndef WRAPPER_CUDA_H
#define WRAPPER_CUDA_H

#include "wrapper_common.h"
#include "cuda_runtime.h"
#include "cublas_v2.h"
#include "cusolver_common.h"

#define SAFECUDACALL(error,call) {error = call; if(error){goto exit;}}

typedef struct
{
	cudaError_t error;
	cublasStatus_t blasStatus;
	cusolverStatus_t solverStatus;
} CudaResults;

#endif