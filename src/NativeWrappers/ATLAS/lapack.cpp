#include "lapack_common.h"
#include "wrapper_common.h"
#include "blas.h"
#include <algorithm>
extern "C" {
#include "clapack.h"
	// to get atlas to link
	float _sqrtf(float x) {return sqrt(x);}
}

template<typename T, typename K> 
inline int lu_factor(int m, T a[], int ipiv[], 
						 int (*getrf) (CBLAS_ORDER, const int, const int, K*, const int, int*))
{
	int info = getrf(CblasColMajor, m, m, a, m, ipiv);
	shift_ipiv_down(m, ipiv);
	return info;
};

template<typename T, typename K> 
inline int lu_inverse(int n, T a[],  
						  int (*getrf) (CBLAS_ORDER, const int, const int, K*, const int, int*),
						  int (*getri) (CBLAS_ORDER, const int, K*, const int, const int*))
{
	int* ipiv = new int[n];
	int info = getrf(CblasColMajor, n, n, a, n, ipiv);

	if (info != 0){
		delete[] ipiv;
		return info;
	}

	info = getri(CblasColMajor, n, a, n, ipiv);
	delete[] ipiv;
	return info;
};

template<typename T, typename K> 
inline int lu_inverse_factored(int n, T a[], int ipiv[],
								   int (*getri) (CBLAS_ORDER, const int, K*, const int, const int*))
{
	shift_ipiv_up(n, ipiv);
	int info = getri(CblasColMajor,n, a, n, ipiv);
	shift_ipiv_down(n, ipiv);
	return info;
}

template<typename T, typename K> 
inline int lu_solve_factored(int n, int nrhs, T a[], int ipiv[], T b[],
									int (*getrs) (CBLAS_ORDER, CBLAS_TRANSPOSE, const int, const int, const K*, const int, const int*, K*, const int))
{
	shift_ipiv_up(n, ipiv);
	int info = getrs(CblasColMajor, CblasNoTrans, n, nrhs, a, n, ipiv, b, n);
	shift_ipiv_down(n, ipiv);
	return info;
}

template<typename T, typename K> 
inline int lu_solve(int n, int nrhs, T a[], T b[], 
						  int (*getrf) (CBLAS_ORDER, const int, const int, K*, const int, int*),
						  int (*getrs) (CBLAS_ORDER, CBLAS_TRANSPOSE, const int, const int, const K*, const int, const int*, K*, const int))
{
	T* clone = Clone(n, n, a);
	int* ipiv = new int[n];
	int info = getrf(CblasColMajor, n, n, clone, n, ipiv);

	if (info != 0){
		delete[] ipiv;
		delete[] clone;
		return info;
	}

	info = getrs(CblasColMajor, CblasNoTrans, n, nrhs, clone, n, ipiv, b, n);
	delete[] ipiv;
	delete[] clone;
	return info;
}

template<typename T, typename K> 
inline int cholesky_factor(int n, T* a, int (*potrf) (CBLAS_ORDER, CBLAS_UPLO, const int, K*, const int))
{
	int info = potrf(CblasColMajor, CblasLower, n, a, n);
	T zero = T();
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

template<typename T, typename K> 
inline int cholesky_solve(int n, int nrhs, T a[], T b[],
							  int (*potrf) (CBLAS_ORDER, CBLAS_UPLO, const int, K*, const int),
							  int (*potrs) (CBLAS_ORDER, CBLAS_UPLO, const int, const int, const K*, const int, K*, const int))
{
	T* clone = Clone(n, n, a);
	int info = potrf(CblasColMajor, CblasLower, n, clone, n);

	if (info != 0){
		delete[] clone;
		return info;
	}

	info = potrs(CblasColMajor, CblasLower, n, nrhs, clone, n, b, n);
	delete[] clone;
	return info;
}

template<typename T, typename K> 
inline int cholesky_solve_factored(int n, int nrhs, T a[], T b[],
									   int (*potrs) (CBLAS_ORDER, CBLAS_UPLO, const int, const int, const K*, const int, K*, const int))
{
	return potrs(CblasColMajor, CblasLower, n, nrhs, a, n, b, n);
}

template<typename T, typename K> 
inline int qr_factor(int m, int n, T r[], T tau[], T q[], T work[], int len,
						 int (*geqrf) (const int, const int, K*, const int, T*),
						 int (*orgqr) (const int, const int, const int, K*, const int, const K*))
{
	int info = geqrf(m, n, r, m, tau);

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
		info = orgqr(m, m, m, q, m, tau);
	}
	else
	{
		info = orgqr(m, m, n, q, m, tau);
	}

	return info;
}

template<typename T> 
inline int qr_thin_factor(int m, int n, T q[], T tau[], T r[], T work[], int len,
						      void (*geqrf) (const int*, const int*, T*, const int*, T*, T*, const int*, int*),
						      void (*orgqr) (const int*, const int*, const int*, T*, const int*, const T*, T*, const int*, int*))
{
	int info = 0;
	geqrf(&m, &n, q, &m, tau, work, &len, &info);

	for (int i = 0; i < n; ++i)
	{
		for (int j = 0; j < n; ++j)
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
inline int qr_solve(int m, int n, int bn, T a[], T b[], T x[], T work[], int len,
						void (*gels) (const char*, const int*, const int*, const int*, T*, 
						const int*, T* b, const int*, T*, const int*, int*))
{
	T* clone_a = new T[m*n];
	std::memcpy(clone_a, a, m*n*sizeof(T));

	T* clone_b = new T[m*bn];
	std::memcpy(clone_b, b, m*bn*sizeof(T));

	char N = 'N';
	int info = 0;
	gels(&N, &m, &n, &bn, clone_a, &m, clone_b, &m, work, &len, &info);
	copyBtoX(n, n, bn, clone_b, x);

	delete[] clone_a;
	delete[] clone_b;
	return info;
}

template<typename T> 
inline int qr_solve_factored(int m, int n, int bn, T r[], T b[], T tau[], T x[], T work[], int len,
								   void (*ormqr) (const char*, const char*, const int*, const int*, const int*, 
								                  const T*, const int*, const T*, T*, const int*, T*, const int*, int* info),
								   void (*trsm) (const CBLAS_ORDER, const CBLAS_SIDE, const CBLAS_UPLO, const CBLAS_TRANSPOSE, const CBLAS_DIAG, 
								                 const int, const int, const T, const T*, const int, T*, const int)) 
{
	T* clone_b = new T[m*bn];
	std::memcpy(clone_b, b, m*bn*sizeof(T));

	char side ='L';
	char tran = 'T';
	int info = 0;
	ormqr(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info);
	trsm(CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, 1.0, r, m, clone_b, m);
	copyBtoX(n, n, bn, clone_b, x);

	delete[] clone_b;
	return info;
}

template<typename T> 
inline int complex_qr_solve_factored(int m, int n, int bn, T r[], T b[], T tau[], T x[], T work[], int len,
								   void (*unmqr) (const char*, const char*, const int*, const int*, const int*, 
								                  const T*, const int*, const T*, T*, const int*, T*, const int*, int* info),
								   void (*trsm) (const CBLAS_ORDER, const CBLAS_SIDE, const CBLAS_UPLO, const CBLAS_TRANSPOSE, const CBLAS_DIAG, 
								                 const int, const int, const void*, const void*, const int, void*, const int ldb)) 
{
	T* clone_b = new T[m*bn];
	std::memcpy(clone_b, b, m*bn*sizeof(T));

	char side ='L';
	char tran = 'C';
	int info = 0;
	unmqr(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info);

	T one = {1.0f, 0.0f};
	trsm(CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, &one, r, m, clone_b, m);
	copyBtoX(n, n, bn, clone_b, x);

	delete[] clone_b;
	return info;
}

template<typename T> 
inline int svd_factor(bool compute_vectors, int m, int n, T a[], T s[], T u[], T v[], T work[], int len,
								  void (*gesvd) (const char*, const char*, const int*, const int*, T*, const int*, 
								                 T*, T*, const int*, T*, const int*, T*, const int*, int*))
{
	int info = 0;
	char job = compute_vectors ? 'A' : 'N';
	gesvd(&job, &job, &m, &n, a, &m, s, u, &m, v, &n, work, &len, &info);
	return info;
}


template<typename T, typename R> 
inline int complex_svd_factor(bool compute_vectors, int m, int n, T a[], T s[], T u[], T v[], T work[], int len,
								  void (*gesvd) (const char*, const char*, const int*, const int*, T*, const int*, 
								                 R*, T*, const int*, T*, const int*, T*, const int*, R*, int*))
{
	int info = 0;
	int dim_s = std::min(m,n);
	R* rwork = new R[5 * dim_s];
	R* s_local = new R[dim_s];
	char job = compute_vectors ? 'A' : 'N';
	gesvd(&job, &job, &m, &n, a, &m, s_local, u, &m, v, &n, work, &len, rwork, &info);

	for(int index = 0; index < dim_s; ++index){
		T value = {s_local[index], 0.0f};
		s[index] = value;
	}

	delete[] rwork;
	delete[] s_local;
	return info;
}

extern "C" {
	DLLEXPORT int s_lu_factor(int m, float a[], int ipiv[]) {
		return lu_factor<float, float>(m, a, ipiv, clapack_sgetrf);
	}
	
	DLLEXPORT int d_lu_factor(int m, double a[], int ipiv[]) {
		return lu_factor<double, double>(m, a, ipiv, clapack_dgetrf);
	}
	
	DLLEXPORT int c_lu_factor(int m, Complex8 a[], int ipiv[]) {
		return lu_factor<Complex8, void>(m, a, ipiv, clapack_cgetrf);
	}
	
	DLLEXPORT int z_lu_factor(int m, Complex16 a[], int ipiv[]) {
		return lu_factor(m, a, ipiv, clapack_zgetrf);
	}

	DLLEXPORT int s_lu_inverse(int n, float a[])
	{
		return lu_inverse<float, float>(n, a, clapack_sgetrf, clapack_sgetri);
	}

	DLLEXPORT int d_lu_inverse(int n, double a[])
	{
		return lu_inverse<double, double>(n, a, clapack_dgetrf, clapack_dgetri);
	}

	DLLEXPORT int c_lu_inverse(int n, Complex8 a[])
	{
		return lu_inverse<Complex8, void>(n, a, clapack_cgetrf, clapack_cgetri);
	}

	DLLEXPORT int z_lu_inverse(int n, Complex16 a[])
	{
		return lu_inverse<Complex16, void>(n, a, clapack_zgetrf, clapack_zgetri);
	}

	DLLEXPORT int s_lu_inverse_factored(int n, float a[], int ipiv[], float work[], int lwork)
	{
		return lu_inverse_factored<float, float>(n, a, ipiv, clapack_sgetri);
	}

	DLLEXPORT int d_lu_inverse_factored(int n, double a[], int ipiv[], double work[], int lwork)
	{
		return lu_inverse_factored<double, double>(n, a, ipiv, clapack_dgetri);
	}

	DLLEXPORT int c_lu_inverse_factored(int n, Complex8 a[], int ipiv[], Complex8 work[], int lwork)
	{
		return lu_inverse_factored<Complex8, void>(n, a, ipiv, clapack_cgetri);
	}

	DLLEXPORT int z_lu_inverse_factored(int n, Complex16 a[], int ipiv[], Complex16 work[], int lwork)
	{
		return lu_inverse_factored<Complex16, void>(n, a, ipiv, clapack_zgetri);
	}

	DLLEXPORT int s_lu_solve_factored(int n, int nrhs, float a[], int ipiv[], float b[])
	{
		return lu_solve_factored<float, float>(n, nrhs, a, ipiv, b, clapack_sgetrs);
	}

	DLLEXPORT int  d_lu_solve_factored(int n, int nrhs, double a[], int ipiv[], double b[])
	{
		return lu_solve_factored<double, double>(n, nrhs, a, ipiv, b, clapack_dgetrs);
	}

	DLLEXPORT int c_lu_solve_factored(int n, int nrhs, Complex8 a[], int ipiv[], Complex8 b[])
	{
		return lu_solve_factored<Complex8, void>(n, nrhs, a, ipiv, b, clapack_cgetrs);
	}

	DLLEXPORT int z_lu_solve_factored(int n, int nrhs, Complex16 a[], int ipiv[], Complex16 b[])
	{
		return lu_solve_factored<Complex16, void>(n, nrhs, a, ipiv, b, clapack_zgetrs);
	}

	DLLEXPORT int s_lu_solve(int n, int nrhs, float a[], float b[])
	{
		return lu_solve<float, float>(n, nrhs, a, b, clapack_sgetrf, clapack_sgetrs);
	}

	DLLEXPORT int d_lu_solve(int n, int nrhs, double a[], double b[])
	{
		return lu_solve<double, double>(n, nrhs, a, b, clapack_dgetrf, clapack_dgetrs);
	}

	DLLEXPORT int c_lu_solve(int n, int nrhs, Complex8 a[], Complex8 b[])
	{
		return lu_solve<Complex8, void>(n, nrhs, a, b, clapack_cgetrf, clapack_cgetrs);
	}
	
	DLLEXPORT int z_lu_solve(int n, int nrhs, Complex16 a[],  Complex16 b[])
	{
		return lu_solve<Complex16, void>(n, nrhs, a, b, clapack_zgetrf, clapack_zgetrs);
	}

	DLLEXPORT int s_cholesky_factor(int n, float a[]){
		return cholesky_factor<float, float>(n, a, clapack_spotrf);
	}

	DLLEXPORT int d_cholesky_factor(int n, double* a){
		return cholesky_factor<double, double>(n, a, clapack_dpotrf);
	}

	DLLEXPORT int c_cholesky_factor(int n, Complex8 a[]){
		return cholesky_factor<Complex8, void>(n, a, clapack_cpotrf);
	}

	DLLEXPORT int z_cholesky_factor(int n, Complex16 a[]){
		return cholesky_factor<Complex16, void>(n, a, clapack_zpotrf);
	}

	DLLEXPORT int s_cholesky_solve(int n, int nrhs, float a[], float b[])
	{
		return cholesky_solve<float, float>(n, nrhs, a, b, clapack_spotrf, clapack_spotrs);
	}

	DLLEXPORT int d_cholesky_solve(int n, int nrhs, double a[], double b[])
	{
		return cholesky_solve<double, double>(n, nrhs, a, b, clapack_dpotrf, clapack_dpotrs);
	}

	DLLEXPORT int c_cholesky_solve(int n, int nrhs, Complex8 a[], Complex8 b[])
	{
		return cholesky_solve<Complex8, void>(n, nrhs, a, b, clapack_cpotrf, clapack_cpotrs);
	}

	DLLEXPORT int z_cholesky_solve(int n, int nrhs, Complex16 a[], Complex16 b[])
	{
		return cholesky_solve<Complex16, void>(n, nrhs, a, b, clapack_zpotrf, clapack_zpotrs);
	}

	DLLEXPORT int s_cholesky_solve_factored(int n, int nrhs, float a[], float b[])
	{
		return cholesky_solve_factored<float, float>(n, nrhs, a, b, clapack_spotrs);
	}

	DLLEXPORT int d_cholesky_solve_factored(int n, int nrhs, double a[], double b[])
	{
		return cholesky_solve_factored<double, double>(n, nrhs, a, b, clapack_dpotrs);
	}

	DLLEXPORT int c_cholesky_solve_factored(int n, int nrhs, Complex8 a[], Complex8 b[])
	{
		return cholesky_solve_factored<Complex8, void>(n, nrhs, a, b, clapack_cpotrs);
	}

	DLLEXPORT int z_cholesky_solve_factored(int n, int nrhs, Complex16 a[], Complex16 b[])
	{
		return cholesky_solve_factored<Complex16, void>(n, nrhs, a, b, clapack_zpotrs);
	}

	/*DLLEXPORT int s_qr_factor(int m, int n, float r[], float tau[], float q[], float work[], int len)
	{
		return qr_factor<float, float>(m, n, r, tau, q, work, len, clapack_sgeqrf, clapack_sorgqr);
	}

	DLLEXPORT int s_qr_thin_factor(int m, int n, float q[], float tau[], float r[], float work[], int len)
	{
		return qr_thin_factor<float>(m, n, q, tau, r, work, len, clapack_sgeqrf, clapack_sorgqr);
	}

	DLLEXPORT int d_qr_factor(int m, int n, double r[], double tau[], double q[], double work[], int len)
	{
		return qr_factor<double>(m, n, r, tau, q, work, len, clapack_dgeqrf, clapack_dorgqr);
	}

	DLLEXPORT int d_qr_thin_factor(int m, int n, double q[], double tau[], double r[], double work[], int len)
	{
		return qr_thin_factor<double>(m, n, q, tau, r, work, len, clapack_dgeqrf, clapack_dorgqr);
	}

	DLLEXPORT int c_qr_factor(int m, int n, Complex8 r[], Complex8 tau[], Complex8 q[], Complex8 work[], int len)
	{
		return qr_factor<Complex8>(m, n, r, tau, q, work, len, clapack_cgeqrf, clapack_cungqr);
	}

	DLLEXPORT int c_qr_thin_factor(int m, int n, Complex8 q[], Complex8 tau[], Complex8 r[], Complex8 work[], int len)
	{
		return qr_thin_factor<Complex8>(m, n, q, tau, r, work, len, clapack_cgeqrf, clapack_cungqr);
	}

	DLLEXPORT int z_qr_factor(int m, int n, Complex16 r[], Complex16 tau[], Complex16 q[])
	{
		return qr_factor<Complex16>(m, n, r, tau, q, work, len, clapack_zgeqrf, clapack_zungqr);
	}

	DLLEXPORT int z_qr_thin_factor(int m, int n, Complex16 q[], Complex16 tau[], Complex16 r[])
	{
		return qr_thin_factor<Complex16>(m, n, q, tau, r, work, len, clapack_zgeqrf, clapack_zungqr);
	}

	DLLEXPORT int s_qr_solve(int m, int n, int bn, float a[], float b[], float x[], float work[], int len)
	{
		return qr_solve<float>(m, n, bn, a, b, x, work, len, sgels);
	}

	DLLEXPORT int d_qr_solve(int m, int n, int bn, double a[], double b[], double x[], double work[], int len)
	{
		return qr_solve<double>(m, n, bn, a, b, x, work, len, dgels);
	}

	DLLEXPORT int c_qr_solve(int m, int n, int bn, Complex8 a[], Complex8 b[], Complex8 x[], Complex8 work[], int len)
	{
		return qr_solve<Complex8>(m, n, bn, a, b, x, work, len, cgels);
	}

	DLLEXPORT int z_qr_solve(int m, int n, int bn, Complex16 a[], Complex16 b[], Complex16 x[], Complex16 work[], int len)
	{
		return qr_solve<Complex16>(m, n, bn, a, b, x, work, len, zgels);
	}

	DLLEXPORT int s_qr_solve_factored(int m, int n, int bn, float r[], float b[], float tau[], float x[], float work[], int len)
	{
		return qr_solve_factored<float>(m, n, bn, r, b, tau, x, work, len, sormqr, cblas_strsm);	
	}

	DLLEXPORT int d_qr_solve_factored(int m, int n, int bn, double r[], double b[], double tau[], double x[], double work[], int len)
	{
		return qr_solve_factored<double>(m, n, bn, r, b, tau, x, work, len, dormqr, cblas_dtrsm);	
	}

	DLLEXPORT int c_qr_solve_factored(int m, int n, int bn, Complex8 r[], Complex8 b[], Complex8 tau[], Complex8 x[], Complex8 work[], int len)
	{
		return complex_qr_solve_factored<Complex8>(m, n, bn, r, b, tau, x, work, len, cunmqr, cblas_ctrsm);	
	}

	DLLEXPORT int z_qr_solve_factored(int m, int n, int bn, Complex16 r[], Complex16 b[], Complex16 tau[], Complex16 x[], Complex16 work[], int len)
	{
		return complex_qr_solve_factored<Complex16>(m, n, bn, r, b, tau, x, work, len, zunmqr, cblas_ztrsm);	
	}

	DLLEXPORT int s_svd_factor(bool compute_vectors, int m, int n, float a[], float s[], float u[], float v[], float work[], int len)
	{
		return svd_factor<float>(compute_vectors, m, n, a, s, u, v, work, len, sgesvd);
	}

	DLLEXPORT int d_svd_factor(bool compute_vectors, int m, int n, double a[], double s[], double u[], double v[], double work[], int len)
	{
		return svd_factor<double>(compute_vectors, m, n, a, s, u, v, work, len, dgesvd);
	}

	DLLEXPORT int c_svd_factor(bool compute_vectors, int m, int n, Complex8 a[], Complex8 s[], Complex8 u[], Complex8 v[], Complex8 work[], int len)
	{
		return complex_svd_factor<Complex8, float>(compute_vectors, m, n, a, s, u, v, work, len, cgesvd);
	}
	
	DLLEXPORT int z_svd_factor(bool compute_vectors, int m, int n, Complex16 a[], Complex16 s[], Complex16 u[], Complex16 v[], Complex16 work[], int len)
	{
		return complex_svd_factor<Complex16, double>(compute_vectors, m, n, a, s, u, v, work, len, zgesvd);
	}*/
}