#include "cblas.h"

#define LAPACK_COMPLEX_CUSTOM

template <typename T>
struct complex
{
	T real, imag;

	complex(T _real = 0, T _imag = 0)
	{
		real = _real;
		imag = _imag;
	}

	complex(const complex<T>& right)
	{
		real = right.real;
		imag = right.imag;
	}
	
	complex& operator=(const complex& right)
	{
		real = right.real;
		imag = right.imag;
		return *this;
	}

	complex& operator=(const T& right)
	{
		real = right;
		imag = 0;
		return *this;
	}

	template<typename _Other> inline
	complex& operator=(const complex<_Other>& right)
	{
		real = (T)right.real;
		imag = (T)right.imag;
		return *this;
	}
};

#define lapack_complex_float complex<float>
#define lapack_complex_double complex<double>

#include "lapacke.h"
#include "lapack_common.h"
#include "wrapper_common.h"
#include <algorithm>

template<typename T, typename GETRF>
inline lapack_int lu_factor(lapack_int m, T a[], lapack_int ipiv[], GETRF getrf)
{
    lapack_int info = 0;
    getrf(&m, &m, a, &m, ipiv, &info);
    shift_ipiv_down(m, ipiv);
    return info;
};

template<typename T, typename GETRF, typename GETRI>
inline lapack_int lu_inverse(lapack_int n, T a[], T work[], lapack_int lwork, GETRF getrf, GETRI getri)
{
    lapack_int* ipiv = new lapack_int[n];
    lapack_int info = 0;
    getrf(&n, &n, a, &n, ipiv, &info);

    if (info != 0)
    {
        delete[] ipiv;
        return info;
    }

    getri(&n, a, &n, ipiv, work, &lwork, &info);
    delete[] ipiv;
    return info;
};

template<typename T, typename GETRI>
inline lapack_int lu_inverse_factored(lapack_int n, T a[], lapack_int ipiv[], T work[], lapack_int lwork, GETRI getri)
{
    shift_ipiv_up(n, ipiv);
    lapack_int info = 0;
    getri(&n, a, &n, ipiv, work, &lwork, &info);
    shift_ipiv_down(n, ipiv);
    return info;
}

template<typename T, typename GETRS>
inline lapack_int lu_solve_factored(lapack_int n, lapack_int nrhs, T a[], lapack_int ipiv[], T b[], GETRS getrs)
{
    shift_ipiv_up(n, ipiv);
    lapack_int info = 0;
    char trans ='N';
    getrs(&trans, &n, &nrhs, a, &n, ipiv, b, &n, &info);
    shift_ipiv_down(n, ipiv);
    return info;
}

template<typename T, typename GETRF, typename GETRS>
inline lapack_int lu_solve(lapack_int n, lapack_int nrhs, T a[], T b[], GETRF getrf, GETRS getrs)
{
    T* clone = Clone(n, n, a);
    lapack_int* ipiv = new lapack_int[n];
    lapack_int info = 0;
    getrf(&n, &n, clone, &n, ipiv, &info);

    if (info != 0)
    {
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


template<typename T, typename POTRF>
inline lapack_int cholesky_factor(lapack_int n, T* a, POTRF potrf)
{
    char uplo = 'L';
    lapack_int info = 0;
    potrf(&uplo, &n, a, &n, &info);
    T zero = T();

    for (lapack_int i = 0; i < n; ++i)
    {
        lapack_int index = i * n;

        for (lapack_int j = 0; j < n && i > j; ++j)
        {
            a[index + j] = zero;
        }
    }

    return info;
}

template<typename T, typename POTRF, typename POTRS>
inline lapack_int cholesky_solve(lapack_int n, lapack_int nrhs, T a[], T b[], POTRF potrf, POTRS potrs)
{
    T* clone = Clone(n, n, a);
    char uplo = 'L';
    lapack_int info = 0;
    potrf(&uplo, &n, clone, &n, &info);

    if (info != 0)
    {
        delete[] clone;
        return info;
    }

    potrs(&uplo, &n, &nrhs, clone, &n, b, &n, &info);
    delete[] clone;
    return info;
}

template<typename T, typename POTRS>
inline lapack_int cholesky_solve_factored(lapack_int n, lapack_int nrhs, T a[], T b[], POTRS potrs)
{
    char uplo = 'L';
    lapack_int info = 0;
    potrs(&uplo, &n, &nrhs, a, &n, b, &n, &info);
    return info;
}

template<typename T, typename GEQRF, typename ORGQR>
inline lapack_int qr_factor(lapack_int m, lapack_int n, T r[], T tau[], T q[], T work[], lapack_int len, GEQRF geqrf, ORGQR orgqr)
{
    lapack_int info = 0;
    geqrf(&m, &n, r, &m, tau, work, &len, &info);

    for (lapack_int i = 0; i < m; ++i)
    {
        for (lapack_int j = 0; j < m && j < n; ++j)
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
inline lapack_int qr_thin_factor(lapack_int m, lapack_int n, T q[], T tau[], T r[], T work[], lapack_int len, GEQRF geqrf, ORGQR orgqr)
{
    lapack_int info = 0;
    geqrf(&m, &n, q, &m, tau, work, &len, &info);

    for (lapack_int i = 0; i < n; ++i)
    {
        for (lapack_int j = 0; j < n; ++j)
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
inline lapack_int qr_solve(lapack_int m, lapack_int n, lapack_int bn, T a[], T b[], T x[], T work[], lapack_int len, GELS gels)
{
    T* clone_a = Clone(m, n, a);
    T* clone_b = Clone(m, bn, b);
    char N = 'N';
    lapack_int info = 0;
    gels(&N, &m, &n, &bn, clone_a, &m, clone_b, &m, work, &len, &info);
    copyBtoX(m, n, bn, clone_b, x);
    delete[] clone_a;
    delete[] clone_b;
    return info;
}

template<typename T, typename ORMQR, typename TRSM>
inline lapack_int qr_solve_factored(lapack_int m, lapack_int n, lapack_int bn, T r[], T b[], T tau[], T x[], T work[], lapack_int len, ORMQR ormqr, TRSM trsm)
{
    T* clone_b = Clone(m, bn, b);
    char side ='L';
    char tran = 'T';
    lapack_int info = 0;
    ormqr(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info);
    trsm(CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, 1.0, r, m, clone_b, m);
    copyBtoX(m, n, bn, clone_b, x);
    delete[] clone_b;
    return info;
}

template<typename T, typename UNMQR, typename TRSM>
inline lapack_int complex_qr_solve_factored(lapack_int m, lapack_int n, lapack_int bn, T r[], T b[], T tau[], T x[], T work[], lapack_int len, UNMQR unmqr, TRSM trsm)
{
    T* clone_b = Clone(m, bn, b);
    char side ='L';
    char tran = 'C';
    lapack_int info = 0;
    unmqr(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info);
    T one = { 1.0f, 0.0f };
	trsm(CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, &(one.real), &(r->real), m, &(clone_b->real), m);
    copyBtoX(m, n, bn, clone_b, x);
    delete[] clone_b;
    return info;
}

template<typename T, typename GESVD>
inline lapack_int svd_factor(bool compute_vectors, lapack_int m, lapack_int n, T a[], T s[], T u[], T v[], T work[], lapack_int len, GESVD gesvd)
{
    lapack_int info = 0;
    char job = compute_vectors ? 'A' : 'N';
    gesvd(&job, &job, &m, &n, a, &m, s, u, &m, v, &n, work, &len, &info);
    return info;
}

template<typename T, typename R, typename GESVD>
inline lapack_int complex_svd_factor(bool compute_vectors, lapack_int m, lapack_int n, T a[], T s[], T u[], T v[], T work[], lapack_int len, GESVD gesvd)
{
    lapack_int info = 0;
    lapack_int dim_s = std::min(m,n);
    R* rwork = new R[5 * dim_s];
    R* s_local = new R[dim_s];
    char job = compute_vectors ? 'A' : 'N';
    gesvd(&job, &job, &m, &n, a, &m, s_local, u, &m, v, &n, work, &len, rwork, &info);

    for (lapack_int index = 0; index < dim_s; ++index)
    {
        s[index] = s_local[index];
    }

    delete[] rwork;
    delete[] s_local;
    return info;
}

template<typename T, typename R, typename GEES, typename TREVC>
inline lapack_int eigen_factor(lapack_int n, T a[], T vectors[], R values[], T d[], GEES gees, TREVC trevc)
{
    T* clone_a = Clone(n, n, a);
    T* wr = new T[n];
    T* wi = new T[n];

    lapack_int sdim;
    lapack_int info = gees(LAPACK_COL_MAJOR, 'V', 'N', nullptr, n, clone_a, n, &sdim, wr, wi, vectors, n);
    if (info != 0)
    {
        delete[] clone_a;
        delete[] wr;
        delete[] wi;
        return info;
    }

    lapack_int m;
    info = trevc(LAPACK_COL_MAJOR, 'R', 'B', nullptr, n, clone_a, n, nullptr, n, vectors, n, n, &m);
    if (info != 0)
    {
        delete[] clone_a;
        delete[] wr;
        delete[] wi;
        return info;
    }

    for (lapack_int index = 0; index < n; ++index)
    {
        values[index] = R(wr[index], wi[index]);
    }

    for (lapack_int  i = 0; i < n; ++i)
    {
        lapack_int in = i * n;
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
inline lapack_int eigen_complex_factor(lapack_int n, T a[], T vectors[], lapack_complex_double values[], T d[], GEES gees, TREVC trevc)
{
    T* clone_a = Clone(n, n, a);
    T* w = new T[n];

    lapack_int sdim;
    lapack_int info = gees(LAPACK_COL_MAJOR, 'V', 'N', nullptr, n, clone_a, n, &sdim, w, vectors, n);
    if (info != 0)
    {
        delete[] clone_a;
        delete[] w;
        return info;
    }

    lapack_int m;
    info = trevc(LAPACK_COL_MAJOR, 'R', 'B', nullptr, n, clone_a, n, nullptr, n, vectors, n, n, &m);
    if (info != 0)
    {
        delete[] clone_a;
        delete[] w;
        return info;
    }

    for (lapack_int i = 0; i < n; ++i)
    {
        values[i] = w[i];
        d[i * n + i] = w[i];
    }

    delete[] clone_a;
    delete[] w;
    return info;
}

template<typename R, typename T, typename SYEV>
inline lapack_int sym_eigen_factor(lapack_int n, T a[], T vectors[], lapack_complex_double values[], T d[], SYEV syev)
{
    T* clone_a = Clone(n, n, a);
    R* w = new R[n];

    lapack_int info = syev(LAPACK_COL_MAJOR, 'V', 'U', n, clone_a, n, w);
    if (info != 0)
    {
        delete[] clone_a;
        delete[] w;
        return info;
    }
    
    memcpy(vectors, clone_a, n*n*sizeof(T));

    for (lapack_int index = 0; index < n; ++index)
    {
        values[index] = lapack_complex_double(w[index]);
    }
    
    for (lapack_int j = 0; j < n; ++j)
    {
        lapack_int jn = j*n;

        for (lapack_int i = 0; i < n; ++i)
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

    DLLEXPORT float s_matrix_norm(char norm, lapack_int m, lapack_int n, float a[], float work[])
    {
        return LAPACKE_slange_work(CblasColMajor, norm, m, n, a, m, work);
    }

    DLLEXPORT double d_matrix_norm(char norm, lapack_int m, lapack_int n, double a[], double work[])
    {
        return LAPACKE_dlange_work(CblasColMajor, norm, m, n, a, m, work);
    }

    DLLEXPORT float c_matrix_norm(char norm, lapack_int m, lapack_int n, lapack_complex_float a[], float work[])
    {
        return LAPACKE_clange_work(CblasColMajor, norm, m, n, a, m, work);
    }

    DLLEXPORT double z_matrix_norm(char norm, lapack_int m, lapack_int n, lapack_complex_double a[], double work[])
    {
        return LAPACKE_zlange_work(CblasColMajor, norm, m, n, a, m, work);
    }

    DLLEXPORT lapack_int s_lu_factor(lapack_int m, float a[], lapack_int ipiv[])
    {
        return lu_factor(m, a, ipiv, LAPACK_sgetrf);
    }

    DLLEXPORT lapack_int d_lu_factor(lapack_int m, double a[], lapack_int ipiv[])
    {
        return lu_factor(m, a, ipiv, LAPACK_dgetrf);
    }

    DLLEXPORT lapack_int c_lu_factor(lapack_int m, lapack_complex_float a[], lapack_int ipiv[])
    {
        return lu_factor(m, a, ipiv, LAPACK_cgetrf);
    }

    DLLEXPORT lapack_int z_lu_factor(lapack_int m, lapack_complex_double a[], lapack_int ipiv[])
    {
        return lu_factor(m, a, ipiv, LAPACK_zgetrf);
    }

    DLLEXPORT lapack_int s_lu_inverse(lapack_int n, float a[], float work[], lapack_int lwork)
    {
        return lu_inverse(n, a, work, lwork, LAPACK_sgetrf, LAPACK_sgetri);
    }

    DLLEXPORT lapack_int d_lu_inverse(lapack_int n, double a[], double work[], lapack_int lwork)
    {
        return lu_inverse(n, a, work, lwork, LAPACK_dgetrf, LAPACK_dgetri);
    }

    DLLEXPORT lapack_int c_lu_inverse(lapack_int n, lapack_complex_float a[], lapack_complex_float work[], lapack_int lwork)
    {
        return lu_inverse(n, a, work, lwork, LAPACK_cgetrf, LAPACK_cgetri);
    }

    DLLEXPORT lapack_int z_lu_inverse(lapack_int n, lapack_complex_double a[], lapack_complex_double work[], lapack_int lwork)
    {
        return lu_inverse(n, a, work, lwork, LAPACK_zgetrf, LAPACK_zgetri);
    }

    DLLEXPORT lapack_int s_lu_inverse_factored(lapack_int n, float a[], lapack_int ipiv[], float work[], lapack_int lwork)
    {
        return lu_inverse_factored(n, a, ipiv, work, lwork, LAPACK_sgetri);
    }

    DLLEXPORT lapack_int d_lu_inverse_factored(lapack_int n, double a[], lapack_int ipiv[], double work[], lapack_int lwork)
    {
        return lu_inverse_factored(n, a, ipiv, work, lwork, LAPACK_dgetri);
    }

    DLLEXPORT lapack_int c_lu_inverse_factored(lapack_int n, lapack_complex_float a[], lapack_int ipiv[], lapack_complex_float work[], lapack_int lwork)
    {
        return lu_inverse_factored(n, a, ipiv, work, lwork, LAPACK_cgetri);
    }

    DLLEXPORT lapack_int z_lu_inverse_factored(lapack_int n, lapack_complex_double a[], lapack_int ipiv[], lapack_complex_double work[], lapack_int lwork)
    {
        return lu_inverse_factored(n, a, ipiv, work, lwork, LAPACK_zgetri);
    }

    DLLEXPORT lapack_int s_lu_solve_factored(lapack_int n, lapack_int nrhs, float a[], lapack_int ipiv[], float b[])
    {
        return lu_solve_factored(n, nrhs, a, ipiv, b, LAPACK_sgetrs);
    }

    DLLEXPORT lapack_int  d_lu_solve_factored(lapack_int n, lapack_int nrhs, double a[], lapack_int ipiv[], double b[])
    {
        return lu_solve_factored(n, nrhs, a, ipiv, b, LAPACK_dgetrs);
    }

    DLLEXPORT lapack_int c_lu_solve_factored(lapack_int n, lapack_int nrhs, lapack_complex_float a[], lapack_int ipiv[], lapack_complex_float b[])
    {
        return lu_solve_factored(n, nrhs, a, ipiv, b, LAPACK_cgetrs);
    }

    DLLEXPORT lapack_int z_lu_solve_factored(lapack_int n, lapack_int nrhs, lapack_complex_double a[], lapack_int ipiv[], lapack_complex_double b[])
    {
        return lu_solve_factored(n, nrhs, a, ipiv, b, LAPACK_zgetrs);
    }

    DLLEXPORT lapack_int s_lu_solve(lapack_int n, lapack_int nrhs, float a[], float b[])
    {
        return lu_solve(n, nrhs, a, b, LAPACK_sgetrf, LAPACK_sgetrs);
    }

    DLLEXPORT lapack_int d_lu_solve(lapack_int n, lapack_int nrhs, double a[], double b[])
    {
        return lu_solve(n, nrhs, a, b, LAPACK_dgetrf, LAPACK_dgetrs);
    }

    DLLEXPORT lapack_int c_lu_solve(lapack_int n, lapack_int nrhs, lapack_complex_float a[], lapack_complex_float b[])
    {
        return lu_solve(n, nrhs, a, b, LAPACK_cgetrf, LAPACK_cgetrs);
    }

    DLLEXPORT lapack_int z_lu_solve(lapack_int n, lapack_int nrhs, lapack_complex_double a[],  lapack_complex_double b[])
    {
        return lu_solve(n, nrhs, a, b, LAPACK_zgetrf, LAPACK_zgetrs);
    }

    DLLEXPORT lapack_int s_cholesky_factor(lapack_int n, float a[])
    {
        return cholesky_factor(n, a, LAPACK_spotrf);
    }

    DLLEXPORT lapack_int d_cholesky_factor(lapack_int n, double* a)
    {
        return cholesky_factor(n, a, LAPACK_dpotrf);
    }

    DLLEXPORT lapack_int c_cholesky_factor(lapack_int n, lapack_complex_float a[])
    {
        return cholesky_factor(n, a, LAPACK_cpotrf);
    }

    DLLEXPORT lapack_int z_cholesky_factor(lapack_int n, lapack_complex_double a[])
    {
        return cholesky_factor(n, a, LAPACK_zpotrf);
    }

    DLLEXPORT lapack_int s_cholesky_solve(lapack_int n, lapack_int nrhs, float a[], float b[])
    {
        return cholesky_solve(n, nrhs, a, b, LAPACK_spotrf, LAPACK_spotrs);
    }

    DLLEXPORT lapack_int d_cholesky_solve(lapack_int n, lapack_int nrhs, double a[], double b[])
    {
        return cholesky_solve(n, nrhs, a, b, LAPACK_dpotrf, LAPACK_dpotrs);
    }

    DLLEXPORT lapack_int c_cholesky_solve(lapack_int n, lapack_int nrhs, lapack_complex_float a[], lapack_complex_float b[])
    {
        return cholesky_solve(n, nrhs, a, b, LAPACK_cpotrf, LAPACK_cpotrs);
    }

    DLLEXPORT lapack_int z_cholesky_solve(lapack_int n, lapack_int nrhs, lapack_complex_double a[], lapack_complex_double b[])
    {
        return cholesky_solve(n, nrhs, a, b, LAPACK_zpotrf, LAPACK_zpotrs);
    }

    DLLEXPORT lapack_int s_cholesky_solve_factored(lapack_int n, lapack_int nrhs, float a[], float b[])
    {
        return cholesky_solve_factored(n, nrhs, a, b, LAPACK_spotrs);
    }

    DLLEXPORT lapack_int d_cholesky_solve_factored(lapack_int n, lapack_int nrhs, double a[], double b[])
    {
        return cholesky_solve_factored(n, nrhs, a, b, LAPACK_dpotrs);
    }

    DLLEXPORT lapack_int c_cholesky_solve_factored(lapack_int n, lapack_int nrhs, lapack_complex_float a[], lapack_complex_float b[])
    {
        return cholesky_solve_factored(n, nrhs, a, b, LAPACK_cpotrs);
    }

    DLLEXPORT lapack_int z_cholesky_solve_factored(lapack_int n, lapack_int nrhs, lapack_complex_double a[], lapack_complex_double b[])
    {
        return cholesky_solve_factored(n, nrhs, a, b, LAPACK_zpotrs);
    }

    DLLEXPORT lapack_int s_qr_factor(lapack_int m, lapack_int n, float r[], float tau[], float q[], float work[], lapack_int len)
    {
        return qr_factor(m, n, r, tau, q, work, len, LAPACK_sgeqrf, LAPACK_sorgqr);
    }

    DLLEXPORT lapack_int s_qr_thin_factor(lapack_int m, lapack_int n, float q[], float tau[], float r[], float work[], lapack_int len)
    {
        return qr_thin_factor(m, n, q, tau, r, work, len, LAPACK_sgeqrf, LAPACK_sorgqr);
    }

    DLLEXPORT lapack_int d_qr_factor(lapack_int m, lapack_int n, double r[], double tau[], double q[], double work[], lapack_int len)
    {
        return qr_factor(m, n, r, tau, q, work, len, LAPACK_dgeqrf, LAPACK_dorgqr);
    }

    DLLEXPORT lapack_int d_qr_thin_factor(lapack_int m, lapack_int n, double q[], double tau[], double r[], double work[], lapack_int len)
    {
        return qr_thin_factor(m, n, q, tau, r, work, len, LAPACK_dgeqrf, LAPACK_dorgqr);
    }

    DLLEXPORT lapack_int c_qr_factor(lapack_int m, lapack_int n, lapack_complex_float r[], lapack_complex_float tau[], lapack_complex_float q[], lapack_complex_float work[], lapack_int len)
    {
        return qr_factor(m, n, r, tau, q, work, len, LAPACK_cgeqrf, LAPACK_cungqr);
    }

    DLLEXPORT lapack_int c_qr_thin_factor(lapack_int m, lapack_int n, lapack_complex_float q[], lapack_complex_float tau[], lapack_complex_float r[], lapack_complex_float work[], lapack_int len)
    {
        return qr_thin_factor(m, n, q, tau, r, work, len, LAPACK_cgeqrf, LAPACK_cungqr);
    }

    DLLEXPORT lapack_int z_qr_factor(lapack_int m, lapack_int n, lapack_complex_double r[], lapack_complex_double tau[], lapack_complex_double q[], lapack_complex_double work[], lapack_int len)
    {
        return qr_factor(m, n, r, tau, q, work, len, LAPACK_zgeqrf, LAPACK_zungqr);
    }

    DLLEXPORT lapack_int z_qr_thin_factor(lapack_int m, lapack_int n, lapack_complex_double q[], lapack_complex_double tau[], lapack_complex_double r[], lapack_complex_double work[], lapack_int len)
    {
        return qr_thin_factor(m, n, q, tau, r, work, len, LAPACK_zgeqrf, LAPACK_zungqr);
    }

    DLLEXPORT lapack_int s_qr_solve(lapack_int m, lapack_int n, lapack_int bn, float a[], float b[], float x[], float work[], lapack_int len)
    {
        return qr_solve(m, n, bn, a, b, x, work, len, LAPACK_sgels);
    }

    DLLEXPORT lapack_int d_qr_solve(lapack_int m, lapack_int n, lapack_int bn, double a[], double b[], double x[], double work[], lapack_int len)
    {
        return qr_solve(m, n, bn, a, b, x, work, len, LAPACK_dgels);
    }

    DLLEXPORT lapack_int c_qr_solve(lapack_int m, lapack_int n, lapack_int bn, lapack_complex_float a[], lapack_complex_float b[], lapack_complex_float x[], lapack_complex_float work[], lapack_int len)
    {
        return qr_solve(m, n, bn, a, b, x, work, len, LAPACK_cgels);
    }

    DLLEXPORT lapack_int z_qr_solve(lapack_int m, lapack_int n, lapack_int bn, lapack_complex_double a[], lapack_complex_double b[], lapack_complex_double x[], lapack_complex_double work[], lapack_int len)
    {
        return qr_solve(m, n, bn, a, b, x, work, len, LAPACK_zgels);
    }

    DLLEXPORT lapack_int s_qr_solve_factored(lapack_int m, lapack_int n, lapack_int bn, float r[], float b[], float tau[], float x[], float work[], lapack_int len)
    {
        return qr_solve_factored(m, n, bn, r, b, tau, x, work, len, LAPACK_sormqr, cblas_strsm);
    }

    DLLEXPORT lapack_int d_qr_solve_factored(lapack_int m, lapack_int n, lapack_int bn, double r[], double b[], double tau[], double x[], double work[], lapack_int len)
    {
        return qr_solve_factored(m, n, bn, r, b, tau, x, work, len, LAPACK_dormqr, cblas_dtrsm);
    }

    DLLEXPORT lapack_int c_qr_solve_factored(lapack_int m, lapack_int n, lapack_int bn, lapack_complex_float r[], lapack_complex_float b[], lapack_complex_float tau[], lapack_complex_float x[], lapack_complex_float work[], lapack_int len)
    {
        return complex_qr_solve_factored(m, n, bn, r, b, tau, x, work, len, LAPACK_cunmqr, cblas_ctrsm);
    }

    DLLEXPORT lapack_int z_qr_solve_factored(lapack_int m, lapack_int n, lapack_int bn, lapack_complex_double r[], lapack_complex_double b[], lapack_complex_double tau[], lapack_complex_double x[], lapack_complex_double work[], lapack_int len)
    {
        return complex_qr_solve_factored(m, n, bn, r, b, tau, x, work, len, LAPACK_zunmqr, cblas_ztrsm);
    }

    DLLEXPORT lapack_int s_svd_factor(bool compute_vectors, lapack_int m, lapack_int n, float a[], float s[], float u[], float v[], float work[], lapack_int len)
    {
        return svd_factor(compute_vectors, m, n, a, s, u, v, work, len, LAPACK_sgesvd);
    }

    DLLEXPORT lapack_int d_svd_factor(bool compute_vectors, lapack_int m, lapack_int n, double a[], double s[], double u[], double v[], double work[], lapack_int len)
    {
        return svd_factor(compute_vectors, m, n, a, s, u, v, work, len, LAPACK_dgesvd);
    }

    DLLEXPORT lapack_int c_svd_factor(bool compute_vectors, lapack_int m, lapack_int n, lapack_complex_float a[], lapack_complex_float s[], lapack_complex_float u[], lapack_complex_float v[], lapack_complex_float work[], lapack_int len)
    {
        return complex_svd_factor<lapack_complex_float, float>(compute_vectors, m, n, a, s, u, v, work, len, LAPACK_cgesvd);
    }

    DLLEXPORT lapack_int z_svd_factor(bool compute_vectors, lapack_int m, lapack_int n, lapack_complex_double a[], lapack_complex_double s[], lapack_complex_double u[], lapack_complex_double v[], lapack_complex_double work[], lapack_int len)
    {
        return complex_svd_factor<lapack_complex_double, double>(compute_vectors, m, n, a, s, u, v, work, len, LAPACK_zgesvd);
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
