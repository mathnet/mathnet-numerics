#include "lapack.h"
#include "wrapper_common.h"
#include <algorithm>

extern "C"{
	DLLEXPORT float s_matrix_norm(char norm, int m, int n, float a[], float work[])
	{
		return slange_(&norm, &m, &n, a, &m, work);
	}

	DLLEXPORT double d_matrix_norm(char norm, int m, int n, double a[], double work[])
	{
		return dlange_(&norm, &m, &n, a, &m, work);
	}

	DLLEXPORT float c_matrix_norm(char norm, int m, int n, Complex8 a[], float work[])
	{
		return clange_(&norm, &m, &n, a, &m, work);
	}

	DLLEXPORT double z_matrix_norm(char norm, int m, int n, Complex16 a[], double work[])
	{
		return zlange_(&norm, &m, &n, a, &m, work);
	}

	DLLEXPORT int s_lu_factor(int m, float a[], int ipiv[])
	{
		int info = 0;
		sgetrf_(&m,&m,a,&m,ipiv,&info);
		for(int i = 0; i < m; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT int d_lu_factor(int m, double a[], int ipiv[])
	{
		int info = 0;
		dgetrf_(&m,&m,a,&m,ipiv,&info);
		for(int i = 0; i < m; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT int c_lu_factor(int m, Complex8 a[], int ipiv[])
	{
		int info = 0;
		cgetrf_(&m,&m,a,&m,ipiv,&info);
		for(int i = 0; i < m; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT int z_lu_factor(int m, Complex16 a[], int ipiv[])
	{
		int info = 0;
		zgetrf_(&m,&m,a,&m,ipiv,&info);
		for(int i = 0; i < m; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT int s_lu_inverse(int n, float a[], float work[], int lwork)
	{
		int* ipiv = new int[n];
		int info = 0;
		sgetrf_(&n,&n,a,&n,ipiv,&info);

		if (info != 0){
			delete[] ipiv;
			return info;
		}

		sgetri_(&n,a,&n,ipiv,work,&lwork,&info);
		delete[] ipiv;
		return info;
	}

	DLLEXPORT int d_lu_inverse(int n, double a[], double work[], int lwork)
	{
		int* ipiv = new int[n];
		int info = 0;
		dgetrf_(&n,&n,a,&n,ipiv,&info);

		if (info != 0){
			delete[] ipiv;
			return info;
		}

		dgetri_(&n,a,&n,ipiv,work,&lwork,&info);
		delete[] ipiv;
		return info;
	}

	DLLEXPORT int c_lu_inverse(int n, Complex8 a[], Complex8 work[], int lwork)
	{
		int* ipiv = new int[n];
		int info = 0;
		cgetrf_(&n,&n,a,&n,ipiv,&info);

		if (info != 0){
			delete[] ipiv;
			return info;
		}

		cgetri_(&n,a,&n,ipiv,work,&lwork,&info);
		delete[] ipiv;
		return info;
	}

	DLLEXPORT int z_lu_inverse(int n, Complex16 a[], Complex16 work[], int lwork)
	{
		int* ipiv = new int[n];
		int info = 0;
		zgetrf_(&n,&n,a,&n,ipiv,&info);

		if (info != 0){
			delete[] ipiv;
			return info;
		}

		zgetri_(&n,a,&n,ipiv,work,&lwork,&info);
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
		sgetri_(&n,a,&n,ipiv,work,&lwork,&info);

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
		dgetri_(&n,a,&n,ipiv,work,&lwork,&info);

		for(i = 0; i < n; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT int c_lu_inverse_factored(int n, Complex8 a[], int ipiv[], Complex8 work[], int lwork)
	{
		int i;
		for(i = 0; i < n; ++i ){
			ipiv[i] += 1;
		}

		int info = 0;
		cgetri_(&n,a,&n,ipiv,work,&lwork,&info);

		for(i = 0; i < n; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT int z_lu_inverse_factored(int n, Complex16 a[], int ipiv[], Complex16 work[], int lwork)
	{
		int i;
		for(i = 0; i < n; ++i ){
			ipiv[i] += 1;
		}

		int info = 0;
		zgetri_(&n,a,&n,ipiv,work,&lwork,&info);

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
		sgetrs_(&trans, &n, &nrhs, a, &n, ipiv, b, &n, &info);
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
		dgetrs_(&trans, &n, &nrhs, a, &n, ipiv, b, &n, &info);
		for(i = 0; i < n; ++i ){
			ipiv[i] -= 1;
		}
		return info;
	}

	DLLEXPORT int c_lu_solve_factored(int n, int nrhs, Complex8 a[], int ipiv[], Complex8 b[])
	{
		int info = 0;
		int i;    
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

	DLLEXPORT int z_lu_solve_factored(int n, int nrhs, Complex16 a[], int ipiv[], Complex16 b[])
	{
		int info = 0;
		int i;    
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

	DLLEXPORT int s_lu_solve(int n, int nrhs, float a[], float b[])
	{
		float* clone = new float[n*n];
		std::memcpy(clone, a, n*n*sizeof(float));

		int* ipiv = new int[n];
		int info = 0;
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

	DLLEXPORT int d_lu_solve(int n, int nrhs, double a[], double b[])
	{
		double* clone = new double[n*n];
		std::memcpy(clone, a, n*n*sizeof(double));

		int* ipiv = new int[n];
		int info = 0;
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

	DLLEXPORT int c_lu_solve(int n, int nrhs, Complex8 a[], Complex8 b[])
	{
		Complex8* clone = new Complex8[n*n];
		std::memcpy(clone, a, n*n*sizeof(Complex8));

		int* ipiv = new int[n];
		int info = 0;
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

	DLLEXPORT int z_lu_solve(int n, int nrhs, Complex16 a[],  Complex16 b[])
	{
		Complex16* clone = new Complex16[n*n];
		std::memcpy(clone, a, n*n*sizeof(Complex16));

		int* ipiv = new int[n];
		int info = 0;
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

	DLLEXPORT int s_cholesky_factor(int n, float a[]){
		char uplo = 'L';
		int info = 0;
		spotrf_(&uplo, &n, a, &n, &info);
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
		dpotrf_(&uplo, &n, a, &n, &info);
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

	DLLEXPORT int c_cholesky_factor(int n, Complex8 a[]){
		char uplo = 'L';
		int info = 0;
		Complex8 zero = {0.0f, 0.0f};
		cpotrf_(&uplo, &n, a, &n, &info);
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

	DLLEXPORT int z_cholesky_factor(int n, Complex16 a[]){
		char uplo = 'L';
		int info = 0;
		Complex16 zero = {0.0, 0.0};
		zpotrf_(&uplo, &n, a, &n, &info);
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
		spotrf_(&uplo, &n, clone, &n, &info);

		if (info != 0){
			delete[] clone;
			return info;
		}

		spotrs_(&uplo, &n, &nrhs, clone, &n, b, &n, &info);
		return info;
	}

	DLLEXPORT int d_cholesky_solve(int n, int nrhs, double a[], double b[])
	{
		double* clone = new double[n*n];
		std::memcpy(clone, a, n*n*sizeof(double));
		char uplo = 'L';
		int info = 0;
		dpotrf_(&uplo, &n, clone, &n, &info);

		if (info != 0){
			delete[] clone;
			return info;
		}

		dpotrs_(&uplo, &n, &nrhs, clone, &n, b, &n, &info);
		return info;
	}

	DLLEXPORT int c_cholesky_solve(int n, int nrhs, Complex8 a[], Complex8 b[])
	{
		Complex8* clone = new Complex8[n*n];
		std::memcpy(clone, a, n*n*sizeof(Complex8));
		char uplo = 'L';
		int info = 0;
		cpotrf_(&uplo, &n, clone, &n, &info);

		if (info != 0){
			delete[] clone;
			return info;
		}

		cpotrs_(&uplo, &n, &nrhs, clone, &n, b, &n, &info);
		return info;
	}

	DLLEXPORT int z_cholesky_solve(int n, int nrhs, Complex16 a[], Complex16 b[])
	{
		Complex16* clone = new Complex16[n*n];
		std::memcpy(clone, a, n*n*sizeof(Complex16));
		char uplo = 'L';
		int info = 0;
		zpotrf_(&uplo, &n, clone, &n, &info);

		if (info != 0){
			delete[] clone;
			return info;
		}

		zpotrs_(&uplo, &n, &nrhs, clone, &n, b, &n, &info);
		return info;
	}

	DLLEXPORT int s_cholesky_solve_factored(int n, int nrhs, float a[], float b[])
	{
		char uplo = 'L';
		int info = 0;
		spotrs_(&uplo, &n, &nrhs, a, &n, b, &n, &info);
		return info;
	}

	DLLEXPORT int d_cholesky_solve_factored(int n, int nrhs, double a[], double b[])
	{
		char uplo = 'L';
		int info = 0;
		dpotrs_(&uplo, &n, &nrhs, a, &n, b, &n, &info);
		return info;
	}

	DLLEXPORT int c_cholesky_solve_factored(int n, int nrhs, Complex8 a[], Complex8 b[])
	{
		char uplo = 'L';
		int info = 0;
		cpotrs_(&uplo, &n, &nrhs, a, &n, b, &n, &info);
		return info;
	}

	DLLEXPORT int z_cholesky_solve_factored(int n, int nrhs, Complex16 a[], Complex16 b[])
	{
		char uplo = 'L';
		int info = 0;
		zpotrs_(&uplo, &n, &nrhs, a, &n, b, &n, &info);
		return info;
	}

	DLLEXPORT int s_qr_factor(int m, int n, float r[], float tau[], float q[], float work[], int len)
	{
		int info = 0;
		sgeqrf_(&m, &n, r, &m, tau, work, &len, &info);

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
			sorgqr_(&m, &m, &m, q, &m, tau, work, &len, &info);
		}
		else
		{
			sorgqr_(&m, &n, &n, q, &m, tau, work, &len, &info);
		}

		return info;
	}

	DLLEXPORT int d_qr_factor(int m, int n, double r[], double tau[], double q[], double work[], int len)
	{
		int info = 0;
		dgeqrf_(&m, &n, r, &m, tau, work, &len, &info);

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
			dorgqr_(&m, &m, &m, q, &m, tau, work, &len, &info);
		}
		else
		{
			dorgqr_(&m, &n, &n, q, &m, tau, work, &len, &info);
		}

		return info;
	}

	DLLEXPORT int c_qr_factor(int m, int n, Complex8 r[], Complex8 tau[], Complex8 q[], Complex8 work[], int len)
	{
		int info = 0;
		cgeqrf_(&m, &n, r, &m, tau, work, &len, &info);

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
			cungqr_(&m, &m, &m, q, &m, tau, work, &len, &info);
		}
		else
		{
			cungqr_(&m, &n, &n, q, &m, tau, work, &len, &info);
		}

		return info;
	}

	DLLEXPORT int z_qr_factor(int m, int n, Complex16 r[], Complex16 tau[], Complex16 q[], Complex16 work[], int len)
	{
		int info = 0;
		zgeqrf_(&m, &n, r, &m, tau, work, &len, &info);

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
			zungqr_(&m, &m, &m, q, &m, tau, work, &len, &info);
		}
		else
		{
			zungqr_(&m, &n, &n, q, &m, tau, work, &len, &info);
		}

		return info;
	}

	DLLEXPORT int s_qr_solve(int m, int n, int bn, float r[], float b[], float x[], float work[], int len)
	{
		int info = 0;
		float* clone_r = new float[m*n];
		std::memcpy(clone_r, r, m*n*sizeof(float));

		float* tau = new float[std::max(1, std::min(m,n))];
		sgeqrf_(&m, &n, clone_r, &m, tau, work, &len, &info);

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
		sormqr_(&side, &tran, &m, &bn, &n, clone_r, &m, tau, clone_b, &m, work, &len, &info);
		cblas_strsm(CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, 1.0, clone_r, m, clone_b, m);
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
		dgeqrf_(&m, &n, clone_r, &m, tau, work, &len, &info);

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

		dormqr_(&side, &tran, &m, &bn, &n, clone_r, &m, tau, clone_b, &m, work, &len, &info);
		cblas_dtrsm(CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, 1.0, clone_r, m, clone_b, m);
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

	DLLEXPORT int c_qr_solve(int m, int n, int bn, Complex8 r[], Complex8 b[], Complex8 x[], Complex8 work[], int len)
	{
		int info = 0;
		Complex8* clone_r = new Complex8[m*n];
		std::memcpy(clone_r, r, m*n*sizeof(Complex8));

		Complex8* tau = new Complex8[std::min(m,n)];
		cgeqrf_(&m, &n, clone_r, &m, tau, work, &len, &info);

		if (info != 0)
		{
			delete[] clone_r;
			delete[] tau;
			return info;
		}

		char side ='L';
		char tran = 'C';

		Complex8* clone_b = new Complex8[m*bn];
		std::memcpy(clone_b, b, m*bn*sizeof(Complex8));

		cunmqr_(&side, &tran, &m, &bn, &n, clone_r, &m, tau, clone_b, &m, work, &len, &info);
		Complex8 one = {1.0, 0.0};
		cblas_ctrsm(CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, &one, clone_r, m, clone_b, m);

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

	DLLEXPORT int z_qr_solve(int m, int n, int bn, Complex16 r[], Complex16 b[], Complex16 x[], Complex16 work[], int len)
	{
		int info = 0;
		Complex16* clone_r = new Complex16[m*n];
		std::memcpy(clone_r, r, m*n*sizeof(Complex16));

		Complex16* tau = new Complex16[std::min(m,n)];
		zgeqrf_(&m, &n, clone_r, &m, tau, work, &len, &info);

		if (info != 0)
		{
			delete[] clone_r;
			delete[] tau;
			return info;
		}

		char side ='L';
		char tran = 'C';

		Complex16* clone_b = new Complex16[m*bn];
		std::memcpy(clone_b, b, m*bn*sizeof(Complex16));

		zunmqr_(&side, &tran, &m, &bn, &n, clone_r, &m, tau, clone_b, &m, work, &len, &info);
		Complex16 one = {1.0, 0.0};
		cblas_ztrsm(CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, &one, clone_r, m, clone_b, m);

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
		int info = 0;

		float* clone_b = new float[m*bn];
		std::memcpy(clone_b, b, m*bn*sizeof(float));

		sormqr_(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info);
		cblas_strsm(CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, 1.0, r, m, clone_b, m);
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
		int info = 0;

		double* clone_b = new double[m*bn];
		std::memcpy(clone_b, b, m*bn*sizeof(double));

		dormqr_(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info);
		cblas_dtrsm(CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, 1.0, r, m, clone_b, m);
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

	DLLEXPORT int c_qr_solve_factored(int m, int n, int bn, Complex8 r[], Complex8 b[], Complex8 tau[], Complex8 x[], Complex8 work[], int len)
	{
		char side ='L';
		char tran = 'C';
		int info = 0;

		Complex8* clone_b = new Complex8[m*bn];
		std::memcpy(clone_b, b, m*bn*sizeof(Complex8));

		cunmqr_(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info);
		Complex8 one = {1.0f, 0.0f};
		cblas_ctrsm(CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, &one, r, m, clone_b, m);
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

	DLLEXPORT int z_qr_solve_factored(int m, int n, int bn, Complex16 r[], Complex16 b[], Complex16 tau[], Complex16 x[], Complex16 work[], int len)
	{
		char side ='L';
		char tran = 'C';
		int info = 0;

		Complex16* clone_b = new Complex16[m*bn];
		std::memcpy(clone_b, b, m*bn*sizeof(Complex16));

		zunmqr_(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info);
		Complex16 one = {1.0, 0.0};
		cblas_ztrsm(CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, &one, r, m, clone_b, m);

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
		sgesvd_(&job, &job, &m, &n, a, &m, s, u, &m, v, &n, work, &len, &info);
		return info;
	}

	DLLEXPORT int d_svd_factor(bool compute_vectors, int m, int n, double a[], double s[], double u[], double v[], double work[], int len)
	{
		int info = 0;
		char job = compute_vectors ? 'A' : 'N';
		dgesvd_(&job, &job, &m, &n, a, &m, s, u, &m, v, &n, work, &len, &info);
		return info;
	}

	DLLEXPORT int c_svd_factor(bool compute_vectors, int m, int n, Complex8 a[], Complex8 s[], Complex8 u[], Complex8 v[], Complex8 work[], int len)
	{
		int info = 0;
		int dim_s = std::min(m,n);
		float* rwork = new float[5 * dim_s];
		float* s_local = new float[dim_s];
		char job = compute_vectors ? 'A' : 'N';
		cgesvd_(&job, &job, &m, &n, a, &m, s_local, u, &m, v, &n, work, &len, rwork, &info);

		for(int index = 0; index < dim_s; ++index){
			Complex8 value = {s_local[index], 0.0f};
			s[index] = value;
		}

		delete[] rwork;
		delete[] s_local;
		return info;
	}

	DLLEXPORT int z_svd_factor(bool compute_vectors, int m, int n, Complex16 a[], Complex16 s[], Complex16 u[], Complex16 v[], Complex16 work[], int len)
	{
		int info = 0;
		int dim_s = std::min(m,n);
		double* rwork = new double[5 * std::min(m, n)];
		double* s_local = new double[dim_s];
		char job = compute_vectors ? 'A' : 'N';
		zgesvd_(&job, &job, &m, &n, a, &m, s_local, u, &m, v, &n, work, &len, rwork, &info);

		for(int index = 0; index < dim_s; ++index){
			Complex16 value = {s_local[index], 0.0f};
			s[index] = value;
		}

		delete[] rwork;
		delete[] s_local;
		return info;
	}
}