#include "blas.h"
#include "lapack_common.h"
#include "wrapper_common.h"

#include <algorithm>

extern "C" {
#include "clapack.h"

	// to get atlas to link
//	float _sqrtf(float x) {return sqrt(x);}
}
#include "lapacke.h"

template<typename T, typename GETRF>
inline int lu_factor(int m, T a[], int ipiv[], GETRF getrf)
{
	int info = getrf(CblasColMajor, m, m, a, m, ipiv);
	shift_ipiv_down(m, ipiv);
	return info;
}

template<typename T, typename GETRF, typename GETRI>
inline int lu_inverse(int n, T a[], GETRF getrf, GETRI getri)
{
	int* ipiv = new int[n];
	int info = getrf(CblasColMajor, n, n, a, n, ipiv);

	if (info != 0)
	{
		delete[] ipiv;
		return info;
	}

	info = getri(CblasColMajor, n, a, n, ipiv);
	delete[] ipiv;
	return info;
}

template<typename T, typename GETRI>
inline int lu_inverse_factored(int n, T a[], int ipiv[], GETRI getri)
{
	shift_ipiv_up(n, ipiv);
	int info = getri(CblasColMajor, n, a, n, ipiv);
	shift_ipiv_down(n, ipiv);
	return info;
}

template<typename T, typename GETRS>
inline int lu_solve_factored(int n, int nrhs, T a[], int ipiv[], T b[], GETRS getrs)
{
	shift_ipiv_up(n, ipiv);
	int info = getrs(CblasColMajor, CblasNoTrans, n, nrhs, a, n, ipiv, b, n);
	shift_ipiv_down(n, ipiv);
	return info;
}

template<typename T, typename GETRF, typename GETRS>
inline int lu_solve(int n, int nrhs, T a[], T b[], GETRF getrf, GETRS getrs)
{
	T* clone = Clone(n, n, a);
	int* ipiv = new int[n];
	int info = getrf(CblasColMajor, n, n, clone, n, ipiv);

	if (info != 0)
	{
		delete[] ipiv;
		delete[] clone;
		return info;
	}

	info = getrs(CblasColMajor, CblasNoTrans, n, nrhs, clone, n, ipiv, b, n);
	delete[] ipiv;
	delete[] clone;
	return info;
}

template<typename T, typename POTRF>
inline int cholesky_factor(int n, T* a, POTRF potrf)
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

template<typename T, typename POTRF, typename POTRS>
inline int cholesky_solve(int n, int nrhs, T a[], T b[], POTRF potrf, POTRS potrs)
{
	T* clone = Clone(n, n, a);
	int info = potrf(CblasColMajor, CblasLower, n, clone, n);

	if (info != 0)
	{
		delete[] clone;
		return info;
	}

	info = potrs(CblasColMajor, CblasLower, n, nrhs, clone, n, b, n);
	delete[] clone;
	return info;
}

template<typename T, typename POTRS>
inline int cholesky_solve_factored(int n, int nrhs, T a[], T b[], POTRS potrs)
{
	return potrs(CblasColMajor, CblasLower, n, nrhs, a, n, b, n);
}

template<typename T, typename GEQRF, typename ORGQR>
inline int qr_factor(int m, int n, T r[], T tau[], T q[], T work[], int len, GEQRF geqrf, ORGQR orgqr)
{
	int info = 0;
	geqrf(&m, &n, r, &m, tau, work, &len, &info);

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
		orgqr(&m, &m, &m, q, &m, tau, work, &len, &info);
	}
	else
	{
		orgqr(&m, &m, &n, q, &m, tau, work, &len, &info);
	}

	return info;
}

template<typename T, typename GEQRF, typename ORGQR>
inline int qr_thin_factor(int m, int n, T q[], T tau[], T r[], T work[], int len, GEQRF geqrf, ORGQR orgqr)
{
	int info = 0;
	geqrf(&m, &n, q, &m, tau, work, &len, &info);

	for (int i = 0; i < n; ++i)
	{
		for (int j = 0; j < n; ++j)
		{
			if (i <= j)
			{
				r[j * n + i] = q[j * m + i];
			}
		}
	}

	orgqr(&m, &n, &n, q, &m, tau, work, &len, &info);
	return info;
}

template<typename T, typename GELS>
inline int qr_solve(int m, int n, int bn, T a[], T b[], T x[], T work[], int len, GELS gels)
{
	T* clone_a = Clone(m, n, a);
	T* clone_b = Clone(m, bn, b);
	char N = 'N';
	int info = 0;
	gels(&N, &m, &n, &bn, clone_a, &m, clone_b, &m, work, &len, &info);
	copyBtoX(m, n, bn, clone_b, x);
	delete[] clone_a;
	delete[] clone_b;
	return info;
}

template<typename T, typename ORMQR, typename TRSM>
inline int qr_solve_factored(int m, int n, int bn, T r[], T b[], T tau[], T x[], T work[], int len, ORMQR ormqr, TRSM trsm)
{
	T* clone_b = Clone(m, bn, b);
	char side = 'L';
	char tran = 'T';
	int info = 0;
	ormqr(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info);
	trsm(CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, 1.0, r, m, clone_b, m);
	copyBtoX(m, n, bn, clone_b, x);
	delete[] clone_b;
	return info;
}

template<typename T, typename UNMQR, typename TRSM>
inline int complex_qr_solve_factored(int m, int n, int bn, T r[], T b[], T tau[], T x[], T work[], int len, UNMQR unmqr, TRSM trsm)
{
	T* clone_b = Clone(m, bn, b);
	char side = 'L';
	char tran = 'C';
	int info = 0;
	unmqr(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info);
	T one = 1.0f;
	trsm(CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, &one, r, m, clone_b, m);
	copyBtoX(m, n, bn, clone_b, x);
	delete[] clone_b;
	return info;
}

template<typename T, typename GESVD>
inline int svd_factor(bool compute_vectors, int m, int n, T a[], T s[], T u[], T v[], T work[], int len, GESVD gesvd)
{
	int info = 0;
	char job = compute_vectors ? 'A' : 'N';
	gesvd(&job, &job, &m, &n, a, &m, s, u, &m, v, &n, work, &len, &info);
	return info;
}

template<typename T, typename R, typename GESVD>
inline int complex_svd_factor(bool compute_vectors, int m, int n, T a[], T s[], T u[], T v[], T work[], int len, GESVD gesvd)
{
	int info = 0;
	int dim_s = std::min(m, n);
	R* rwork = new R[5 * dim_s];
	R* s_local = new R[dim_s];
	char job = compute_vectors ? 'A' : 'N';
	gesvd(&job, &job, &m, &n, a, &m, s_local, u, &m, v, &n, work, &len, rwork, &info);

	for (int index = 0; index < dim_s; ++index)
	{
		s[index] = s_local[index];
	}

	delete[] rwork;
	delete[] s_local;
	return info;
}

template<typename T, typename R, typename GEES, typename TREVC>
inline int eigen_factor(int n, T a[], T vectors[], R values[], T d[], GEES gees, TREVC trevc)
{
	T* clone_a = Clone(n, n, a);
	T* wr = new T[n];
	T* wi = new T[n];

	int sdim;
	int info = gees(LAPACK_COL_MAJOR, 'V', 'N', nullptr, n, clone_a, n, &sdim, wr, wi, vectors, n);
	if (info != 0)
	{
		delete[] clone_a;
		delete[] wr;
		delete[] wi;
		return info;
	}

	int m;
	info = trevc(LAPACK_COL_MAJOR, 'R', 'B', nullptr, n, clone_a, n, nullptr, n, vectors, n, n, &m);
	if (info != 0)
	{
		delete[] clone_a;
		delete[] wr;
		delete[] wi;
		return info;
	}

	for (int index = 0; index < n; ++index)
	{
		values[index] = R(wr[index], wi[index]);
	}

	for (int i = 0; i < n; ++i)
	{
		int in = i * n;
		d[in + i] = wr[i];

		if (wi[i] > 0)
		{
			d[in + n + i] = wi[i];
		}
		else if (wi[i] < 0)
		{
			d[in - n + i] = wi[i];
		}
	}

	delete[] clone_a;
	delete[] wr;
	delete[] wi;
	return info;
}

template<typename T, typename GEES, typename TREVC>
inline int eigen_complex_factor(int n, T a[], T vectors[], Complex16 values[], T d[], GEES gees, TREVC trevc)
{
	T* clone_a = Clone(n, n, a);
	T* w = new T[n];

	int sdim;
	int info = gees(LAPACK_COL_MAJOR, 'V', 'N', nullptr, n, clone_a, n, &sdim, w, vectors, n);
	if (info != 0)
	{
		delete[] clone_a;
		delete[] w;
		return info;
	}

	int m;
	info = trevc(LAPACK_COL_MAJOR, 'R', 'B', nullptr, n, clone_a, n, nullptr, n, vectors, n, n, &m);
	if (info != 0)
	{
		delete[] clone_a;
		delete[] w;
		return info;
	}

	for (int i = 0; i < n; ++i)
	{
		values[i] = w[i];
		d[i * n + i] = w[i];
	}

	delete[] clone_a;
	delete[] w;
	return info;
}

template<typename R, typename T, typename SYEV>
inline int sym_eigen_factor(int n, T a[], T vectors[], Complex16 values[], T d[], SYEV syev)
{
	T* clone_a = Clone(n, n, a);
	R* w = new R[n];

	int info = syev(LAPACK_COL_MAJOR, 'V', 'U', n, clone_a, n, w);
	if (info != 0)
	{
		delete[] clone_a;
		delete[] w;
		return info;
	}

	memcpy(vectors, clone_a, n*n*sizeof(T));

	for (int index = 0; index < n; ++index)
	{
		values[index] = Complex16(w[index]);
	}

	for (int j = 0; j < n; ++j)
	{
		int jn = j*n;

		for (int i = 0; i < n; ++i)
		{
			if (i == j)
			{
				d[jn + i] = w[i];
			}
		}
	}

	delete[] clone_a;
	delete[] w;
	return info;
}



extern "C" {
	DLLEXPORT int s_lu_factor(int m, float a[], int ipiv[]) {
		return lu_factor(m, a, ipiv, clapack_sgetrf);
	}
	
	DLLEXPORT int d_lu_factor(int m, double a[], int ipiv[]) {
		return lu_factor(m, a, ipiv, clapack_dgetrf);
	}
	
	DLLEXPORT int c_lu_factor(int m, Complex8 a[], int ipiv[]) {
		return lu_factor(m, a, ipiv, clapack_cgetrf);
	}
	
	DLLEXPORT int z_lu_factor(int m, Complex16 a[], int ipiv[]) {
		return lu_factor(m, a, ipiv, clapack_zgetrf);
	}

	DLLEXPORT int s_lu_inverse(int n, float a[])
	{
		return lu_inverse(n, a, clapack_sgetrf, clapack_sgetri);
	}

	DLLEXPORT int d_lu_inverse(int n, double a[])
	{
		return lu_inverse(n, a, clapack_dgetrf, clapack_dgetri);
	}

	DLLEXPORT int c_lu_inverse(int n, Complex8 a[])
	{
		return lu_inverse(n, a, clapack_cgetrf, clapack_cgetri);
	}

	DLLEXPORT int z_lu_inverse(int n, Complex16 a[])
	{
		return lu_inverse(n, a, clapack_zgetrf, clapack_zgetri);
	}

	DLLEXPORT int s_lu_inverse_factored(int n, float a[], int ipiv[], float work[], int lwork)
	{
		return lu_inverse_factored(n, a, ipiv, clapack_sgetri);
	}

	DLLEXPORT int d_lu_inverse_factored(int n, double a[], int ipiv[], double work[], int lwork)
	{
		return lu_inverse_factored(n, a, ipiv, clapack_dgetri);
	}

	DLLEXPORT int c_lu_inverse_factored(int n, Complex8 a[], int ipiv[], Complex8 work[], int lwork)
	{
		return lu_inverse_factored(n, a, ipiv, clapack_cgetri);
	}

	DLLEXPORT int z_lu_inverse_factored(int n, Complex16 a[], int ipiv[], Complex16 work[], int lwork)
	{
		return lu_inverse_factored(n, a, ipiv, clapack_zgetri);
	}

	DLLEXPORT int s_lu_solve_factored(int n, int nrhs, float a[], int ipiv[], float b[])
	{
		return lu_solve_factored(n, nrhs, a, ipiv, b, clapack_sgetrs);
	}

	DLLEXPORT int  d_lu_solve_factored(int n, int nrhs, double a[], int ipiv[], double b[])
	{
		return lu_solve_factored(n, nrhs, a, ipiv, b, clapack_dgetrs);
	}

	DLLEXPORT int c_lu_solve_factored(int n, int nrhs, Complex8 a[], int ipiv[], Complex8 b[])
	{
		return lu_solve_factored(n, nrhs, a, ipiv, b, clapack_cgetrs);
	}

	DLLEXPORT int z_lu_solve_factored(int n, int nrhs, Complex16 a[], int ipiv[], Complex16 b[])
	{
		return lu_solve_factored(n, nrhs, a, ipiv, b, clapack_zgetrs);
	}

	DLLEXPORT int s_lu_solve(int n, int nrhs, float a[], float b[])
	{
		return lu_solve(n, nrhs, a, b, clapack_sgetrf, clapack_sgetrs);
	}

	DLLEXPORT int d_lu_solve(int n, int nrhs, double a[], double b[])
	{
		return lu_solve(n, nrhs, a, b, clapack_dgetrf, clapack_dgetrs);
	}

	DLLEXPORT int c_lu_solve(int n, int nrhs, Complex8 a[], Complex8 b[])
	{
		return lu_solve(n, nrhs, a, b, clapack_cgetrf, clapack_cgetrs);
	}
	
	DLLEXPORT int z_lu_solve(int n, int nrhs, Complex16 a[],  Complex16 b[])
	{
		return lu_solve(n, nrhs, a, b, clapack_zgetrf, clapack_zgetrs);
	}

	DLLEXPORT int s_cholesky_factor(int n, float a[]){
		return cholesky_factor(n, a, clapack_spotrf);
	}

	DLLEXPORT int d_cholesky_factor(int n, double* a){
		return cholesky_factor(n, a, clapack_dpotrf);
	}

	DLLEXPORT int c_cholesky_factor(int n, Complex8 a[]){
		return cholesky_factor(n, a, clapack_cpotrf);
	}

	DLLEXPORT int z_cholesky_factor(int n, Complex16 a[]){
		return cholesky_factor(n, a, clapack_zpotrf);
	}

	DLLEXPORT int s_cholesky_solve(int n, int nrhs, float a[], float b[])
	{
		return cholesky_solve(n, nrhs, a, b, clapack_spotrf, clapack_spotrs);
	}

	DLLEXPORT int d_cholesky_solve(int n, int nrhs, double a[], double b[])
	{
		return cholesky_solve(n, nrhs, a, b, clapack_dpotrf, clapack_dpotrs);
	}

	DLLEXPORT int c_cholesky_solve(int n, int nrhs, Complex8 a[], Complex8 b[])
	{
		return cholesky_solve(n, nrhs, a, b, clapack_cpotrf, clapack_cpotrs);
	}

	DLLEXPORT int z_cholesky_solve(int n, int nrhs, Complex16 a[], Complex16 b[])
	{
		return cholesky_solve(n, nrhs, a, b, clapack_zpotrf, clapack_zpotrs);
	}

	DLLEXPORT int s_cholesky_solve_factored(int n, int nrhs, float a[], float b[])
	{
		return cholesky_solve_factored(n, nrhs, a, b, clapack_spotrs);
	}

	DLLEXPORT int d_cholesky_solve_factored(int n, int nrhs, double a[], double b[])
	{
		return cholesky_solve_factored(n, nrhs, a, b, clapack_dpotrs);
	}

	DLLEXPORT int c_cholesky_solve_factored(int n, int nrhs, Complex8 a[], Complex8 b[])
	{
		return cholesky_solve_factored(n, nrhs, a, b, clapack_cpotrs);
	}

	DLLEXPORT int z_cholesky_solve_factored(int n, int nrhs, Complex16 a[], Complex16 b[])
	{
		return cholesky_solve_factored(n, nrhs, a, b, clapack_zpotrs);
	}

	DLLEXPORT int s_qr_factor(int m, int n, float r[], float tau[], float q[], float work[], int len)
	{
		return qr_factor(m, n, r, tau, q, work, len, clapack_sgeqrf, clapack_sorgqr);
	}

	DLLEXPORT int s_qr_thin_factor(int m, int n, float q[], float tau[], float r[], float work[], int len)
	{
		return qr_thin_factor(m, n, q, tau, r, work, len, clapack_sgeqrf, clapack_sorgqr);
	}

	/*DLLEXPORT int d_qr_factor(int m, int n, double r[], double tau[], double q[], double work[], int len)
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