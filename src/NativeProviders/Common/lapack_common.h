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
inline T* Clone(const int m, const int n, const T* a){
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

#ifndef LAPACK_MEMORY
#define LAPACK_MEMORY

#include <memory>

const int INSUFFICIENT_MEMORY = -999999;
const int ALIGNMENT = 64;

template <typename T> using array_ptr = std::unique_ptr<T[]>;

template<typename T>
inline array_ptr<T> array_new(const int size, int alignment = ALIGNMENT)
{
	return array_ptr<T>(new T[size]);
}

#endif

template<typename T>
inline array_ptr<T> array_clone(const int size, const T* array) {
	auto clone = array_new<T>(size);
	memcpy(clone.get(), array, size * sizeof(T));
	return clone;
}

#endif