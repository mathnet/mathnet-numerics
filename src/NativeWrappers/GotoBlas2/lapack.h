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

	float slange_(char*, integer*, integer*, float*, integer*, float*);
	float dlange_(char*, integer*, integer*, double*, integer*, double*);
	float clange_(char*, integer*, integer*, complex*, integer*, float*);
	float zlange_(char*, integer*, integer*, doublecomplex*, integer*, double*);

	void cblas_strsm(CBLAS_ORDER, CBLAS_SIDE, CBLAS_UPLO, CBLAS_TRANSPOSE, CBLAS_DIAG, integer, integer, float, float*, integer, float*, integer);
	void cblas_dtrsm(CBLAS_ORDER, CBLAS_SIDE, CBLAS_UPLO, CBLAS_TRANSPOSE, CBLAS_DIAG, integer, integer, double, double*, integer, double*, integer);
	void cblas_ctrsm(CBLAS_ORDER, CBLAS_SIDE, CBLAS_UPLO, CBLAS_TRANSPOSE, CBLAS_DIAG, integer, integer, complex*, complex*, integer, complex*, integer);
	void cblas_ztrsm(CBLAS_ORDER, CBLAS_SIDE, CBLAS_UPLO, CBLAS_TRANSPOSE, CBLAS_DIAG, integer, integer, doublecomplex*, doublecomplex*, integer, doublecomplex*, integer);

}

#endif 

