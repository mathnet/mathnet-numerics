#include "acml.h"
#include "wrapper_common.h"
#include <algorithm>

extern "C"{

	DLLEXPORT int s_lu_factor(int m, float a[], int ipiv[])
	{
		int info = 0;
		sgetrf(m, m, a, m,ipiv,&info);
		for(int i = 0; i < m; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT int d_lu_factor(int m, double a[], int ipiv[])
	{
		int info = 0;
		dgetrf(m, m,a, m, ipiv, &info);
		for(int i = 0; i < m; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT int c_lu_factor(int m, complex a[], int ipiv[])
	{
		int info = 0;
		cgetrf(m, m, a, m,ipiv, &info);
		for(int i = 0; i < m; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT int z_lu_factor(int m, doublecomplex a[], int ipiv[])
	{
		int info = 0;
		zgetrf(m, m, a, m, ipiv, &info);
		for(int i = 0; i < m; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT int s_lu_inverse(int n, float a[], float work[], int lwork)
	{
		int* ipiv = new int[n];
		int info = 0;
		sgetrf(n, n, a, n, ipiv, &info);

		if (info != 0){
			delete[] ipiv;
			return info;
		}

		SGETRI(&n, a, &n, ipiv, work, &lwork, &info);
		delete[] ipiv;
		return info;
	}

	DLLEXPORT int d_lu_inverse(int n, double a[], double work[], int lwork)
	{
		int* ipiv = new int[n];
		int info = 0;
		dgetrf(n, n, a, n, ipiv, &info);

		if (info != 0){
			delete[] ipiv;
			return info;
		}

		DGETRI(&n, a, &n, ipiv, work, &lwork, &info);
		delete[] ipiv;
		return info;
	}

	DLLEXPORT int c_lu_inverse(int n, complex a[], complex work[], int lwork)
	{
		int* ipiv = new int[n];
		int info = 0;
		cgetrf(n, n, a, n, ipiv, &info);

		if (info != 0){
			delete[] ipiv;
			return info;
		}

		CGETRI(&n, a, &n, ipiv, work, &lwork, &info);
		delete[] ipiv;
		return info;
	}

	DLLEXPORT int z_lu_inverse(int n, doublecomplex a[], doublecomplex work[], int lwork)
	{
		int* ipiv = new int[n];
		int info = 0;
		zgetrf(n, n, a, n, ipiv, &info);

		if (info != 0){
			delete[] ipiv;
			return info;
		}

		ZGETRI(&n, a, &n, ipiv, work, &lwork, &info);
		delete[] ipiv;
		return info;
	}

	DLLEXPORT int s_lu_inverse_factored(int n, float a[], int ipiv[], float work[], int lwork)
	{
		int i;
		for(i = 0; i < n; ++i ){
			ipiv[i] += 1;
		}
		int info = 0;
		SGETRI(&n, a, &n, ipiv, work, &lwork, &info);

		for(i = 0; i < n; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT int d_lu_inverse_factored(int n, double a[], int ipiv[], double work[], int lwork)
	{
		int i;
		for(i = 0; i < n; ++i ){
			ipiv[i] += 1;
		}

		int info = 0;
		DGETRI(&n, a, &n, ipiv, work, &lwork, &info);

		for(i = 0; i < n; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT int c_lu_inverse_factored(int n, complex a[], int ipiv[], complex work[], int lwork)
	{
		int i;
		for(i = 0; i < n; ++i ){
			ipiv[i] += 1;
		}

		int info = 0;
		CGETRI(&n, a, &n, ipiv, work, &lwork, &info);

		for(i = 0; i < n; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT int z_lu_inverse_factored(int n, doublecomplex a[], int ipiv[], doublecomplex work[], int lwork)
	{
		int i;
		for(i = 0; i < n; ++i ){
			ipiv[i] += 1;
		}

		int info = 0;
		ZGETRI(&n, a, &n, ipiv, work, &lwork, &info);

		for(i = 0; i < n; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT int s_lu_solve_factored(int n, int nrhs, float a[], int ipiv[], float b[])
	{
		int info = 0;
		int i;    
		for(i = 0; i < n; ++i ){
			ipiv[i] += 1;
		}

		char trans ='N';
		sgetrs(trans, n, nrhs, a, n, ipiv, b, n, &info);
		for(i = 0; i < n; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT int  d_lu_solve_factored(int n, int nrhs, double a[], int ipiv[], double b[])
	{
		int info = 0;
		int i;    
		for(i = 0; i < n; ++i ){
			ipiv[i] += 1;
		}

		char trans ='N';
		dgetrs(trans, n, nrhs, a, n, ipiv, b, n, &info);
		for(i = 0; i < n; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT int c_lu_solve_factored(int n, int nrhs, complex a[], int ipiv[], complex b[])
	{
		int info = 0;
		int i;    
		for(i = 0; i < n; ++i ){
			ipiv[i] += 1;
		}

		char trans ='N';
		cgetrs(trans, n, nrhs, a, n, ipiv, b, n, &info);
		for(i = 0; i < n; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT int z_lu_solve_factored(int n, int nrhs, doublecomplex a[], int ipiv[], doublecomplex b[])
	{
		int info = 0;
		int i;    
		for(i = 0; i < n; ++i ){
			ipiv[i] += 1;
		}

		char trans ='N';
		zgetrs(trans, n, nrhs, a, n, ipiv, b, n, &info);
		for(i = 0; i < n; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT int s_lu_solve(int n, int nrhs, float a[], float b[])
	{
		float* clone = new float[n*n];
		std::memcpy(clone, a, n*n*sizeof(float));

		int* ipiv = new int[n];
		int info = 0;
		sgetrf(n, n, clone, n, ipiv, &info);

		if (info != 0){
			delete[] ipiv;
			delete[] clone;
			return info;
		}

		char trans ='N';
		sgetrs(trans, n, nrhs, clone, n, ipiv, b, n, &info);
		delete[] ipiv;
		delete[] clone;
		return info;
	}

	DLLEXPORT int d_lu_solve(int n, int nrhs, double a[], double b[])
	{
		double* clone = new double[n*n];
		std::memcpy(clone, a, n*n*sizeof(double));

		int* ipiv = new int[n];
		int info = 0;
		dgetrf(n, n, clone, n, ipiv, &info);

		if (info != 0){
			delete[] ipiv;
			delete[] clone;
			return info;
		}

		char trans ='N';
		dgetrs(trans, n, nrhs, clone, n, ipiv, b, n, &info);
		delete[] ipiv;
		delete[] clone;
		return info;
	}

	DLLEXPORT int c_lu_solve(int n, int nrhs, complex a[], complex b[])
	{
		complex* clone = new complex[n*n];
		std::memcpy(clone, a, n*n*sizeof(complex));

		int* ipiv = new int[n];
		int info = 0;
		cgetrf(n, n, clone, n, ipiv, &info);

		if (info != 0){
			delete[] ipiv;
			delete[] clone;
			return info;
		}

		char trans ='N';
		cgetrs(trans, n, nrhs, clone, n, ipiv, b, n, &info);
		delete[] ipiv;
		delete[] clone;
		return info;
	}

	DLLEXPORT int z_lu_solve(int n, int nrhs, doublecomplex a[],  doublecomplex b[])
	{
		doublecomplex* clone = new doublecomplex[n*n];
		std::memcpy(clone, a, n*n*sizeof(doublecomplex));

		int* ipiv = new int[n];
		int info = 0;
		zgetrf(n, n, clone, n, ipiv, &info);

		if (info != 0){
			delete[] ipiv;
			delete[] clone;
			return info;
		}

		char trans ='N';
		zgetrs(trans, n, nrhs, clone, n, ipiv, b, n, &info);
		delete[] ipiv;
		delete[] clone;
		return info;
	}

	DLLEXPORT int s_cholesky_factor(int n, float a[]){
		char uplo = 'L';
		int info = 0;
		spotrf(uplo, n, a, n, &info);
		for (int i = 0; i < n; ++i)
		{
			int index = i * n;
			for (int j = 0; j < n && i > j; ++j)
			{
				a[index + j] = 0;
			}
		}
		return info;
	}

	DLLEXPORT int d_cholesky_factor(int n, double* a){
		char uplo = 'L';
		int info = 0;
		dpotrf(uplo, n, a, n, &info);
		for (int i = 0; i < n; ++i)
		{
			int index = i * n;
			for (int j = 0; j < n && i > j; ++j)
			{
				a[index + j] = 0;
			}
		}
		return info;
	}

	DLLEXPORT int c_cholesky_factor(int n, complex a[]){
		char uplo = 'L';
		int info = 0;
		complex zero = {0.0f, 0.0f};
		cpotrf(uplo, n, a, n, &info);
		for (int i = 0; i < n; ++i)
		{
			int index = i * n;
			for (int j = 0; j < n && i > j; ++j)
			{
				a[index + j] = zero;
			}
		}
		return info;
	}

	DLLEXPORT int z_cholesky_factor(int n, doublecomplex a[]){
		char uplo = 'L';
		int info = 0;
		doublecomplex zero = {0.0, 0.0};
		zpotrf(uplo, n, a, n, &info);
		for (int i = 0; i < n; ++i)
		{
			int index = i * n;
			for (int j = 0; j < n && i > j; ++j)
			{
				a[index + j] = zero;
			}
		}
		return info;
	}

	DLLEXPORT int s_cholesky_solve(int n, int nrhs, float a[], float b[])
	{
		float* clone = new float[n*n];
		std::memcpy(clone, a, n*n*sizeof(float));
		char uplo = 'L';
		int info = 0;
		spotrf(uplo, n, clone, n, &info);

		if (info != 0){
			delete[] clone;
			return info;
		}

		spotrs(uplo, n, nrhs, clone, n, b, n, &info);
		delete[] clone;
		return info;
	}

	DLLEXPORT int d_cholesky_solve(int n, int nrhs, double a[], double b[])
	{
		double* clone = new double[n*n];
		std::memcpy(clone, a, n*n*sizeof(double));
		char uplo = 'L';
		int info = 0;
		dpotrf(uplo, n, clone, n, &info);

		if (info != 0){
			delete[] clone;
			return info;
		}

		dpotrs(uplo, n, nrhs, clone, n, b, n, &info);
		delete[] clone;
		return info;
	}

	DLLEXPORT int c_cholesky_solve(int n, int nrhs, complex a[], complex b[])
	{
		complex* clone = new complex[n*n];
		std::memcpy(clone, a, n*n*sizeof(complex));
		char uplo = 'L';
		int info = 0;
		cpotrf(uplo, n, clone, n, &info);

		if (info != 0){
			delete[] clone;
			return info;
		}

		cpotrs(uplo, n, nrhs, clone, n, b, n, &info);
		delete[] clone;
		return info;
	}

	DLLEXPORT int z_cholesky_solve(int n, int nrhs, doublecomplex a[], doublecomplex b[])
	{
		doublecomplex* clone = new doublecomplex[n*n];
		std::memcpy(clone, a, n*n*sizeof(doublecomplex));
		char uplo = 'L';
		int info = 0;
		zpotrf(uplo, n, clone, n, &info);

		if (info != 0){
			delete[] clone;
			return info;
		}

		zpotrs(uplo, n, nrhs, clone, n, b, n, &info);
		delete[] clone;
		return info;
	}

	DLLEXPORT int s_cholesky_solve_factored(int n, int nrhs, float a[], float b[])
	{
		char uplo = 'L';
		int info = 0;
		spotrs(uplo, n, nrhs, a, n, b, n, &info);
		return info;
	}

	DLLEXPORT int d_cholesky_solve_factored(int n, int nrhs, double a[], double b[])
	{
		char uplo = 'L';
		int info = 0;
		dpotrs(uplo, n, nrhs, a, n, b, n, &info);
		return info;
	}

	DLLEXPORT int c_cholesky_solve_factored(int n, int nrhs, complex a[], complex b[])
	{
		char uplo = 'L';
		int info = 0;
		cpotrs(uplo, n, nrhs, a, n, b, n, &info);
		return info;
	}

	DLLEXPORT int z_cholesky_solve_factored(int n, int nrhs, doublecomplex a[], doublecomplex b[])
	{
		char uplo = 'L';
		int info = 0;
		zpotrs(uplo, n, nrhs, a, n, b, n, &info);
		return info;
	}

	DLLEXPORT int s_qr_factor(int m, int n, float r[], float tau[], float q[], float work[], int len)
	{
		int info = 0;
		SGEQRF(&m, &n, r, &m, tau, work, &len, &info);

		for (int i = 0; i < m; ++i)
		{
			for (int j = 0; j < m && j < n; ++j)
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
			SORGQR(&m, &m, &m, q, &m, tau, work, &len, &info);
		}
		else
		{
			SORGQR(&m, &n, &n, q, &m, tau, work, &len, &info);
		}

		return info;
	}

	DLLEXPORT int d_qr_factor(int m, int n, double r[], double tau[], double q[], double work[], int len)
	{
		int info = 0;
		DGEQRF(&m, &n, r, &m, tau, work, &len, &info);

		for (int i = 0; i < m; ++i)
		{
			for (int j = 0; j < m && j < n; ++j)
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
			DORGQR(&m, &m, &m, q, &m, tau, work, &len, &info);
		}
		else
		{
			DORGQR(&m, &n, &n, q, &m, tau, work, &len, &info);
		}

		return info;
	}

	DLLEXPORT int c_qr_factor(int m, int n, complex r[], complex tau[], complex q[], complex work[], int len)
	{
		int info = 0;
		CGEQRF(&m, &n, r, &m, tau, work, &len, &info);

		for (int i = 0; i < m; ++i)
		{
			for (int j = 0; j < m && j < n; ++j)
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
			CUNGQR(&m, &m, &m, q, &m, tau, work, &len, &info);
		}
		else
		{
			CUNGQR(&m, &n, &n, q, &m, tau, work, &len, &info);
		}

		return info;
	}

	DLLEXPORT int z_qr_factor(int m, int n, doublecomplex r[], doublecomplex tau[], doublecomplex q[], doublecomplex work[], int len)
	{
		int info = 0;
		ZGEQRF(&m, &n, r, &m, tau, work, &len, &info);

		for (int i = 0; i < m; ++i)
		{
			for (int j = 0; j < m && j < n; ++j)
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
			ZUNGQR(&m, &m, &m, q, &m, tau, work, &len, &info);
		}
		else
		{
			ZUNGQR(&m, &n, &n, q, &m, tau, work, &len, &info);
		}

		return info;
	}

	DLLEXPORT int s_qr_solve(int m, int n, int bn, float r[], float b[], float x[], float work[], int len)
	{
		int info = 0;
		float* clone_r = new float[m*n];
		std::memcpy(clone_r, r, m*n*sizeof(float));

		float* tau = new float[std::max(1, std::min(m,n))];
		SGEQRF(&m, &n, clone_r, &m, tau, work, &len, &info);

		if (info != 0)
		{
			delete[] clone_r;
			delete[] tau;
			return info;
		}

		float* clone_b = new float[m*bn];
		std::memcpy(clone_b, b, m*bn*sizeof(float));

		char side ='L';
		char tran = 'T';
		char upper = 'U';
		char not = 'N';
		SORMQR(&side, &tran, &m, &bn, &n, clone_r, &m, tau, clone_b, &m, work, &len, &info, 1, 1);
		strsm(side, upper, not, not, n, bn, 1.0, clone_r, m, clone_b, m);
		for (int i = 0; i < n; ++i)
		{
			for (int j = 0; j < bn; ++j)
			{
				x[j * n + i] = clone_b[j * m + i];
			}
		}

		delete[] clone_r;
		delete[] tau;
		delete[] clone_b;
		return info;
	}

	DLLEXPORT int d_qr_solve(int m, int n, int bn, double r[], double b[], double x[], double work[], int len)
	{
		int info = 0;
		double* clone_r = new double[m*n];
		std::memcpy(clone_r, r, m*n*sizeof(double));

		double* tau = new double[std::max(1, std::min(m,n))];
		DGEQRF(&m, &n, clone_r, &m, tau, work, &len, &info);

		if (info != 0)
		{
			delete[] clone_r;
			delete[] tau;
			return info;
		}

		double* clone_b = new double[m*bn];
		std::memcpy(clone_b, b, m*bn*sizeof(double));

		char side ='L';
		char tran = 'T';
		char upper = 'U';
		char not = 'N';

		DORMQR(&side, &tran, &m, &bn, &n, clone_r, &m, tau, clone_b, &m, work, &len, &info, 1, 1);
		dtrsm(side, upper, not, not, n, bn, 1.0, clone_r, m, clone_b, m);
		for (int i = 0; i < n; ++i)
		{
			for (int j = 0; j < bn; ++j)
			{
				x[j * n + i] = clone_b[j * m + i];
			}
		}

		delete[] clone_b;
		delete[] tau;
		delete[] clone_r;
		return info;
	}

	DLLEXPORT int c_qr_solve(int m, int n, int bn, complex r[], complex b[], complex x[], complex work[], int len)
	{
		int info = 0;
		complex* clone_r = new complex[m*n];
		std::memcpy(clone_r, r, m*n*sizeof(complex));

		complex* tau = new complex[std::min(m,n)];
		CGEQRF(&m, &n, clone_r, &m, tau, work, &len, &info);

		if (info != 0)
		{
			delete[] clone_r;
			delete[] tau;
			return info;
		}

		char side ='L';
		char tran = 'C';
		char upper = 'U';
		char not = 'N';

		complex* clone_b = new complex[m*bn];
		std::memcpy(clone_b, b, m*bn*sizeof(complex));

		CUNMQR(&side, &tran, &m, &bn, &n, clone_r, &m, tau, clone_b, &m, work, &len, &info, 1, 1);
		complex one = {1.0, 0.0};
		ctrsm(side, upper, not, not, n, bn, &one, clone_r, m, clone_b, m);

		for (int i = 0; i < n; ++i)
		{
			for (int j = 0; j < bn; ++j)
			{
				x[j * n + i] = clone_b[j * m + i];
			}
		}

		delete[] clone_r;
		delete[] tau;
		delete[] clone_b;
		return info;
	}

	DLLEXPORT int z_qr_solve(int m, int n, int bn, doublecomplex r[], doublecomplex b[], doublecomplex x[], doublecomplex work[], int len)
	{
		int info = 0;
		doublecomplex* clone_r = new doublecomplex[m*n];
		std::memcpy(clone_r, r, m*n*sizeof(doublecomplex));

		doublecomplex* tau = new doublecomplex[std::min(m,n)];
		ZGEQRF(&m, &n, clone_r, &m, tau, work, &len, &info);

		if (info != 0)
		{
			delete[] clone_r;
			delete[] tau;
			return info;
		}

		char side ='L';
		char tran = 'C';
		char upper = 'U';
		char not = 'N';

		doublecomplex* clone_b = new doublecomplex[m*bn];
		std::memcpy(clone_b, b, m*bn*sizeof(doublecomplex));

		ZUNMQR(&side, &tran, &m, &bn, &n, clone_r, &m, tau, clone_b, &m, work, &len, &info, 1, 1);
		doublecomplex one = {1.0, 0.0};
		ztrsm(side, upper, not, not, n, bn, &one, clone_r, m, clone_b, m);

		for (int i = 0; i < n; ++i)
		{
			for (int j = 0; j < bn; ++j)
			{
				x[j * n + i] = clone_b[j * m + i];
			}
		}

		delete[] clone_r;
		delete[] tau;
		delete[] clone_b;
		return info;
	}

	DLLEXPORT int s_qr_solve_factored(int m, int n, int bn, float r[], float b[], float tau[], float x[], float work[], int len)
	{
		char side ='L';
		char tran = 'T';
		char upper = 'U';
		char not = 'N';
		int info = 0;

		float* clone_b = new float[m*bn];
		std::memcpy(clone_b, b, m*bn*sizeof(float));

		SORMQR(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info, 1, 1);
		strsm(side, upper, not, not, n, bn, 1.0, r, m, clone_b, m);
		for (int i = 0; i < n; ++i)
		{
			for (int j = 0; j < bn; ++j)
			{
				x[j * n + i] = clone_b[j * m + i];
			}
		}

		delete[] clone_b;
		return info;
	}

	DLLEXPORT int d_qr_solve_factored(int m, int n, int bn, double r[], double b[], double tau[], double x[], double work[], int len)
	{
		char side ='L';
		char tran = 'T';
		char upper = 'U';
		char not = 'N';
		int info = 0;

		double* clone_b = new double[m*bn];
		std::memcpy(clone_b, b, m*bn*sizeof(double));

		DORMQR(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info, 1, 1);
		dtrsm(side, upper, not, not, n, bn, 1.0, r, m, clone_b, m);
		for (int i = 0; i < n; ++i)
		{
			for (int j = 0; j < bn; ++j)
			{
				x[j * n + i] = clone_b[j * m + i];
			}
		}

		delete[] clone_b;
		return info;
	}

	DLLEXPORT int c_qr_solve_factored(int m, int n, int bn, complex r[], complex b[], complex tau[], complex x[], complex work[], int len)
	{
		char side ='L';
		char tran = 'C';
		char upper = 'U';
		char not = 'N';
		int info = 0;

		complex* clone_b = new complex[m*bn];
		std::memcpy(clone_b, b, m*bn*sizeof(complex));

		CUNMQR(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info, 1, 1);
		complex one = {1.0f, 0.0f};
		ctrsm(side, upper, not, not, n, bn, &one, r, m, clone_b, m);
		for (int i = 0; i < n; ++i)
		{
			for (int j = 0; j < bn; ++j)
			{
				x[j * n + i] = clone_b[j * m + i];
			}
		}

		delete[] clone_b;
		return info;
	}

	DLLEXPORT int z_qr_solve_factored(int m, int n, int bn, doublecomplex r[], doublecomplex b[], doublecomplex tau[], doublecomplex x[], doublecomplex work[], int len)
	{
		char side ='L';
		char tran = 'C';
		char upper = 'U';
		char not = 'N';
		int info = 0;

		doublecomplex* clone_b = new doublecomplex[m*bn];
		std::memcpy(clone_b, b, m*bn*sizeof(doublecomplex));

		ZUNMQR(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info, 1, 1);
		doublecomplex one = {1.0, 0.0};
		ztrsm(side, upper, not, not, n, bn, &one, r, m, clone_b, m);

		for (int i = 0; i < n; ++i)
		{
			for (int j = 0; j < bn; ++j)
			{
				x[j * n + i] = clone_b[j * m + i];
			}
		}

		delete[] clone_b;
		return info;
	}

	DLLEXPORT int s_svd_factor(bool compute_vectors, int m, int n, float a[], float s[], float u[], float v[], float work[], int len)
	{
		int info = 0;
		char job = compute_vectors ? 'A' : 'N';
		SGESVD(&job, &job, &m, &n, a, &m, s, u, &m, v, &n, work, &len, &info, 1, 1);
		return info;
	}

	DLLEXPORT int d_svd_factor(bool compute_vectors, int m, int n, double a[], double s[], double u[], double v[], double work[], int len)
	{
		int info = 0;
		char job = compute_vectors ? 'A' : 'N';
		DGESVD(&job, &job, &m, &n, a, &m, s, u, &m, v, &n, work, &len, &info, 1, 1);
		return info;
	}

	DLLEXPORT int c_svd_factor(bool compute_vectors, int m, int n, complex a[], complex s[], complex u[], complex v[], complex work[], int len)
	{
		int info = 0;
		int dim_s = std::min(m,n);
		float* rwork = new float[5 * dim_s];
		float* s_local = new float[dim_s];
		char job = compute_vectors ? 'A' : 'N';
		CGESVD(&job, &job, &m, &n, a, &m, s_local, u, &m, v, &n, work, &len, rwork, &info, 1 ,1);

		for(int index = 0; index < dim_s; ++index){
			complex value = {s_local[index], 0.0f};
			s[index] = value;
		}

		delete[] rwork;
		delete[] s_local;
		return info;
	}

	DLLEXPORT int z_svd_factor(bool compute_vectors, int m, int n, doublecomplex a[], doublecomplex s[], doublecomplex u[], doublecomplex v[], doublecomplex work[], int len)
	{
		int info = 0;
		int dim_s = std::min(m,n);
		double* rwork = new double[5 * std::min(m, n)];
		double* s_local = new double[dim_s];
		char job = compute_vectors ? 'A' : 'N';
		ZGESVD(&job, &job, &m, &n, a, &m, s_local, u, &m, v, &n, work, &len, rwork, &info, 1, 1);

		for(int index = 0; index < dim_s; ++index){
			doublecomplex value = {s_local[index], 0.0f};
			s[index] = value;
		}

		delete[] rwork;
		delete[] s_local;
		return info;
	}
}