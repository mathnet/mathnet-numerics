#include <stdio.h>
#include "cublas_v2.h"
#include "cuda_runtime.h"
#include "wrapper_cuda.h"

template<typename T, typename AXPY>
CudaResults cuda_axpy(const cublasHandle_t blasHandle, const int n, const T alpha, const T x[], int incX, T y[], int incY, AXPY axpy)
{
	T *d_X = NULL;
	T *d_Y = NULL;
	CudaResults results;

	SAFECUDACALL(results.error, cudaMalloc((void**)&d_X, n*sizeof(T)));
	SAFECUDACALL(results.error, cudaMalloc((void**)&d_Y, n*sizeof(T)));

	SAFECUDACALL(results.blasStatus, cublasSetVector(n, sizeof(T), x, incX, d_X, incX));
	SAFECUDACALL(results.blasStatus, cublasSetVector(n, sizeof(T), y, incY, d_Y, incY));

	SAFECUDACALL(results.blasStatus, axpy(blasHandle, n, &alpha, d_X, incX, d_Y, incX));

	SAFECUDACALL(results.blasStatus, cublasGetVector(n, sizeof(T), d_Y, incY, y, incY));

exit:
	cudaFree(d_X);
	cudaFree(d_Y);

	return results;
}

template<typename T, typename SCAL>
CudaResults cuda_scal(const cublasHandle_t blasHandle, const int n, const T alpha, T x[], int incX, SCAL scal)
{
	T *d_X = NULL;
	CudaResults results;

	SAFECUDACALL(results.error, cudaMalloc((void**)&d_X, n*sizeof(T)));
	SAFECUDACALL(results.blasStatus, cublasSetVector(n, sizeof(T), x, incX, d_X, incX));
	SAFECUDACALL(results.blasStatus, scal(blasHandle, n, &alpha, d_X, incX));
	SAFECUDACALL(results.blasStatus, cublasGetVector(n, sizeof(T), d_X, incX, x, incX));

exit:
	cudaFree(d_X);

	return results;
}

template<typename T, typename DOT>
CudaResults cuda_dot(const cublasHandle_t blasHandle, const int n, const T x[], int incX, const T y[], int incY, T* result, DOT dot)
{
	T *d_X = NULL;
	T *d_Y = NULL;
	CudaResults results;

	SAFECUDACALL(results.error, cudaMalloc((void**)&d_X, n*sizeof(T)));
	SAFECUDACALL(results.error, cudaMalloc((void**)&d_Y, n*sizeof(T)));

	SAFECUDACALL(results.blasStatus, cublasSetVector(n, sizeof(T), x, incX, d_X, incX));
	SAFECUDACALL(results.blasStatus, cublasSetVector(n, sizeof(T), y, incY, d_Y, incY));

	SAFECUDACALL(results.blasStatus, dot(blasHandle, n, d_X, incX, d_Y, incY, result));

exit:
	cudaFree(d_X);
	cudaFree(d_Y);

	return results;
}

template<typename T, typename GEMM>
CudaResults cuda_gemm(const cublasHandle_t handle, const cublasOperation_t transa, const cublasOperation_t transb, int m, int n, int k, const T alpha, const T A[], int lda, const T B[], int ldb, const T beta, T C[], int ldc, GEMM gemm)
{
	T *d_A = NULL;
	T *d_B = NULL;
	T *d_C = NULL;
	CudaResults results;

	SAFECUDACALL(results.error, cudaMalloc((void**)&d_A, m*k*sizeof(T)));
	SAFECUDACALL(results.blasStatus, cublasSetMatrix(m, k, sizeof(T), A, m, d_A, m));

	SAFECUDACALL(results.error, cudaMalloc((void**)&d_B, k*n*sizeof(T)));
	SAFECUDACALL(results.blasStatus, cublasSetMatrix(k, n, sizeof(T), B, k, d_B, k));

	SAFECUDACALL(results.error, cudaMalloc((void**)&d_C, m*n*sizeof(T)));
	SAFECUDACALL(results.blasStatus, cublasSetMatrix(m, n, sizeof(T), C, m, d_C, m));

	SAFECUDACALL(results.blasStatus, gemm(handle, transa, transb, m, n, k, &alpha, d_A, lda, d_B, ldb, &beta, d_C, ldc));

	SAFECUDACALL(results.blasStatus, cublasGetMatrix(m, n, sizeof(T), d_C, m, C, m));

exit:
	cudaFree(d_A);
	cudaFree(d_B);
	cudaFree(d_C);

	return results;
}

extern "C" {

	DLLEXPORT CudaResults s_axpy(const cublasHandle_t blasHandle, const int n, const float alpha, const float x[], float y[]){
		return cuda_axpy(blasHandle, n, alpha, x, 1, y, 1, cublasSaxpy);
	}

	DLLEXPORT CudaResults d_axpy(const cublasHandle_t blasHandle, const int n, const double alpha, const double x[], double y[]){
		return cuda_axpy(blasHandle, n, alpha, x, 1, y, 1, cublasDaxpy);
	}

	DLLEXPORT CudaResults c_axpy(const cublasHandle_t blasHandle, const int n, const cuComplex alpha, const cuComplex x[], cuComplex y[]){
		return cuda_axpy(blasHandle, n, alpha, x, 1, y, 1, cublasCaxpy);
	}

	DLLEXPORT CudaResults z_axpy(const cublasHandle_t blasHandle, const int n, const cuDoubleComplex alpha, const cuDoubleComplex x[], cuDoubleComplex y[]){
		return cuda_axpy(blasHandle, n, alpha, x, 1, y, 1, cublasZaxpy);
	}

	DLLEXPORT CudaResults s_scale(const cublasHandle_t blasHandle, const int n, const float alpha, float x[]){
		return cuda_scal(blasHandle, n, alpha, x, 1, cublasSscal);
	}

	DLLEXPORT CudaResults d_scale(const cublasHandle_t blasHandle, const int n, const double alpha, double x[]){
		return cuda_scal(blasHandle, n, alpha, x, 1, cublasDscal);
	}

	DLLEXPORT CudaResults c_scale(const cublasHandle_t blasHandle, const int n, const cuComplex alpha, cuComplex x[]){
		return cuda_scal(blasHandle, n, alpha, x, 1, cublasCscal);
	}

	DLLEXPORT CudaResults z_scale(const cublasHandle_t blasHandle, const int n, const cuDoubleComplex alpha, cuDoubleComplex x[]){
		return cuda_scal(blasHandle, n, alpha, x, 1, cublasZscal);
	}

	DLLEXPORT CudaResults s_dot_product(const cublasHandle_t blasHandle, const int n, const float x[], const float y[], float *result){
		return cuda_dot(blasHandle, n, x, 1, y, 1, result, cublasSdot);
	}

	DLLEXPORT CudaResults d_dot_product(const cublasHandle_t blasHandle, const int n, const double x[], const double y[], double *result){
		return cuda_dot(blasHandle, n, x, 1, y, 1, result, cublasDdot);
	}

	DLLEXPORT CudaResults c_dot_product(const cublasHandle_t blasHandle, const int n, const cuComplex x[], const cuComplex y[], cuComplex *result){
		return cuda_dot(blasHandle, n, x, 1, y, 1, result, cublasCdotu);
	}

	DLLEXPORT CudaResults z_dot_product(const cublasHandle_t blasHandle, const int n, const cuDoubleComplex x[], const cuDoubleComplex y[], cuDoubleComplex *result){
		return cuda_dot(blasHandle, n, x, 1, y, 1, result, cublasZdotu);
	}

	DLLEXPORT CudaResults s_matrix_multiply(const cublasHandle_t blasHandle, cublasOperation_t transA, cublasOperation_t transB, const int m, const int n, const int k, const float alpha, const float x[], const float y[], const float beta, float c[]){
		int lda = transA == CUBLAS_OP_N ? m : k;
		int ldb = transB == CUBLAS_OP_N ? k : n;

		return cuda_gemm(blasHandle, transA, transB, m, n, k, alpha, x, lda, y, ldb, beta, c, m, cublasSgemm);
	}

	DLLEXPORT CudaResults d_matrix_multiply(const cublasHandle_t blasHandle, cublasOperation_t transA, cublasOperation_t transB, const int m, const int n, const int k, const double alpha, const double x[], const double y[], const double beta, double c[]){
		int lda = transA == CUBLAS_OP_N ? m : k;
		int ldb = transB == CUBLAS_OP_N ? k : n;

		return cuda_gemm(blasHandle, transA, transB, m, n, k, alpha, x, lda, y, ldb, beta, c, m, cublasDgemm);
	}

	DLLEXPORT CudaResults c_matrix_multiply(const cublasHandle_t blasHandle, cublasOperation_t transA, cublasOperation_t transB, const int m, const int n, const int k, const cuComplex alpha, const cuComplex x[], const cuComplex y[], const cuComplex beta, cuComplex c[]){
		int lda = transA == CUBLAS_OP_N ? m : k;
		int ldb = transB == CUBLAS_OP_N ? k : n;

		return cuda_gemm(blasHandle, transA, transB, m, n, k, alpha, x, lda, y, ldb, beta, c, m, cublasCgemm);
	}

	DLLEXPORT CudaResults z_matrix_multiply(const cublasHandle_t blasHandle, cublasOperation_t transA, cublasOperation_t transB, const int m, const int n, const int k, const cuDoubleComplex alpha, const cuDoubleComplex x[], const cuDoubleComplex y[], const cuDoubleComplex beta, cuDoubleComplex c[]){
		int lda = transA == CUBLAS_OP_N ? m : k;
		int ldb = transB == CUBLAS_OP_N ? k : n;

		return cuda_gemm(blasHandle, transA, transB, m, n, k, alpha, x, lda, y, ldb, beta, c, m, cublasZgemm);
	}

}

