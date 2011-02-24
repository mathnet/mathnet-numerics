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

    DLLEXPORT void s_lu_factor(int m, float a[], int ipiv[])
    {
        int info;
        SGETRF(&m,&m,a,&m,ipiv,&info);
        for(int i = 0; i < m; ++i ){
            ipiv[i] -= 1;
        }
    }

    DLLEXPORT void d_lu_factor(int m, double a[], int ipiv[])
    {
        int info;
        DGETRF(&m,&m,a,&m,ipiv,&info);
        for(int i = 0; i < m; ++i ){
            ipiv[i] -= 1;
        }
    }

    DLLEXPORT void c_lu_factor(int m, MKL_Complex8 a[], int ipiv[])
    {
        int info;
        CGETRF(&m,&m,a,&m,ipiv,&info);
        for(int i = 0; i < m; ++i ){
            ipiv[i] -= 1;
        }
    }

    DLLEXPORT void z_lu_factor(int m, MKL_Complex16 a[], int ipiv[])
    {
        int info;
        ZGETRF(&m,&m,a,&m,ipiv,&info);
        for(int i = 0; i < m; ++i ){
            ipiv[i] -= 1;
        }
    }

    DLLEXPORT void s_lu_inverse(int n, float a[], float work[], int lwork)
    {
        int* ipiv = new int[n];
        int info;
        SGETRF(&n,&n,a,&n,ipiv,&info);
        SGETRI(&n,a,&n,ipiv,work,&lwork,&info);
		delete[] ipiv;
    }

    DLLEXPORT void d_lu_inverse(int n, double a[], double work[], int lwork)
    {
        int* ipiv = new int[n];
        int info;
        DGETRF(&n,&n,a,&n,ipiv,&info);
        DGETRI(&n,a,&n,ipiv,work,&lwork,&info);
		delete[] ipiv;
    }

    DLLEXPORT void c_lu_inverse(int n, MKL_Complex8 a[], MKL_Complex8 work[], int lwork)
    {
        int* ipiv = new int[n];
        int info;
        CGETRF(&n,&n,a,&n,ipiv,&info);
        CGETRI(&n,a,&n,ipiv,work,&lwork,&info);
		delete[] ipiv;
    }

    DLLEXPORT void z_lu_inverse(int n, MKL_Complex16 a[], MKL_Complex16 work[], int lwork)
    {
        int* ipiv = new int[n];
        int info;
        ZGETRF(&n,&n,a,&n,ipiv,&info);
        ZGETRI(&n,a,&n,ipiv,work,&lwork,&info);
		delete[] ipiv;
    }

	DLLEXPORT void s_lu_inverse_factored(int n, float a[], int ipiv[], float work[], int lwork)
    {
        int i;
        for(i = 0; i < n; ++i ){
            ipiv[i] += 1;
        }
        int info;
        SGETRI(&n,a,&n,ipiv,work,&lwork,&info);

        for(i = 0; i < n; ++i ){
            ipiv[i] -= 1;
        }
    }

    DLLEXPORT void d_lu_inverse_factored(int n, double a[], int ipiv[], double work[], int lwork)
    {
        int i;
        for(i = 0; i < n; ++i ){
            ipiv[i] += 1;
        }

        int info;
        DGETRI(&n,a,&n,ipiv,work,&lwork,&info);

        for(i = 0; i < n; ++i ){
            ipiv[i] -= 1;
        }
    }

    DLLEXPORT void c_lu_inverse_factored(int n, MKL_Complex8 a[], int ipiv[], MKL_Complex8 work[], int lwork)
    {
        int i;
        for(i = 0; i < n; ++i ){
            ipiv[i] += 1;
        }

        int info;
        CGETRI(&n,a,&n,ipiv,work,&lwork,&info);

        for(i = 0; i < n; ++i ){
            ipiv[i] -= 1;
        }
    }

    DLLEXPORT void z_lu_inverse_factored(int n, MKL_Complex16 a[], int ipiv[], MKL_Complex16 work[], int lwork)
    {
        int i;
        for(i = 0; i < n; ++i ){
            ipiv[i] += 1;
        }

        int info;
        ZGETRI(&n,a,&n,ipiv,work,&lwork,&info);

        for(i = 0; i < n; ++i ){
            ipiv[i] -= 1;
        }
    }

    DLLEXPORT void s_lu_solve_factored(int n, int nrhs, float a[], int ipiv[], float b[])
    {
        int info;
        int i;    
        for(i = 0; i < n; ++i ){
            ipiv[i] += 1;
        }

        char trans = 'N';
        SGETRS(&trans, &n, &nrhs, a, &n, ipiv, b, &n, &info);
        for(i = 0; i < n; ++i ){
            ipiv[i] -= 1;
        }
    }

    DLLEXPORT void  d_lu_solve_factored(int n, int nrhs, double a[], int ipiv[], double b[])
    {
        int info;
        int i;    
        for(i = 0; i < n; ++i ){
            ipiv[i] += 1;
        }

        char trans = 'N';
        DGETRS(&trans, &n, &nrhs, a, &n, ipiv, b, &n, &info);
        for(i = 0; i < n; ++i ){
            ipiv[i] -= 1;
        }
    }

    DLLEXPORT void c_lu_solve_factored(int n, int nrhs, MKL_Complex8 a[], int ipiv[], MKL_Complex8 b[])
    {
        int info;
        int i;    
        for(i = 0; i < n; ++i ){
            ipiv[i] += 1;
        }

        char trans = 'N';
        CGETRS(&trans, &n, &nrhs, a, &n, ipiv, b, &n, &info);
        for(i = 0; i < n; ++i ){
            ipiv[i] -= 1;
        }
    }

    DLLEXPORT void z_lu_solve_factored(int n, int nrhs, MKL_Complex16 a[], int ipiv[], MKL_Complex16 b[])
    {
        int info;
        int i;    
        for(i = 0; i < n; ++i ){
            ipiv[i] += 1;
        }

        char trans = 'N';
        ZGETRS(&trans, &n, &nrhs, a, &n, ipiv, b, &n, &info);
        for(i = 0; i < n; ++i ){
            ipiv[i] -= 1;
        }
    }

	DLLEXPORT void s_lu_solve(int n, int nrhs, float a[], float b[])
    {
        int* ipiv = new int[n];
        int info;
        SGETRF(&n,&n,a,&n,ipiv,&info);

        char trans = 'N';
        SGETRS(&trans, &n, &nrhs, a, &n, ipiv, b, &n, &info);
		delete[] ipiv;
    }

    DLLEXPORT void  d_lu_solve(int n, int nrhs, double a[], double b[])
    {
        int* ipiv = new int[n];
        int info;
        DGETRF(&n,&n,a,&n,ipiv,&info);

        char trans = 'N';
        DGETRS(&trans, &n, &nrhs, a, &n, ipiv, b, &n, &info);
		delete[] ipiv;
    }

    DLLEXPORT void c_lu_solve(int n, int nrhs, MKL_Complex8 a[], MKL_Complex8 b[])
    {
        int* ipiv = new int[n];
        int info;
        CGETRF(&n,&n,a,&n,ipiv,&info);

        char trans = 'N';
        CGETRS(&trans, &n, &nrhs, a, &n, ipiv, b, &n, &info);
		delete[] ipiv;
    }

    DLLEXPORT void z_lu_solve(int n, int nrhs, MKL_Complex16 a[],  MKL_Complex16 b[])
    {
        int* ipiv = new int[n];
        int info;
        ZGETRF(&n,&n,a,&n,ipiv,&info);

        char trans = 'N';
        ZGETRS(&trans, &n, &nrhs, a, &n, ipiv, b, &n, &info);
		delete[] ipiv;
    }

	
	DLLEXPORT void s_cholesky_solve(int n, int nrhs, float a[], float b[])
    {
        char uplo = 'L';
        int info = 0;
        SPOTRS(&uplo, &n, &nrhs, a, &n, b, &n, &info);
    }

    DLLEXPORT void d_cholesky_solve(int n, int nrhs, double a[], double b[])
    {
        char uplo = 'L';
        int info = 0;
        DPOTRS(&uplo, &n, &nrhs, a, &n, b, &n, &info);
    }

    DLLEXPORT void c_cholesky_solve(int n, int nrhs, MKL_Complex8 a[], MKL_Complex8 b[])
    {
        char uplo = 'L';
        int info = 0;
        CPOTRS(&uplo, &n, &nrhs, a, &n, b, &n, &info);
    }

    DLLEXPORT void z_cholesky_solve(int n, int nrhs, MKL_Complex16 a[], MKL_Complex16 b[])
    {
        char uplo = 'L';
        int info = 0;
        ZPOTRS(&uplo, &n, &nrhs, a, &n, b, &n, &info);
    }

    DLLEXPORT void s_qr_factor(int m, int n, float r[], float tau[], float q[], float work[], int len)
    {
        int info = 0;
        SGEQRF(&m, &n, r, &m, tau, work, &len, &info);

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
            SORGQR(&m, &m, &m, q, &m, tau, work, &len, &info);
        }
        else
        {
            SORGQR(&m, &n, &n, q, &m, tau, work, &len, &info);
        }
    }

    DLLEXPORT void d_qr_factor(int m, int n, double r[], double tau[], double q[], double work[], int len)
    {
        int info = 0;
        DGEQRF(&m, &n, r, &m, tau, work, &len, &info);

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
            DORGQR(&m, &m, &m, q, &m, tau, work, &len, &info);
        }
        else
        {
            DORGQR(&m, &n, &n, q, &m, tau, work, &len, &info);
        }
    }

    DLLEXPORT void c_qr_factor(int m, int n, MKL_Complex8 r[], MKL_Complex8 tau[], MKL_Complex8 q[], MKL_Complex8 work[], int len)
    {
        int info = 0;
        CGEQRF(&m, &n, r, &m, tau, work, &len, &info);

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
            CUNGQR(&m, &m, &m, q, &m, tau, work, &len, &info);
        }
        else
        {
            CUNGQR(&m, &n, &n, q, &m, tau, work, &len, &info);
        }
    }

    DLLEXPORT void z_qr_factor(int m, int n, MKL_Complex16 r[], MKL_Complex16 tau[], MKL_Complex16 q[], MKL_Complex16 work[], int len)
    {
        int info = 0;
        ZGEQRF(&m, &n, r, &m, tau, work, &len, &info);

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
            ZUNGQR(&m, &m, &m, q, &m, tau, work, &len, &info);
        }
        else
        {
            ZUNGQR(&m, &n, &n, q, &m, tau, work, &len, &info);
        }
    }

    DLLEXPORT void s_qr_solve(int m, int n, int bn, float r[], float b[], float tau[], float x[], float work[], int len)
    {
        char side ='L';
        char tran = 'T';
        int info = 0;
        SORMQR(&side, &tran, &m, &bn, &n, r, &m, tau, b, &m, work, &len, &info);
        cblas_strsm(CblasColMajor,CblasLeft,CblasUpper,CblasNoTrans,CblasNonUnit, n, bn, 1.0, r, m, b, m);
        for (int i = 0; i < n; ++i)
        {
            for (int j = 0; j < bn; ++j)
            {
                x[j * n + i] = b[j * m + i];
            }
        }
    }

    DLLEXPORT void d_qr_solve(int m, int n, int bn, double r[], double b[], double tau[], double x[], double work[], int len)
    {
        char side ='L';
        char tran = 'T';
        int info = 0;
        DORMQR(&side, &tran, &m, &bn, &n, r, &m, tau, b, &m, work, &len, &info);
        cblas_dtrsm(CblasColMajor,CblasLeft,CblasUpper,CblasNoTrans,CblasNonUnit, n, bn, 1.0, r, m, b, m);
        for (int i = 0; i < n; ++i)
        {
            for (int j = 0; j < bn; ++j)
            {
                x[j * n + i] = b[j * m + i];
            }
        }
    }

    DLLEXPORT void c_qr_solve(int m, int n, int bn, MKL_Complex8 r[], MKL_Complex8 b[], MKL_Complex8 tau[], MKL_Complex8 x[], MKL_Complex8 work[], int len)
    {
        char side ='L';
        char tran = 'T';
        int info = 0;
        CUNMQR(&side, &tran, &m, &bn, &n, r, &m, tau, b, &m, work, &len, &info);
        MKL_Complex8 one;
        one.real = 1.0;
        cblas_ctrsm(CblasColMajor,CblasLeft,CblasUpper,CblasNoTrans,CblasNonUnit, n, bn, &one, r, m, b, m);
        for (int i = 0; i < n; ++i)
        {
            for (int j = 0; j < bn; ++j)
            {
                x[j * n + i] = b[j * m + i];
            }
        }
    }

    DLLEXPORT void z_qr_solve(int m, int n, int bn, MKL_Complex16 r[], MKL_Complex16 b[], MKL_Complex16 tau[], MKL_Complex16 x[], MKL_Complex16 work[], int len)
    {
        char side ='L';
        char tran = 'T';
        int info = 0;
        ZUNMQR(&side, &tran, &m, &bn, &n, r, &m, tau, b, &m, work, &len, &info);
        MKL_Complex16 one;
        one.real = 1.0;
        cblas_ztrsm(CblasColMajor,CblasLeft,CblasUpper,CblasNoTrans,CblasNonUnit, n, bn, &one, r, m, b, m);
        for (int i = 0; i < n; ++i)
        {
            for (int j = 0; j < bn; ++j)
            {
                x[j * n + i] = b[j * m + i];
            }
        }
    }

    DLLEXPORT void s_svd_factor(bool compute_vectors, int m, int n, float a[], float s[], float u[], float v[], float work[], int len)
    {
        int info = 0;
        char job = compute_vectors ? 'A' : 'N';
        SGESVD(&job, &job, &m, &n, a, &m, s, u, &m, v, &n, work, &len, &info);
    }

    DLLEXPORT void d_svd_factor(bool compute_vectors, int m, int n, double a[], double s[], double u[], double v[], double work[], int len)
    {
        int info = 0;
        char job = compute_vectors ? 'A' : 'N';
        DGESVD(&job, &job, &m, &n, a, &m, s, u, &m, v, &n, work, &len, &info);
    }

    DLLEXPORT void c_svd_factor(bool compute_vectors, int m, int n, MKL_Complex8 a[], float s[], MKL_Complex8 u[], MKL_Complex8 v[], MKL_Complex8 work[], int len, float rwork[])
    {
        int info = 0;
        char job = compute_vectors ? 'A' : 'N';
        CGESVD(&job, &job, &m, &n, a, &m, s, u, &m, v, &n, work, &len, rwork, &info);
    }

    DLLEXPORT void z_svd_factor(bool compute_vectors, int m, int n, MKL_Complex16 a[], double s[], MKL_Complex16 u[], MKL_Complex16 v[], MKL_Complex16 work[], int len, double rwork[])
    {
        int info = 0;
        char job = compute_vectors ? 'A' : 'N';
        ZGESVD(&job, &job, &m, &n, a, &m, s, u, &m, v, &n, work, &len, rwork, &info);
    }
}