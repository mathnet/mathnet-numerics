#include "wrapper_common.h"
#include <algorithm>
#include "lapack.h"

extern "C"{
	void STRSM(char*, char*, char*, char*, integer*, integer*, float*, float*, integer*, float*, integer*);
	void DTRSM(char*, char*, char*, char*, integer*, integer*, double*, double*, integer*, double*, integer*);
	void CTRSM(char*, char*, char*, char*, integer*, integer*, complex*, complex*, integer*, complex*, integer*);
	void ZTRSM(char*, char*, char*, char*, integer*, integer*, doublecomplex*, doublecomplex*, integer*, doublecomplex*, integer*);


	DLLEXPORT float s_matrix_norm(char norm, integer m, integer n, float a[], float work[])
	{
		return slange_(&norm, &m, &n, a, &m, work);
	}

	DLLEXPORT double d_matrix_norm(char norm, integer m, integer n, double a[], double work[])
	{
		return dlange_(&norm, &m, &n, a, &m, work);
	}

	DLLEXPORT float c_matrix_norm(char norm, integer m, integer n, complex a[], float work[])
	{
		return clange_(&norm, &m, &n, a, &m, work);
	}

	DLLEXPORT double z_matrix_norm(char norm, integer m, integer n, doublecomplex a[], double work[])
	{
		return zlange_(&norm, &m, &n, a, &m, work);
	}

	DLLEXPORT integer s_lu_factor(integer m, float a[], integer ipiv[])
	{
		integer info = 0;
		sgetrf_(&m,&m,a,&m,ipiv,&info);
		for(integer i = 0; i < m; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT integer d_lu_factor(integer m, double a[], integer ipiv[])
	{
		integer info = 0;
		dgetrf_(&m,&m,a,&m,ipiv,&info);
		for(integer i = 0; i < m; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT integer c_lu_factor(integer m, complex a[], integer ipiv[])
	{
		integer info = 0;
		cgetrf_(&m,&m,a,&m,ipiv,&info);
		for(integer i = 0; i < m; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT integer z_lu_factor(integer m, doublecomplex a[], integer ipiv[])
	{
		integer info = 0;
		zgetrf_(&m,&m,a,&m,ipiv,&info);
		for(integer i = 0; i < m; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT integer s_lu_inverse(integer n, float a[], float work[], integer lwork)
	{
		integer* ipiv = new integer[n];
		integer info = 0;
		sgetrf_(&n,&n,a,&n,ipiv,&info);

		if (info != 0){
			delete[] ipiv;
			return info;
		}

		sgetri_(&n,a,&n,ipiv,work,&lwork,&info);
		delete[] ipiv;
		return info;
	}

	DLLEXPORT integer d_lu_inverse(integer n, double a[], double work[], integer lwork)
	{
		integer* ipiv = new integer[n];
		integer info = 0;
		dgetrf_(&n,&n,a,&n,ipiv,&info);

		if (info != 0){
			delete[] ipiv;
			return info;
		}

		dgetri_(&n,a,&n,ipiv,work,&lwork,&info);
		delete[] ipiv;
		return info;
	}

	DLLEXPORT integer c_lu_inverse(integer n, complex a[], complex work[], integer lwork)
	{
		integer* ipiv = new integer[n];
		integer info = 0;
		cgetrf_(&n,&n,a,&n,ipiv,&info);

		if (info != 0){
			delete[] ipiv;
			return info;
		}

		cgetri_(&n,a,&n,ipiv,work,&lwork,&info);
		delete[] ipiv;
		return info;
	}

	DLLEXPORT integer z_lu_inverse(integer n, doublecomplex a[], doublecomplex work[], integer lwork)
	{
		integer* ipiv = new integer[n];
		integer info = 0;
		zgetrf_(&n,&n,a,&n,ipiv,&info);

		if (info != 0){
			delete[] ipiv;
			return info;
		}

		zgetri_(&n,a,&n,ipiv,work,&lwork,&info);
		delete[] ipiv;
		return info;
	}

	DLLEXPORT integer s_lu_inverse_factored(integer n, float a[], integer ipiv[], float work[], integer lwork)
	{
		integer i;
		for(i = 0; i < n; ++i ){
			ipiv[i] += 1;
		}
		integer info = 0;
		sgetri_(&n,a,&n,ipiv,work,&lwork,&info);

		for(i = 0; i < n; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT integer d_lu_inverse_factored(integer n, double a[], integer ipiv[], double work[], integer lwork)
	{
		integer i;
		for(i = 0; i < n; ++i ){
			ipiv[i] += 1;
		}

		integer info = 0;
		dgetri_(&n,a,&n,ipiv,work,&lwork,&info);

		for(i = 0; i < n; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT integer c_lu_inverse_factored(integer n, complex a[], integer ipiv[], complex work[], integer lwork)
	{
		integer i;
		for(i = 0; i < n; ++i ){
			ipiv[i] += 1;
		}

		integer info = 0;
		cgetri_(&n,a,&n,ipiv,work,&lwork,&info);

		for(i = 0; i < n; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT integer z_lu_inverse_factored(integer n, doublecomplex a[], integer ipiv[], doublecomplex work[], integer lwork)
	{
		integer i;
		for(i = 0; i < n; ++i ){
			ipiv[i] += 1;
		}

		integer info = 0;
		zgetri_(&n,a,&n,ipiv,work,&lwork,&info);

		for(i = 0; i < n; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT integer s_lu_solve_factored(integer n, integer nrhs, float a[], integer ipiv[], float b[])
	{
		integer info = 0;
		integer i;    
		for(i = 0; i < n; ++i ){
			ipiv[i] += 1;
		}

		char trans ='N';
		sgetrs_(&trans, &n, &nrhs, a, &n, ipiv, b, &n, &info);
		for(i = 0; i < n; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT integer  d_lu_solve_factored(integer n, integer nrhs, double a[], integer ipiv[], double b[])
	{
		integer info = 0;
		integer i;    
		for(i = 0; i < n; ++i ){
			ipiv[i] += 1;
		}

		char trans ='N';
		dgetrs_(&trans, &n, &nrhs, a, &n, ipiv, b, &n, &info);
		for(i = 0; i < n; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT integer c_lu_solve_factored(integer n, integer nrhs, complex a[], integer ipiv[], complex b[])
	{
		integer info = 0;
		integer i;    
		for(i = 0; i < n; ++i ){
			ipiv[i] += 1;
		}

		char trans ='N';
		cgetrs_(&trans, &n, &nrhs, a, &n, ipiv, b, &n, &info);
		for(i = 0; i < n; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT integer z_lu_solve_factored(integer n, integer nrhs, doublecomplex a[], integer ipiv[], doublecomplex b[])
	{
		integer info = 0;
		integer i;    
		for(i = 0; i < n; ++i ){
			ipiv[i] += 1;
		}

		char trans ='N';
		zgetrs_(&trans, &n, &nrhs, a, &n, ipiv, b, &n, &info);
		for(i = 0; i < n; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT integer s_lu_solve(integer n, integer nrhs, float a[], float b[])
	{
		float* clone = new float[n*n];
		memcpy(clone, a, n*n*sizeof(float));

		integer* ipiv = new integer[n];
		integer info = 0;
		sgetrf_(&n, &n, clone, &n, ipiv, &info);

		if (info != 0){
			delete[] ipiv;
			delete[] clone;
			return info;
		}

		char trans ='N';
		sgetrs_(&trans, &n, &nrhs, clone, &n, ipiv, b, &n, &info);
		delete[] ipiv;
		delete[] clone;
		return info;
	}

	DLLEXPORT integer d_lu_solve(integer n, integer nrhs, double a[], double b[])
	{
		double* clone = new double[n*n];
		memcpy(clone, a, n*n*sizeof(double));

		integer* ipiv = new integer[n];
		integer info = 0;
		dgetrf_(&n, &n, clone, &n, ipiv, &info);

		if (info != 0){
			delete[] ipiv;
			delete[] clone;
			return info;
		}

		char trans ='N';
		dgetrs_(&trans, &n, &nrhs, clone, &n, ipiv, b, &n, &info);
		delete[] ipiv;
		delete[] clone;
		return info;
	}

	DLLEXPORT integer c_lu_solve(integer n, integer nrhs, complex a[], complex b[])
	{
		complex* clone = new complex[n*n];
		memcpy(clone, a, n*n*sizeof(complex));

		integer* ipiv = new integer[n];
		integer info = 0;
		cgetrf_(&n, &n, clone, &n, ipiv, &info);

		if (info != 0){
			delete[] ipiv;
			delete[] clone;
			return info;
		}

		char trans ='N';
		cgetrs_(&trans, &n, &nrhs, clone, &n, ipiv, b, &n, &info);
		delete[] ipiv;
		delete[] clone;
		return info;
	}

	DLLEXPORT integer z_lu_solve(integer n, integer nrhs, doublecomplex a[],  doublecomplex b[])
	{
		doublecomplex* clone = new doublecomplex[n*n];
		memcpy(clone, a, n*n*sizeof(doublecomplex));

		integer* ipiv = new integer[n];
		integer info = 0;
		zgetrf_(&n, &n, clone, &n, ipiv, &info);

		if (info != 0){
			delete[] ipiv;
			delete[] clone;
			return info;
		}

		char trans ='N';
		zgetrs_(&trans, &n, &nrhs, clone, &n, ipiv, b, &n, &info);
		delete[] ipiv;
		delete[] clone;
		return info;
	}

	DLLEXPORT integer s_cholesky_factor(integer n, float a[]){
		char uplo = 'L';
		integer info = 0;
		spotrf_(&uplo, &n, a, &n, &info);
		for (integer i = 0; i < n; ++i)
		{
			integer index = i * n;
			for (integer j = 0; j < n && i > j; ++j)
			{
				a[index + j] = 0;
			}
		}
		return info;
	}

	DLLEXPORT integer d_cholesky_factor(integer n, double* a){
		char uplo = 'L';
		integer info = 0;
		dpotrf_(&uplo, &n, a, &n, &info);
		for (integer i = 0; i < n; ++i)
		{
			integer index = i * n;
			for (integer j = 0; j < n && i > j; ++j)
			{
				a[index + j] = 0;
			}
		}
		return info;
	}

	DLLEXPORT integer c_cholesky_factor(integer n, complex a[]){
		char uplo = 'L';
		integer info = 0;
		complex zero = {0.0f, 0.0f};
		cpotrf_(&uplo, &n, a, &n, &info);
		for (integer i = 0; i < n; ++i)
		{
			integer index = i * n;
			for (integer j = 0; j < n && i > j; ++j)
			{
				a[index + j] = zero;
			}
		}
		return info;
	}

	DLLEXPORT integer z_cholesky_factor(integer n, doublecomplex a[]){
		char uplo = 'L';
		integer info = 0;
		doublecomplex zero = {0.0, 0.0};
		zpotrf_(&uplo, &n, a, &n, &info);
		for (integer i = 0; i < n; ++i)
		{
			integer index = i * n;
			for (integer j = 0; j < n && i > j; ++j)
			{
				a[index + j] = zero;
			}
		}
		return info;
	}

	DLLEXPORT integer s_cholesky_solve(integer n, integer nrhs, float a[], float b[])
	{
		float* clone = new float[n*n];
		memcpy(clone, a, n*n*sizeof(float));
		char uplo = 'L';
		integer info = 0;
		spotrf_(&uplo, &n, clone, &n, &info);

		if (info != 0){
			delete[] clone;
			return info;
		}

		spotrs_(&uplo, &n, &nrhs, clone, &n, b, &n, &info);
		delete[] clone;
		return info;
	}

	DLLEXPORT integer d_cholesky_solve(integer n, integer nrhs, double a[], double b[])
	{
		double* clone = new double[n*n];
		memcpy(clone, a, n*n*sizeof(double));
		char uplo = 'L';
		integer info = 0;
		dpotrf_(&uplo, &n, clone, &n, &info);

		if (info != 0){
			delete[] clone;
			return info;
		}

		dpotrs_(&uplo, &n, &nrhs, clone, &n, b, &n, &info);
		delete[] clone;
		return info;
	}

	DLLEXPORT integer c_cholesky_solve(integer n, integer nrhs, complex a[], complex b[])
	{
		complex* clone = new complex[n*n];
		memcpy(clone, a, n*n*sizeof(complex));
		char uplo = 'L';
		integer info = 0;
		cpotrf_(&uplo, &n, clone, &n, &info);

		if (info != 0){
			delete[] clone;
			return info;
		}

		cpotrs_(&uplo, &n, &nrhs, clone, &n, b, &n, &info);
		delete[] clone;
		return info;
	}

	DLLEXPORT integer z_cholesky_solve(integer n, integer nrhs, doublecomplex a[], doublecomplex b[])
	{
		doublecomplex* clone = new doublecomplex[n*n];
		memcpy(clone, a, n*n*sizeof(doublecomplex));
		char uplo = 'L';
		integer info = 0;
		zpotrf_(&uplo, &n, clone, &n, &info);

		if (info != 0){
			delete[] clone;
			return info;
		}

		zpotrs_(&uplo, &n, &nrhs, clone, &n, b, &n, &info);
		delete[] clone;
		return info;
	}

	DLLEXPORT integer s_cholesky_solve_factored(integer n, integer nrhs, float a[], float b[])
	{
		char uplo = 'L';
		integer info = 0;
		spotrs_(&uplo, &n, &nrhs, a, &n, b, &n, &info);
		return info;
	}

	DLLEXPORT integer d_cholesky_solve_factored(integer n, integer nrhs, double a[], double b[])
	{
		char uplo = 'L';
		integer info = 0;
		dpotrs_(&uplo, &n, &nrhs, a, &n, b, &n, &info);
		return info;
	}

	DLLEXPORT integer c_cholesky_solve_factored(integer n, integer nrhs, complex a[], complex b[])
	{
		char uplo = 'L';
		integer info = 0;
		cpotrs_(&uplo, &n, &nrhs, a, &n, b, &n, &info);
		return info;
	}

	DLLEXPORT integer z_cholesky_solve_factored(integer n, integer nrhs, doublecomplex a[], doublecomplex b[])
	{
		char uplo = 'L';
		integer info = 0;
		zpotrs_(&uplo, &n, &nrhs, a, &n, b, &n, &info);
		return info;
	}

	DLLEXPORT integer s_qr_factor(integer m, integer n, float r[], float tau[], float q[], float work[], integer len)
	{
		integer info = 0;
		sgeqrf_(&m, &n, r, &m, tau, work, &len, &info);

		for (integer i = 0; i < m; ++i)
		{
			for (integer j = 0; j < m && j < n; ++j)
			{
				if (i > j)
				{
					q[j * m + i] = r[j * m + i];
				}
			}
		}

		//compute the q elements explicitly
		if (m <= n)
		{
			sorgqr_(&m, &m, &m, q, &m, tau, work, &len, &info);
		}
		else
		{
			sorgqr_(&m, &n, &n, q, &m, tau, work, &len, &info);
		}

		return info;
	}

	DLLEXPORT integer d_qr_factor(integer m, integer n, double r[], double tau[], double q[], double work[], integer len)
	{
		integer info = 0;
		dgeqrf_(&m, &n, r, &m, tau, work, &len, &info);

		for (integer i = 0; i < m; ++i)
		{
			for (integer j = 0; j < m && j < n; ++j)
			{
				if (i > j)
				{
					q[j * m + i] = r[j * m + i];
				}
			}
		}

		//compute the q elements explicitly
		if (m <= n)
		{
			dorgqr_(&m, &m, &m, q, &m, tau, work, &len, &info);
		}
		else
		{
			dorgqr_(&m, &n, &n, q, &m, tau, work, &len, &info);
		}

		return info;
	}

	DLLEXPORT integer c_qr_factor(integer m, integer n, complex r[], complex tau[], complex q[], complex work[], integer len)
	{
		integer info = 0;
		cgeqrf_(&m, &n, r, &m, tau, work, &len, &info);

		for (integer i = 0; i < m; ++i)
		{
			for (integer j = 0; j < m && j < n; ++j)
			{
				if (i > j)
				{
					q[j * m + i] = r[j * m + i];
				}
			}
		}

		//compute the q elements explicitly
		if (m <= n)
		{
			cungqr_(&m, &m, &m, q, &m, tau, work, &len, &info);
		}
		else
		{
			cungqr_(&m, &n, &n, q, &m, tau, work, &len, &info);
		}

		return info;
	}

	DLLEXPORT integer z_qr_factor(integer m, integer n, doublecomplex r[], doublecomplex tau[], doublecomplex q[], doublecomplex work[], integer len)
	{
		integer info = 0;
		zgeqrf_(&m, &n, r, &m, tau, work, &len, &info);

		for (integer i = 0; i < m; ++i)
		{
			for (integer j = 0; j < m && j < n; ++j)
			{
				if (i > j)
				{
					q[j * m + i] = r[j * m + i];
				}
			}
		}

		//compute the q elements explicitly
		if (m <= n)
		{
			zungqr_(&m, &m, &m, q, &m, tau, work, &len, &info);
		}
		else
		{
			zungqr_(&m, &n, &n, q, &m, tau, work, &len, &info);
		}

		return info;
	}

	DLLEXPORT integer s_qr_solve(integer m, integer n, integer bn, float r[], float b[], float x[], float work[], integer len)
	{
		integer info = 0;
		float* clone_r = new float[m*n];
		memcpy(clone_r, r, m*n*sizeof(float));

		float* tau = new float[max(1, min(m,n))];
		sgeqrf_(&m, &n, clone_r, &m, tau, work, &len, &info);

		if (info != 0)
		{
			delete[] clone_r;
			delete[] tau;
			return info;
		}

		float* clone_b = new float[m*bn];
		memcpy(clone_b, b, m*bn*sizeof(float));

		char side ='L';
		char tran = 'T';
		char upper = 'U';
		char no = 'N';
		float one = 1.f;
		sormqr_(&side, &tran, &m, &bn, &n, clone_r, &m, tau, clone_b, &m, work, &len, &info);
		STRSM(&side, &upper, &no, &no, &n, &bn, &one, clone_r, &m, clone_b, &m);
		for (integer i = 0; i < n; ++i)
		{
			for (integer j = 0; j < bn; ++j)
			{
				x[j * n + i] = clone_b[j * m + i];
			}
		}

		delete[] clone_r;
		delete[] tau;
		delete[] clone_b;
		return info;
	}

	DLLEXPORT integer d_qr_solve(integer m, integer n, integer bn, double r[], double b[], double x[], double work[], integer len)
	{
		integer info = 0;
		double* clone_r = new double[m*n];
		memcpy(clone_r, r, m*n*sizeof(double));

		double* tau = new double[max(1, min(m,n))];
		dgeqrf_(&m, &n, clone_r, &m, tau, work, &len, &info);

		if (info != 0)
		{
			delete[] clone_r;
			delete[] tau;
			return info;
		}

		double* clone_b = new double[m*bn];
		memcpy(clone_b, b, m*bn*sizeof(double));

		char side ='L';
		char tran = 'T';
		char upper = 'U';
		char no = 'N';
		double one = 1.;

		dormqr_(&side, &tran, &m, &bn, &n, clone_r, &m, tau, clone_b, &m, work, &len, &info);
		DTRSM(&side, &upper, &no, &no, &n, &bn, &one, clone_r, &m, clone_b, &m);
		for (integer i = 0; i < n; ++i)
		{
			for (integer j = 0; j < bn; ++j)
			{
				x[j * n + i] = clone_b[j * m + i];
			}
		}

		delete[] clone_b;
		delete[] tau;
		delete[] clone_r;
		return info;
	}

	DLLEXPORT integer c_qr_solve(integer m, integer n, integer bn, complex r[], complex b[], complex x[], complex work[], integer len)
	{
		integer info = 0;
		complex* clone_r = new complex[m*n];
		memcpy(clone_r, r, m*n*sizeof(complex));

		complex* tau = new complex[min(m,n)];
		cgeqrf_(&m, &n, clone_r, &m, tau, work, &len, &info);

		if (info != 0)
		{
			delete[] clone_r;
			delete[] tau;
			return info;
		}

		char side ='L';
		char tran = 'C';
		char upper = 'U';
		char no = 'N';
		complex* clone_b = new complex[m*bn];
		memcpy(clone_b, b, m*bn*sizeof(complex));

		cunmqr_(&side, &tran, &m, &bn, &n, clone_r, &m, tau, clone_b, &m, work, &len, &info);
		complex one = {1.0, 0.0};
		CTRSM(&side, &upper, &no, &no, &n, &bn, &one, clone_r, &m, clone_b, &m);

		for (integer i = 0; i < n; ++i)
		{
			for (integer j = 0; j < bn; ++j)
			{
				x[j * n + i] = clone_b[j * m + i];
			}
		}

		delete[] clone_r;
		delete[] tau;
		delete[] clone_b;
		return info;
	}

	DLLEXPORT integer z_qr_solve(integer m, integer n, integer bn, doublecomplex r[], doublecomplex b[], doublecomplex x[], doublecomplex work[], integer len)
	{
		integer info = 0;
		doublecomplex* clone_r = new doublecomplex[m*n];
		memcpy(clone_r, r, m*n*sizeof(doublecomplex));

		doublecomplex* tau = new doublecomplex[min(m,n)];
		zgeqrf_(&m, &n, clone_r, &m, tau, work, &len, &info);

		if (info != 0)
		{
			delete[] clone_r;
			delete[] tau;
			return info;
		}

		char side ='L';
		char tran = 'C';
		char upper = 'U';
		char no = 'N';
		doublecomplex* clone_b = new doublecomplex[m*bn];
		memcpy(clone_b, b, m*bn*sizeof(doublecomplex));

		zunmqr_(&side, &tran, &m, &bn, &n, clone_r, &m, tau, clone_b, &m, work, &len, &info);
		doublecomplex one = {1.0, 0.0};
		ZTRSM(&side, &upper, &no, &no, &n, &bn, &one, clone_r, &m, clone_b, &m);

		for (integer i = 0; i < n; ++i)
		{
			for (integer j = 0; j < bn; ++j)
			{
				x[j * n + i] = clone_b[j * m + i];
			}
		}

		delete[] clone_r;
		delete[] tau;
		delete[] clone_b;
		return info;
	}

	DLLEXPORT integer s_qr_solve_factored(integer m, integer n, integer bn, float r[], float b[], float tau[], float x[], float work[], integer len)
	{
		char side ='L';
		char tran = 'T';
		integer info = 0;
		char upper = 'U';
		char no = 'N';
		float one = 1.f;

		float* clone_b = new float[m*bn];
		memcpy(clone_b, b, m*bn*sizeof(float));

		sormqr_(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info);
		STRSM(&side, &upper, &no, &no, &n, &bn, &one, r, &m, clone_b, &m);
		for (integer i = 0; i < n; ++i)
		{
			for (integer j = 0; j < bn; ++j)
			{
				x[j * n + i] = clone_b[j * m + i];
			}
		}

		delete[] clone_b;
		return info;
	}

	DLLEXPORT integer d_qr_solve_factored(integer m, integer n, integer bn, double r[], double b[], double tau[], double x[], double work[], integer len)
	{
		char side ='L';
		char tran = 'T';
		integer info = 0;
		char upper = 'U';
		char no = 'N';
		double one = 1.;

		double* clone_b = new double[m*bn];
		memcpy(clone_b, b, m*bn*sizeof(double));

		dormqr_(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info);
		DTRSM(&side, &upper, &no, &no, &n, &bn, &one, r, &m, clone_b, &m);
		for (integer i = 0; i < n; ++i)
		{
			for (integer j = 0; j < bn; ++j)
			{
				x[j * n + i] = clone_b[j * m + i];
			}
		}

		delete[] clone_b;
		return info;
	}

	DLLEXPORT integer c_qr_solve_factored(integer m, integer n, integer bn, complex r[], complex b[], complex tau[], complex x[], complex work[], integer len)
	{
		char side ='L';
		char tran = 'C';
		integer info = 0;
		char upper = 'U';
		char no = 'N';

		complex* clone_b = new complex[m*bn];
		memcpy(clone_b, b, m*bn*sizeof(complex));

		cunmqr_(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info);
		complex one = {1.0f, 0.0f};
		CTRSM(&side, &upper, &no, &no, &n, &bn, &one, r, &m, clone_b, &m);
		for (integer i = 0; i < n; ++i)
		{
			for (integer j = 0; j < bn; ++j)
			{
				x[j * n + i] = clone_b[j * m + i];
			}
		}

		delete[] clone_b;
		return info;
	}

	DLLEXPORT integer z_qr_solve_factored(integer m, integer n, integer bn, doublecomplex r[], doublecomplex b[], doublecomplex tau[], doublecomplex x[], doublecomplex work[], integer len)
	{
		char side ='L';
		char tran = 'C';
		integer info = 0;
		char upper = 'U';
		char no = 'N';

		doublecomplex* clone_b = new doublecomplex[m*bn];
		memcpy(clone_b, b, m*bn*sizeof(doublecomplex));

		zunmqr_(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info);
		doublecomplex one = {1.0, 0.0};
		ZTRSM(&side, &upper, &no, &no, &n, &bn, &one, r, &m, clone_b, &m);

		for (integer i = 0; i < n; ++i)
		{
			for (integer j = 0; j < bn; ++j)
			{
				x[j * n + i] = clone_b[j * m + i];
			}
		}

		delete[] clone_b;
		return info;
	}

	DLLEXPORT integer s_svd_factor(bool compute_vectors, integer m, integer n, float a[], float s[], float u[], float v[], float work[], integer len)
	{
		integer info = 0;
		char job = compute_vectors ? 'A' : 'N';
		sgesvd_(&job, &job, &m, &n, a, &m, s, u, &m, v, &n, work, &len, &info);
		return info;
	}

	DLLEXPORT integer d_svd_factor(bool compute_vectors, integer m, integer n, double a[], double s[], double u[], double v[], double work[], integer len)
	{
		integer info = 0;
		char job = compute_vectors ? 'A' : 'N';
		dgesvd_(&job, &job, &m, &n, a, &m, s, u, &m, v, &n, work, &len, &info);
		return info;
	}

	DLLEXPORT integer c_svd_factor(bool compute_vectors, integer m, integer n, complex a[], complex s[], complex u[], complex v[], complex work[], integer len)
	{
		integer info = 0;
		integer dim_s = min(m,n);
		float* rwork = new float[5 * dim_s];
		float* s_local = new float[dim_s];
		char job = compute_vectors ? 'A' : 'N';
		cgesvd_(&job, &job, &m, &n, a, &m, s_local, u, &m, v, &n, work, &len, rwork, &info);

		for(integer index = 0; index < dim_s; ++index){
			complex value = {s_local[index], 0.0f};
			s[index] = value;
		}

		delete[] rwork;
		delete[] s_local;
		return info;
	}

	DLLEXPORT integer z_svd_factor(bool compute_vectors, integer m, integer n, doublecomplex a[], doublecomplex s[], doublecomplex u[], doublecomplex v[], doublecomplex work[], integer len)
	{
		integer info = 0;
		integer dim_s = min(m,n);
		double* rwork = new double[5 * min(m, n)];
		double* s_local = new double[dim_s];
		char job = compute_vectors ? 'A' : 'N';
		zgesvd_(&job, &job, &m, &n, a, &m, s_local, u, &m, v, &n, work, &len, rwork, &info);

		for(integer index = 0; index < dim_s; ++index){
			doublecomplex value = {s_local[index], 0.0f};
			s[index] = value;
		}

		delete[] rwork;
		delete[] s_local;
		return info;
	}
}