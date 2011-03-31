#include "wrapper_common.h"
#include <stdlib.h>

typedef struct { float r, i; } complex;
typedef struct { double r, i; } doublecomplex;

enum TRANSPOSE {CblasNoTrans=111, CblasTrans=112, CblasConjTrans=113, CblasConjNoTrans=114};

void SAXPY(int*, float*, float *x, int* incx, float *y, int* incy);
void DAXPY(int*, double*, double *x, int* incx, double *y, int* incy);
void CAXPY(int*, complex*, complex *x, int* incx, complex *y, int* incy);
void ZAXPY(int*, doublecomplex*, doublecomplex *x, int* incx, doublecomplex *y, int* incy);

void SSCAL(int*, float* alpha, float*, int*);
void DSCAL(int*, double* alpha, double*, int*);
void CSCAL(int*, complex* alpha, complex*, int*);
void ZSCAL(int*, doublecomplex* alpha, doublecomplex*, int*);

float SDOT(int*, float*, int*, float*, int*);
double DDOT(int*, double*, int*, double*, int*);
complex CDOTU(int*, complex*, int*, complex*, int*);
doublecomplex ZDOTU(int*, doublecomplex*, int*, doublecomplex*, int*);

void SGEMM(char*, char*, int*, int*, int*, float*, float*, int*, float*, int*, float*, float*, int*);
void DGEMM(char*, char*, int*, int*, int*, double*, double*, int*, double*, int*, double*, double*, int*);
void CGEMM(char*, char*, int*, int*, int*, complex*, complex*, int*, complex*, int*, complex*, complex*, int*);
void ZGEMM(char*, char*, int*, int*, int*, doublecomplex*, doublecomplex*, int*, doublecomplex*, int*, doublecomplex*, doublecomplex*, int*);

char getTransChar(TRANSPOSE);


int one = 1;

DLLEXPORT void s_axpy(int n, float alpha, float x[], float y[]){
	
	SAXPY(&n, &alpha, x, &one, y, &one);
}

DLLEXPORT void d_axpy(int n, double alpha, double x[], double y[]){
	DAXPY(&n, &alpha, x, &one, y, &one);
}

DLLEXPORT void c_axpy(int n, complex alpha, complex x[], complex y[]){
	CAXPY(&n, &alpha, x, &one, y, &one);
}

DLLEXPORT void z_axpy(int n, doublecomplex alpha, doublecomplex x[], doublecomplex y[]){
	ZAXPY(&n, &alpha, x, &one, y, &one);
}

DLLEXPORT void s_scale(int n, float alpha, float x[]){
	SSCAL(&n, &alpha, x, &one);
}

DLLEXPORT void d_scale(int n, double alpha, double x[]){
	DSCAL(&n, &alpha, x, &one);
}

DLLEXPORT void c_scale(int n, complex alpha, complex x[]){
	CSCAL(&n, &alpha, x, &one);
}

DLLEXPORT void z_scale(int n, doublecomplex alpha, doublecomplex x[]){
	ZSCAL(&n, &alpha, x, &one);
}

DLLEXPORT float s_dot_product(int n, float x[], float y[]){
	return SDOT(&n, x, &one, y, &one);
}

DLLEXPORT double d_dot_product(int n, double x[], double y[]){
	return DDOT(&n, x, &one, y, &one);
}

DLLEXPORT complex c_dot_product(int n, complex x[], complex y[]){
	return CDOTU(&n, x, &one, y, &one);
}

DLLEXPORT doublecomplex z_dot_product(int n, doublecomplex x[], doublecomplex y[]){
	return ZDOTU(&n, x, &one, y, &one);
}

DLLEXPORT void s_matrix_multiply(enum TRANSPOSE transA, enum TRANSPOSE transB, int m, int n, int k, float alpha, float x[], float y[], float beta, float c[]){
	int lda = transA == CblasNoTrans ? m : k;
	int ldb = transB == CblasNoTrans ? k : n;
	char transAchar = getTransChar(transA);
	char transBchar = getTransChar(transB); 
	SGEMM(&transAchar, &transBchar, &m, &n, &k, &alpha, x, &lda, y, &ldb, &beta, c, &m);
}

DLLEXPORT void d_matrix_multiply(enum TRANSPOSE transA, enum TRANSPOSE transB, int m, int n, int k, double alpha, double x[], double y[], double beta, double c[]){
	int lda = transA == CblasNoTrans ? m : k;
	int ldb = transB == CblasNoTrans ? k : n;
	char transAchar = getTransChar(transA);
	char transBchar = getTransChar(transB); 
	DGEMM(&transAchar, &transBchar, &m, &n, &k, &alpha, x, &lda, y, &ldb, &beta, c, &m);
}

DLLEXPORT void c_matrix_multiply(enum TRANSPOSE transA, enum TRANSPOSE transB, int m, int n, int k, complex alpha, complex x[], complex y[], complex beta, complex c[]){
	int lda = transA == CblasNoTrans ? m : k;
	int ldb = transB == CblasNoTrans ? k : n;
	char transAchar = getTransChar(transA);
	char transBchar = getTransChar(transB); 
	CGEMM(&transAchar, &transBchar, &m, &n, &k, &alpha, x, &lda, y, &ldb, &beta, c, &m);
}

DLLEXPORT void z_matrix_multiply(enum TRANSPOSE transA, enum TRANSPOSE transB, int m, int n, int k, doublecomplex alpha, doublecomplex x[], doublecomplex y[], doublecomplex beta, doublecomplex c[]){
	int lda = transA == CblasNoTrans ? m : k;
	int ldb = transB == CblasNoTrans ? k : n;
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

