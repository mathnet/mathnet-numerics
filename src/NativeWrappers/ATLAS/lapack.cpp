#include "common.h"
#include "blas.h"
#include "clapack.h"

extern "C" {

	DLLEXPORT int s_cholesky_factor(int n, float a[]){
		int info = clapack_spotrf(CblasColMajor, CblasLower, n, a, n);
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
		int info = clapack_dpotrf(CblasColMajor, CblasLower, n, a, n);
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
		int info = clapack_cpotrf(CblasColMajor, CblasLower, n, a, n);
		Complex8 zero;
		zero.real = 0.0;
		zero.real = 0.0;
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
		int info = clapack_zpotrf(CblasColMajor, CblasLower, n, a, n);
		Complex16 zero;
		zero.real = 0.0;
		zero.real = 0.0;
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
}