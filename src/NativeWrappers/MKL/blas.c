#include "mkl_cblas.h"
#include "wrapper_common.h"

DLLEXPORT void s_axpy(const MKL_INT n, const float alpha, const float x[], float y[]){
	cblas_saxpy(n, alpha, x, 1, y, 1);
}

DLLEXPORT void d_axpy(const MKL_INT n, const double alpha, const double x[], double y[]){
	cblas_daxpy(n, alpha, x, 1, y, 1);
}

DLLEXPORT void c_axpy(const MKL_INT n, const MKL_Complex8 alpha, const MKL_Complex8 x[], MKL_Complex8 y[]){
	cblas_caxpy(n, &alpha, x, 1, y, 1);
}

DLLEXPORT void z_axpy(const MKL_INT n, const MKL_Complex16 alpha, const MKL_Complex16 x[], MKL_Complex16 y[]){
	cblas_zaxpy(n, &alpha, x, 1, y, 1);
}

DLLEXPORT void s_scale(const MKL_INT n, const float alpha, float x[]){
	cblas_sscal(n, alpha, x, 1);
}

DLLEXPORT void d_scale(const MKL_INT n, const double alpha, double x[]){
	cblas_dscal(n, alpha, x, 1);
}

DLLEXPORT void c_scale(const MKL_INT n, const MKL_Complex8 alpha, MKL_Complex8 x[]){
	cblas_cscal(n, &alpha, x, 1);
}

DLLEXPORT void z_scale(const MKL_INT n, const MKL_Complex16 alpha, MKL_Complex16 x[]){
	cblas_zscal(n, &alpha, x, 1);
}

DLLEXPORT float s_dot_product(const MKL_INT n, const float x[], const float y[]){
	return cblas_sdot(n, x, 1, y, 1);
}

DLLEXPORT double d_dot_product(const MKL_INT n, const double x[], const double y[]){
	return cblas_ddot(n, x, 1, y, 1);
}

DLLEXPORT MKL_Complex8 c_dot_product(const MKL_INT n, const MKL_Complex8 x[], const MKL_Complex8 y[]){
	MKL_Complex8 ret;
	cblas_cdotu_sub(n, x, 1, y, 1, &ret);
	return ret;
}

DLLEXPORT MKL_Complex16 z_dot_product(const MKL_INT n, const MKL_Complex16 x[], const MKL_Complex16 y[]){
	MKL_Complex16 ret;
	cblas_zdotu_sub(n, x, 1, y, 1, &ret);
	return ret;
}

DLLEXPORT void s_matrix_multiply(CBLAS_TRANSPOSE transA, CBLAS_TRANSPOSE transB, const MKL_INT m, const MKL_INT n, const MKL_INT k, const float alpha, const float x[], const float y[], const float beta, float c[]){
	MKL_INT lda = transA == CblasNoTrans ? m : k;
	MKL_INT ldb = transB == CblasNoTrans ? k : n;

	cblas_sgemm(CblasColMajor, transA, transB, m, n, k, alpha, x, lda, y, ldb, beta, c, m);
}

DLLEXPORT void d_matrix_multiply(CBLAS_TRANSPOSE transA, CBLAS_TRANSPOSE transB, const MKL_INT m, const MKL_INT n, const MKL_INT k, const double alpha, const double x[], const double y[], const double beta, double c[]){
	MKL_INT lda = transA == CblasNoTrans ? m : k;
	MKL_INT ldb = transB == CblasNoTrans ? k : n;

	cblas_dgemm(CblasColMajor, transA, transB, m, n, k, alpha, x, lda, y, ldb, beta, c, m);
}

DLLEXPORT void c_matrix_multiply(CBLAS_TRANSPOSE transA, CBLAS_TRANSPOSE transB, const MKL_INT m, const MKL_INT n, const MKL_INT k, const MKL_Complex8 alpha, const MKL_Complex8 x[], const MKL_Complex8 y[], const MKL_Complex8 beta, MKL_Complex8 c[]){
	MKL_INT lda = transA == CblasNoTrans ? m : k;
	MKL_INT ldb = transB == CblasNoTrans ? k : n;

	cblas_cgemm(CblasColMajor, transA, transB, m, n, k, &alpha, x, lda, y, ldb, &beta, c, m);
}

DLLEXPORT void z_matrix_multiply(CBLAS_TRANSPOSE transA, CBLAS_TRANSPOSE transB, const MKL_INT m, const MKL_INT n, const MKL_INT k, const MKL_Complex16 alpha, const MKL_Complex16 x[], const MKL_Complex16 y[], const MKL_Complex16 beta, MKL_Complex16 c[]){
	MKL_INT lda = transA == CblasNoTrans ? m : k;
	MKL_INT ldb = transB == CblasNoTrans ? k : n;

	cblas_zgemm(CblasColMajor, transA, transB, m, n, k, &alpha, x, lda, y, ldb, &beta, c, m);
}
