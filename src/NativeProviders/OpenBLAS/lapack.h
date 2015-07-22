#pragma once

#define LAPACK_COMPLEX_CUSTOM
#include <complex>
#define lapack_complex_float std::complex<float>
#define lapack_complex_double std::complex<double>

#include "cblas.h"
#include "lapacke.h"


