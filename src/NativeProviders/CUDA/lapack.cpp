#include "lapack_common.h"
#include "wrapper_common.h"
#include "cublas.h"
#include "cusolverDn.h"
#include <algorithm>

template<typename T, typename K>
inline int lu_factor(int m, T a[], int ipiv[],
	int(*getrf) (CBLAS_ORDER, const int, const int, K*, const int, int*))
{
	int info = getrf(CblasColMajor, m, m, a, m, ipiv);
	shift_ipiv_down(m, ipiv);
	return info;
};

template<typename T, typename K>
inline int lu_inverse(int n, T a[],
	int(*getrf) (CBLAS_ORDER, const int, const int, K*, const int, int*),
	int(*getri) (CBLAS_ORDER, const int, K*, const int, const int*))
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
	int(*getri) (CBLAS_ORDER, const int, K*, const int, const int*))
{
	shift_ipiv_up(n, ipiv);
	int info = getri(CblasColMajor, n, a, n, ipiv);
	shift_ipiv_down(n, ipiv);
	return info;
}

template<typename T, typename K>
inline int lu_solve_factored(int n, int nrhs, T a[], int ipiv[], T b[],
	int(*getrs) (CBLAS_ORDER, CBLAS_TRANSPOSE, const int, const int, const K*, const int, const int*, K*, const int))
{
	shift_ipiv_up(n, ipiv);
	int info = getrs(CblasColMajor, CblasNoTrans, n, nrhs, a, n, ipiv, b, n);
	shift_ipiv_down(n, ipiv);
	return info;
}

template<typename T, typename K>
inline int lu_solve(int n, int nrhs, T a[], T b[],
	int(*getrf) (CBLAS_ORDER, const int, const int, K*, const int, int*),
	int(*getrs) (CBLAS_ORDER, CBLAS_TRANSPOSE, const int, const int, const K*, const int, const int*, K*, const int))
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
inline int cholesky_factor(int n, T* a, int(*potrf) (CBLAS_ORDER, CBLAS_UPLO, const int, K*, const int))
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
	int(*potrf) (CBLAS_ORDER, CBLAS_UPLO, const int, K*, const int),
	int(*potrs) (CBLAS_ORDER, CBLAS_UPLO, const int, const int, const K*, const int, K*, const int))
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
	int(*potrs) (CBLAS_ORDER, CBLAS_UPLO, const int, const int, const K*, const int, K*, const int))
{
	return potrs(CblasColMajor, CblasLower, n, nrhs, a, n, b, n);
}

template<typename T, typename K>
inline int qr_factor(int m, int n, T r[], T tau[], T q[], T work[], int len,
	int(*geqrf) (const int, const int, K*, const int, T*),
	int(*orgqr) (const int, const int, const int, K*, const int, const K*))
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
	void(*geqrf) (const int*, const int*, T*, const int*, T*, T*, const int*, int*),
	void(*orgqr) (const int*, const int*, const int*, T*, const int*, const T*, T*, const int*, int*))
{
	int info = 0;
	geqrf(&m, &n, q, &m, tau, work, &len, &info);

	for (int i = 0; i < n; ++i)
	{
		for (int j = 0; j < n; ++j)
		{
			if (i <= j) {
				r[j * n + i] = q[j * m + i];
			}
		}
	}

	orgqr(&m, &n, &n, q, &m, tau, work, &len, &info);

	return info;
}

template<typename T>
inline int qr_solve(int m, int n, int bn, T a[], T b[], T x[], T work[], int len,
	void(*gels) (const char*, const int*, const int*, const int*, T*,
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
	void(*ormqr) (const char*, const char*, const int*, const int*, const int*,
	const T*, const int*, const T*, T*, const int*, T*, const int*, int* info),
	void(*trsm) (const CBLAS_ORDER, const CBLAS_SIDE, const CBLAS_UPLO, const CBLAS_TRANSPOSE, const CBLAS_DIAG,
	const int, const int, const T, const T*, const int, T*, const int))
{
	T* clone_b = new T[m*bn];
	std::memcpy(clone_b, b, m*bn*sizeof(T));

	char side = 'L';
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
	void(*unmqr) (const char*, const char*, const int*, const int*, const int*,
	const T*, const int*, const T*, T*, const int*, T*, const int*, int* info),
	void(*trsm) (const CBLAS_ORDER, const CBLAS_SIDE, const CBLAS_UPLO, const CBLAS_TRANSPOSE, const CBLAS_DIAG,
	const int, const int, const void*, const void*, const int, void*, const int ldb))
{
	T* clone_b = new T[m*bn];
	std::memcpy(clone_b, b, m*bn*sizeof(T));

	char side = 'L';
	char tran = 'C';
	int info = 0;
	unmqr(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info);

	T one = { 1.0f, 0.0f };
	trsm(CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, &one, r, m, clone_b, m);
	copyBtoX(n, n, bn, clone_b, x);

	delete[] clone_b;
	return info;
}

template<typename T>
inline int svd_factor(bool compute_vectors, int m, int n, T a[], T s[], T u[], T v[], T work[], int len,
	void(*gesvd) (const char*, const char*, const int*, const int*, T*, const int*,
	T*, T*, const int*, T*, const int*, T*, const int*, int*))
{
	int info = 0;
	char job = compute_vectors ? 'A' : 'N';
	gesvd(&job, &job, &m, &n, a, &m, s, u, &m, v, &n, work, &len, &info);
	return info;
}


template<typename T, typename R>
inline int complex_svd_factor(bool compute_vectors, int m, int n, T a[], T s[], T u[], T v[], T work[], int len,
	void(*gesvd) (const char*, const char*, const int*, const int*, T*, const int*,
	R*, T*, const int*, T*, const int*, T*, const int*, R*, int*))
{
	int info = 0;
	int dim_s = std::min(m, n);
	R* rwork = new R[5 * dim_s];
	R* s_local = new R[dim_s];
	char job = compute_vectors ? 'A' : 'N';
	gesvd(&job, &job, &m, &n, a, &m, s_local, u, &m, v, &n, work, &len, rwork, &info);

	for (int index = 0; index < dim_s; ++index){
		T value = { s_local[index], 0.0f };
		s[index] = value;
	}

	delete[] rwork;
	delete[] s_local;
	return info;
}

extern "C" {
	DLLEXPORT int s_lu_factor(int m, float a[], int ipiv[]) {
		return lu_factor<float, float>(m, a, ipiv, cusolverDnSgetrf);
	}

	DLLEXPORT int d_lu_factor(int m, double a[], int ipiv[]) {
		return lu_factor<double, double>(m, a, ipiv, cusolverDnDgetrf);
	}

	DLLEXPORT int c_lu_factor(int m, cuComplex a[], int ipiv[]) {
		return lu_factor<cuComplex, void>(m, a, ipiv, cusolverDnCgetrf);
	}

	DLLEXPORT int z_lu_factor(int m, cuDoubleComplex a[], int ipiv[]) {
		return lu_factor(m, a, ipiv, cusolverDnZgetrf);
	}

	DLLEXPORT int s_lu_inverse(int n, float a[])
	{
		return lu_inverse<float, float>(n, a, cusolverDnSgetrf, cusolverDnSgetri);
	}

	DLLEXPORT int d_lu_inverse(int n, double a[])
	{
		return lu_inverse<double, double>(n, a, cusolverDnDgetrf, cusolverDnDgetri);
	}

	DLLEXPORT int c_lu_inverse(int n, cuComplex a[])
	{
		return lu_inverse<cuComplex, void>(n, a, cusolverDnCgetrf, cusolverDnCgetri);
	}

	DLLEXPORT int z_lu_inverse(int n, cuDoubleComplex a[])
	{
		return lu_inverse<cuDoubleComplex, void>(n, a, cusolverDnZgetrf, cusolverDnZgetri);
	}

	DLLEXPORT int s_lu_inverse_factored(int n, float a[], int ipiv[], float work[], int lwork)
	{
		return lu_inverse_factored<float, float>(n, a, ipiv, cusolverDnSgetri);
	}

	DLLEXPORT int d_lu_inverse_factored(int n, double a[], int ipiv[], double work[], int lwork)
	{
		return lu_inverse_factored<double, double>(n, a, ipiv, cusolverDnDgetri);
	}

	DLLEXPORT int c_lu_inverse_factored(int n, cuComplex a[], int ipiv[], cuComplex work[], int lwork)
	{
		return lu_inverse_factored<cuComplex, void>(n, a, ipiv, cusolverDnCgetri);
	}

	DLLEXPORT int z_lu_inverse_factored(int n, cuDoubleComplex a[], int ipiv[], cuDoubleComplex work[], int lwork)
	{
		return lu_inverse_factored<cuDoubleComplex, void>(n, a, ipiv, cusolverDnZgetri);
	}

	DLLEXPORT int s_lu_solve_factored(int n, int nrhs, float a[], int ipiv[], float b[])
	{
		return lu_solve_factored<float, float>(n, nrhs, a, ipiv, b, cusolverDnSgetrs);
	}

	DLLEXPORT int  d_lu_solve_factored(int n, int nrhs, double a[], int ipiv[], double b[])
	{
		return lu_solve_factored<double, double>(n, nrhs, a, ipiv, b, cusolverDnDgetrs);
	}

	DLLEXPORT int c_lu_solve_factored(int n, int nrhs, cuComplex a[], int ipiv[], cuComplex b[])
	{
		return lu_solve_factored<cuComplex, void>(n, nrhs, a, ipiv, b, cusolverDnCgetrs);
	}

	DLLEXPORT int z_lu_solve_factored(int n, int nrhs, cuDoubleComplex a[], int ipiv[], cuDoubleComplex b[])
	{
		return lu_solve_factored<cuDoubleComplex, void>(n, nrhs, a, ipiv, b, cusolverDnZgetrs);
	}

	DLLEXPORT int s_lu_solve(int n, int nrhs, float a[], float b[])
	{
		return lu_solve<float, float>(n, nrhs, a, b, cusolverDnSgetrf, cusolverDnSgetrs);
	}

	DLLEXPORT int d_lu_solve(int n, int nrhs, double a[], double b[])
	{
		return lu_solve<double, double>(n, nrhs, a, b, cusolverDnDgetrf, cusolverDnDgetrs);
	}

	DLLEXPORT int c_lu_solve(int n, int nrhs, cuComplex a[], cuComplex b[])
	{
		return lu_solve<cuComplex, void>(n, nrhs, a, b, cusolverDnCgetrf, cusolverDnCgetrs);
	}

	DLLEXPORT int z_lu_solve(int n, int nrhs, cuDoubleComplex a[], cuDoubleComplex b[])
	{
		return lu_solve<cuDoubleComplex, void>(n, nrhs, a, b, cusolverDnZgetrf, cusolverDnZgetrs);
	}

	DLLEXPORT int s_cholesky_factor(int n, float a[]){
		return cholesky_factor<float, float>(n, a, cusolverDnSpotrf);
	}

	DLLEXPORT int d_cholesky_factor(int n, double* a){
		return cholesky_factor<double, double>(n, a, cusolverDnDpotrf);
	}

	DLLEXPORT int c_cholesky_factor(int n, cuComplex a[]){
		return cholesky_factor<cuComplex, void>(n, a, cusolverDnCpotrf);
	}

	DLLEXPORT int z_cholesky_factor(int n, cuDoubleComplex a[]){
		return cholesky_factor<cuDoubleComplex, void>(n, a, cusolverDnZpotrf);
	}

	DLLEXPORT int s_cholesky_solve(int n, int nrhs, float a[], float b[])
	{
		return cholesky_solve<float, float>(n, nrhs, a, b, cusolverDnSpotrf, cusolverDnSpotrs);
	}

	DLLEXPORT int d_cholesky_solve(int n, int nrhs, double a[], double b[])
	{
		return cholesky_solve<double, double>(n, nrhs, a, b, cusolverDnDpotrf, cusolverDnDpotrs);
	}

	DLLEXPORT int c_cholesky_solve(int n, int nrhs, cuComplex a[], cuComplex b[])
	{
		return cholesky_solve<cuComplex, void>(n, nrhs, a, b, cusolverDnCpotrf, cusolverDnCpotrs);
	}

	DLLEXPORT int z_cholesky_solve(int n, int nrhs, cuDoubleComplex a[], cuDoubleComplex b[])
	{
		return cholesky_solve<cuDoubleComplex, void>(n, nrhs, a, b, cusolverDnZpotrf, cusolverDnZpotrs);
	}

	DLLEXPORT int s_cholesky_solve_factored(int n, int nrhs, float a[], float b[])
	{
		return cholesky_solve_factored<float, float>(n, nrhs, a, b, cusolverDnSpotrs);
	}

	DLLEXPORT int d_cholesky_solve_factored(int n, int nrhs, double a[], double b[])
	{
		return cholesky_solve_factored<double, double>(n, nrhs, a, b, cusolverDnDpotrs);
	}

	DLLEXPORT int c_cholesky_solve_factored(int n, int nrhs, cuComplex a[], cuComplex b[])
	{
		return cholesky_solve_factored<cuComplex, void>(n, nrhs, a, b, cusolverDnCpotrs);
	}

	DLLEXPORT int z_cholesky_solve_factored(int n, int nrhs, cuDoubleComplex a[], cuDoubleComplex b[])
	{
		return cholesky_solve_factored<cuDoubleComplex, void>(n, nrhs, a, b, cusolverDnZpotrs);
	}

	/*DLLEXPORT int s_qr_factor(int m, int n, float r[], float tau[], float q[], float work[], int len)
	{
	return qr_factor<float, float>(m, n, r, tau, q, work, len, cusolverDnSgeqrf, cusolverDnSorgqr);
	}

	DLLEXPORT int s_qr_thin_factor(int m, int n, float q[], float tau[], float r[], float work[], int len)
	{
	return qr_thin_factor<float>(m, n, q, tau, r, work, len, cusolverDnSgeqrf, cusolverDnSorgqr);
	}

	DLLEXPORT int d_qr_factor(int m, int n, double r[], double tau[], double q[], double work[], int len)
	{
	return qr_factor<double>(m, n, r, tau, q, work, len, cusolverDnDgeqrf, cusolverDnDorgqr);
	}

	DLLEXPORT int d_qr_thin_factor(int m, int n, double q[], double tau[], double r[], double work[], int len)
	{
	return qr_thin_factor<double>(m, n, q, tau, r, work, len, cusolverDnDgeqrf, cusolverDnDorgqr);
	}

	DLLEXPORT int c_qr_factor(int m, int n, cuComplex r[], cuComplex tau[], cuComplex q[], cuComplex work[], int len)
	{
	return qr_factor<cuComplex>(m, n, r, tau, q, work, len, cusolverDnCgeqrf, cusolverDnCungqr);
	}

	DLLEXPORT int c_qr_thin_factor(int m, int n, cuComplex q[], cuComplex tau[], cuComplex r[], cuComplex work[], int len)
	{
	return qr_thin_factor<cuComplex>(m, n, q, tau, r, work, len, cusolverDnCgeqrf, cusolverDnCungqr);
	}

	DLLEXPORT int z_qr_factor(int m, int n, cuDoubleComplex r[], cuDoubleComplex tau[], cuDoubleComplex q[])
	{
	return qr_factor<cuDoubleComplex>(m, n, r, tau, q, work, len, cusolverDnZgeqrf, cusolverDnZungqr);
	}

	DLLEXPORT int z_qr_thin_factor(int m, int n, cuDoubleComplex q[], cuDoubleComplex tau[], cuDoubleComplex r[])
	{
	return qr_thin_factor<cuDoubleComplex>(m, n, q, tau, r, work, len, cusolverDnZgeqrf, cusolverDnZungqr);
	}

	DLLEXPORT int s_qr_solve(int m, int n, int bn, float a[], float b[], float x[], float work[], int len)
	{
	return qr_solve<float>(m, n, bn, a, b, x, work, len, sgels);
	}

	DLLEXPORT int d_qr_solve(int m, int n, int bn, double a[], double b[], double x[], double work[], int len)
	{
	return qr_solve<double>(m, n, bn, a, b, x, work, len, dgels);
	}

	DLLEXPORT int c_qr_solve(int m, int n, int bn, cuComplex a[], cuComplex b[], cuComplex x[], cuComplex work[], int len)
	{
	return qr_solve<cuComplex>(m, n, bn, a, b, x, work, len, cgels);
	}

	DLLEXPORT int z_qr_solve(int m, int n, int bn, cuDoubleComplex a[], cuDoubleComplex b[], cuDoubleComplex x[], cuDoubleComplex work[], int len)
	{
	return qr_solve<cuDoubleComplex>(m, n, bn, a, b, x, work, len, zgels);
	}

	DLLEXPORT int s_qr_solve_factored(int m, int n, int bn, float r[], float b[], float tau[], float x[], float work[], int len)
	{
	return qr_solve_factored<float>(m, n, bn, r, b, tau, x, work, len, sormqr, cblas_strsm);
	}

	DLLEXPORT int d_qr_solve_factored(int m, int n, int bn, double r[], double b[], double tau[], double x[], double work[], int len)
	{
	return qr_solve_factored<double>(m, n, bn, r, b, tau, x, work, len, dormqr, cblas_dtrsm);
	}

	DLLEXPORT int c_qr_solve_factored(int m, int n, int bn, cuComplex r[], cuComplex b[], cuComplex tau[], cuComplex x[], cuComplex work[], int len)
	{
	return complex_qr_solve_factored<cuComplex>(m, n, bn, r, b, tau, x, work, len, cunmqr, cblas_ctrsm);
	}

	DLLEXPORT int z_qr_solve_factored(int m, int n, int bn, cuDoubleComplex r[], cuDoubleComplex b[], cuDoubleComplex tau[], cuDoubleComplex x[], cuDoubleComplex work[], int len)
	{
	return complex_qr_solve_factored<cuDoubleComplex>(m, n, bn, r, b, tau, x, work, len, zunmqr, cblas_ztrsm);
	}

	DLLEXPORT int s_svd_factor(bool compute_vectors, int m, int n, float a[], float s[], float u[], float v[], float work[], int len)
	{
	return svd_factor<float>(compute_vectors, m, n, a, s, u, v, work, len, sgesvd);
	}

	DLLEXPORT int d_svd_factor(bool compute_vectors, int m, int n, double a[], double s[], double u[], double v[], double work[], int len)
	{
	return svd_factor<double>(compute_vectors, m, n, a, s, u, v, work, len, dgesvd);
	}

	DLLEXPORT int c_svd_factor(bool compute_vectors, int m, int n, cuComplex a[], cuComplex s[], cuComplex u[], cuComplex v[], cuComplex work[], int len)
	{
	return complex_svd_factor<cuComplex, float>(compute_vectors, m, n, a, s, u, v, work, len, cgesvd);
	}

	DLLEXPORT int z_svd_factor(bool compute_vectors, int m, int n, cuDoubleComplex a[], cuDoubleComplex s[], cuDoubleComplex u[], cuDoubleComplex v[], cuDoubleComplex work[], int len)
	{
	return complex_svd_factor<cuDoubleComplex, double>(compute_vectors, m, n, a, s, u, v, work, len, zgesvd);
	}*/
}