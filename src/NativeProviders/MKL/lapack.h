#pragma once

#include <complex>
#include <memory>

#define MKL_Complex8 std::complex<float>
#define MKL_Complex16 std::complex<double>
#define LAPACK_MEMORY

#include "mkl.h"

const int ALIGNMENT = 64;

struct array_free
{
	void operator()(void* x) { mkl_free(x); }
};

template <typename T> using array_ptr = std::unique_ptr<T[], array_free>;

template<typename T>
inline array_ptr<T> array_new(const int size)
{
	auto ret = static_cast<T*>(mkl_malloc(size * sizeof(T), ALIGNMENT));

	if (!ret)
	{
		throw new std::bad_alloc();
	}

	return array_ptr<T>(ret);
}
