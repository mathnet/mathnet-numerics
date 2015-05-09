#include <stdio.h>
#include "cublas_v2.h"
#include "cuda_runtime.h"
#include "wrapper_common.h"

template<typename T, typename AXPY>
void cuda_axpy(const cublasHandle_t blasHandle, const int n, const T alpha, const T x[], int incX, T y[], int incY, AXPY axpy)
{
	T *d_X = NULL;
	T *d_Y = NULL;
	cudaMalloc((void**)&d_X, n*sizeof(T));
	cudaMalloc((void**)&d_Y, n*sizeof(T));

	cublasSetVector(n, sizeof(T), x, incX, d_X, incX);
	cublasSetVector(n, sizeof(T), y, incY, d_Y, incY);

	axpy(blasHandle, n, &alpha, d_X, incX, d_Y, incX);

	cublasGetVector(n, sizeof(T), d_Y, incY, y, incY);

	cudaFree(d_X);
	cudaFree(d_Y);
}

template<typename T, typename SCAL>
void cuda_scal(const cublasHandle_t blasHandle, const int n, const T alpha, T x[], int incX, SCAL scal)
{
	T *d_X = NULL;
	cudaMalloc((void**)&d_X, n*sizeof(T));

	cublasSetVector(n, sizeof(T), x, incX, d_X, incX);

	scal(blasHandle, n, &alpha, d_X, incX);

	cublasGetVector(n, sizeof(T), d_X, incX, x, incX);

	cudaFree(d_X);
}

template<typename T, typename DOT>
void cuda_dot(const cublasHandle_t blasHandle, const int n, const T x[], int incX, const T y[], int incY, T* result, DOT dot)
{
	T *d_X = NULL;
	T *d_Y = NULL;
	cudaMalloc((void**)&d_X, n*sizeof(T));
	cudaMalloc((void**)&d_Y, n*sizeof(T));

	cublasSetVector(n, sizeof(T), x, incX, d_X, incX);
	cublasSetVector(n, sizeof(T), y, incY, d_Y, incY);

	dot(blasHandle, n, d_X, incX, d_Y, incY, result);

	cudaFree(d_X);
	cudaFree(d_Y);
}

template<typename T, typename GEMM>
void cuda_gemm(const cublasHandle_t handle, const cublasOperation_t transa, const cublasOperation_t transb, int m, int n, int k, const T alpha, const T A[], int lda, const T B[], int ldb, const T beta, T C[], int ldc, GEMM gemm)
{
	T *d_A = NULL;
	cudaMalloc((void**)&d_A, m*k*sizeof(T));
	cublasSetMatrix(m, k, sizeof(T), A, m, d_A, m);

	T *d_B = NULL;
	cudaMalloc((void**)&d_B, k*n*sizeof(T));
	cublasSetMatrix(k, n, sizeof(T), B, k, d_B, k);

	T *d_C = NULL;
	cudaMalloc((void**)&d_C, m*n*sizeof(T));
	cublasSetMatrix(m, n, sizeof(T), C, m, d_C, m);

	gemm(handle, transa, transb, m, n, k, &alpha, d_A, lda, d_B, ldb, &beta, d_C, ldc);

	cublasGetMatrix(m, n, sizeof(T), d_C, m, C, m);

	cudaFree(d_A);
	cudaFree(d_B);
	cudaFree(d_C);
}

extern "C" {

	DLLEXPORT void s_axpy(const cublasHandle_t blasHandle, const int n, const float alpha, const float x[], float y[]){
		cuda_axpy(blasHandle, n, alpha, x, 1, y, 1, cublasSaxpy);
	}

	DLLEXPORT void d_axpy(const cublasHandle_t blasHandle, const int n, const double alpha, const double x[], double y[]){
		cuda_axpy(blasHandle, n, alpha, x, 1, y, 1, cublasDaxpy);
	}

	DLLEXPORT void c_axpy(const cublasHandle_t blasHandle, const int n, const cuComplex alpha, const cuComplex x[], cuComplex y[]){
		cuda_axpy(blasHandle, n, alpha, x, 1, y, 1, cublasCaxpy);
	}

	DLLEXPORT void z_axpy(const cublasHandle_t blasHandle, const int n, const cuDoubleComplex alpha, const cuDoubleComplex x[], cuDoubleComplex y[]){
		cuda_axpy(blasHandle, n, alpha, x, 1, y, 1, cublasZaxpy);
	}

	DLLEXPORT void s_scale(const cublasHandle_t blasHandle, const int n, const float alpha, float x[]){
		cuda_scal(blasHandle, n, alpha, x, 1, cublasSscal);
	}

	DLLEXPORT void d_scale(const cublasHandle_t blasHandle, const int n, const double alpha, double x[]){
		cuda_scal(blasHandle, n, alpha, x, 1, cublasDscal);
	}

	DLLEXPORT void c_scale(const cublasHandle_t blasHandle, const int n, const cuComplex alpha, cuComplex x[]){
		cuda_scal(blasHandle, n, alpha, x, 1, cublasCscal);
	}

	DLLEXPORT void z_scale(const cublasHandle_t blasHandle, const int n, const cuDoubleComplex alpha, cuDoubleComplex x[]){
		cuda_scal(blasHandle, n, alpha, x, 1, cublasZscal);
	}

	DLLEXPORT float s_dot_product(const cublasHandle_t blasHandle, const int n, const float x[], const float y[]){
		float ret;
		cuda_dot(blasHandle, n, x, 1, y, 1, &ret, cublasSdot);
		return ret;
	}

	DLLEXPORT double d_dot_product(const cublasHandle_t blasHandle, const int n, const double x[], const double y[]){
		double ret;
		cuda_dot(blasHandle, n, x, 1, y, 1, &ret, cublasDdot);
		return ret;
	}

	DLLEXPORT cuComplex c_dot_product(const cublasHandle_t blasHandle, const int n, const cuComplex x[], const cuComplex y[]){
		cuComplex ret;
		cuda_dot(blasHandle, n, x, 1, y, 1, &ret, cublasCdotu);
		return ret;
	}

	DLLEXPORT cuDoubleComplex z_dot_product(const cublasHandle_t blasHandle, const int n, const cuDoubleComplex x[], const cuDoubleComplex y[]){
		cuDoubleComplex ret;
		cuda_dot(blasHandle, n, x, 1, y, 1, &ret, cublasZdotu);
		return ret;
	}

	DLLEXPORT void s_matrix_multiply(const cublasHandle_t blasHandle, cublasOperation_t transA, cublasOperation_t transB, const int m, const int n, const int k, const float alpha, const float x[], const float y[], const float beta, float c[]){
		int lda = transA == CUBLAS_OP_N ? m : k;
		int ldb = transB == CUBLAS_OP_N ? k : n;

		cuda_gemm(blasHandle, transA, transB, m, n, k, alpha, x, lda, y, ldb, beta, c, m, cublasSgemm);
	}

	DLLEXPORT void d_matrix_multiply(const cublasHandle_t blasHandle, cublasOperation_t transA, cublasOperation_t transB, const int m, const int n, const int k, const double alpha, const double x[], const double y[], const double beta, double c[]){
		int lda = transA == CUBLAS_OP_N ? m : k;
		int ldb = transB == CUBLAS_OP_N ? k : n;

		cuda_gemm(blasHandle, transA, transB, m, n, k, alpha, x, lda, y, ldb, beta, c, m, cublasDgemm);
	}

	DLLEXPORT void c_matrix_multiply(const cublasHandle_t blasHandle, cublasOperation_t transA, cublasOperation_t transB, const int m, const int n, const int k, const cuComplex alpha, const cuComplex x[], const cuComplex y[], const cuComplex beta, cuComplex c[]){
		int lda = transA == CUBLAS_OP_N ? m : k;
		int ldb = transB == CUBLAS_OP_N ? k : n;

		cuda_gemm(blasHandle, transA, transB, m, n, k, alpha, x, lda, y, ldb, beta, c, m, cublasCgemm);
	}

	DLLEXPORT void z_matrix_multiply(const cublasHandle_t blasHandle, cublasOperation_t transA, cublasOperation_t transB, const int m, const int n, const int k, const cuDoubleComplex alpha, const cuDoubleComplex x[], const cuDoubleComplex y[], const cuDoubleComplex beta, cuDoubleComplex c[]){
		int lda = transA == CUBLAS_OP_N ? m : k;
		int ldb = transB == CUBLAS_OP_N ? k : n;

		cuda_gemm(blasHandle, transA, transB, m, n, k, alpha, x, lda, y, ldb, beta, c, m, cublasZgemm);
	}

}

