#include "include\cblas.h"
#include "wrapper_common.h"

DLLEXPORT void s_axpy(int n, float alpha, float x[], float y[]){
	cblas_saxpy(n, alpha, x, 1, y, 1);
}

DLLEXPORT void d_axpy(int n, double alpha, double x[], double y[]){
	cblas_daxpy(n, alpha, x, 1, y, 1);
}

DLLEXPORT void c_axpy(int n, Complex8 alpha, Complex8 x[], Complex8 y[]){
	cblas_caxpy(n, &alpha, x, 1, y, 1);
}

DLLEXPORT void z_axpy(int n, Complex16 alpha, Complex16 x[], Complex16 y[]){
	cblas_zaxpy(n, &alpha, x, 1, y, 1);
}

DLLEXPORT void s_scale(int n, float alpha, float x[]){
	cblas_sscal(n, alpha, x, 1);
}

DLLEXPORT void d_scale(int n, double alpha, double x[]){
	cblas_dscal(n, alpha, x, 1);
}

DLLEXPORT void c_scale(int n, Complex8 alpha, Complex8 x[]){
	cblas_cscal(n, &alpha, x, 1);
}

DLLEXPORT void z_scale(int n, Complex16 alpha, Complex16 x[]){
	cblas_zscal(n, &alpha, x, 1);
}

DLLEXPORT float s_dot_product(int n, float x[], float y[]){
	return cblas_sdot(n, x, 1, y, 1);
}

DLLEXPORT double d_dot_product(int n, double x[], double y[]){
	return cblas_ddot(n, x, 1, y, 1);
}

DLLEXPORT Complex8 c_dot_product(int n, Complex8 x[], Complex8 y[]){
	Complex8 ret;
	cblas_cdotu_sub(n, x, 1, y, 1, &ret);
	return ret;
}

DLLEXPORT Complex16 z_dot_product(int n, Complex16 x[], Complex16 y[]){
	Complex16 ret;
	cblas_zdotu_sub(n, x, 1, y, 1, &ret);
	return ret;
}

DLLEXPORT void s_matrix_multiply(enum CBLAS_TRANSPOSE transA, enum CBLAS_TRANSPOSE transB, int m, int n, int k, float alpha, float x[], float y[], float beta, float c[]){
	int lda = transA == CblasNoTrans ? m : k;
	int ldb = transB == CblasNoTrans ? k : n;

	cblas_sgemm(CblasColMajor, transA, transB, m, n, k, alpha, x, lda, y, ldb, beta, c, m);
}

DLLEXPORT void d_matrix_multiply(enum CBLAS_TRANSPOSE transA, enum CBLAS_TRANSPOSE transB, int m, int n, int k, double alpha, double x[], double y[], double beta, double c[]){
	int lda = transA == CblasNoTrans ? m : k;
	int ldb = transB == CblasNoTrans ? k : n;

	cblas_dgemm(CblasColMajor, transA, transB, m, n, k, alpha, x, lda, y, ldb, beta, c, m);
}

DLLEXPORT void c_matrix_multiply(enum CBLAS_TRANSPOSE transA, enum CBLAS_TRANSPOSE transB, int m, int n, int k, Complex8 alpha, Complex8 x[], Complex8 y[], Complex8 beta, Complex8 c[]){
	int lda = transA == CblasNoTrans ? m : k;
	int ldb = transB == CblasNoTrans ? k : n;

	cblas_cgemm(CblasColMajor, transA, transB, m, n, k, &alpha, x, lda, y, ldb, &beta, c, m);
}

DLLEXPORT void z_matrix_multiply(enum CBLAS_TRANSPOSE transA, enum CBLAS_TRANSPOSE transB, int m, int n, int k, Complex16 alpha, Complex16 x[], Complex16 y[], Complex16 beta, Complex16 c[]){
	int lda = transA == CblasNoTrans ? m : k;
	int ldb = transB == CblasNoTrans ? k : n;

	cblas_zgemm(CblasColMajor, transA, transB, m, n, k, &alpha, x, lda, y, ldb, &beta, c, m);
}
