#include "wrapper_common.h"

#include "lapack.h"
#include "lapack_common.h"
#include <algorithm>
#include <cstring>

template<typename T, typename GETRF>
inline lapack_int lu_factor(lapack_int m, T a[], lapack_int ipiv[], GETRF getrf)
{
	auto info = getrf(LAPACK_COL_MAJOR, m, m, a, m, ipiv);
	shift_ipiv_down(m, ipiv);
	return info;
}

template<typename T, typename GETRF, typename GETRI>
inline lapack_int lu_inverse(lapack_int n, T a[], GETRF getrf, GETRI getri)
{
	try
	{
		auto ipiv = array_new<lapack_int>(n);
		auto info = getrf(LAPACK_COL_MAJOR, n, n, a, n, ipiv.get());

		if (info != 0)
		{
			return info;
		}

		info = getri(LAPACK_COL_MAJOR, n, a, n, ipiv.get());
		return info;
	}
	catch (std::bad_alloc&)
	{
		return INSUFFICIENT_MEMORY;
	}
}

template<typename T, typename GETRI>
inline lapack_int lu_inverse_factored(lapack_int n, T a[], lapack_int ipiv[], GETRI getri)
{
	shift_ipiv_up(n, ipiv);
	auto info = getri(LAPACK_COL_MAJOR, n, a, n, ipiv);
	shift_ipiv_down(n, ipiv);
	return info;
}

template<typename T, typename GETRS>
inline lapack_int lu_solve_factored(lapack_int n, lapack_int nrhs, T a[], lapack_int ipiv[], T b[], GETRS getrs)
{
	shift_ipiv_up(n, ipiv);
	auto info = getrs(LAPACK_COL_MAJOR, 'N', n, nrhs, a, n, ipiv, b, n);
	shift_ipiv_down(n, ipiv);
	return info;
}

template<typename T, typename GETRF, typename GETRS>
inline lapack_int lu_solve(lapack_int n, lapack_int nrhs, T a[], T b[], GETRF getrf, GETRS getrs)
{
	try
	{
		auto clone = array_clone(n * n, a);
		auto ipiv = array_new<lapack_int>(n);
		auto info = getrf(LAPACK_COL_MAJOR, n, n, clone.get(), n, ipiv.get());

		if (info != 0)
		{
			return info;
		}

		return getrs(LAPACK_COL_MAJOR, 'N', n, nrhs, clone.get(), n, ipiv.get(), b, n);
	}
	catch (std::bad_alloc&)
	{
		return INSUFFICIENT_MEMORY;
	}
}

template<typename T, typename POTRF>
inline lapack_int cholesky_factor(lapack_int n, T* a, POTRF potrf)
{
	auto info = potrf(LAPACK_COL_MAJOR, 'L', n, a, n);
	auto zero = T();

	for (auto i = 0; i < n; ++i)
	{
		auto index = i * n;

		for (auto j = 0; j < n && i > j; ++j)
		{
			a[index + j] = zero;
		}
	}

	return info;
}

template<typename T, typename POTRF, typename POTRS>
inline lapack_int cholesky_solve(lapack_int n, lapack_int nrhs, T a[], T b[], POTRF potrf, POTRS potrs)
{
	try
	{
		auto clone = array_clone(n * n, a);
		auto info = potrf(LAPACK_COL_MAJOR, 'L', n, clone.get(), n);

		if (info != 0)
		{
			return info;
		}

		return potrs(LAPACK_COL_MAJOR, 'L', n, nrhs, clone.get(), n, b, n);
	}
	catch (std::bad_alloc&)
	{
		return INSUFFICIENT_MEMORY;
	}
}


template<typename T, typename GEQRF, typename ORGQR>
inline lapack_int qr_factor(lapack_int m, lapack_int n, T r[], T tau[], T q[], GEQRF geqrf, ORGQR orgqr)
{
	auto info = geqrf(LAPACK_COL_MAJOR, m, n, r, m, tau);

	for (auto i = 0; i < m; ++i)
	{
		for (auto j = 0; j < m && j < n; ++j)
		{
			if (i > j)
			{
				q[j * m + i] = r[j * m + i];
			}
		}
	}

	if (info != 0)
	{
		return info;
	}

	//compute the q elements explicitly
	if (m <= n)
	{
		info = orgqr(LAPACK_COL_MAJOR, m, m, m, q, m, tau);
	}
	else
	{
		info = orgqr(LAPACK_COL_MAJOR, m, m, n, q, m, tau);
	}

	return info;
}

template<typename T, typename GEQRF, typename ORGQR>
inline lapack_int qr_thin_factor(lapack_int m, lapack_int n, T q[], T tau[], T r[], GEQRF geqrf, ORGQR orgqr)
{
	auto info = geqrf(LAPACK_COL_MAJOR, m, n, q, m, tau);

	for (auto i = 0; i < n; ++i)
	{
		for (auto j = 0; j < n; ++j)
		{
			if (i <= j)
			{
				r[j * n + i] = q[j * m + i];
			}
		}
	}

	if (info != 0)
	{
		return info;
	}

	info = orgqr(LAPACK_COL_MAJOR, m, n, n, q, m, tau);
	return info;
}

template<typename T, typename GELS>
inline lapack_int qr_solve(lapack_int m, lapack_int n, lapack_int bn, T a[], T b[], T x[], GELS gels)
{
	try
	{
		auto clone_a = array_clone(m * n, a);
		auto clone_b = array_clone(m * bn, b);
		auto info = gels(LAPACK_COL_MAJOR, 'N', m, n, bn, clone_a.get(), m, clone_b.get(), m);

		if (info != 0)
		{
			return info;
		}

		copyBtoX(m, n, bn, clone_b.get(), x);
		return info;
	}
	catch (std::bad_alloc&)
	{
		return INSUFFICIENT_MEMORY;
	}
}

template<typename T, typename ORMQR, typename TRSM>
inline lapack_int qr_solve_factored(lapack_int m, lapack_int n, lapack_int bn, T r[], T b[], T tau[], T x[], ORMQR ormqr, TRSM trsm)
{
	try
	{
		auto clone_b = array_clone(m * bn, b);
		auto info = ormqr(LAPACK_COL_MAJOR, 'L', 'T', m, bn, n, r, m, tau, clone_b.get(), m);

		if (info != 0)
		{
			return info;
		}

		trsm(CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, 1.0, r, m, clone_b.get(), m);
		copyBtoX(m, n, bn, clone_b.get(), x);
		return info;
	}
	catch (std::bad_alloc&)
	{
		return INSUFFICIENT_MEMORY;
	}
}

template<typename T, typename R, typename UNMQR, typename TRSM>
inline lapack_int complex_qr_solve_factored(lapack_int m, lapack_int n, lapack_int bn, T r[], T b[], T tau[], T x[], UNMQR unmqr, TRSM trsm)
{
	try
	{
		auto clone_b = array_clone(m * bn, b);
		auto info = unmqr(LAPACK_COL_MAJOR, 'L', 'C', m, bn, n, r, m, tau, clone_b.get(), m);

		if (info != 0)
		{
			return info;
		}

		T one = 1.0f;
		trsm(CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, reinterpret_cast<R*>(&one), reinterpret_cast<R*>(r), m, reinterpret_cast<R*>(clone_b.get()), m);
		copyBtoX(m, n, bn, clone_b.get(), x);
		return info;
	}
	catch (std::bad_alloc&)
	{
		return INSUFFICIENT_MEMORY;
	}
}

template<typename T, typename GESVD>
inline lapack_int svd_factor(bool compute_vectors, lapack_int m, lapack_int n, T a[], T s[], T u[], T v[], GESVD gesvd)
{
	try
	{
		auto job = compute_vectors ? 'A' : 'N';
		auto dim_s = std::min(m, n);
		auto superb = array_new<T>(std::max(2, dim_s) - 1);
        return gesvd(LAPACK_COL_MAJOR, job, job, m, n, a, m, s, u, m, v, n, superb.get());
	}
	catch (std::bad_alloc&)
	{
		return INSUFFICIENT_MEMORY;
	}
}

template<typename T, typename R, typename GESVD>
inline lapack_int complex_svd_factor(bool compute_vectors, lapack_int m, lapack_int n, T a[], T s[], T u[], T v[], GESVD gesvd)
{
	try
	{
		auto dim_s = std::min(m, n);
		auto s_local = array_new<R>(dim_s);
		auto superb = array_new<R>(std::max(2, dim_s) - 1);
		auto job = compute_vectors ? 'A' : 'N';
		auto info = gesvd(LAPACK_COL_MAJOR, job, job, m, n, a, m, s_local.get(), u, m, v, n, superb.get());

		for (auto index = 0; index < dim_s; ++index)
		{
			s[index] = s_local.get()[index];
		}

		return info;
	}
	catch (std::bad_alloc&)
	{
		return INSUFFICIENT_MEMORY;
	}
}

template<typename T, typename R, typename GEES, typename TREVC>
inline lapack_int eigen_factor(lapack_int n, T a[], T vectors[], R values[], T d[], GEES gees, TREVC trevc)
{
	try
	{
		auto clone_a = array_clone(n * n, a);
		auto wr = array_new<T>(n);
		auto wi = array_new<T>(n);

		lapack_int sdim;
		lapack_int info = gees(LAPACK_COL_MAJOR, 'V', 'N', nullptr, n, clone_a.get(), n, &sdim, wr.get(), wi.get(), vectors, n);
		if (info != 0)
		{
			return info;
		}

		lapack_int m;
		info = trevc(LAPACK_COL_MAJOR, 'R', 'B', nullptr, n, clone_a.get(), n, nullptr, n, vectors, n, n, &m);
		if (info != 0)
		{
			return info;
		}

		for (auto index = 0; index < n; ++index)
		{
			values[index] = R(wr.get()[index], wi.get()[index]);
		}

		for (auto i = 0; i < n; ++i)
		{
			auto in = i * n;
			d[in + i] = wr.get()[i];

			if (wi.get()[i] > 0)
			{
				d[in + n + i] = wi.get()[i];
			}
			else if (wi.get()[i] < 0)
			{
				d[in - n + i] = wi.get()[i];
			}
		}
		return info;
	}
	catch (std::bad_alloc&)
	{
		return INSUFFICIENT_MEMORY;
	}
}

template<typename T, typename GEES, typename TREVC>
inline lapack_int eigen_complex_factor(lapack_int n, T a[], T vectors[], lapack_complex_double values[], T d[], GEES gees, TREVC trevc)
{
	try
	{
		auto clone_a = array_clone(n * n, a);
		auto w = array_new<T>(n);

		lapack_int sdim;
		lapack_int info = gees(LAPACK_COL_MAJOR, 'V', 'N', nullptr, n, clone_a.get(), n, &sdim, w.get(), vectors, n);
		if (info != 0)
		{
			return info;
		}

		lapack_int m;
		info = trevc(LAPACK_COL_MAJOR, 'R', 'B', nullptr, n, clone_a.get(), n, nullptr, n, vectors, n, n, &m);
		if (info != 0)
		{
			return info;
		}

		for (auto i = 0; i < n; ++i)
		{
			values[i] = w.get()[i];
			d[i * n + i] = w.get()[i];
		}

		return info;
	}
	catch (std::bad_alloc&)
	{
		return INSUFFICIENT_MEMORY;
	}
}

template<typename R, typename T, typename SYEV>
inline lapack_int sym_eigen_factor(lapack_int n, T a[], T vectors[], lapack_complex_double values[], T d[], SYEV syev)
{
	try
	{
		auto clone_a = array_clone(n * n, a);
		auto w = array_new<R>(n);

		lapack_int info = syev(LAPACK_COL_MAJOR, 'V', 'U', n, clone_a.get(), n, w.get());
		if (info != 0)
		{
			return info;
		}

		memcpy(vectors, clone_a.get(), n*n*sizeof(T));

		for (auto index = 0; index < n; ++index)
		{
			values[index] = lapack_complex_double(w.get()[index]);
		}

		for (auto j = 0; j < n; ++j)
		{
			auto jn = j*n;

			for (auto i = 0; i < n; ++i)
			{
				if (i == j)
				{
					d[jn + i] = w.get()[i];
				}
			}
		}

		return info;
	}
	catch (std::bad_alloc&)
	{
		return INSUFFICIENT_MEMORY;
	}
}

extern "C" {

	DLLEXPORT float s_matrix_norm(char norm, lapack_int m, lapack_int n, float a[])
	{
		return LAPACKE_slange(LAPACK_COL_MAJOR, norm, m, n, a, m);
	}

	DLLEXPORT double d_matrix_norm(char norm, lapack_int m, lapack_int n, double a[])
	{
		return LAPACKE_dlange(LAPACK_COL_MAJOR, norm, m, n, a, m);
	}

	DLLEXPORT float c_matrix_norm(char norm, lapack_int m, lapack_int n,  lapack_complex_float a[])
	{
		return LAPACKE_clange(LAPACK_COL_MAJOR, norm, m, n, a, m);
	}

	DLLEXPORT double z_matrix_norm(char norm, lapack_int m, lapack_int n, lapack_complex_double a[])
	{
		return LAPACKE_zlange(LAPACK_COL_MAJOR, norm, m, n, a, m);
	}

	DLLEXPORT lapack_int s_lu_factor(lapack_int m, float a[], lapack_int ipiv[])
	{
		return lu_factor(m, a, ipiv, LAPACKE_sgetrf);
	}

	DLLEXPORT lapack_int d_lu_factor(lapack_int m, double a[], lapack_int ipiv[])
	{
		return lu_factor(m, a, ipiv, LAPACKE_dgetrf);
	}

	DLLEXPORT lapack_int c_lu_factor(lapack_int m, lapack_complex_float a[], lapack_int ipiv[])
	{
		return lu_factor(m, a, ipiv, LAPACKE_cgetrf);
	}

	DLLEXPORT lapack_int z_lu_factor(lapack_int m, lapack_complex_double a[], lapack_int ipiv[])
	{
		return lu_factor(m, a, ipiv, LAPACKE_zgetrf);
	}

	DLLEXPORT lapack_int s_lu_inverse(lapack_int n, float a[], float work[], lapack_int lwork)
	{
		return lu_inverse(n, a, LAPACKE_sgetrf, LAPACKE_sgetri);
	}

	DLLEXPORT lapack_int d_lu_inverse(lapack_int n, double a[], double work[], lapack_int lwork)
	{
		return lu_inverse(n, a, LAPACKE_dgetrf, LAPACKE_dgetri);
	}

	DLLEXPORT lapack_int c_lu_inverse(lapack_int n, lapack_complex_float a[], lapack_complex_float work[], lapack_int lwork)
	{
		return lu_inverse(n, a, LAPACKE_cgetrf, LAPACKE_cgetri);
	}

	DLLEXPORT lapack_int z_lu_inverse(lapack_int n, lapack_complex_double a[], lapack_complex_double work[], lapack_int lwork)
	{
		return lu_inverse(n, a, LAPACKE_zgetrf, LAPACKE_zgetri);
	}

	DLLEXPORT lapack_int s_lu_inverse_factored(lapack_int n, float a[], lapack_int ipiv[], float work[], lapack_int lwork)
	{
		return lu_inverse_factored(n, a, ipiv, LAPACKE_sgetri);
	}

	DLLEXPORT lapack_int d_lu_inverse_factored(lapack_int n, double a[], lapack_int ipiv[], double work[], lapack_int lwork)
	{
		return lu_inverse_factored(n, a, ipiv, LAPACKE_dgetri);
	}

	DLLEXPORT lapack_int c_lu_inverse_factored(lapack_int n, lapack_complex_float a[], lapack_int ipiv[], lapack_complex_float work[], lapack_int lwork)
	{
		return lu_inverse_factored(n, a, ipiv, LAPACKE_cgetri);
	}

	DLLEXPORT lapack_int z_lu_inverse_factored(lapack_int n, lapack_complex_double a[], lapack_int ipiv[], lapack_complex_double work[], lapack_int lwork)
	{
		return lu_inverse_factored(n, a, ipiv, LAPACKE_zgetri);
	}

	DLLEXPORT lapack_int s_lu_solve_factored(lapack_int n, lapack_int nrhs, float a[], lapack_int ipiv[], float b[])
	{
		return lu_solve_factored(n, nrhs, a, ipiv, b, LAPACKE_sgetrs);
	}

	DLLEXPORT lapack_int  d_lu_solve_factored(lapack_int n, lapack_int nrhs, double a[], lapack_int ipiv[], double b[])
	{
		return lu_solve_factored(n, nrhs, a, ipiv, b, LAPACKE_dgetrs);
	}

	DLLEXPORT lapack_int c_lu_solve_factored(lapack_int n, lapack_int nrhs, lapack_complex_float a[], lapack_int ipiv[], lapack_complex_float b[])
	{
		return lu_solve_factored(n, nrhs, a, ipiv, b, LAPACKE_cgetrs);
	}

	DLLEXPORT lapack_int z_lu_solve_factored(lapack_int n, lapack_int nrhs, lapack_complex_double a[], lapack_int ipiv[], lapack_complex_double b[])
	{
		return lu_solve_factored(n, nrhs, a, ipiv, b, LAPACKE_zgetrs);
	}

	DLLEXPORT lapack_int s_lu_solve(lapack_int n, lapack_int nrhs, float a[], float b[])
	{
		return lu_solve(n, nrhs, a, b, LAPACKE_sgetrf, LAPACKE_sgetrs);
	}

	DLLEXPORT lapack_int d_lu_solve(lapack_int n, lapack_int nrhs, double a[], double b[])
	{
		return lu_solve(n, nrhs, a, b, LAPACKE_dgetrf, LAPACKE_dgetrs);
	}

	DLLEXPORT lapack_int c_lu_solve(lapack_int n, lapack_int nrhs, lapack_complex_float a[], lapack_complex_float b[])
	{
		return lu_solve(n, nrhs, a, b, LAPACKE_cgetrf, LAPACKE_cgetrs);
	}

	DLLEXPORT lapack_int z_lu_solve(lapack_int n, lapack_int nrhs, lapack_complex_double a[], lapack_complex_double b[])
	{
		return lu_solve(n, nrhs, a, b, LAPACKE_zgetrf, LAPACKE_zgetrs);
	}

	DLLEXPORT lapack_int s_cholesky_factor(lapack_int n, float a[])
	{
		return cholesky_factor(n, a, LAPACKE_spotrf);
	}

	DLLEXPORT lapack_int d_cholesky_factor(lapack_int n, double* a)
	{
		return cholesky_factor(n, a, LAPACKE_dpotrf);
	}

	DLLEXPORT lapack_int c_cholesky_factor(lapack_int n, lapack_complex_float a[])
	{
		return cholesky_factor(n, a, LAPACKE_cpotrf);
	}

	DLLEXPORT lapack_int z_cholesky_factor(lapack_int n, lapack_complex_double a[])
	{
		return cholesky_factor(n, a, LAPACKE_zpotrf);
	}

	DLLEXPORT lapack_int s_cholesky_solve(lapack_int n, lapack_int nrhs, float a[], float b[])
	{
		return cholesky_solve(n, nrhs, a, b, LAPACKE_spotrf, LAPACKE_spotrs);
	}

	DLLEXPORT lapack_int d_cholesky_solve(lapack_int n, lapack_int nrhs, double a[], double b[])
	{
		return cholesky_solve(n, nrhs, a, b, LAPACKE_dpotrf, LAPACKE_dpotrs);
	}

	DLLEXPORT lapack_int c_cholesky_solve(lapack_int n, lapack_int nrhs, lapack_complex_float a[], lapack_complex_float b[])
	{
		return cholesky_solve(n, nrhs, a, b, LAPACKE_cpotrf, LAPACKE_cpotrs);
	}

	DLLEXPORT lapack_int z_cholesky_solve(lapack_int n, lapack_int nrhs, lapack_complex_double a[], lapack_complex_double b[])
	{
		return cholesky_solve(n, nrhs, a, b, LAPACKE_zpotrf, LAPACKE_zpotrs);
	}

	DLLEXPORT lapack_int s_cholesky_solve_factored(lapack_int n, lapack_int nrhs, float a[], float b[])
	{
		return LAPACKE_spotrs(LAPACK_COL_MAJOR, 'L', n, nrhs, a, n, b, n);
	}

	DLLEXPORT lapack_int d_cholesky_solve_factored(lapack_int n, lapack_int nrhs, double a[], double b[])
	{
		return LAPACKE_dpotrs(LAPACK_COL_MAJOR, 'L', n, nrhs, a, n, b, n);
	}

	DLLEXPORT lapack_int c_cholesky_solve_factored(lapack_int n, lapack_int nrhs, lapack_complex_float a[], lapack_complex_float b[])
	{
		return LAPACKE_cpotrs(LAPACK_COL_MAJOR, 'L', n, nrhs, a, n, b, n);
	}

	DLLEXPORT lapack_int z_cholesky_solve_factored(lapack_int n, lapack_int nrhs, lapack_complex_double a[], lapack_complex_double b[])
	{
		return LAPACKE_zpotrs(LAPACK_COL_MAJOR, 'L', n, nrhs, a, n, b, n);
	}

	DLLEXPORT lapack_int s_qr_factor(lapack_int m, lapack_int n, float r[], float tau[], float q[])
	{
		return qr_factor(m, n, r, tau, q, LAPACKE_sgeqrf, LAPACKE_sorgqr);
	}

	DLLEXPORT lapack_int s_qr_thin_factor(lapack_int m, lapack_int n, float q[], float tau[], float r[])
	{
		return qr_thin_factor(m, n, q, tau, r, LAPACKE_sgeqrf, LAPACKE_sorgqr);
	}

	DLLEXPORT lapack_int d_qr_factor(lapack_int m, lapack_int n, double r[], double tau[], double q[])
	{
		return qr_factor(m, n, r, tau, q, LAPACKE_dgeqrf, LAPACKE_dorgqr);
	}

	DLLEXPORT lapack_int d_qr_thin_factor(lapack_int m, lapack_int n, double q[], double tau[], double r[])
	{
		return qr_thin_factor(m, n, q, tau, r, LAPACKE_dgeqrf, LAPACKE_dorgqr);
	}

	DLLEXPORT lapack_int c_qr_factor(lapack_int m, lapack_int n, lapack_complex_float r[], lapack_complex_float tau[], lapack_complex_float q[])
	{
		return qr_factor(m, n, r, tau, q, LAPACKE_cgeqrf, LAPACKE_cungqr);
	}

	DLLEXPORT lapack_int c_qr_thin_factor(lapack_int m, lapack_int n, lapack_complex_float q[], lapack_complex_float tau[], lapack_complex_float r[])
	{
		return qr_thin_factor(m, n, q, tau, r, LAPACKE_cgeqrf, LAPACKE_cungqr);
	}

	DLLEXPORT lapack_int z_qr_factor(lapack_int m, lapack_int n, lapack_complex_double r[], lapack_complex_double tau[], lapack_complex_double q[])
	{
		return qr_factor(m, n, r, tau, q, LAPACKE_zgeqrf, LAPACKE_zungqr);
	}

	DLLEXPORT lapack_int z_qr_thin_factor(lapack_int m, lapack_int n, lapack_complex_double q[], lapack_complex_double tau[], lapack_complex_double r[])
	{
		return qr_thin_factor(m, n, q, tau, r, LAPACKE_zgeqrf, LAPACKE_zungqr);
	}

	DLLEXPORT lapack_int s_qr_solve(lapack_int m, lapack_int n, lapack_int bn, float a[], float b[], float x[])
	{
		return qr_solve(m, n, bn, a, b, x, LAPACKE_sgels);
	}

	DLLEXPORT lapack_int d_qr_solve(lapack_int m, lapack_int n, lapack_int bn, double a[], double b[], double x[])
	{
		return qr_solve(m, n, bn, a, b, x, LAPACKE_dgels);
	}

	DLLEXPORT lapack_int c_qr_solve(lapack_int m, lapack_int n, lapack_int bn, lapack_complex_float a[], lapack_complex_float b[], lapack_complex_float x[])
	{
		return qr_solve(m, n, bn, a, b, x, LAPACKE_cgels);
	}

	DLLEXPORT lapack_int z_qr_solve(lapack_int m, lapack_int n, lapack_int bn, lapack_complex_double a[], lapack_complex_double b[], lapack_complex_double x[])
	{
		return qr_solve(m, n, bn, a, b, x, LAPACKE_zgels);
	}

	DLLEXPORT lapack_int s_qr_solve_factored(lapack_int m, lapack_int n, lapack_int bn, float r[], float b[], float tau[], float x[])
	{
		return qr_solve_factored(m, n, bn, r, b, tau, x, LAPACKE_sormqr, cblas_strsm);
	}

	DLLEXPORT lapack_int d_qr_solve_factored(lapack_int m, lapack_int n, lapack_int bn, double r[], double b[], double tau[], double x[])
	{
		return qr_solve_factored(m, n, bn, r, b, tau, x, LAPACKE_dormqr, cblas_dtrsm);
	}

	DLLEXPORT lapack_int c_qr_solve_factored(lapack_int m, lapack_int n, lapack_int bn, lapack_complex_float r[], lapack_complex_float b[], lapack_complex_float tau[], lapack_complex_float x[])
	{
		return complex_qr_solve_factored<lapack_complex_float, float>(m, n, bn, r, b, tau, x, LAPACKE_cunmqr, cblas_ctrsm);
	}

	DLLEXPORT lapack_int z_qr_solve_factored(lapack_int m, lapack_int n, lapack_int bn, lapack_complex_double r[], lapack_complex_double b[], lapack_complex_double tau[], lapack_complex_double x[])
	{
		return complex_qr_solve_factored<lapack_complex_double, double>(m, n, bn, r, b, tau, x, LAPACKE_zunmqr, cblas_ztrsm);
	}

	DLLEXPORT lapack_int s_svd_factor(bool compute_vectors, lapack_int m, lapack_int n, float a[], float s[], float u[], float v[])
	{
		return svd_factor(compute_vectors, m, n, a, s, u, v, LAPACKE_sgesvd);
	}

	DLLEXPORT lapack_int d_svd_factor(bool compute_vectors, lapack_int m, lapack_int n, double a[], double s[], double u[], double v[])
	{
		return svd_factor(compute_vectors, m, n, a, s, u, v, LAPACKE_dgesvd);
	}

	DLLEXPORT lapack_int c_svd_factor(bool compute_vectors, lapack_int m, lapack_int n, lapack_complex_float a[], lapack_complex_float s[], lapack_complex_float u[], lapack_complex_float v[])
	{
		return complex_svd_factor<lapack_complex_float, float>(compute_vectors, m, n, a, s, u, v, LAPACKE_cgesvd);
	}

	DLLEXPORT lapack_int z_svd_factor(bool compute_vectors, lapack_int m, lapack_int n, lapack_complex_double a[], lapack_complex_double s[], lapack_complex_double u[], lapack_complex_double v[])
	{
		return complex_svd_factor<lapack_complex_double, double>(compute_vectors, m, n, a, s, u, v, LAPACKE_zgesvd);
	}

	DLLEXPORT lapack_int s_eigen(bool isSymmetric, lapack_int n, float a[], float vectors[], lapack_complex_double values[], float d[])
	{
		if (isSymmetric)
		{
			return sym_eigen_factor<float>(n, a, vectors, values, d, LAPACKE_ssyev);
		}
		else
		{
			return eigen_factor(n, a, vectors, values, d, LAPACKE_sgees, LAPACKE_strevc);
		}
	}

	DLLEXPORT lapack_int d_eigen(bool isSymmetric, lapack_int n, double a[], double vectors[], lapack_complex_double values[], double d[])
	{
		if (isSymmetric)
		{
			return sym_eigen_factor<double>(n, a, vectors, values, d, LAPACKE_dsyev);
		}
		else
		{
			return eigen_factor(n, a, vectors, values, d, LAPACKE_dgees, LAPACKE_dtrevc);
		}
	}

	DLLEXPORT lapack_int c_eigen(bool isSymmetric, lapack_int n, lapack_complex_float a[], lapack_complex_float vectors[], lapack_complex_double values[], lapack_complex_float d[])
	{
		if (isSymmetric)
		{
			return sym_eigen_factor<float>(n, a, vectors, values, d, LAPACKE_cheev);
		}
		else
		{
			return eigen_complex_factor(n, a, vectors, values, d, LAPACKE_cgees, LAPACKE_ctrevc);
		}
	}

	DLLEXPORT lapack_int z_eigen(bool isSymmetric, lapack_int n, lapack_complex_double a[], lapack_complex_double vectors[], lapack_complex_double values[], lapack_complex_double d[])
	{
		if (isSymmetric)
		{
			return sym_eigen_factor<double>(n, a, vectors, values, d, LAPACKE_zheev);
		}
		else
		{
			return eigen_complex_factor(n, a, vectors, values, d, LAPACKE_zgees, LAPACKE_ztrevc);
		}
	}
}
