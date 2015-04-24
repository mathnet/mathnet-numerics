#include "cublas_v2.h"
#include "wrapper_common.h"

#if GCC 
extern "C" {
#endif
	DLLEXPORT void s_axpy(const cublasHandle_t handle, const int n, const float alpha, const float x[], float y[]){
		cublasSaxpy(handle, n, &alpha, x, 1, y, 1);
	}

	DLLEXPORT void d_axpy(const cublasHandle_t handle, const int n, const double alpha, const double x[], double y[]){
		cublasDaxpy(handle, n, &alpha, x, 1, y, 1);
	}

	DLLEXPORT void c_axpy(const cublasHandle_t handle, const int n, const cuComplex alpha, const cuComplex x[], cuComplex y[]){
		cublasCaxpy(handle, n, &alpha, x, 1, y, 1);
	}

	DLLEXPORT void z_axpy(const cublasHandle_t handle, const int n, const cuDoubleComplex alpha, const cuDoubleComplex x[], cuDoubleComplex y[]){
		cublasZaxpy(handle, n, &alpha, x, 1, y, 1);
	}

	DLLEXPORT void s_scale(const cublasHandle_t handle, const int n, const float alpha, float x[]){
		cublasSscal(handle, n, &alpha, x, 1);
	}

	DLLEXPORT void d_scale(const cublasHandle_t handle, const int n, const double alpha, double x[]){
		cublasDscal(handle, n, &alpha, x, 1);
	}

	DLLEXPORT void c_scale(const cublasHandle_t handle, const int n, const cuComplex alpha, cuComplex x[]){
		cublasCscal(handle, n, &alpha, x, 1);
	}

	DLLEXPORT void z_scale(const cublasHandle_t handle, const int n, const cuDoubleComplex alpha, cuDoubleComplex x[]){
		cublasZscal(handle, n, &alpha, x, 1);
	}

	DLLEXPORT float s_dot_product(const cublasHandle_t handle, const int n, const float x[], const float y[]){
		float ret;
		cublasSdot(handle, n, x, 1, y, 1, &ret);
		return ret;
	}

	DLLEXPORT double d_dot_product(const cublasHandle_t handle, const int n, const double x[], const double y[]){
		double ret;
		cublasDdot(handle, n, x, 1, y, 1, &ret);
		return ret;
	}

	DLLEXPORT cuComplex c_dot_product(const cublasHandle_t handle, const int n, const cuComplex x[], const cuComplex y[]){
		cuComplex ret;
		cublasCdotu(handle, n, x, 1, y, 1, &ret);
		return ret;
	}

	DLLEXPORT cuDoubleComplex z_dot_product(const cublasHandle_t handle, const int n, const cuDoubleComplex x[], const cuDoubleComplex y[]){
		cuDoubleComplex ret;
		cublasZdotu(handle, n, x, 1, y, 1, &ret);
		return ret;
	}

	DLLEXPORT void s_matrix_multiply(const cublasHandle_t handle, cublasOperation_t transA, cublasOperation_t transB, const int m, const int n, const int k, const float alpha, const float x[], const float y[], const float beta, float c[]){
		int lda = transA == CUBLAS_OP_N ? m : k;
		int ldb = transB == CUBLAS_OP_N ? k : n;

		cublasSgemm(handle, transA, transB, m, n, k, &alpha, x, lda, y, ldb, &beta, c, m);
	}

	DLLEXPORT void d_matrix_multiply(const cublasHandle_t handle, cublasOperation_t transA, cublasOperation_t transB, const int m, const int n, const int k, const double alpha, const double x[], const double y[], const double beta, double c[]){
		int lda = transA == CUBLAS_OP_N ? m : k;
		int ldb = transB == CUBLAS_OP_N ? k : n;

		cublasDgemm(handle, transA, transB, m, n, k, &alpha, x, lda, y, ldb, &beta, c, m);
	}

	DLLEXPORT void c_matrix_multiply(const cublasHandle_t handle, cublasOperation_t transA, cublasOperation_t transB, const int m, const int n, const int k, const cuComplex alpha, const cuComplex x[], const cuComplex y[], const cuComplex beta, cuComplex c[]){
		int lda = transA == CUBLAS_OP_N ? m : k;
		int ldb = transB == CUBLAS_OP_N ? k : n;

		cublasCgemm(handle, transA, transB, m, n, k, &alpha, x, lda, y, ldb, &beta, c, m);
	}

	DLLEXPORT void z_matrix_multiply(const cublasHandle_t handle, cublasOperation_t transA, cublasOperation_t transB, const int m, const int n, const int k, const cuDoubleComplex alpha, const cuDoubleComplex x[], const cuDoubleComplex y[], const cuDoubleComplex beta, cuDoubleComplex c[]){
		int lda = transA == CUBLAS_OP_N ? m : k;
		int ldb = transB == CUBLAS_OP_N ? k : n;

		cublasZgemm(handle, transA, transB, m, n, k, &alpha, x, lda, y, ldb, &beta, c, m);
	}

#if GCC 
}
#endif
