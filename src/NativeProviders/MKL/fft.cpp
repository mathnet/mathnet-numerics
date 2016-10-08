#include "wrapper_common.h"

#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <float.h>
#include "mkl_dfti.h"

template<typename Data, typename Precision, typename FFT>
inline MKL_INT64 fft_1d_inplace(const MKL_INT64 n, Data x[], const Precision forward_scale, const Precision backward_scale, const DFTI_CONFIG_VALUE precision, const DFTI_CONFIG_VALUE domain, FFT fft)
{
	MKL_LONG status;
	DFTI_DESCRIPTOR_HANDLE descriptor = nullptr;
	status = DftiCreateDescriptor(&descriptor, precision, domain, 1, static_cast<MKL_LONG>(n));
	if (0 != status) goto cleanup;

	status = DftiSetValue(descriptor, DFTI_FORWARD_SCALE, forward_scale);
	if (0 != status) goto cleanup;

	status = DftiSetValue(descriptor, DFTI_BACKWARD_SCALE, backward_scale);
	if (0 != status) goto cleanup;

	status = DftiCommitDescriptor(descriptor);
	if (0 != status) goto cleanup;

	status = fft(descriptor, x);

cleanup:
	DftiFreeDescriptor(&descriptor);
	return static_cast<MKL_INT64>(status);
}

extern "C" {

	DLLEXPORT MKL_INT64 z_fft_forward_inplace(const MKL_INT64 n, const double scaling, MKL_Complex16 x[])
	{
		return fft_1d_inplace(n, x, scaling, 1.0, DFTI_DOUBLE, DFTI_COMPLEX, DftiComputeForward);
	}

	DLLEXPORT MKL_INT64 c_fft_forward_inplace(const MKL_INT64 n, const float scaling, MKL_Complex8 x[])
	{
		return fft_1d_inplace(n, x, scaling, 1.0f, DFTI_SINGLE, DFTI_COMPLEX, DftiComputeForward);
	}

	DLLEXPORT MKL_INT64 z_fft_backward_inplace(const MKL_INT64 n, const double scaling, MKL_Complex16 x[])
	{
		return fft_1d_inplace(n, x, 1.0, scaling, DFTI_DOUBLE, DFTI_COMPLEX, DftiComputeBackward);
	}

	DLLEXPORT MKL_INT64 c_fft_backward_inplace(const MKL_INT64 n, const float scaling, MKL_Complex8 x[])
	{
		return fft_1d_inplace(n, x, 1.0f, scaling, DFTI_SINGLE, DFTI_COMPLEX, DftiComputeBackward);
	}
}
