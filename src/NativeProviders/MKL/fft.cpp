#include "wrapper_common.h"

#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <float.h>
#include "mkl_dfti.h"

template<typename Precision>
inline MKL_LONG fft_create_1d(DFTI_DESCRIPTOR_HANDLE* handle, const MKL_LONG n, const Precision forward_scale, const Precision backward_scale, const DFTI_CONFIG_VALUE precision, const DFTI_CONFIG_VALUE domain)
{
	MKL_LONG status = DftiCreateDescriptor(handle, precision, domain, 1, n);
	DFTI_DESCRIPTOR_HANDLE descriptor = *handle;
	if (0 == status) status = DftiSetValue(descriptor, DFTI_FORWARD_SCALE, forward_scale);
	if (0 == status) status = DftiSetValue(descriptor, DFTI_BACKWARD_SCALE, backward_scale);
	if (0 == status) status = DftiSetValue(descriptor, DFTI_CONJUGATE_EVEN_STORAGE, DFTI_COMPLEX_COMPLEX);
	if (0 == status) status = DftiCommitDescriptor(descriptor);
	return status;
}

template<typename Precision>
inline MKL_LONG fft_create_md(DFTI_DESCRIPTOR_HANDLE* handle, MKL_LONG dimensions, MKL_LONG n[], const Precision forward_scale, const Precision backward_scale, const DFTI_CONFIG_VALUE precision, const DFTI_CONFIG_VALUE domain)
{
	MKL_LONG status = DftiCreateDescriptor(handle, precision, domain, dimensions, n);
	DFTI_DESCRIPTOR_HANDLE descriptor = *handle;
	if (0 == status) status = DftiSetValue(descriptor, DFTI_FORWARD_SCALE, forward_scale);
	if (0 == status) status = DftiSetValue(descriptor, DFTI_BACKWARD_SCALE, backward_scale);
	if (0 == status) status = DftiSetValue(descriptor, DFTI_CONJUGATE_EVEN_STORAGE, DFTI_COMPLEX_COMPLEX);
	if (0 == status) status = DftiCommitDescriptor(descriptor);
	return status;
}

extern "C" {

	DLLEXPORT MKL_LONG x_fft_free(DFTI_DESCRIPTOR_HANDLE* handle)
	{
		return DftiFreeDescriptor(handle);
	}

	DLLEXPORT MKL_LONG z_fft_create(DFTI_DESCRIPTOR_HANDLE* handle, const MKL_LONG n, const double forward_scale, const double backward_scale)
	{
		return fft_create_1d(handle, n, forward_scale, backward_scale, DFTI_DOUBLE, DFTI_COMPLEX);
	}

	DLLEXPORT MKL_LONG c_fft_create(DFTI_DESCRIPTOR_HANDLE* handle, const MKL_LONG n, const float forward_scale, const float backward_scale)
	{
		return fft_create_1d(handle, n, forward_scale, backward_scale, DFTI_SINGLE, DFTI_COMPLEX);
	}

	DLLEXPORT MKL_LONG d_fft_create(DFTI_DESCRIPTOR_HANDLE* handle, const MKL_LONG n, const double forward_scale, const double backward_scale)
	{
		return fft_create_1d(handle, n, forward_scale, backward_scale, DFTI_DOUBLE, DFTI_REAL);
	}

	DLLEXPORT MKL_LONG s_fft_create(DFTI_DESCRIPTOR_HANDLE* handle, const MKL_LONG n, const float forward_scale, const float backward_scale)
	{
		return fft_create_1d(handle, n, forward_scale, backward_scale, DFTI_SINGLE, DFTI_REAL);
	}

	DLLEXPORT MKL_LONG z_fft_create_multidim(DFTI_DESCRIPTOR_HANDLE* handle, MKL_LONG dimensions, MKL_LONG n[], const double forward_scale, const double backward_scale)
	{
		return fft_create_md(handle, dimensions, n, forward_scale, backward_scale, DFTI_DOUBLE, DFTI_COMPLEX);
	}

	DLLEXPORT MKL_LONG c_fft_create_multidim(DFTI_DESCRIPTOR_HANDLE* handle, MKL_LONG dimensions, MKL_LONG n[], const float forward_scale, const float backward_scale)
	{
		return fft_create_md(handle, dimensions, n, forward_scale, backward_scale, DFTI_SINGLE, DFTI_COMPLEX);
	}

	DLLEXPORT MKL_LONG z_fft_forward(const DFTI_DESCRIPTOR_HANDLE handle, MKL_Complex16 x[])
	{
		return DftiComputeForward(handle, x);
	}

	DLLEXPORT MKL_LONG c_fft_forward(const DFTI_DESCRIPTOR_HANDLE handle, MKL_Complex8 x[])
	{
		return DftiComputeForward(handle, x);
	}

	DLLEXPORT MKL_LONG d_fft_forward(const DFTI_DESCRIPTOR_HANDLE handle, double x[])
	{
		return DftiComputeForward(handle, x);
	}

	DLLEXPORT MKL_LONG s_fft_forward(const DFTI_DESCRIPTOR_HANDLE handle, float x[])
	{
		return DftiComputeForward(handle, x);
	}

	DLLEXPORT MKL_LONG z_fft_backward(const DFTI_DESCRIPTOR_HANDLE handle, MKL_Complex16 x[])
	{
		return DftiComputeBackward(handle, x);
	}

	DLLEXPORT MKL_LONG c_fft_backward(const DFTI_DESCRIPTOR_HANDLE handle, MKL_Complex8 x[])
	{
		return DftiComputeBackward(handle, x);
	}

	DLLEXPORT MKL_LONG d_fft_backward(const DFTI_DESCRIPTOR_HANDLE handle, double x[])
	{
		return DftiComputeBackward(handle, x);
	}

	DLLEXPORT MKL_LONG s_fft_backward(const DFTI_DESCRIPTOR_HANDLE handle, float x[])
	{
		return DftiComputeBackward(handle, x);
	}
}
