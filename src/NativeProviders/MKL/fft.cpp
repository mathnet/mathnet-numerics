#include "wrapper_common.h"

#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <float.h>
#include "mkl_service.h"
#include "mkl_dfti.h"

extern "C" {

	DLLEXPORT MKL_LONG z_fft_forward_inplace(MKL_LONG n, MKL_Complex16 x[])
	{
		MKL_LONG status = 0;
		DFTI_DESCRIPTOR_HANDLE hand = 0;
		status = DftiCreateDescriptor(&hand, DFTI_DOUBLE, DFTI_COMPLEX, 1, n);
		if (0 != status) goto failed;

		status = DftiCommitDescriptor(hand);
		if (0 != status) goto failed;

		status = DftiComputeForward(hand, x);
		if (0 != status) goto failed;

	cleanup:
		DftiFreeDescriptor(&hand);
		return status;

	failed:
		status = 1;
		goto cleanup;
	}
}
