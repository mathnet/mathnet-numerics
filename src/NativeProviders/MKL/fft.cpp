#include "wrapper_common.h"

#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <float.h>
#include "mkl_dfti.h"

inline MKL_INT64 fft_free(DFTI_DESCRIPTOR_HANDLE* handle)
{
	MKL_LONG status = DftiFreeDescriptor(handle);
	return static_cast<MKL_INT64>(status);
}

template<typename Precision>
inline MKL_INT64 fft_create_1d(DFTI_DESCRIPTOR_HANDLE* handle, const MKL_INT64 n, const Precision forward_scale, const Precision backward_scale, const DFTI_CONFIG_VALUE precision, const DFTI_CONFIG_VALUE domain)
{
	MKL_LONG status = DftiCreateDescriptor(handle, precision, domain, 1, static_cast<MKL_LONG>(n));
	DFTI_DESCRIPTOR_HANDLE descriptor = *handle;
	if (0 == status) status = DftiSetValue(descriptor, DFTI_FORWARD_SCALE, forward_scale);
	if (0 == status) status = DftiSetValue(descriptor, DFTI_BACKWARD_SCALE, backward_scale);
	if (0 == status) status = DftiCommitDescriptor(descriptor);
	return static_cast<MKL_INT64>(status);
}

template<typename Data, typename FFT>
inline MKL_INT64 fft_compute(const DFTI_DESCRIPTOR_HANDLE handle, Data x[], FFT fft)
{
	MKL_LONG status = fft(handle, x);
	return static_cast<MKL_INT64>(status);
}

extern "C" {

	DLLEXPORT MKL_INT64 x_fft_free(DFTI_DESCRIPTOR_HANDLE* handle)
	{
		return fft_free(handle);
	}

	DLLEXPORT MKL_INT64 z_fft_create(DFTI_DESCRIPTOR_HANDLE* handle, const MKL_INT64 n, const double forward_scale, const double backward_scale)
	{
		return fft_create_1d(handle, n, forward_scale, backward_scale, DFTI_DOUBLE, DFTI_COMPLEX);
	}

	DLLEXPORT MKL_INT64 c_fft_create(DFTI_DESCRIPTOR_HANDLE* handle, const MKL_INT64 n, const float forward_scale, const float backward_scale)
	{
		return fft_create_1d(handle, n, forward_scale, backward_scale, DFTI_SINGLE, DFTI_COMPLEX);
	}

	DLLEXPORT MKL_INT64 z_fft_forward(const DFTI_DESCRIPTOR_HANDLE handle, MKL_Complex16 x[])
	{
		return fft_compute(handle, x, DftiComputeForward);
	}

	DLLEXPORT MKL_INT64 c_fft_forward(const DFTI_DESCRIPTOR_HANDLE handle, MKL_Complex8 x[])
	{
		return fft_compute(handle, x, DftiComputeForward);
	}

	DLLEXPORT MKL_INT64 z_fft_backward(const DFTI_DESCRIPTOR_HANDLE handle, MKL_Complex16 x[])
	{
		return fft_compute(handle, x, DftiComputeBackward);
	}

	DLLEXPORT MKL_INT64 c_fft_backward(const DFTI_DESCRIPTOR_HANDLE handle, MKL_Complex8 x[])
	{
		return fft_compute(handle, x, DftiComputeBackward);
	}
}
