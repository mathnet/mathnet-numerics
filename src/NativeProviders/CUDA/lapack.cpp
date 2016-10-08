#include <algorithm>

#include "lapack_common.h"
#include "wrapper_common.h"
#include "cublas_v2.h"
#include "cusolverDn.h"
#include "cuda_runtime.h"

template<typename T, typename GETRF, typename GETRFBSIZE>
inline int lu_factor(cusolverDnHandle_t solverHandle, int m, T a[], int ipiv[], GETRF getrf, GETRFBSIZE getrfbsize)
{
	int info = 0;

	T* d_A = NULL;
	cudaMalloc((void**)&d_A, m*m*sizeof(T));
	cublasSetMatrix(m, m, sizeof(T), a, m, d_A, m);

	int* d_I = NULL;
	cudaMalloc((void**)&d_I, m*sizeof(int));

	T* work = NULL;
	int lwork = 0;
	getrfbsize(solverHandle, m, m, a, m, &lwork);
	cudaMalloc((void**)&work, sizeof(T)*lwork);

	int* d_info = NULL;
	cudaMalloc((void**)&d_info, sizeof(int));

	getrf(solverHandle, m, m, d_A, m, work, d_I, d_info);

	cudaMemcpy(&info, d_info, sizeof(int), cudaMemcpyDeviceToHost);

	cublasGetMatrix(m, m, sizeof(T), d_A, m, a, m);
	cublasGetVector(m, sizeof(int), d_I, 1, ipiv, 1);

	shift_ipiv_down(m, ipiv);

	cudaFree(d_A);
	cudaFree(d_I);
	cudaFree(d_info);
	cudaFree(work);

	return info;
};

template<typename T, typename GETRF, typename GETRIBATCHED, typename GETRFBSIZE>
inline int lu_inverse(cusolverDnHandle_t solverHandle, cublasHandle_t blasHandle, int n, T a[], GETRF getrf, GETRIBATCHED getribatched, GETRFBSIZE getrfbsize)
{
	int info = 0;

	int* d_I = NULL;
	cudaMalloc((void**)&d_I, n*sizeof(int));

	T* d_A = NULL;
	cudaMalloc((void**)&d_A, n*n*sizeof(T));
	cublasSetMatrix(n, n, sizeof(T), a, n, d_A, n);

	T* work = NULL;
	int lwork = 0;
	getrfbsize(solverHandle, n, n, d_A, n, &lwork);
	cudaMalloc((void**)&work, sizeof(T)*lwork);

	int* d_info = NULL;
	cudaMalloc((void**)&d_info, sizeof(int));

	getrf(solverHandle, n, n, d_A, n, work, d_I, d_info);
	cudaMemcpy(&info, d_info, sizeof(int), cudaMemcpyDeviceToHost);

	cudaFree(work);

	if (info != 0)
	{
		cudaFree(d_A);
		cudaFree(d_I);
		cudaFree(d_info);
		return info;
	}

	T* d_C = NULL;
	cudaMalloc((void**)&d_C, n*n*sizeof(T));

	const T **d_Aarray = NULL;
	cudaMalloc((void**)&d_Aarray, sizeof(T*));
	cudaMemcpy(d_Aarray, &d_A, sizeof(T*), cudaMemcpyHostToDevice);

	T **d_Carray = NULL;
	cudaMalloc((void**)&d_Carray, sizeof(T*));
	cudaMemcpy(d_Carray, &d_C, sizeof(T*), cudaMemcpyHostToDevice);

	getribatched(blasHandle, n, d_Aarray, n, d_I, d_Carray, n, d_info, 1);
	cudaMemcpy(&info, d_info, sizeof(int), cudaMemcpyDeviceToHost);

	cublasGetMatrix(n, n, sizeof(T), d_C, n, a, n);

	cudaFree(d_A);
	cudaFree(d_I);
	cudaFree(d_C);
	cudaFree(d_info);
	cudaFree(d_Aarray);
	cudaFree(d_Carray);

	return info;
};

template<typename T, typename GETRI>
inline int lu_inverse_factored(cublasHandle_t blasHandle, int n, T a[], int ipiv[], GETRI getri)
{
	int info = 0;

	shift_ipiv_up(n, ipiv);

	T* d_A = NULL;
	cudaMalloc((void**)&d_A, n*n*sizeof(T));
	cublasSetMatrix(n, n, sizeof(T), a, n, d_A, n);

	T* d_C = NULL;
	cudaMalloc((void**)&d_C, n*n*sizeof(T));

	int* d_I = NULL;
	cudaMalloc((void**)&d_I, n*sizeof(int));
	cublasSetVector(n, sizeof(int), ipiv, 1, d_I, 1);

	int* d_info = NULL;
	cudaMalloc((void**)&d_info, sizeof(int));

	const T **d_Aarray = NULL;
	cudaMalloc((void**)&d_Aarray, sizeof(T*));
	cudaMemcpy(d_Aarray, &d_A, sizeof(T*), cudaMemcpyHostToDevice);

	T **d_Carray = NULL;
	cudaMalloc((void**)&d_Carray, sizeof(T*));
	cudaMemcpy(d_Carray, &d_C, sizeof(T*), cudaMemcpyHostToDevice);

	getri(blasHandle, n, d_Aarray, n, d_I, d_Carray, n, d_info, 1);
	cudaMemcpy(&info, d_info, sizeof(int), cudaMemcpyDeviceToHost);

	cublasGetMatrix(n, n, sizeof(T), d_C, n, a, n);
	cublasGetVector(n, sizeof(int), d_I, 1, ipiv, 1);

	shift_ipiv_down(n, ipiv);

	cudaFree(d_A);
	cudaFree(d_I);
	cudaFree(d_C);
	cudaFree(d_info);
	cudaFree(d_Aarray);
	cudaFree(d_Carray);

	return info;
}

template<typename T, typename GETRS>
inline int lu_solve_factored(cusolverDnHandle_t solverHandle, int n, int nrhs, T a[], int ipiv[], T b[], GETRS getrs)
{
	int info = 0;

	shift_ipiv_up(n, ipiv);

	T* d_A = NULL;
	cudaMalloc((void**)&d_A, n*n*sizeof(T));
	cublasSetMatrix(n, n, sizeof(T), a, n, d_A, n);

	T* d_B = NULL;
	cudaMalloc((void**)&d_B, n*nrhs*sizeof(T));
	cublasSetMatrix(n, nrhs, sizeof(T), b, n, d_B, n);

	int* d_I = NULL;
	cudaMalloc((void**)&d_I, n*sizeof(int));
	cublasSetVector(n, sizeof(int), ipiv, 1, d_I, 1);

	int* d_info = NULL;
	cudaMalloc((void**)&d_info, sizeof(int));

	getrs(solverHandle, CUBLAS_OP_N, n, nrhs, d_A, n, d_I, d_B, n, d_info);
	cudaMemcpy(&info, d_info, sizeof(int), cudaMemcpyDeviceToHost);

	cublasGetMatrix(n, nrhs, sizeof(T), d_B, n, b, n);

	shift_ipiv_down(n, ipiv);

	cudaFree(d_A);
	cudaFree(d_B);
	cudaFree(d_I);
	cudaFree(d_info);

	return info;
}

template<typename T, typename GETRF, typename GETRS, typename GETRFBSIZE>
inline int lu_solve(cusolverDnHandle_t solverHandle, int n, int nrhs, T a[], T b[], GETRF getrf, GETRS getrs, GETRFBSIZE getrfbsize)
{
	int info = 0;

	int* d_I = NULL;
	cudaMalloc((void**)&d_I, n*sizeof(int));

	T* d_A = NULL;
	cudaMalloc((void**)&d_A, n*n*sizeof(T));
	cublasSetMatrix(n, n, sizeof(T), a, n, d_A, n);

	T* work = NULL;
	int lwork = 0;
	getrfbsize(solverHandle, n, n, a, n, &lwork);
	cudaMalloc((void**)&work, sizeof(T)*lwork);

	int* d_info = NULL;
	cudaMalloc((void**)&d_info, sizeof(int));

	getrf(solverHandle, n, n, d_A, n, work, d_I, d_info);
	cudaMemcpy(&info, d_info, sizeof(int), cudaMemcpyDeviceToHost);

	cudaFree(work);

	if (info != 0)
	{
		cudaFree(d_I);
		cudaFree(d_A);
		cudaFree(d_info);
		return info;
	}

	T* d_B = NULL;
	cudaMalloc((void**)&d_B, n*nrhs*sizeof(T));
	cublasSetMatrix(n, nrhs, sizeof(T), b, n, d_B, n);

	getrs(solverHandle, CUBLAS_OP_N, n, nrhs, d_A, n, d_I, d_B, n, d_info);
	cudaMemcpy(&info, d_info, 1, cudaMemcpyDeviceToHost);

	cublasGetMatrix(n, nrhs, sizeof(T), d_B, n, b, n);

	cudaFree(d_A);
	cudaFree(d_B);
	cudaFree(d_I);
	cudaFree(d_info);

	return info;
}


template<typename T, typename POTRF, typename POTRFBSIZE>
inline int cholesky_factor(cusolverDnHandle_t solverHandle, int n, T a[], POTRF potrf, POTRFBSIZE potrfbsize)
{
	int info = 0;

	T* d_A = NULL;
	cudaMalloc((void**)&d_A, n*n*sizeof(T));
	cublasSetMatrix(n, n, sizeof(T), a, n, d_A, n);

	T* work = NULL;
	int lWork = 0;
	potrfbsize(solverHandle, CUBLAS_FILL_MODE_LOWER, n, d_A, n, &lWork);
	cudaMalloc((void**)&work, sizeof(T)*lWork);

	int* d_info = NULL;
	cudaMalloc((void**)&d_info, sizeof(int));

	potrf(solverHandle, CUBLAS_FILL_MODE_LOWER, n, d_A, n, work, lWork, d_info);
	cudaMemcpy(&info, d_info, sizeof(int), cudaMemcpyDeviceToHost);

	cublasGetMatrix(n, n, sizeof(T), d_A, n, a, n);

	T zero = T();

	for (int i = 0; i < n; ++i)
	{
		int index = i * n;

		for (int j = 0; j < n && i > j; ++j)
		{
			a[index + j] = zero;
		}
	}

	cudaFree(d_A);
	cudaFree(d_info);
	cudaFree(work);

	return info;
}

template<typename T, typename POTRF, typename POTRS, typename POTRFBSIZE>
inline int cholesky_solve(cusolverDnHandle_t solverHandle, int n, int nrhs, T a[], T b[], POTRF potrf, POTRS potrs, POTRFBSIZE potrfbsize)
{
	int info = 0;

	T* d_A = NULL;
	cudaMalloc((void**)&d_A, n*n*sizeof(T));
	cublasSetMatrix(n, n, sizeof(T), a, n, d_A, n);

	T* work = NULL;
	int lWork = 0;
	potrfbsize(solverHandle, CUBLAS_FILL_MODE_LOWER, n, d_A, n, &lWork);
	cudaMalloc((void**)&work, sizeof(T)*lWork);

	int* d_info = NULL;
	cudaMalloc((void**)&d_info, sizeof(int));

	potrf(solverHandle, CUBLAS_FILL_MODE_LOWER, n, d_A, n, work, lWork, d_info);
	cudaMemcpy(&info, d_info, sizeof(int), cudaMemcpyDeviceToHost);

	cudaFree(work);

	if (info != 0)
	{
		cudaFree(d_A);
		cudaFree(d_info);
		return info;
	}

	T* d_B = NULL;
	cudaMalloc((void**)&d_B, n*nrhs*sizeof(T));
	cublasSetMatrix(n, nrhs, sizeof(T), b, n, d_B, n);

	potrs(solverHandle, CUBLAS_FILL_MODE_LOWER, n, nrhs, d_A, n, d_B, n, d_info);
	cudaMemcpy(&info, d_info, sizeof(int), cudaMemcpyDeviceToHost);

	cublasGetMatrix(n, nrhs, sizeof(T), d_B, n, b, n);

	cudaFree(d_A);
	cudaFree(d_B);
	cudaFree(d_info);

	return info;
}

template<typename T, typename POTRS>
inline int cholesky_solve_factored(cusolverDnHandle_t solverHandle, int n, int nrhs, T a[], T b[], POTRS potrs)
{
	int info = 0;

	T* d_A = NULL;
	cudaMalloc((void**)&d_A, n*n*sizeof(T));
	cublasSetMatrix(n, n, sizeof(T), a, n, d_A, n);

	T* d_B = NULL;
	cudaMalloc((void**)&d_B, n*nrhs*sizeof(T));
	cublasSetMatrix(n, nrhs, sizeof(T), b, n, d_B, n);

	int* d_info = NULL;
	cudaMalloc((void**)&d_info, sizeof(int));

	potrs(solverHandle, CUBLAS_FILL_MODE_LOWER, n, nrhs, d_A, n, d_B, n, d_info);
	cudaMemcpy(&info, d_info, sizeof(int), cudaMemcpyDeviceToHost);

	cublasGetMatrix(n, nrhs, sizeof(T), d_B, n, b, n);

	cudaFree(d_A);
	cudaFree(d_B);
	cudaFree(d_info);

	return info;
}

//template<typename T, typename GEQRF, typename ORGQR>
//inline int qr_factor(int m, int n, T r[], T tau[], T q[], T work[], int len, GEQRF geqrf, ORGQR orgqr)
//{
//	int info = 0;
//	geqrf(&m, &n, r, &m, tau, work, &len, &info);
//
//	for (int i = 0; i < m; ++i)
//	{
//		for (int j = 0; j < m && j < n; ++j)
//		{
//			if (i > j)
//			{
//				q[j * m + i] = r[j * m + i];
//			}
//		}
//	}
//
//	//compute the q elements explicitly
//	if (m <= n)
//	{
//		orgqr(&m, &m, &m, q, &m, tau, work, &len, &info);
//	}
//	else
//	{
//		orgqr(&m, &m, &n, q, &m, tau, work, &len, &info);
//	}
//
//	return info;
//}
//
//template<typename T, typename GEQRF, typename ORGQR>
//inline int qr_thin_factor(int m, int n, T q[], T tau[], T r[], T work[], int len, GEQRF geqrf, ORGQR orgqr)
//{
//	int info = 0;
//	geqrf(&m, &n, q, &m, tau, work, &len, &info);
//
//	for (int i = 0; i < n; ++i)
//	{
//		for (int j = 0; j < n; ++j)
//		{
//			if (i <= j)
//			{
//				r[j * n + i] = q[j * m + i];
//			}
//		}
//	}
//
//	orgqr(&m, &n, &n, q, &m, tau, work, &len, &info);
//	return info;
//}
//
//template<typename T, typename GELS>
//inline int qr_solve(int m, int n, int bn, T a[], T b[], T x[], T work[], int len, GELS gels)
//{
//	T* clone_a = Clone(m, n, a);
//	T* clone_b = Clone(m, bn, b);
//	char N = 'N';
//	int info = 0;
//	gels(&N, &m, &n, &bn, clone_a, &m, clone_b, &m, work, &len, &info);
//	copyBtoX(m, n, bn, clone_b, x);
//	delete[] clone_a;
//	delete[] clone_b;
//	return info;
//}

//template<typename T, typename ORMQR, typename TRSM>
//inline int qr_solve_factored(cusolverDnHandle_t solverHandle, cublasHandle_t blasHandle, int m, int n, int bn, T r[], T b[], T tau[], T x[], T work[], int len, ORMQR ormqr, TRSM trsm)
//{
//	T* clone_b = Clone(m, bn, b);
//	char side = 'L';
//	char tran = 'T';
//	int info = 0;
//	ormqr(solverHandle, &side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info);
//	trsm(blasHandle, CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, 1.0, r, m, clone_b, m);
//
//	copyBtoX(m, n, bn, clone_b, x);
//	delete[] clone_b;
//	return info;
//}

//template<typename T, typename UNMQR, typename TRSM>
//inline int complex_qr_solve_factored(int m, int n, int bn, T r[], T b[], T tau[], T x[], T work[], int len, UNMQR unmqr, TRSM trsm)
//{
//	T* clone_b = Clone(m, bn, b);
//	char side = 'L';
//	char tran = 'C';
//	int info = 0;
//	unmqr(&side, &tran, &m, &bn, &n, r, &m, tau, clone_b, &m, work, &len, &info);
//	T one = 1.0f;
//	trsm(CblasColMajor, CblasLeft, CblasUpper, CblasNoTrans, CblasNonUnit, n, bn, &one, r, m, clone_b, m);
//	copyBtoX(m, n, bn, clone_b, x);
//	delete[] clone_b;
//	return info;
//}

template<typename T, typename GESVD, typename GESVDBSIZE>
inline int svd_factor(cusolverDnHandle_t solverHandle, bool compute_vectors, int m, int n, T a[], T s[], T u[], T v[], GESVD gesvd, GESVDBSIZE gesvdbsize)
{
	int info = 0;
	int dim_s = std::min(m, n);

	T* d_A = NULL;
	cudaMalloc((void**)&d_A, m*n*sizeof(T));
	cublasSetMatrix(m, n, sizeof(T), a, m, d_A, m);

	T* d_S = NULL;
	cudaMalloc((void**)&d_S, dim_s*sizeof(T));

	T* d_U = NULL;
	cudaMalloc((void**)&d_U, m*m*sizeof(T));

	T* d_V = NULL;
	cudaMalloc((void**)&d_V, n*n*sizeof(T));

	T* work = NULL;
	int lWork = 0;
	gesvdbsize(solverHandle, m, n, &lWork);
	cudaMalloc((void**)&work, lWork*sizeof(T));

	T* rwork = NULL;
	cudaMalloc((void**)&rwork, 5 * dim_s * sizeof(T));

	int* d_info = NULL;
	cudaMalloc((void**)&d_info, sizeof(int));

	char job = compute_vectors ? 'A' : 'N';
	gesvd(solverHandle, job, job, m, n, d_A, m, d_S, d_U, m, d_V, n, work, lWork, rwork, d_info);
	cudaMemcpy(&info, d_info, sizeof(int), cudaMemcpyDeviceToHost);

	cublasGetVector(dim_s, sizeof(T), d_S, 1, s, 1);
	cublasGetMatrix(m, m, sizeof(T), d_U, m, u, m);
	cublasGetMatrix(n, n, sizeof(T), d_V, n, v, n);

	cudaFree(d_A);
	cudaFree(d_S);
	cudaFree(d_U);
	cudaFree(d_V);
	cudaFree(work);
	cudaFree(rwork);
	cudaFree(d_info);

	return info;
}

template<typename T, typename R, typename GESVD, typename GESVDBSIZE>
inline int complex_svd_factor(cusolverDnHandle_t solverHandle, bool compute_vectors, int m, int n, T a[], T s[], T u[], T v[], GESVD gesvd, GESVDBSIZE gesvdbsize)
{
	int info = 0;
	int dim_s = std::min(m, n);

	T* d_A = NULL;
	cudaMalloc((void**)&d_A, m*n*sizeof(T));
	cublasSetMatrix(m, n, sizeof(T), a, m, d_A, m);

	R* s_local = new R[dim_s];
	R* d_S = NULL;
	cudaMalloc((void**)&d_S, dim_s*sizeof(R));

	T* d_U = NULL;
	cudaMalloc((void**)&d_U, m*m*sizeof(T));

	T* d_V = NULL;
	cudaMalloc((void**)&d_V, n*m*sizeof(T));

	T* work = NULL;
	int lWork = 0;
	gesvdbsize(solverHandle, m, n, &lWork);
	cudaMalloc((void**)&work, lWork*sizeof(T));

	R* rwork = NULL;
	cudaMalloc((void**)&rwork, 5 * dim_s * sizeof(R));

	int* d_info = NULL;
	cudaMalloc((void**)&d_info, sizeof(int));

	char job = compute_vectors ? 'A' : 'N';
	gesvd(solverHandle, job, job, m, n, d_A, m, d_S, d_U, m, d_V, n, work, lWork, rwork, d_info);
	cudaMemcpy(&info, d_info, sizeof(int), cudaMemcpyDeviceToHost);

	cublasGetVector(dim_s, sizeof(R), d_S, 1, s_local, 1);
	cublasGetMatrix(m, m, sizeof(T), d_U, m, u, m);
	cublasGetMatrix(n, n, sizeof(T), d_V, n, v, n);

	for (int index = 0; index < dim_s; ++index)
	{
		s[index].x = s_local[index];
	}

	delete[] s_local;
	cudaFree(d_A);
	cudaFree(d_S);
	cudaFree(d_U);
	cudaFree(d_V);
	cudaFree(work);
	cudaFree(rwork);
	cudaFree(d_info);

	return info;
}

//template<typename T, typename R, typename GEES, typename TREVC>
//inline int eigen_factor(int n, T a[], T vectors[], R values[], T d[], GEES gees, TREVC trevc)
//{
//	T* clone_a = Clone(n, n, a);
//	T* wr = new T[n];
//	T* wi = new T[n];
//
//	int sdim;
//	int info = gees(LAPACK_COL_MAJOR, 'V', 'N', nullptr, n, clone_a, n, &sdim, wr, wi, vectors, n);
//	if (info != 0)
//	{
//		delete[] clone_a;
//		delete[] wr;
//		delete[] wi;
//		return info;
//	}
//
//	int m;
//	info = trevc(LAPACK_COL_MAJOR, 'R', 'B', nullptr, n, clone_a, n, nullptr, n, vectors, n, n, &m);
//	if (info != 0)
//	{
//		delete[] clone_a;
//		delete[] wr;
//		delete[] wi;
//		return info;
//	}
//
//	for (int index = 0; index < n; ++index)
//	{
//		values[index] = R(wr[index], wi[index]);
//	}
//
//	for (int i = 0; i < n; ++i)
//	{
//		int in = i * n;
//		d[in + i] = wr[i];
//
//		if (wi[i] > 0)
//		{
//			d[in + n + i] = wi[i];
//		}
//		else if (wi[i] < 0)
//		{
//			d[in - n + i] = wi[i];
//		}
//	}
//
//	delete[] clone_a;
//	delete[] wr;
//	delete[] wi;
//	return info;
//}
//
//template<typename T, typename GEES, typename TREVC>
//inline int eigen_complex_factor(int n, T a[], T vectors[], cuDoubleComplex values[], T d[], GEES gees, TREVC trevc)
//{
//	T* clone_a = Clone(n, n, a);
//	T* w = new T[n];
//
//	int sdim;
//	int info = gees(LAPACK_COL_MAJOR, 'V', 'N', nullptr, n, clone_a, n, &sdim, w, vectors, n);
//	if (info != 0)
//	{
//		delete[] clone_a;
//		delete[] w;
//		return info;
//	}
//
//	int m;
//	info = trevc(LAPACK_COL_MAJOR, 'R', 'B', nullptr, n, clone_a, n, nullptr, n, vectors, n, n, &m);
//	if (info != 0)
//	{
//		delete[] clone_a;
//		delete[] w;
//		return info;
//	}
//
//	for (int i = 0; i < n; ++i)
//	{
//		values[i] = w[i];
//		d[i * n + i] = w[i];
//	}
//
//	delete[] clone_a;
//	delete[] w;
//	return info;
//}
//
//template<typename R, typename T, typename SYEV>
//inline int sym_eigen_factor(int n, T a[], T vectors[], cuDoubleComplex values[], T d[], SYEV syev)
//{
//	T* clone_a = Clone(n, n, a);
//	R* w = new R[n];
//
//	int info = syev(LAPACK_COL_MAJOR, 'V', 'U', n, clone_a, n, w);
//	if (info != 0)
//	{
//		delete[] clone_a;
//		delete[] w;
//		return info;
//	}
//
//	memcpy(vectors, clone_a, n*n*sizeof(T));
//
//	for (int index = 0; index < n; ++index)
//	{
//		values[index] = cuDoubleComplex(w[index]);
//	}
//
//	for (int j = 0; j < n; ++j)
//	{
//		int jn = j*n;
//
//		for (int i = 0; i < n; ++i)
//		{
//			if (i == j)
//			{
//				d[jn + i] = w[i];
//			}
//		}
//	}
//
//	delete[] clone_a;
//	delete[] w;
//	return info;
//}

#define sgetrf cusolverDnSgetrf
#define dgetrf cusolverDnDgetrf
#define cgetrf cusolverDnCgetrf
#define zgetrf cusolverDnZgetrf
#define sgetrfbsize cusolverDnSgetrf_bufferSize
#define dgetrfbsize cusolverDnDgetrf_bufferSize
#define cgetrfbsize cusolverDnCgetrf_bufferSize
#define zgetrfbsize cusolverDnZgetrf_bufferSize

#define sgetrs cusolverDnSgetrs
#define dgetrs cusolverDnDgetrs
#define cgetrs cusolverDnCgetrs
#define zgetrs cusolverDnZgetrs

#define spotrf cusolverDnSpotrf
#define dpotrf cusolverDnDpotrf
#define cpotrf cusolverDnCpotrf
#define zpotrf cusolverDnZpotrf
#define spotrfbsize cusolverDnSpotrf_bufferSize
#define dpotrfbsize cusolverDnDpotrf_bufferSize
#define cpotrfbsize cusolverDnCpotrf_bufferSize
#define zpotrfbsize cusolverDnZpotrf_bufferSize

#define spotrs cusolverDnSpotrs
#define dpotrs cusolverDnDpotrs
#define cpotrs cusolverDnCpotrs
#define zpotrs cusolverDnZpotrs

#define sgeqrf cusolverDnSgeqrf
#define dgeqrf cusolverDnDgeqrf
#define cgeqrf cusolverDnCgeqrf
#define zgeqrf cusolverDnZgeqrf

#define sormqr cusolverDnSormqr
#define dormqr cusolverDnDormqr

#define sgesvd cusolverDnSgesvd
#define dgesvd cusolverDnDgesvd
#define cgesvd cusolverDnCgesvd
#define zgesvd cusolverDnZgesvd
#define sgesvdbsize cusolverDnSgesvd_bufferSize
#define dgesvdbsize cusolverDnDgesvd_bufferSize
#define cgesvdbsize cusolverDnCgesvd_bufferSize
#define zgesvdbsize cusolverDnZgesvd_bufferSize

#define sgetribatched cublasSgetriBatched
#define dgetribatched cublasDgetriBatched
#define cgetribatched cublasCgetriBatched
#define zgetribatched cublasZgetriBatched

extern "C" {

	DLLEXPORT int s_lu_factor(cusolverDnHandle_t solverHandle, int m, float a[], int ipiv[])
	{
		return lu_factor(solverHandle, m, a, ipiv, sgetrf, sgetrfbsize);
	}

	DLLEXPORT int d_lu_factor(cusolverDnHandle_t solverHandle, int m, double a[], int ipiv[])
	{
		return lu_factor(solverHandle, m, a, ipiv, dgetrf, dgetrfbsize);
	}

	DLLEXPORT int c_lu_factor(cusolverDnHandle_t solverHandle, int m, cuComplex a[], int ipiv[])
	{
		return lu_factor(solverHandle, m, a, ipiv, cgetrf, cgetrfbsize);
	}

	DLLEXPORT int z_lu_factor(cusolverDnHandle_t solverHandle, int m, cuDoubleComplex a[], int ipiv[])
	{
		return lu_factor(solverHandle, m, a, ipiv, zgetrf, zgetrfbsize);
	}

	DLLEXPORT int s_lu_inverse(cusolverDnHandle_t solverHandle, cublasHandle_t blasHandle, int n, float a[])
	{
		return lu_inverse(solverHandle, blasHandle, n, a, sgetrf, sgetribatched, sgetrfbsize);
	}

	DLLEXPORT int d_lu_inverse(cusolverDnHandle_t solverHandle, cublasHandle_t blasHandle, int n, double a[])
	{
		return lu_inverse(solverHandle, blasHandle, n, a, dgetrf, dgetribatched, dgetrfbsize);
	}

	DLLEXPORT int c_lu_inverse(cusolverDnHandle_t solverHandle, cublasHandle_t blasHandle, int n, cuComplex a[])
	{
		return lu_inverse(solverHandle, blasHandle, n, a, cgetrf, cgetribatched, cgetrfbsize);
	}

	DLLEXPORT int z_lu_inverse(cusolverDnHandle_t solverHandle, cublasHandle_t blasHandle, int n, cuDoubleComplex a[])
	{
		return lu_inverse(solverHandle, blasHandle, n, a, zgetrf, zgetribatched, zgetrfbsize);
	}

	DLLEXPORT int s_lu_inverse_factored(cublasHandle_t blasHandle, int n, float a[], int ipiv[])
	{
		return lu_inverse_factored(blasHandle, n, a, ipiv, sgetribatched);
	}

	DLLEXPORT int d_lu_inverse_factored(cublasHandle_t blasHandle, int n, double a[], int ipiv[])
	{
		return lu_inverse_factored(blasHandle, n, a, ipiv, dgetribatched);
	}

	DLLEXPORT int c_lu_inverse_factored(cublasHandle_t blasHandle, int n, cuComplex a[], int ipiv[])
	{
		return lu_inverse_factored(blasHandle, n, a, ipiv, cgetribatched);
	}

	DLLEXPORT int z_lu_inverse_factored(cublasHandle_t blasHandle, int n, cuDoubleComplex a[], int ipiv[])
	{
		return lu_inverse_factored(blasHandle, n, a, ipiv, zgetribatched);
	}

	DLLEXPORT int s_lu_solve_factored(cusolverDnHandle_t solverHandle, int n, int nrhs, float a[], int ipiv[], float b[])
	{
		return lu_solve_factored(solverHandle, n, nrhs, a, ipiv, b, sgetrs);
	}

	DLLEXPORT int  d_lu_solve_factored(cusolverDnHandle_t solverHandle, int n, int nrhs, double a[], int ipiv[], double b[])
	{
		return lu_solve_factored(solverHandle, n, nrhs, a, ipiv, b, dgetrs);
	}

	DLLEXPORT int c_lu_solve_factored(cusolverDnHandle_t solverHandle, int n, int nrhs, cuComplex a[], int ipiv[], cuComplex b[])
	{
		return lu_solve_factored(solverHandle, n, nrhs, a, ipiv, b, cgetrs);
	}

	DLLEXPORT int z_lu_solve_factored(cusolverDnHandle_t solverHandle, int n, int nrhs, cuDoubleComplex a[], int ipiv[], cuDoubleComplex b[])
	{
		return lu_solve_factored(solverHandle, n, nrhs, a, ipiv, b, zgetrs);
	}

	DLLEXPORT int s_lu_solve(cusolverDnHandle_t solverHandle, int n, int nrhs, float a[], float b[])
	{
		return lu_solve(solverHandle, n, nrhs, a, b, sgetrf, sgetrs, sgetrfbsize);
	}

	DLLEXPORT int d_lu_solve(cusolverDnHandle_t solverHandle, int n, int nrhs, double a[], double b[])
	{
		return lu_solve(solverHandle, n, nrhs, a, b, dgetrf, dgetrs, dgetrfbsize);
	}

	DLLEXPORT int c_lu_solve(cusolverDnHandle_t solverHandle, int n, int nrhs, cuComplex a[], cuComplex b[])
	{
		return lu_solve(solverHandle, n, nrhs, a, b, cgetrf, cgetrs, cgetrfbsize);
	}

	DLLEXPORT int z_lu_solve(cusolverDnHandle_t solverHandle, int n, int nrhs, cuDoubleComplex a[], cuDoubleComplex b[])
	{
		return lu_solve(solverHandle, n, nrhs, a, b, zgetrf, zgetrs, zgetrfbsize);
	}

	DLLEXPORT int s_cholesky_factor(cusolverDnHandle_t solverHandle, int n, float a[])
	{
		return cholesky_factor(solverHandle, n, a, spotrf, spotrfbsize);
	}

	DLLEXPORT int d_cholesky_factor(cusolverDnHandle_t solverHandle, int n, double a[])
	{
		return cholesky_factor(solverHandle, n, a, dpotrf, dpotrfbsize);
	}

	DLLEXPORT int c_cholesky_factor(cusolverDnHandle_t solverHandle, int n, cuComplex a[])
	{
		return cholesky_factor(solverHandle, n, a, cpotrf, cpotrfbsize);
	}

	DLLEXPORT int z_cholesky_factor(cusolverDnHandle_t solverHandle, int n, cuDoubleComplex a[])
	{
		return cholesky_factor(solverHandle, n, a, zpotrf, zpotrfbsize);
	}

	DLLEXPORT int s_cholesky_solve(cusolverDnHandle_t solverHandle, int n, int nrhs, float a[], float b[])
	{
		return cholesky_solve(solverHandle, n, nrhs, a, b, spotrf, spotrs, spotrfbsize);
	}

	DLLEXPORT int d_cholesky_solve(cusolverDnHandle_t solverHandle, int n, int nrhs, double a[], double b[])
	{
		return cholesky_solve(solverHandle, n, nrhs, a, b, dpotrf, dpotrs, dpotrfbsize);
	}

	DLLEXPORT int c_cholesky_solve(cusolverDnHandle_t solverHandle, int n, int nrhs, cuComplex a[], cuComplex b[])
	{
		return cholesky_solve(solverHandle, n, nrhs, a, b, cpotrf, cpotrs, cpotrfbsize);
	}

	DLLEXPORT int z_cholesky_solve(cusolverDnHandle_t solverHandle, int n, int nrhs, cuDoubleComplex a[], cuDoubleComplex b[])
	{
		return cholesky_solve(solverHandle, n, nrhs, a, b, zpotrf, zpotrs, zpotrfbsize);
	}

	DLLEXPORT int s_cholesky_solve_factored(cusolverDnHandle_t solverHandle, int n, int nrhs, float a[], float b[])
	{
		return cholesky_solve_factored(solverHandle, n, nrhs, a, b, spotrs);
	}

	DLLEXPORT int d_cholesky_solve_factored(cusolverDnHandle_t solverHandle, int n, int nrhs, double a[], double b[])
	{
		return cholesky_solve_factored(solverHandle, n, nrhs, a, b, dpotrs);
	}

	DLLEXPORT int c_cholesky_solve_factored(cusolverDnHandle_t solverHandle, int n, int nrhs, cuComplex a[], cuComplex b[])
	{
		return cholesky_solve_factored(solverHandle, n, nrhs, a, b, cpotrs);
	}

	DLLEXPORT int z_cholesky_solve_factored(cusolverDnHandle_t solverHandle, int n, int nrhs, cuDoubleComplex a[], cuDoubleComplex b[])
	{
		return cholesky_solve_factored(solverHandle, n, nrhs, a, b, zpotrs);
	}

	// MJ: I am fairly certain that it would be straightforward to implement ?orgqr and ?gels but I'm focusing on getting the low-hanging fruit working first
	/*DLLEXPORT int s_qr_factor(int m, int n, float r[], float tau[], float q[], float work[], int len)
	{
		return qr_factor(m, n, r, tau, q, work, len, sgeqrf, sorgqr);
	}

	DLLEXPORT int s_qr_thin_factor(int m, int n, float q[], float tau[], float r[], float work[], int len)
	{
		return qr_thin_factor(m, n, q, tau, r, work, len, sgeqrf, sorgqr);
	}

	DLLEXPORT int d_qr_factor(int m, int n, double r[], double tau[], double q[], double work[], int len)
	{
		return qr_factor(m, n, r, tau, q, work, len, dgeqrf, dorgqr);
	}

	DLLEXPORT int d_qr_thin_factor(int m, int n, double q[], double tau[], double r[], double work[], int len)
	{
		return qr_thin_factor(m, n, q, tau, r, work, len, dgeqrf, dorgqr);
	}

	DLLEXPORT int c_qr_factor(int m, int n, cuComplex r[], cuComplex tau[], cuComplex q[], cuComplex work[], int len)
	{
		return qr_factor(m, n, r, tau, q, work, len, cgeqrf, cungqr);
	}

	DLLEXPORT int c_qr_thin_factor(int m, int n, cuComplex q[], cuComplex tau[], cuComplex r[], cuComplex work[], int len)
	{
		return qr_thin_factor(m, n, q, tau, r, work, len, cgeqrf, cungqr);
	}

	DLLEXPORT int z_qr_factor(int m, int n, cuDoubleComplex r[], cuDoubleComplex tau[], cuDoubleComplex q[], cuDoubleComplex work[], int len)
	{
		return qr_factor(m, n, r, tau, q, work, len, zgeqrf, zungqr);
	}

	DLLEXPORT int z_qr_thin_factor(int m, int n, cuDoubleComplex q[], cuDoubleComplex tau[], cuDoubleComplex r[], cuDoubleComplex work[], int len)
	{
		return qr_thin_factor(m, n, q, tau, r, work, len, zgeqrf, zungqr);
	}

	DLLEXPORT int s_qr_solve(int m, int n, int bn, float a[], float b[], float x[], float work[], int len)
	{
		return qr_solve(m, n, bn, a, b, x, work, len, sgels);
	}

	DLLEXPORT int d_qr_solve(int m, int n, int bn, double a[], double b[], double x[], double work[], int len)
	{
		return qr_solve(m, n, bn, a, b, x, work, len, dgels);
	}

	DLLEXPORT int c_qr_solve(int m, int n, int bn, cuComplex a[], cuComplex b[], cuComplex x[], cuComplex work[], int len)
	{
		return qr_solve(m, n, bn, a, b, x, work, len, cgels);
	}

	DLLEXPORT int z_qr_solve(int m, int n, int bn, cuDoubleComplex a[], cuDoubleComplex b[], cuDoubleComplex x[], cuDoubleComplex work[], int len)
	{
		return qr_solve(m, n, bn, a, b, x, work, len, zgels);
	}*/

	//DLLEXPORT int s_qr_solve_factored(cusolverDnHandle_t solverHandle, cublasHandle_t blasHandle, int m, int n, int bn, float r[], float b[], float tau[], float x[], float work[], int len)
	//{
	//	return qr_solve_factored(solverHandle, blasHandle, m, n, bn, r, b, tau, x, work, len, sormqr, cublasStrsm);
	//}

	//DLLEXPORT int d_qr_solve_factored(cusolverDnHandle_t solverHandle, cublasHandle_t blasHandle, int m, int n, int bn, double r[], double b[], double tau[], double x[], double work[], int len)
	//{
	//	return qr_solve_factored(solverHandle, blasHandle, m, n, bn, r, b, tau, x, work, len, dormqr, cublasDtrsm);
	//}

	//DLLEXPORT int c_qr_solve_factored(int m, int n, int bn, cuComplex r[], cuComplex b[], cuComplex tau[], cuComplex x[], cuComplex work[], int len)
	//{
	//	return complex_qr_solve_factored(m, n, bn, r, b, tau, x, work, len, cunmqr, cublasCtrsm);
	//}

	//DLLEXPORT int z_qr_solve_factored(int m, int n, int bn, cuDoubleComplex r[], cuDoubleComplex b[], cuDoubleComplex tau[], cuDoubleComplex x[], cuDoubleComplex work[], int len)
	//{
	//	return complex_qr_solve_factored(m, n, bn, r, b, tau, x, work, len, zunmqr, cublasZtrsm);
	//}

	DLLEXPORT int s_svd_factor(cusolverDnHandle_t solverHandle, bool compute_vectors, int m, int n, float a[], float s[], float u[], float v[])
	{
		return svd_factor(solverHandle, compute_vectors, m, n, a, s, u, v, sgesvd, sgesvdbsize);
	}

	DLLEXPORT int d_svd_factor(cusolverDnHandle_t solverHandle, bool compute_vectors, int m, int n, double a[], double s[], double u[], double v[])
	{
		return svd_factor(solverHandle, compute_vectors, m, n, a, s, u, v,dgesvd, dgesvdbsize);
	}

	DLLEXPORT int c_svd_factor(cusolverDnHandle_t solverHandle, bool compute_vectors, int m, int n, cuComplex a[], cuComplex s[], cuComplex u[], cuComplex v[])
	{
		return complex_svd_factor<cuComplex, float>(solverHandle, compute_vectors, m, n, a, s, u, v, cgesvd, cgesvdbsize);
	}

	DLLEXPORT int z_svd_factor(cusolverDnHandle_t solverHandle, bool compute_vectors, int m, int n, cuDoubleComplex a[], cuDoubleComplex s[], cuDoubleComplex u[], cuDoubleComplex v[])
	{
		return complex_svd_factor<cuDoubleComplex, double>(solverHandle, compute_vectors, m, n, a, s, u, v, zgesvd, zgesvdbsize);
	}

	/*DLLEXPORT int s_eigen(bool isSymmetric, int n, float a[], float vectors[], cuDoubleComplex values[], float d[])
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

	DLLEXPORT int d_eigen(bool isSymmetric, int n, double a[], double vectors[], cuDoubleComplex values[], double d[])
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

	DLLEXPORT int c_eigen(bool isSymmetric, int n, cuComplex a[], cuComplex vectors[], cuDoubleComplex values[], cuComplex d[])
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

	DLLEXPORT int z_eigen(bool isSymmetric, int n, cuDoubleComplex a[], cuDoubleComplex vectors[], cuDoubleComplex values[], cuDoubleComplex d[])
	{
		if (isSymmetric)
		{
			return sym_eigen_factor<double>(n, a, vectors, values, d, LAPACKE_zheev);
		}
		else
		{
			return eigen_complex_factor(n, a, vectors, values, d, LAPACKE_zgees, LAPACKE_ztrevc);
		}
	}*/
}
