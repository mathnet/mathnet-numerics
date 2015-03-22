#include "cblas.h"
#include "wrapper_common.h"

#if GCC 
extern "C" { 
#endif
DLLEXPORT void s_axpy(const blasint n, const float alpha, const float x[], float y[]){
	cblas_saxpy(n, alpha, x, 1, y, 1);
}

DLLEXPORT void d_axpy(const blasint n, const double alpha, const double x[], double y[]){
	cblas_daxpy(n, alpha, x, 1, y, 1);
}

DLLEXPORT void c_axpy(const blasint n, const openblas_complex_float alpha, const openblas_complex_float x[], openblas_complex_float y[]){
	cblas_caxpy(n, (float*)&alpha, (float*)x, 1, (float*)y, 1);
}

DLLEXPORT void z_axpy(const blasint n, const openblas_complex_double alpha, const openblas_complex_double x[], openblas_complex_double y[]){
	cblas_zaxpy(n, (double*)&alpha, (double*)x, 1, (double*)y, 1);
}

DLLEXPORT void s_scale(const blasint n, const float alpha, float x[]){
	cblas_sscal(n, alpha, x, 1);
}

DLLEXPORT void d_scale(const blasint n, const double alpha, double x[]){
	cblas_dscal(n, alpha, x, 1);
}

DLLEXPORT void c_scale(const blasint n, const openblas_complex_float alpha, openblas_complex_float x[]){
	cblas_cscal(n, (float*)&alpha, (float*)x, 1);
}

DLLEXPORT void z_scale(const blasint n, const openblas_complex_double alpha, openblas_complex_double x[]){
	cblas_zscal(n, (double*)&alpha, (double*)x, 1);
}

DLLEXPORT float s_dot_product(const blasint n, const float x[], const float y[]){
	return cblas_sdot(n, x, 1, y, 1);
}

DLLEXPORT double d_dot_product(const blasint n, const double x[], const double y[]){
	return cblas_ddot(n, x, 1, y, 1);
}

DLLEXPORT openblas_complex_float c_dot_product(const blasint n, const openblas_complex_float x[], const openblas_complex_float y[]){
	openblas_complex_float ret;
	cblas_cdotu_sub(n, (float*)x, 1, (float*)y, 1, &ret);
	return ret;
}

DLLEXPORT openblas_complex_double z_dot_product(const blasint n, const openblas_complex_double x[], const openblas_complex_double y[]){
	openblas_complex_double ret;
	cblas_zdotu_sub(n, (double*)x, 1, (double*)y, 1, &ret);
	return ret;
}

DLLEXPORT void s_matrix_multiply(CBLAS_TRANSPOSE transA, CBLAS_TRANSPOSE transB, const blasint m, const blasint n, const blasint k, const float alpha, const float x[], const float y[], const float beta, float c[]){
	blasint lda = transA == CblasNoTrans ? m : k;
	blasint ldb = transB == CblasNoTrans ? k : n;

	cblas_sgemm(CblasColMajor, transA, transB, m, n, k, alpha, x, lda, y, ldb, beta, c, m);
}

DLLEXPORT void d_matrix_multiply(CBLAS_TRANSPOSE transA, CBLAS_TRANSPOSE transB, const blasint m, const blasint n, const blasint k, const double alpha, const double x[], const double y[], const double beta, double c[]){
	blasint lda = transA == CblasNoTrans ? m : k;
	blasint ldb = transB == CblasNoTrans ? k : n;

	cblas_dgemm(CblasColMajor, transA, transB, m, n, k, alpha, x, lda, y, ldb, beta, c, m);
}

DLLEXPORT void c_matrix_multiply(CBLAS_TRANSPOSE transA, CBLAS_TRANSPOSE transB, const blasint m, const blasint n, const blasint k, const openblas_complex_float alpha, const openblas_complex_float x[], const openblas_complex_float y[], const openblas_complex_float beta, openblas_complex_float c[]){
	blasint lda = transA == CblasNoTrans ? m : k;
	blasint ldb = transB == CblasNoTrans ? k : n;

	cblas_cgemm(CblasColMajor, transA, transB, m, n, k, (float*)&alpha, (float*)x, lda, (float*)y, ldb, (float*)&beta, (float*)c, m);
}

DLLEXPORT void z_matrix_multiply(CBLAS_TRANSPOSE transA, CBLAS_TRANSPOSE transB, const blasint m, const blasint n, const blasint k, const openblas_complex_double alpha, const openblas_complex_double x[], const openblas_complex_double y[], const openblas_complex_double beta, openblas_complex_double c[]){
	blasint lda = transA == CblasNoTrans ? m : k;
	blasint ldb = transB == CblasNoTrans ? k : n;

	cblas_zgemm(CblasColMajor, transA, transB, m, n, k, (double*)&alpha, (double*)x, lda, (double*)y, ldb, (double*)&beta, (double*)c, m);
}

#if GCC 
} 
#endif
