#include "wrapper_common.h"

#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <float.h>
#include "mkl_dfti.h"

inline MKL_LONG fft_free(DFTI_DESCRIPTOR_HANDLE* handle)
{
	return DftiFreeDescriptor(handle);
}

template<typename Precision>
inline MKL_LONG fft_create_1d(DFTI_DESCRIPTOR_HANDLE* handle, const MKL_LONG n, const Precision forward_scale, const Precision backward_scale, const DFTI_CONFIG_VALUE precision, const DFTI_CONFIG_VALUE domain)
{
	MKL_LONG status = DftiCreateDescriptor(handle, precision, domain, 1, n);
	DFTI_DESCRIPTOR_HANDLE descriptor = *handle;
	if (0 == status) status = DftiSetValue(descriptor, DFTI_FORWARD_SCALE, forward_scale);
	if (0 == status) status = DftiSetValue(descriptor, DFTI_BACKWARD_SCALE, backward_scale);
	if (0 == status) status = DftiCommitDescriptor(descriptor);
	return status;
}

template<typename Precision>
inline MKL_LONG fft_create_2d(DFTI_DESCRIPTOR_HANDLE* handle, const MKL_LONG m, const MKL_LONG n, const Precision forward_scale, const Precision backward_scale, const DFTI_CONFIG_VALUE precision, const DFTI_CONFIG_VALUE domain)
{
	MKL_LONG sizes[2];
	sizes[0] = m;
	sizes[1] = n;
	MKL_LONG status = DftiCreateDescriptor(handle, precision, domain, 2, sizes);
	DFTI_DESCRIPTOR_HANDLE descriptor = *handle;
	if (0 == status) status = DftiSetValue(descriptor, DFTI_FORWARD_SCALE, forward_scale);
	if (0 == status) status = DftiSetValue(descriptor, DFTI_BACKWARD_SCALE, backward_scale);
	if (0 == status) status = DftiCommitDescriptor(descriptor);
	return status;
}

template<typename Data, typename FFT>
inline MKL_LONG fft_compute(const DFTI_DESCRIPTOR_HANDLE handle, Data x[], FFT fft)
{
	return fft(handle, x);
}

extern "C" {

	DLLEXPORT MKL_LONG x_fft_free(DFTI_DESCRIPTOR_HANDLE* handle)
	{
		return fft_free(handle);
	}

	DLLEXPORT MKL_LONG z_fft_create(DFTI_DESCRIPTOR_HANDLE* handle, const MKL_LONG n, const double forward_scale, const double backward_scale)
	{
		return fft_create_1d(handle, n, forward_scale, backward_scale, DFTI_DOUBLE, DFTI_COMPLEX);
	}

	DLLEXPORT MKL_LONG c_fft_create(DFTI_DESCRIPTOR_HANDLE* handle, const MKL_LONG n, const float forward_scale, const float backward_scale)
	{
		return fft_create_1d(handle, n, forward_scale, backward_scale, DFTI_SINGLE, DFTI_COMPLEX);
	}

	DLLEXPORT MKL_LONG z_fft_create_2d(DFTI_DESCRIPTOR_HANDLE* handle, const MKL_LONG m, const MKL_LONG n, const double forward_scale, const double backward_scale)
	{
		return fft_create_2d(handle, m, n, forward_scale, backward_scale, DFTI_DOUBLE, DFTI_COMPLEX);
	}

	DLLEXPORT MKL_LONG c_fft_create_2d(DFTI_DESCRIPTOR_HANDLE* handle, const MKL_LONG m, const MKL_LONG n, const float forward_scale, const float backward_scale)
	{
		return fft_create_2d(handle, m, n, forward_scale, backward_scale, DFTI_SINGLE, DFTI_COMPLEX);
	}

	DLLEXPORT MKL_LONG z_fft_forward(const DFTI_DESCRIPTOR_HANDLE handle, MKL_Complex16 x[])
	{
		return fft_compute(handle, x, DftiComputeForward);
	}

	DLLEXPORT MKL_LONG c_fft_forward(const DFTI_DESCRIPTOR_HANDLE handle, MKL_Complex8 x[])
	{
		return fft_compute(handle, x, DftiComputeForward);
	}

	DLLEXPORT MKL_LONG z_fft_backward(const DFTI_DESCRIPTOR_HANDLE handle, MKL_Complex16 x[])
	{
		return fft_compute(handle, x, DftiComputeBackward);
	}

	DLLEXPORT MKL_LONG c_fft_backward(const DFTI_DESCRIPTOR_HANDLE handle, MKL_Complex8 x[])
	{
		return fft_compute(handle, x, DftiComputeBackward);
	}
}
