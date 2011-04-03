#include "wrapper_common.h"
#include "f2c.h"

enum TRANSPOSE {CblasNoTrans=111, CblasTrans=112, CblasConjTrans=113, CblasConjNoTrans=114};

void SAXPY(integer*, float*, float *x, integer* incx, float *y, integer* incy);
void DAXPY(integer*, double*, double *x, integer* incx, double *y, integer* incy);
void CAXPY(integer*, complex*, complex *x, integer* incx, complex *y, integer* incy);
void ZAXPY(integer*, doublecomplex*, doublecomplex *x, integer* incx, doublecomplex *y, integer* incy);

void SSCAL(integer*, float* alpha, float*, integer*);
void DSCAL(integer*, double* alpha, double*, integer*);
void CSCAL(integer*, complex* alpha, complex*, integer*);
void ZSCAL(integer*, doublecomplex* alpha, doublecomplex*, integer*);

float SDOT(integer*, float*, integer*, float*, integer*);
double DDOT(integer*, double*, integer*, double*, integer*);
complex CDOTU(integer*, complex*, integer*, complex*, integer*);
doublecomplex ZDOTU(integer*, doublecomplex*, integer*, doublecomplex*, integer*);

void SGEMM(char*, char*, integer*, integer*, integer*, float*, float*, integer*, float*, integer*, float*, float*, integer*);
void DGEMM(char*, char*, integer*, integer*, integer*, double*, double*, integer*, double*, integer*, double*, double*, integer*);
void CGEMM(char*, char*, integer*, integer*, integer*, complex*, complex*, integer*, complex*, integer*, complex*, complex*, integer*);
void ZGEMM(char*, char*, integer*, integer*, integer*, doublecomplex*, doublecomplex*, integer*, doublecomplex*, integer*, doublecomplex*, doublecomplex*, integer*);

char getTransChar(TRANSPOSE);


integer one = 1;

DLLEXPORT void s_axpy(integer n, float alpha, float x[], float y[]){
	
	SAXPY(&n, &alpha, x, &one, y, &one);
}

DLLEXPORT void d_axpy(integer n, double alpha, double x[], double y[]){
	DAXPY(&n, &alpha, x, &one, y, &one);
}

DLLEXPORT void c_axpy(integer n, complex alpha, complex x[], complex y[]){
	CAXPY(&n, &alpha, x, &one, y, &one);
}

DLLEXPORT void z_axpy(integer n, doublecomplex alpha, doublecomplex x[], doublecomplex y[]){
	ZAXPY(&n, &alpha, x, &one, y, &one);
}

DLLEXPORT void s_scale(integer n, float alpha, float x[]){
	SSCAL(&n, &alpha, x, &one);
}

DLLEXPORT void d_scale(integer n, double alpha, double x[]){
	DSCAL(&n, &alpha, x, &one);
}

DLLEXPORT void c_scale(integer n, complex alpha, complex x[]){
	CSCAL(&n, &alpha, x, &one);
}

DLLEXPORT void z_scale(integer n, doublecomplex alpha, doublecomplex x[]){
	ZSCAL(&n, &alpha, x, &one);
}

DLLEXPORT float s_dot_product(integer n, float x[], float y[]){
	return SDOT(&n, x, &one, y, &one);
}

DLLEXPORT double d_dot_product(integer n, double x[], double y[]){
	return DDOT(&n, x, &one, y, &one);
}

DLLEXPORT complex c_dot_product(integer n, complex x[], complex y[]){
	return CDOTU(&n, x, &one, y, &one);
}

DLLEXPORT doublecomplex z_dot_product(integer n, doublecomplex x[], doublecomplex y[]){
	return ZDOTU(&n, x, &one, y, &one);
}

DLLEXPORT void s_matrix_multiply(enum TRANSPOSE transA, enum TRANSPOSE transB, integer m, integer n, integer k, float alpha, float x[], float y[], float beta, float c[]){
	integer lda = transA == CblasNoTrans ? m : k;
	integer ldb = transB == CblasNoTrans ? k : n;
	char transAchar = getTransChar(transA);
	char transBchar = getTransChar(transB); 
	SGEMM(&transAchar, &transBchar, &m, &n, &k, &alpha, x, &lda, y, &ldb, &beta, c, &m);
}

DLLEXPORT void d_matrix_multiply(enum TRANSPOSE transA, enum TRANSPOSE transB, integer m, integer n, integer k, double alpha, double x[], double y[], double beta, double c[]){
	integer lda = transA == CblasNoTrans ? m : k;
	integer ldb = transB == CblasNoTrans ? k : n;
	char transAchar = getTransChar(transA);
	char transBchar = getTransChar(transB); 
	DGEMM(&transAchar, &transBchar, &m, &n, &k, &alpha, x, &lda, y, &ldb, &beta, c, &m);
}

DLLEXPORT void c_matrix_multiply(enum TRANSPOSE transA, enum TRANSPOSE transB, integer m, integer n, integer k, complex alpha, complex x[], complex y[], complex beta, complex c[]){
	integer lda = transA == CblasNoTrans ? m : k;
	integer ldb = transB == CblasNoTrans ? k : n;
	char transAchar = getTransChar(transA);
	char transBchar = getTransChar(transB); 
	CGEMM(&transAchar, &transBchar, &m, &n, &k, &alpha, x, &lda, y, &ldb, &beta, c, &m);
}

DLLEXPORT void z_matrix_multiply(enum TRANSPOSE transA, enum TRANSPOSE transB, integer m, integer n, integer k, doublecomplex alpha, doublecomplex x[], doublecomplex y[], doublecomplex beta, doublecomplex c[]){
	integer lda = transA == CblasNoTrans ? m : k;
	integer ldb = transB == CblasNoTrans ? k : n;
	char transAchar = getTransChar(transA);
	char transBchar = getTransChar(transB); 
	ZGEMM(&transAchar, &transBchar, &m, &n, &k, &alpha, x, &lda, y, &ldb, &beta, c, &m);
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

