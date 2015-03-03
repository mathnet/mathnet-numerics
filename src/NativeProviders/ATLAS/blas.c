#include "wrapper_common.h"
#include "blas.h"

#if __cplusplus
extern "C" { 
#endif
DLLEXPORT void s_axpy(const int n, const float alpha, const float x[], float y[]){
	cblas_saxpy(n, alpha, x, 1, y, 1);
}

DLLEXPORT void d_axpy(const int n, const double alpha, const double x[], double y[]){
	cblas_daxpy(n, alpha, x, 1, y, 1);
}

DLLEXPORT void c_axpy(const int n, const Complex8 alpha, const Complex8 x[], Complex8 y[]){
	cblas_caxpy(n, &alpha, x, 1, y, 1);
}

DLLEXPORT void z_axpy(const int n, const Complex16 alpha, const Complex16 x[], Complex16 y[]){
	cblas_zaxpy(n, &alpha, x, 1, y, 1);
}

DLLEXPORT void s_scale(const int n, const float alpha, float x[]){
	cblas_sscal(n, alpha, x, 1);
}

DLLEXPORT void d_scale(const int n, const double alpha, double x[]){
	cblas_dscal(n, alpha, x, 1);
}

DLLEXPORT void c_scale(const int n, const Complex8 alpha, Complex8 x[]){
	cblas_cscal(n, &alpha, x, 1);
}

DLLEXPORT void z_scale(const int n, const Complex16 alpha, Complex16 x[]){
	cblas_zscal(n, &alpha, x, 1);
}

DLLEXPORT float s_dot_product(const int n, const float x[], const float y[]){
	return cblas_sdot(n, x, 1, y, 1);
}

DLLEXPORT double d_dot_product(const int n, const double x[], const double y[]){
	return cblas_ddot(n, x, 1, y, 1);
}

DLLEXPORT Complex8 c_dot_product(const int n, const Complex8 x[], const Complex8 y[]){
	Complex8 ret;
	cblas_cdotu_sub(n, x, 1, y, 1, &ret);
	return ret;
}

DLLEXPORT Complex16 z_dot_product(const int n, const Complex16 x[], const Complex16 y[]){
	Complex16 ret;
	cblas_zdotu_sub(n, x, 1, y, 1, &ret);
	return ret;
}

DLLEXPORT void s_matrix_multiply(const enum CBLAS_TRANSPOSE transA, const enum CBLAS_TRANSPOSE transB, const int m, const int n, const int k, const float alpha, const float x[], const float y[], const float beta, float c[]){
	int lda = transA == CblasNoTrans ? m : k;
	int ldb = transB == CblasNoTrans ? k : n;

	cblas_sgemm(CblasColMajor, transA, transB, m, n, k, alpha, x, lda, y, ldb, beta, c, m);
}

DLLEXPORT void d_matrix_multiply(const enum CBLAS_TRANSPOSE transA, const enum CBLAS_TRANSPOSE transB, const int m, const int n, const int k, const double alpha, const double x[], const double y[], const double beta, double c[]){
	int lda = transA == CblasNoTrans ? m : k;
	int ldb = transB == CblasNoTrans ? k : n;

	cblas_dgemm(CblasColMajor, transA, transB, m, n, k, alpha, x, lda, y, ldb, beta, c, m);
}

DLLEXPORT void c_matrix_multiply(const enum CBLAS_TRANSPOSE transA, const enum CBLAS_TRANSPOSE transB, const int m, const int n, const int k, const Complex8 alpha, const Complex8 x[], const Complex8 y[], const Complex8 beta, Complex8 c[]){
	int lda = transA == CblasNoTrans ? m : k;
	int ldb = transB == CblasNoTrans ? k : n;

	cblas_cgemm(CblasColMajor, transA, transB, m, n, k, &alpha, x, lda, y, ldb, &beta, c, m);
}

DLLEXPORT void z_matrix_multiply(const enum CBLAS_TRANSPOSE transA, const enum CBLAS_TRANSPOSE transB, const int m, const int n, const int k, const Complex16 alpha, const Complex16 x[], const Complex16 y[], const Complex16 beta, Complex16 c[]){
	int lda = transA == CblasNoTrans ? m : k;
	int ldb = transB == CblasNoTrans ? k : n;

	cblas_zgemm(CblasColMajor, transA, transB, m, n, k, &alpha, x, lda, y, ldb, &beta, c, m);
}

#if __cplusplus 
}
#endif