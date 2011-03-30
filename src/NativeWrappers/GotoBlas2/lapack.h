#ifndef LAPACK_H
#define LAPACK_H

extern "C"{
	#include "f2c.h"
	#include "clapack.h"
	
	enum CBLAS_ORDER     {CblasRowMajor=101, CblasColMajor=102};
	enum CBLAS_TRANSPOSE {CblasNoTrans=111, CblasTrans=112, CblasConjTrans=113, CblasConjNoTrans=114};
	enum CBLAS_UPLO      {CblasUpper=121, CblasLower=122};
	enum CBLAS_DIAG      {CblasNonUnit=131, CblasUnit=132};
	enum CBLAS_SIDE      {CblasLeft=141, CblasRight=142};

	float slange_(char*, int*, int*, float*, int*, float*);
	float dlange_(char*, int*, int*, double*, int*, double*);
	float clange_(char*, int*, int*, complex*, int*, float*);
	float zlange_(char*, int*, int*, doublecomplex*, int*, double*);

	void cblas_strsm(CBLAS_ORDER, CBLAS_SIDE, CBLAS_UPLO, CBLAS_TRANSPOSE, CBLAS_DIAG, int, int, float, float*, int, float*, int);
	void cblas_dtrsm(CBLAS_ORDER, CBLAS_SIDE, CBLAS_UPLO, CBLAS_TRANSPOSE, CBLAS_DIAG, int, int, double, double*, int, double*, int);
	void cblas_ctrsm(CBLAS_ORDER, CBLAS_SIDE, CBLAS_UPLO, CBLAS_TRANSPOSE, CBLAS_DIAG, int, int, complex*, complex*, int, complex*, int);
	void cblas_ztrsm(CBLAS_ORDER, CBLAS_SIDE, CBLAS_UPLO, CBLAS_TRANSPOSE, CBLAS_DIAG, int, int, doublecomplex*, doublecomplex*, int, doublecomplex*, int);

}

#endif 

