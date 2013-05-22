#include <algorithm>
#include <complex>
#define MKL_Complex8 std::complex<float>
#define MKL_Complex16 std::complex<double>

#include "mkl_lapack.h"
#include "mkl_cblas.h"
#include "lapack_common.h"
#include "wrapper_common.h"
#include "mkl_lapacke.h"

template<typename T>
inline MKL_INT lu_factor(MKL_INT m, T a[], MKL_INT ipiv[],
                         void (*getrf)(const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*, MKL_INT*))
{
    std::complex<double> x = 5;
	MKL_INT info = 0;
    getrf(&m, &m, a, &m, ipiv, &info);
    shift_ipiv_down(m, ipiv);
    return info;
};

template<typename T>
inline MKL_INT lu_inverse(MKL_INT n, T a[], T work[], MKL_INT lwork,
                          void (*getrf)(const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*, MKL_INT*),
                          void (*getri)(const MKL_INT*, T*, const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*))
{
    MKL_INT* ipiv = new MKL_INT[n];
    MKL_INT info = 0;
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

template<typename T>
inline MKL_INT lu_inverse_factored(MKL_INT n, T a[], MKL_INT ipiv[], T work[], MKL_INT lwork,
                                   void (*getri)(const MKL_INT*, T*, const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*))
{
    shift_ipiv_up(n, ipiv);
    MKL_INT info = 0;
    getri(&n, a, &n, ipiv, work, &lwork, &info);
    shift_ipiv_down(n, ipiv);
    return info;
}

template<typename T>
inline MKL_INT lu_solve_factored(MKL_INT n, MKL_INT nrhs, T a[], MKL_INT ipiv[], T b[],
                                 void (*getrs)(const char*, const MKL_INT*, const MKL_INT*, const T*, const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*))
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
                        void (*getrf)(const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*, MKL_INT*),
                        void (*getrs)(const char*, const MKL_INT*, const MKL_INT*, const T*, const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*))
{
    T* clone = Clone(n, n, a);
    MKL_INT* ipiv = new MKL_INT[n];
    MKL_INT info = 0;
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


template<typename T>
inline MKL_INT cholesky_factor(MKL_INT n, T* a,
                               void (*potrf)(const char*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*))
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
                              void (*potrf)(const char*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*),
                              void (*potrs)(const char*, const MKL_INT*, const MKL_INT*, const T*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*))
{
    T* clone = Clone(n, n, a);
    char uplo = 'L';
    MKL_INT info = 0;
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

template<typename T>
inline MKL_INT cholesky_solve_factored(MKL_INT n, MKL_INT nrhs, T a[], T b[],
                                       void (*potrs)(const char*, const MKL_INT*, const MKL_INT*, const T*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*))
{
    char uplo = 'L';
    MKL_INT info = 0;
    potrs(&uplo, &n, &nrhs, a, &n, b, &n, &info);
    return info;
}

template<typename T>
inline MKL_INT qr_factor(MKL_INT m, MKL_INT n, T r[], T tau[], T q[], T work[], MKL_INT len,
                         void (*geqrf)(const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, T*, T*, const MKL_INT*, MKL_INT*),
                         void (*orgqr)(const MKL_INT*, const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, const T*, T*, const MKL_INT*, MKL_INT*))
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
                              void (*geqrf)(const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, T*, T*, const MKL_INT*, MKL_INT*),
                              void (*orgqr)(const MKL_INT*, const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, const T*, T*, const MKL_INT*, MKL_INT*))
{
    MKL_INT info = 0;
    geqrf(&m, &n, q, &m, tau, work, &len, &info);

    for (MKL_INT i = 0; i < n; ++i)
    {
        for (MKL_INT j = 0; j < n; ++j)
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

template<typename T>
inline MKL_INT qr_solve(MKL_INT m, MKL_INT n, MKL_INT bn, T a[], T b[], T x[], T work[], MKL_INT len,
                        void (*gels)(const char*, const MKL_INT*, const MKL_INT*, const MKL_INT*, T*,
                                     const MKL_INT*, T* b, const MKL_INT*, T*, const MKL_INT*, MKL_INT*))
{
    T* clone_a = Clone(m, n, a);
    T* clone_b = Clone(m, bn, b);
    char N = 'N';
    MKL_INT info = 0;
    gels(&N, &m, &n, &bn, clone_a, &m, clone_b, &m, work, &len, &info);
    copyBtoX(m, n, bn, clone_b, x);
    delete[] clone_a;
    delete[] clone_b;
    return info;
}

template<typename T>
inline MKL_INT qr_solve_factored(MKL_INT m, MKL_INT n, MKL_INT bn, T r[], T b[], T tau[], T x[], T work[], MKL_INT len,
                                 void (*ormqr)(const char*, const char*, const MKL_INT*, const MKL_INT*, const MKL_INT*,
                                         const T*, const MKL_INT*, const T*, T*, const MKL_INT*, T*, const MKL_INT*, MKL_INT* info),
                                 void (*trsm)(const CBLAS_ORDER, const CBLAS_SIDE, const CBLAS_UPLO, const CBLAS_TRANSPOSE, const CBLAS_DIAG,
                                         const MKL_INT, const MKL_INT, const T, const T*, const MKL_INT, T*, const MKL_INT))
{
    T* clone_b = Clone(m, bn, b);
    char side ='L';
    char tran = 'T';
    MKL_INT info = 0;
    ormqr(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info);
    trsm(CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, 1.0, r, m, clone_b, m);
    copyBtoX(m, n, bn, clone_b, x);
    delete[] clone_b;
    return info;
}

template<typename T>
inline MKL_INT complex_qr_solve_factored(MKL_INT m, MKL_INT n, MKL_INT bn, T r[], T b[], T tau[], T x[], T work[], MKL_INT len,
        void (*unmqr)(const char*, const char*, const MKL_INT*, const MKL_INT*, const MKL_INT*,
                      const T*, const MKL_INT*, const T*, T*, const MKL_INT*, T*, const MKL_INT*, MKL_INT* info),
        void (*trsm)(const CBLAS_ORDER, const CBLAS_SIDE, const CBLAS_UPLO, const CBLAS_TRANSPOSE, const CBLAS_DIAG,
                     const MKL_INT, const MKL_INT, const void*, const void*, const MKL_INT, void*, const MKL_INT ldb))
{
    T* clone_b = Clone(m, bn, b);
    char side ='L';
    char tran = 'C';
    MKL_INT info = 0;
    unmqr(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info);
	T one = 1.0f;
    trsm(CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, &one, r, m, clone_b, m);
    copyBtoX(m, n, bn, clone_b, x);
    delete[] clone_b;
    return info;
}

template<typename T>
inline MKL_INT svd_factor(bool compute_vectors, MKL_INT m, MKL_INT n, T a[], T s[], T u[], T v[], T work[], MKL_INT len,
                          void (*gesvd)(const char*, const char*, const MKL_INT*, const MKL_INT*, T*, const MKL_INT*,
                                        T*, T*, const MKL_INT*, T*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*))
{
    MKL_INT info = 0;
    char job = compute_vectors ? 'A' : 'N';
    gesvd(&job, &job, &m, &n, a, &m, s, u, &m, v, &n, work, &len, &info);
    return info;
}

template<typename T, typename R>
inline MKL_INT complex_svd_factor(bool compute_vectors, MKL_INT m, MKL_INT n, T a[], T s[], T u[], T v[], T work[], MKL_INT len,
                                  void (*gesvd)(const char*, const char*, const MKL_INT*, const MKL_INT*, T*, const MKL_INT*,
                                          R*, T*, const MKL_INT*, T*, const MKL_INT*, T*, const MKL_INT*, R*, MKL_INT*))
{
    MKL_INT info = 0;
    MKL_INT dim_s = std::min(m,n);
    R* rwork = new R[5 * dim_s];
    R* s_local = new R[dim_s];
    char job = compute_vectors ? 'A' : 'N';
    gesvd(&job, &job, &m, &n, a, &m, s_local, u, &m, v, &n, work, &len, rwork, &info);

    for (MKL_INT index = 0; index < dim_s; ++index)
    {
        s[index] = s_local[index];
    }

    delete[] rwork;
    delete[] s_local;
    return info;
}

template<typename T>
inline MKL_INT eigen_factor(MKL_INT n, T a[], T vectors[], MKL_Complex16 values[], T d[],
                    MKL_INT(*gees)(MKL_INT, char, char, int(*)(const T*, const T*), MKL_INT, T* a,
                                   MKL_INT, MKL_INT*, T*, T*, T*, MKL_INT),
                    MKL_INT(*trevc)(MKL_INT, char, char, lapack_logical*, MKL_INT, const T*,
                                    MKL_INT, T*, MKL_INT, T*, MKL_INT, MKL_INT, MKL_INT*))
{
    T* clone_a = Clone(n, n, a);
    T* wr = new T[n];
    T* wi = new T[n];

	MKL_INT sdim;
    MKL_INT info = gees(LAPACK_COL_MAJOR, 'V', 'N', nullptr, n, clone_a, n, &sdim, wr, wi, vectors, n);
    if (info != 0)
    {
        delete[] clone_a;
        delete[] wr;
        delete[] wi;
        return info;
    }

    MKL_INT m;
    info = trevc(LAPACK_COL_MAJOR, 'R', 'B', nullptr, n, clone_a, n, nullptr, n, vectors, n, n, &m);
    if (info != 0)
    {
        delete[] clone_a;
        delete[] wr;
        delete[] wi;
        return info;
    }

    for (MKL_INT index = 0; index < n; ++index)
    {
        values[index] = MKL_Complex16(wr[index], wi[index]);
    }

    for (MKL_INT  i = 0; i < n; ++i)
    {
        MKL_INT in = i * n;
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

template<typename T>
inline MKL_INT eigen_complex_factor(MKL_INT n, T a[], T vectors[], MKL_Complex16 values[], T d[],
							MKL_INT(*gees)(MKL_INT, char, char, int(*)(const T*), MKL_INT, T* a,
							MKL_INT, MKL_INT*, T*, T*, MKL_INT),
							MKL_INT(*trevc)(MKL_INT, char, char, const lapack_logical*, MKL_INT, T*,
							MKL_INT, T*, MKL_INT, T*, MKL_INT, MKL_INT, MKL_INT*))
{
	T* clone_a = Clone(n, n, a);
	T* w = new T[n];

	MKL_INT sdim;
	MKL_INT info = gees(LAPACK_COL_MAJOR, 'V', 'N', nullptr, n, clone_a, n, &sdim, w, vectors, n);
	if (info != 0)
	{
		delete[] clone_a;
		delete[] w;
		return info;
	}

	MKL_INT m;
	info = trevc(LAPACK_COL_MAJOR, 'R', 'B', nullptr, n, clone_a, n, nullptr, n, vectors, n, n, &m);
	if (info != 0)
	{
		delete[] clone_a;
		delete[] w;
		return info;
	}

	for (MKL_INT i = 0; i < n; ++i)
	{
		values[i] = w[i];
		d[i * n + i] = w[i];
	}

	delete[] clone_a;
	delete[] w;
	return info;
}

template<typename T, typename R>
inline MKL_INT sym_eigen_factor(MKL_INT n, T a[], T vectors[], MKL_Complex16 values[], T d[],
                    MKL_INT(*syev)(int, char, char, int, T*, int, R*))
{
    T* clone_a = Clone(n, n, a);
    R* w = new R[n];

	MKL_INT info = syev(LAPACK_COL_MAJOR, 'V', 'U', n, clone_a, n, w);
	if (info != 0)
	{
		delete[] clone_a;
		delete[] w;
		return info;
	}

	memcpy(vectors, clone_a, n*n*sizeof(T));

	for (MKL_INT index = 0; index < n; ++index)
	{
		values[index] = MKL_Complex16(w[index]);
	}
    
	for (MKL_INT j = 0; j < n; j++)
    {
        MKL_INT jn = j*n;

        for (MKL_INT i = 0; i < n; ++i)
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

    DLLEXPORT MKL_INT s_lu_factor(MKL_INT m, float a[], MKL_INT ipiv[])
    {
        return lu_factor<float>(m, a, ipiv, sgetrf);
    }

    DLLEXPORT MKL_INT d_lu_factor(MKL_INT m, double a[], MKL_INT ipiv[])
    {
        return lu_factor<double>(m, a, ipiv, dgetrf);
    }

    DLLEXPORT MKL_INT c_lu_factor(MKL_INT m, MKL_Complex8 a[], MKL_INT ipiv[])
    {
        return lu_factor<MKL_Complex8>(m, a, ipiv, cgetrf);
    }

    DLLEXPORT MKL_INT z_lu_factor(MKL_INT m, MKL_Complex16 a[], MKL_INT ipiv[])
    {
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

    DLLEXPORT MKL_INT s_cholesky_factor(MKL_INT n, float a[])
    {
        return cholesky_factor<float>(n, a, spotrf);
    }

    DLLEXPORT MKL_INT d_cholesky_factor(MKL_INT n, double* a)
    {
        return cholesky_factor<double>(n, a, dpotrf);
    }

    DLLEXPORT MKL_INT c_cholesky_factor(MKL_INT n, MKL_Complex8 a[])
    {
        return cholesky_factor<MKL_Complex8>(n, a, cpotrf);
    }

    DLLEXPORT MKL_INT z_cholesky_factor(MKL_INT n, MKL_Complex16 a[])
    {
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

    DLLEXPORT MKL_INT s_eigen(bool isSymmetric, MKL_INT n, float a[], float vectors[], MKL_Complex16 values[], float d[])
    {
		if (isSymmetric)
		{
			return sym_eigen_factor<float, float>(n, a, vectors, values, d, LAPACKE_ssyev);
		}
		else
		{
			return eigen_factor<float>(n, a, vectors, values, d, LAPACKE_sgees, LAPACKE_strevc);
		}
    }

    DLLEXPORT MKL_INT d_eigen(bool isSymmetric, MKL_INT n, double a[], double vectors[], MKL_Complex16 values[], double d[])
    {
        if (isSymmetric)
        {
            return sym_eigen_factor<double, double>(n, a, vectors, values, d, LAPACKE_dsyev);
        }
        else
        {
            return eigen_factor<double>(n, a, vectors, values, d, LAPACKE_dgees, LAPACKE_dtrevc);
        }
    }

    DLLEXPORT MKL_INT c_eigen(bool isSymmetric, MKL_INT n, MKL_Complex8 a[], MKL_Complex8 vectors[], MKL_Complex16 values[], MKL_Complex8 d[])
    {
		if (isSymmetric)
		{
			return sym_eigen_factor<MKL_Complex8, float>(n, a, vectors, values, d, LAPACKE_cheev);
		}
		else
		{
			return -1;
			//return eigen_factor<MKL_Complex16, LAPACK_Z_SELECT1>(n, a, vectors, values, d, LAPACKE_zgees, LAPACKE_ztrevc);
		}
    }

    DLLEXPORT MKL_INT z_eigen(bool isSymmetric, MKL_INT n, MKL_Complex16 a[], MKL_Complex16 vectors[], MKL_Complex16 values[], MKL_Complex16 d[])
    {
		if (isSymmetric)
		{
			return sym_eigen_factor<MKL_Complex16, double>(n, a, vectors, values, d, LAPACKE_zheev);
		}
		else
		{
			return eigen_complex_factor<MKL_Complex16>(n, a, vectors, values, d, LAPACKE_zgees, LAPACKE_ztrevc);
		}
    }
}