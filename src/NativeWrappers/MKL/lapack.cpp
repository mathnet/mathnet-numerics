#include "common.h"
#include "blas.h"
#include "mkl_lapack.h"

extern "C" {

	DLLEXPORT int s_cholesky_factor(int n, float a[]){
		char uplo = 'L';
		int info = 0;
		SPOTRF(&uplo, &n, a, &n, &info);
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
		DPOTRF(&uplo, &n, a, &n, &info);
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
		Complex8 zero;
		zero.real = 0.0;
		zero.real = 0.0;
		CPOTRF(&uplo, &n, a, &n, &info);
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
		Complex16 zero;
		zero.real = 0.0;
		zero.real = 0.0;
		ZPOTRF(&uplo, &n, a, &n, &info);
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

	DLLEXPORT float s_matrix_norm(char norm, int m, int n, float a[], float work[])
	{
		return SLANGE(&norm, &m, &n, a, &m, work);
	}

	DLLEXPORT double d_matrix_norm(char norm, int m, int n, double a[], double work[])
	{
		return DLANGE(&norm, &m, &n, a, &m, work);
	}
	
	DLLEXPORT float c_matrix_norm(char norm, int m, int n, MKL_Complex8 a[], float work[])
	{
		return CLANGE(&norm, &m, &n, a, &m, work);
	}
	
	DLLEXPORT double z_matrix_norm(char norm, int m, int n, MKL_Complex16 a[], double work[])
	{
		return ZLANGE(&norm, &m, &n, a, &m, work);
	}
}