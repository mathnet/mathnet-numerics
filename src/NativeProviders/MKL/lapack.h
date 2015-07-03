#pragma once

#include <memory>
#define MKL_Complex8 std::complex<float>
#define MKL_Complex16 std::complex<double>

#include "mkl.h"

const int INSUFFICIENT_MEMORY = -999999;
const int ALIGNMENT = 64;

//#define PTRALLOC( size, alignment  ) LAPACKE_malloc( size )
//#define PTRFREE( p ) LAPACKE_free( p )

#define PTRALLOC( size, alignment ) mkl_malloc( size, alignment )
#define PTRFREE( p ) mkl_free( p )

