#include "acml.h"
#include "wrapper_common.h"

enum TRANSPOSE {CblasNoTrans=111, CblasTrans=112, CblasConjTrans=113, CblasConjNoTrans=114};
char getTransChar(TRANSPOSE);

DLLEXPORT void s_axpy(const int n, const float alpha, float x[], float y[]){
	saxpy(n, alpha, x, 1, y, 1);
}

DLLEXPORT void d_axpy(const int n, const double alpha, double x[], double y[]){
	daxpy(n, alpha, x, 1, y, 1);
}

DLLEXPORT void c_axpy(const int n, complex alpha, complex x[], complex y[]){
	caxpy(n, &alpha, x, 1, y, 1);
}

DLLEXPORT void z_axpy(const int n, doublecomplex alpha, doublecomplex x[], doublecomplex y[]){
	zaxpy(n, &alpha, x, 1, y, 1);
}

DLLEXPORT void s_scale(const int n, const float alpha, float x[]){
	sscal(n, alpha, x, 1);
}

DLLEXPORT void d_scale(const int n, const double alpha, double x[]){
	dscal(n, alpha, x, 1);
}

DLLEXPORT void c_scale(const int n, complex alpha, complex x[]){
	cscal(n, &alpha, x, 1);
}

DLLEXPORT void z_scale(const int n, doublecomplex alpha, doublecomplex x[]){
	zscal(n, &alpha, x, 1);
}

DLLEXPORT float s_dot_product(const int n, float x[], float y[]){
	return sdot(n, x, 1, y, 1);
}

DLLEXPORT double d_dot_product(const int n, double x[], double y[]){
	return ddot(n, x, 1, y, 1);
}

DLLEXPORT complex c_dot_product(const int n, complex x[], complex y[]){
	return cdotu(n, x, 1, y, 1);
}

DLLEXPORT doublecomplex z_dot_product(int n, doublecomplex x[], doublecomplex y[]){
	return zdotu(n, x, 1, y, 1);
}

DLLEXPORT void s_matrix_multiply(const enum TRANSPOSE transA, const enum TRANSPOSE transB, const int m, const int n, const int k, float alpha, float x[], float y[], float beta, float c[]){
	int lda = transA == CblasNoTrans ? m : k;
	int ldb = transB == CblasNoTrans ? k : n;
	char transAchar = getTransChar(transA);
	char transBchar = getTransChar(transB); 
	sgemm(transAchar, transBchar, m, n, k, alpha, x, lda, y, ldb, beta, c, m);
}

DLLEXPORT void d_matrix_multiply(const enum TRANSPOSE transA, const enum TRANSPOSE transB, const int m, const int n, const int k, double alpha, double x[], double y[], double beta, double c[]){
	int lda = transA == CblasNoTrans ? m : k;
	int ldb = transB == CblasNoTrans ? k : n;
	char transAchar = getTransChar(transA);
	char transBchar = getTransChar(transB); 
	dgemm(transAchar, transBchar, m, n, k, alpha, x, lda, y, ldb, beta, c, m);
}

DLLEXPORT void c_matrix_multiply(const enum TRANSPOSE transA, const enum TRANSPOSE transB, const int m, const int n, const int k, complex alpha, complex x[], complex y[], complex beta, complex c[]){
	int lda = transA == CblasNoTrans ? m : k;
	int ldb = transB == CblasNoTrans ? k : n;
	char transAchar = getTransChar(transA);
	char transBchar = getTransChar(transB); 
	cgemm(transAchar, transBchar, m, n, k, &alpha, x, lda, y, ldb, &beta, c, m);
}

DLLEXPORT void z_matrix_multiply(const enum TRANSPOSE transA, const enum TRANSPOSE transB, const int m, const int n, const int k, doublecomplex alpha, doublecomplex x[], doublecomplex y[], doublecomplex beta, doublecomplex c[]){
	int lda = transA == CblasNoTrans ? m : k;
	int ldb = transB == CblasNoTrans ? k : n;
	char transAchar = getTransChar(transA);
	char transBchar = getTransChar(transB); 
	zgemm(transAchar, transBchar, m, n, k, &alpha, x, lda, y, ldb, &beta, c, m);
}

char getTransChar(enum TRANSPOSE trans){
	char cTrans;
	switch( trans ){
		case  CblasNoTrans : cTrans = 'N';
			break;
		case  CblasTrans : cTrans = 'T';
			break;
		case  CblasConjTrans : cTrans = 'C';
			break;
	}
	return cTrans;
}
