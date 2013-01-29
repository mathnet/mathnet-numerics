#ifndef LAPACK_COMMON_H
#define LAPACK_COMMON_H

#include <string.h>

void shift_ipiv_down(int m, int ipiv[]);
inline void shift_ipiv_down(int m, int ipiv[]){
	for(int i = 0; i < m; ++i ){
		ipiv[i] -= 1;
	}
}

void shift_ipiv_up(int m, int ipiv[]);
inline void shift_ipiv_up(int m, int ipiv[]){
	for(int i = 0; i < m; ++i ){
		ipiv[i] += 1;
	}
}

template<typename T> 
inline T* Clone(const int m, const int n, const T* a)
{
	T* clone = new T[m*n];
	memcpy(clone, a, m*n*sizeof(T));
	return clone;
}

template<typename T> 
inline void copyBtoX (int m, int n, int bn, T b[], T x[]){
	for (int i = 0; i < n; ++i)
	{
		for (int j = 0; j < bn; ++j)
		{
			x[j * n + i] = b[j * m + i];
		}
	}
}

#endif
