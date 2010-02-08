#include "mkl_vml.h"
#include "common.h"

DLLEXPORT void d_vector_add( const int n, const double x[], const double y[], double ret[]){
	vdAdd( n, x, y, ret );
}

DLLEXPORT void d_vector_subtract( const int n, const double x[], const double y[], double ret[]){
	vdSub( n, x, y, ret );
}

DLLEXPORT void d_vector_multiply( const int n, const double x[], const double y[], double ret[]){
	vdMul( n, x, y, ret );
}
