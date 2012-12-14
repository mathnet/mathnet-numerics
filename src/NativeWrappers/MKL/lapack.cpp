﻿#include "mkl_lapack.h"
#include "mkl_cblas.h"
#include "wrapper_common.h"
#include <algorithm>

template<typename T> 
inline void copyBtoX (MKL_INT m, MKL_INT n, MKL_INT bn, T b[], T x[]){
	for (MKL_INT i = 0; i < n; ++i)
	{
		for (MKL_INT j = 0; j < bn; ++j)
		{
			x[j * n + i] = clone_b[j * m + i];
		}
	}
};

inline void shift_ipiv_down(MKL_INT m, MKL_INT ipiv[]){
	for(MKL_INT i = 0; i < m; ++i ){
		ipiv[i] -= 1;
	}
}

inline void shift_ipiv_up(MKL_INT m, MKL_INT ipiv[]){
	for(MKL_INT i = 0; i < m; ++i ){
		ipiv[i] += 1;
	}
}

template<typename T> 
inline MKL_INT lu_factor(MKL_INT m, T a[], MKL_INT ipiv[], 
						 void (*getrf) (const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*, MKL_INT*))
{
	MKL_INT info = 0;
	getrf(&m, &m, a, &m, ipiv, &info);
	shift_ipiv_down(m, ipiv);
	return info;
};

template<typename T> 
inline MKL_INT lu_inverse(MKL_INT n, T a[], T work[], MKL_INT lwork, 
						  void (*getrf) (const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*, MKL_INT*),
						  void (*getri) (const MKL_INT*, T*, const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*))
{
	MKL_INT* ipiv = new MKL_INT[n];
	MKL_INT info = 0;
	getrf(&n, &n, a, &n, ipiv, &info);

	if (info != 0){
		delete[] ipiv;
		return info;
	}

	getri(&n, a, &n, ipiv, work, &lwork, &info);
	delete[] ipiv;
	return info;
};

template<typename T> 
inline MKL_INT lu_inverse_factored(MKL_INT n, T a[], MKL_INT ipiv[], T work[], MKL_INT lwork,
								   void (*getri) (const MKL_INT*, T*, const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*))
{
	shift_ipiv_up(n, ipiv);
	MKL_INT info = 0;
	getri(&n, a, &n, ipiv, work, &lwork, &info);
	shift_ipiv_down(n, ipiv);
	return info;
}

template<typename T> 
inline MKL_INT lu_solve_factored(MKL_INT n, MKL_INT nrhs, T a[], MKL_INT ipiv[], T b[],
									void (*getrs) (const char*, const MKL_INT*, const MKL_INT*, const T*, const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*))
{
	shift_ipiv_up(n, ipiv);
	MKL_INT info = 0;
	char trans ='N';
	getrs(&trans, &n, &nrhs, a, &n, ipiv, b, &n, &info);
	shift_ipiv_down(n, ipiv);
	return info;
}

template<typename T> 
inline MKL_INT lu_solve(MKL_INT n, MKL_INT nrhs, T a[], T b[], 
						  void (*getrf) (const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*, MKL_INT*),
						  void (*getrs) (const char*, const MKL_INT*, const MKL_INT*, const T*, const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*))
{
	T* clone = new T[n*n];
	std::memcpy(clone, a, n*n*sizeof(T));

	MKL_INT* ipiv = new MKL_INT[n];
	MKL_INT info = 0;
	getrf(&n, &n, clone, &n, ipiv, &info);

	if (info != 0){
		delete[] ipiv;
		delete[] clone;
		return info;
	}

	char trans ='N';
	getrs(&trans, &n, &nrhs, clone, &n, ipiv, b, &n, &info);
	delete[] ipiv;
	delete[] clone;
	return info;
}


template<typename T> 
inline MKL_INT cholesky_factor(MKL_INT n, T* a,
								  void (*potrf) (const char*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*))
{
	char uplo = 'L';
	MKL_INT info = 0;
	potrf(&uplo, &n, a, &n, &info);
	T zero = T();
	for (MKL_INT i = 0; i < n; ++i)
	{
		MKL_INT index = i * n;
		for (MKL_INT j = 0; j < n && i > j; ++j)
		{
			a[index + j] = zero;
		}
	}
	return info;
}

template<typename T> 
inline MKL_INT cholesky_solve(MKL_INT n, MKL_INT nrhs, T a[], T b[],
							  void (*potrf) (const char*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*),
							  void (*potrs) (const char*, const MKL_INT*, const MKL_INT*, const T*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*))
{
	T* clone = new T[n*n];
	std::memcpy(clone, a, n*n*sizeof(T));
	char uplo = 'L';
	MKL_INT info = 0;
	potrf(&uplo, &n, clone, &n, &info);

	if (info != 0){
		delete[] clone;
		return info;
	}

	potrs(&uplo, &n, &nrhs, clone, &n, b, &n, &info);
	delete[] clone;
	return info;
}

template<typename T> 
inline MKL_INT cholesky_solve_factored(MKL_INT n, MKL_INT nrhs, T a[], T b[],
									   void (*potrs) (const char*, const MKL_INT*, const MKL_INT*, const T*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*))
{
	char uplo = 'L';
	MKL_INT info = 0;
	potrs(&uplo, &n, &nrhs, a, &n, b, &n, &info);
	return info;
}

template<typename T> 
inline MKL_INT qr_factor(MKL_INT m, MKL_INT n, T r[], T tau[], T q[], T work[], MKL_INT len,
						 void (*geqrf) (const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, T*, T*, const MKL_INT*, MKL_INT*),
						 void (*orgqr) (const MKL_INT*, const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, const T*, T*, const MKL_INT*, MKL_INT*))
{
	MKL_INT info = 0;
	geqrf(&m, &n, r, &m, tau, work, &len, &info);

	for (MKL_INT i = 0; i < m; ++i)
	{
		for (MKL_INT j = 0; j < m && j < n; ++j)
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
		orgqr(&m, &m, &m, q, &m, tau, work, &len, &info);
	}
	else
	{
		orgqr(&m, &m, &n, q, &m, tau, work, &len, &info);
	}

	return info;
}

template<typename T> 
inline MKL_INT qr_thin_factor(MKL_INT m, MKL_INT n, T q[], T tau[], T r[], T work[], MKL_INT len,
						      void (*geqrf) (const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, T*, T*, const MKL_INT*, MKL_INT*),
						      void (*orgqr) (const MKL_INT*, const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, const T*, T*, const MKL_INT*, MKL_INT*))
{
	MKL_INT info = 0;
	geqrf(&m, &n, q, &m, tau, work, &len, &info);

	for (MKL_INT i = 0; i < n; ++i)
	{
		for (MKL_INT j = 0; j < n; ++j)
		{
			if( i <= j) {
				r[j * n + i] = q[j * m + i];
			}
		}
	}

	orgqr(&m, &n, &n, q, &m, tau, work, &len, &info);

	return info;
}

template<typename T> 
inline MKL_INT qr_solve(MKL_INT m, MKL_INT n, MKL_INT bn, T a[], T b[], T x[], T work[], MKL_INT len,
						void (*gels) (const char*, const MKL_INT*, const MKL_INT*, const MKL_INT*, T*, 
						const MKL_INT*, T* b, const MKL_INT*, T*, const MKL_INT*, MKL_INT*))
{
	T* clone_a = new T[m*n];
	std::memcpy(clone_a, a, m*n*sizeof(T));

	T* clone_b = new T[m*bn];
	std::memcpy(clone_b, b, m*bn*sizeof(T));

	char N = 'N';
	MKL_INT info = 0;
	gels(&N, &m, &n, &bn, clone_a, &m, clone_b, &m, work, &len, &info);

	for (MKL_INT i = 0; i < n; ++i)
	{
		for (MKL_INT j = 0; j < bn; ++j)
		{
			x[j * n + i] = clone_b[j * m + i];
		}
	}

	delete[] clone_a;
	delete[] clone_b;
	return info;
}

// combine the next two some how
// the problem is that complex trsm takes void* instead of MKL_COMPLEX
template<typename T> 
inline MKL_INT qr_solve_factored(MKL_INT m, MKL_INT n, MKL_INT bn, T r[], T b[], T tau[], T x[], T work[], MKL_INT len,
								   void (*ormqr) (const char*, const char*, const MKL_INT*, const MKL_INT*, const MKL_INT*, 
								                  const T*, const MKL_INT*, const T*, T*, const MKL_INT*, T*, const MKL_INT*, MKL_INT* info),
								   void (*trsm) (const CBLAS_ORDER, const CBLAS_SIDE, const CBLAS_UPLO, const CBLAS_TRANSPOSE, const CBLAS_DIAG, 
								                 const MKL_INT, const MKL_INT, const T, const T*, const MKL_INT, T*, const MKL_INT)) 
{
	T* clone_b = new T[m*bn];
	std::memcpy(clone_b, b, m*bn*sizeof(T));

	char side ='L';
	char tran = 'T';
	MKL_INT info = 0;
	ormqr(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info);
	trsm(CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, 1.0, r, m, clone_b, m);
	for (MKL_INT i = 0; i < n; ++i)
	{
		for (MKL_INT j = 0; j < bn; ++j)
		{
			x[j * n + i] = clone_b[j * m + i];
		}
	}

	delete[] clone_b;
	return info;
}

template<typename T> 
inline MKL_INT complex_qr_solve_factored(MKL_INT m, MKL_INT n, MKL_INT bn, T r[], T b[], T tau[], T x[], T work[], MKL_INT len,
								   void (*unmqr) (const char*, const char*, const MKL_INT*, const MKL_INT*, const MKL_INT*, 
								                  const T*, const MKL_INT*, const T*, T*, const MKL_INT*, T*, const MKL_INT*, MKL_INT* info),
								   void (*trsm) (const CBLAS_ORDER, const CBLAS_SIDE, const CBLAS_UPLO, const CBLAS_TRANSPOSE, const CBLAS_DIAG, 
								                 const MKL_INT, const MKL_INT, const void*, const void*, const MKL_INT, void*, const MKL_INT ldb)) 
{
	T* clone_b = new T[m*bn];
	std::memcpy(clone_b, b, m*bn*sizeof(T));

	char side ='L';
	char tran = 'C';
	MKL_INT info = 0;
	unmqr(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info);

	T one = {1.0f, 0.0f};
	trsm(CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, &one, r, m, clone_b, m);
	for (MKL_INT i = 0; i < n; ++i)
	{
		for (MKL_INT j = 0; j < bn; ++j)
		{
			x[j * n + i] = clone_b[j * m + i];
		}
	}

	delete[] clone_b;
	return info;
}

template<typename T> 
inline MKL_INT svd_factor(bool compute_vectors, MKL_INT m, MKL_INT n, T a[], T s[], T u[], T v[], T work[], MKL_INT len,
								  void (*gesvd) (const char*, const char*, const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, 
								                 T*, T*, const MKL_INT*, T*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*))
{
	MKL_INT info = 0;
	char job = compute_vectors ? 'A' : 'N';
	gesvd(&job, &job, &m, &n, a, &m, s, u, &m, v, &n, work, &len, &info);
	return info;
}


template<typename T, typename R> 
inline MKL_INT complex_svd_factor(bool compute_vectors, MKL_INT m, MKL_INT n, T a[], T s[], T u[], T v[], T work[], MKL_INT len,
								  void (*gesvd) (const char*, const char*, const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, 
								                 R*, T*, const MKL_INT*, T*, const MKL_INT*, T*, const MKL_INT*, R*, MKL_INT*))
{
	MKL_INT info = 0;
	MKL_INT dim_s = std::min(m,n);
	R* rwork = new R[5 * dim_s];
	R* s_local = new R[dim_s];
	char job = compute_vectors ? 'A' : 'N';
	gesvd(&job, &job, &m, &n, a, &m, s_local, u, &m, v, &n, work, &len, rwork, &info);

	for(MKL_INT index = 0; index < dim_s; ++index){
		T value = {s_local[index], 0.0f};
		s[index] = value;
	}

	delete[] rwork;
	delete[] s_local;
	return info;
}

extern "C" {

	DLLEXPORT float s_matrix_norm(char norm, MKL_INT m, MKL_INT n, float a[], float work[])
	{
		return slange(&norm, &m, &n, a, &m, work);
	}

	DLLEXPORT double d_matrix_norm(char norm, MKL_INT m, MKL_INT n, double a[], double work[])
	{
		return dlange(&norm, &m, &n, a, &m, work);
	}

	DLLEXPORT float c_matrix_norm(char norm, MKL_INT m, MKL_INT n, MKL_Complex8 a[], float work[])
	{
		return clange(&norm, &m, &n, a, &m, work);
	}

	DLLEXPORT double z_matrix_norm(char norm, MKL_INT m, MKL_INT n, MKL_Complex16 a[], double work[])
	{
		return zlange(&norm, &m, &n, a, &m, work);
	}

	DLLEXPORT MKL_INT s_lu_factor(MKL_INT m, float a[], MKL_INT ipiv[]) {
		return lu_factor<float>(m, a, ipiv, sgetrf);
	}
	
	DLLEXPORT MKL_INT d_lu_factor(MKL_INT m, double a[], MKL_INT ipiv[]) {
		return lu_factor<double>(m, a, ipiv, dgetrf);
	}
	
	DLLEXPORT MKL_INT c_lu_factor(MKL_INT m, MKL_Complex8 a[], MKL_INT ipiv[]) {
		return lu_factor<MKL_Complex8>(m, a, ipiv, cgetrf);
	}
	
	DLLEXPORT MKL_INT z_lu_factor(MKL_INT m, MKL_Complex16 a[], MKL_INT ipiv[]) {
		return lu_factor<MKL_Complex16>(m, a, ipiv, zgetrf);
	}

	DLLEXPORT MKL_INT s_lu_inverse(MKL_INT n, float a[], float work[], MKL_INT lwork)
	{
		return lu_inverse<float>(n, a, work, lwork, sgetrf, sgetri);
	}

	DLLEXPORT MKL_INT d_lu_inverse(MKL_INT n, double a[], double work[], MKL_INT lwork)
	{
		return lu_inverse<double>(n, a, work, lwork, dgetrf, dgetri);
	}

	DLLEXPORT MKL_INT c_lu_inverse(MKL_INT n, MKL_Complex8 a[], MKL_Complex8 work[], MKL_INT lwork)
	{
		return lu_inverse<MKL_Complex8>(n, a, work, lwork, cgetrf, cgetri);
	}

	DLLEXPORT MKL_INT z_lu_inverse(MKL_INT n, MKL_Complex16 a[], MKL_Complex16 work[], MKL_INT lwork)
	{
		return lu_inverse<MKL_Complex16>(n, a, work, lwork, zgetrf, zgetri);
	}

	DLLEXPORT MKL_INT s_lu_inverse_factored(MKL_INT n, float a[], MKL_INT ipiv[], float work[], MKL_INT lwork)
	{
		return lu_inverse_factored<float>(n, a, ipiv, work, lwork, sgetri);
	}

	DLLEXPORT MKL_INT d_lu_inverse_factored(MKL_INT n, double a[], MKL_INT ipiv[], double work[], MKL_INT lwork)
	{
		return lu_inverse_factored<double>(n, a, ipiv, work, lwork, dgetri);
	}

	DLLEXPORT MKL_INT c_lu_inverse_factored(MKL_INT n, MKL_Complex8 a[], MKL_INT ipiv[], MKL_Complex8 work[], MKL_INT lwork)
	{
		return lu_inverse_factored<MKL_Complex8>(n, a, ipiv, work, lwork, cgetri);
	}

	DLLEXPORT MKL_INT z_lu_inverse_factored(MKL_INT n, MKL_Complex16 a[], MKL_INT ipiv[], MKL_Complex16 work[], MKL_INT lwork)
	{
		return lu_inverse_factored<MKL_Complex16>(n, a, ipiv, work, lwork, zgetri);
	}

	DLLEXPORT MKL_INT s_lu_solve_factored(MKL_INT n, MKL_INT nrhs, float a[], MKL_INT ipiv[], float b[])
	{
		return lu_solve_factored<float>(n, nrhs, a, ipiv, b, sgetrs);
	}

	DLLEXPORT MKL_INT  d_lu_solve_factored(MKL_INT n, MKL_INT nrhs, double a[], MKL_INT ipiv[], double b[])
	{
		return lu_solve_factored<double>(n, nrhs, a, ipiv, b, dgetrs);
	}

	DLLEXPORT MKL_INT c_lu_solve_factored(MKL_INT n, MKL_INT nrhs, MKL_Complex8 a[], MKL_INT ipiv[], MKL_Complex8 b[])
	{
		return lu_solve_factored<MKL_Complex8>(n, nrhs, a, ipiv, b, cgetrs);
	}

	DLLEXPORT MKL_INT z_lu_solve_factored(MKL_INT n, MKL_INT nrhs, MKL_Complex16 a[], MKL_INT ipiv[], MKL_Complex16 b[])
	{
		return lu_solve_factored<MKL_Complex16>(n, nrhs, a, ipiv, b, zgetrs);
	}

	DLLEXPORT MKL_INT s_lu_solve(MKL_INT n, MKL_INT nrhs, float a[], float b[])
	{
		return lu_solve<float>(n, nrhs, a, b, sgetrf, sgetrs);
	}

	DLLEXPORT MKL_INT d_lu_solve(MKL_INT n, MKL_INT nrhs, double a[], double b[])
	{
		return lu_solve<double>(n, nrhs, a, b, dgetrf, dgetrs);
	}

	DLLEXPORT MKL_INT c_lu_solve(MKL_INT n, MKL_INT nrhs, MKL_Complex8 a[], MKL_Complex8 b[])
	{
		return lu_solve<MKL_Complex8>(n, nrhs, a, b, cgetrf, cgetrs);
	}

	DLLEXPORT MKL_INT z_lu_solve(MKL_INT n, MKL_INT nrhs, MKL_Complex16 a[],  MKL_Complex16 b[])
	{
		return lu_solve<MKL_Complex16>(n, nrhs, a, b, zgetrf, zgetrs);
	}

	DLLEXPORT MKL_INT s_cholesky_factor(MKL_INT n, float a[]){
		return cholesky_factor<float>(n, a, spotrf);
	}

	DLLEXPORT MKL_INT d_cholesky_factor(MKL_INT n, double* a){
		return cholesky_factor<double>(n, a, dpotrf);
	}

	DLLEXPORT MKL_INT c_cholesky_factor(MKL_INT n, MKL_Complex8 a[]){
		return cholesky_factor<MKL_Complex8>(n, a, cpotrf);
	}

	DLLEXPORT MKL_INT z_cholesky_factor(MKL_INT n, MKL_Complex16 a[]){
		return cholesky_factor<MKL_Complex16>(n, a, zpotrf);
	}

	DLLEXPORT MKL_INT s_cholesky_solve(MKL_INT n, MKL_INT nrhs, float a[], float b[])
	{
		return cholesky_solve<float>(n, nrhs, a, b, spotrf, spotrs);
	}

	DLLEXPORT MKL_INT d_cholesky_solve(MKL_INT n, MKL_INT nrhs, double a[], double b[])
	{
		return cholesky_solve<double>(n, nrhs, a, b, dpotrf, dpotrs);
	}

	DLLEXPORT MKL_INT c_cholesky_solve(MKL_INT n, MKL_INT nrhs, MKL_Complex8 a[], MKL_Complex8 b[])
	{
		return cholesky_solve<MKL_Complex8>(n, nrhs, a, b, cpotrf, cpotrs);
	}

	DLLEXPORT MKL_INT z_cholesky_solve(MKL_INT n, MKL_INT nrhs, MKL_Complex16 a[], MKL_Complex16 b[])
	{
		return cholesky_solve<MKL_Complex16>(n, nrhs, a, b, zpotrf, zpotrs);
	}

	DLLEXPORT MKL_INT s_cholesky_solve_factored(MKL_INT n, MKL_INT nrhs, float a[], float b[])
	{
		return cholesky_solve_factored<float>(n, nrhs, a, b, spotrs);
	}

	DLLEXPORT MKL_INT d_cholesky_solve_factored(MKL_INT n, MKL_INT nrhs, double a[], double b[])
	{
		return cholesky_solve_factored<double>(n, nrhs, a, b, dpotrs);
	}

	DLLEXPORT MKL_INT c_cholesky_solve_factored(MKL_INT n, MKL_INT nrhs, MKL_Complex8 a[], MKL_Complex8 b[])
	{
		return cholesky_solve_factored<MKL_Complex8>(n, nrhs, a, b, cpotrs);
	}

	DLLEXPORT MKL_INT z_cholesky_solve_factored(MKL_INT n, MKL_INT nrhs, MKL_Complex16 a[], MKL_Complex16 b[])
	{
		return cholesky_solve_factored<MKL_Complex16>(n, nrhs, a, b, zpotrs);
	}

	DLLEXPORT MKL_INT s_qr_factor(MKL_INT m, MKL_INT n, float r[], float tau[], float q[], float work[], MKL_INT len)
	{
		return qr_factor<float>(m, n, r, tau, q, work, len, sgeqrf, sorgqr);
	}

	DLLEXPORT MKL_INT s_qr_thin_factor(MKL_INT m, MKL_INT n, float q[], float tau[], float r[], float work[], MKL_INT len)
	{
		return qr_thin_factor<float>(m, n, q, tau, r, work, len, sgeqrf, sorgqr);
	}

	DLLEXPORT MKL_INT d_qr_factor(MKL_INT m, MKL_INT n, double r[], double tau[], double q[], double work[], MKL_INT len)
	{
		return qr_factor<double>(m, n, r, tau, q, work, len, dgeqrf, dorgqr);
	}

	DLLEXPORT MKL_INT d_qr_thin_factor(MKL_INT m, MKL_INT n, double q[], double tau[], double r[], double work[], MKL_INT len)
	{
		return qr_thin_factor<double>(m, n, q, tau, r, work, len, dgeqrf, dorgqr);
	}

	DLLEXPORT MKL_INT c_qr_factor(MKL_INT m, MKL_INT n, MKL_Complex8 r[], MKL_Complex8 tau[], MKL_Complex8 q[], MKL_Complex8 work[], MKL_INT len)
	{
		return qr_factor<MKL_Complex8>(m, n, r, tau, q, work, len, cgeqrf, cungqr);
	}

	DLLEXPORT MKL_INT c_qr_thin_factor(MKL_INT m, MKL_INT n, MKL_Complex8 q[], MKL_Complex8 tau[], MKL_Complex8 r[], MKL_Complex8 work[], MKL_INT len)
	{
		return qr_thin_factor<MKL_Complex8>(m, n, q, tau, r, work, len, cgeqrf, cungqr);
	}

	DLLEXPORT MKL_INT z_qr_factor(MKL_INT m, MKL_INT n, MKL_Complex16 r[], MKL_Complex16 tau[], MKL_Complex16 q[], MKL_Complex16 work[], MKL_INT len)
	{
		return qr_factor<MKL_Complex16>(m, n, r, tau, q, work, len, zgeqrf, zungqr);
	}

	DLLEXPORT MKL_INT z_qr_thin_factor(MKL_INT m, MKL_INT n, MKL_Complex16 q[], MKL_Complex16 tau[], MKL_Complex16 r[], MKL_Complex16 work[], MKL_INT len)
	{
		return qr_thin_factor<MKL_Complex16>(m, n, q, tau, r, work, len, zgeqrf, zungqr);
	}

	DLLEXPORT MKL_INT s_qr_solve(MKL_INT m, MKL_INT n, MKL_INT bn, float a[], float b[], float x[], float work[], MKL_INT len)
	{
		return qr_solve<float>(m, n, bn, a, b, x, work, len, sgels);
	}

	DLLEXPORT MKL_INT d_qr_solve(MKL_INT m, MKL_INT n, MKL_INT bn, double a[], double b[], double x[], double work[], MKL_INT len)
	{
		return qr_solve<double>(m, n, bn, a, b, x, work, len, dgels);
	}

	DLLEXPORT MKL_INT c_qr_solve(MKL_INT m, MKL_INT n, MKL_INT bn, MKL_Complex8 a[], MKL_Complex8 b[], MKL_Complex8 x[], MKL_Complex8 work[], MKL_INT len)
	{
		return qr_solve<MKL_Complex8>(m, n, bn, a, b, x, work, len, cgels);
	}

	DLLEXPORT MKL_INT z_qr_solve(MKL_INT m, MKL_INT n, MKL_INT bn, MKL_Complex16 a[], MKL_Complex16 b[], MKL_Complex16 x[], MKL_Complex16 work[], MKL_INT len)
	{
		return qr_solve<MKL_Complex16>(m, n, bn, a, b, x, work, len, zgels);
	}

	DLLEXPORT MKL_INT s_qr_solve_factored(MKL_INT m, MKL_INT n, MKL_INT bn, float r[], float b[], float tau[], float x[], float work[], MKL_INT len)
	{
		return qr_solve_factored<float>(m, n, bn, r, b, tau, x, work, len, sormqr, cblas_strsm);	
	}

	DLLEXPORT MKL_INT d_qr_solve_factored(MKL_INT m, MKL_INT n, MKL_INT bn, double r[], double b[], double tau[], double x[], double work[], MKL_INT len)
	{
		return qr_solve_factored<double>(m, n, bn, r, b, tau, x, work, len, dormqr, cblas_dtrsm);	
	}

	DLLEXPORT MKL_INT c_qr_solve_factored(MKL_INT m, MKL_INT n, MKL_INT bn, MKL_Complex8 r[], MKL_Complex8 b[], MKL_Complex8 tau[], MKL_Complex8 x[], MKL_Complex8 work[], MKL_INT len)
	{
		return complex_qr_solve_factored<MKL_Complex8>(m, n, bn, r, b, tau, x, work, len, cunmqr, cblas_ctrsm);	
	}

	DLLEXPORT MKL_INT z_qr_solve_factored(MKL_INT m, MKL_INT n, MKL_INT bn, MKL_Complex16 r[], MKL_Complex16 b[], MKL_Complex16 tau[], MKL_Complex16 x[], MKL_Complex16 work[], MKL_INT len)
	{
		return complex_qr_solve_factored<MKL_Complex16>(m, n, bn, r, b, tau, x, work, len, zunmqr, cblas_ztrsm);	
	}

	DLLEXPORT MKL_INT s_svd_factor(bool compute_vectors, MKL_INT m, MKL_INT n, float a[], float s[], float u[], float v[], float work[], MKL_INT len)
	{
		return svd_factor<float>(compute_vectors, m, n, a, s, u, v, work, len, sgesvd);
	}

	DLLEXPORT MKL_INT d_svd_factor(bool compute_vectors, MKL_INT m, MKL_INT n, double a[], double s[], double u[], double v[], double work[], MKL_INT len)
	{
		return svd_factor<double>(compute_vectors, m, n, a, s, u, v, work, len, dgesvd);
	}

	DLLEXPORT MKL_INT c_svd_factor(bool compute_vectors, MKL_INT m, MKL_INT n, MKL_Complex8 a[], MKL_Complex8 s[], MKL_Complex8 u[], MKL_Complex8 v[], MKL_Complex8 work[], MKL_INT len)
	{
		return complex_svd_factor<MKL_Complex8, float>(compute_vectors, m, n, a, s, u, v, work, len, cgesvd);
	}
	
	DLLEXPORT MKL_INT z_svd_factor(bool compute_vectors, MKL_INT m, MKL_INT n, MKL_Complex16 a[], MKL_Complex16 s[], MKL_Complex16 u[], MKL_Complex16 v[], MKL_Complex16 work[], MKL_INT len)
	{
		return complex_svd_factor<MKL_Complex16, double>(compute_vectors, m, n, a, s, u, v, work, len, zgesvd);
	}
}