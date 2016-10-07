#include "wrapper_common.h"

#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <float.h>
#include "mkl_service.h"
#include "mkl_dfti.h"

template<typename Data, typename FFT>
inline MKL_LONG fft_inplace(MKL_LONG n, Data x[], DFTI_CONFIG_VALUE precision, DFTI_CONFIG_VALUE domain, FFT fft)
{
	MKL_LONG status = 0;
	DFTI_DESCRIPTOR_HANDLE descriptor = 0;
	status = DftiCreateDescriptor(&descriptor, precision, domain, 1, n);
	if (0 != status) goto failed;

	status = DftiCommitDescriptor(descriptor);
	if (0 != status) goto failed;

	status = fft(descriptor, x);
	if (0 != status) goto failed;

cleanup:
	DftiFreeDescriptor(&descriptor);
	return status;

failed:
	status = 1;
	goto cleanup;
}

extern "C" {

	DLLEXPORT MKL_LONG z_fft_forward_inplace(MKL_LONG n, MKL_Complex16 x[])
	{
		return fft_inplace(n, x, DFTI_DOUBLE, DFTI_COMPLEX, DftiComputeForward);
	}

	DLLEXPORT MKL_LONG c_fft_forward_inplace(MKL_LONG n, MKL_Complex8 x[])
	{
		return fft_inplace(n, x, DFTI_SINGLE, DFTI_COMPLEX, DftiComputeForward);
	}

	DLLEXPORT MKL_LONG z_fft_backward_inplace(MKL_LONG n, MKL_Complex16 x[])
	{
		return fft_inplace(n, x, DFTI_DOUBLE, DFTI_COMPLEX, DftiComputeBackward);
	}

	DLLEXPORT MKL_LONG c_fft_backward_inplace(MKL_LONG n, MKL_Complex8 x[])
	{
		return fft_inplace(n, x, DFTI_SINGLE, DFTI_COMPLEX, DftiComputeBackward);
	}
}
