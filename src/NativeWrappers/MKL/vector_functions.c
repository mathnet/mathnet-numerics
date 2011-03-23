#include "mkl_vml.h"
#include "blas.h"
#include "wrapper_common.h"


DLLEXPORT void s_vector_add( const int n, const float x[], const float y[], float result[] ){
	vsAdd( n, x, y, result );
}

DLLEXPORT void s_vector_subtract( const int n, const float x[], const float y[], float result[] ){
	vsSub( n, x, y, result );
}

DLLEXPORT void s_vector_multiply( const int n, const float x[], const float y[], float result[] ){
	vsMul( n, x, y, result );
}

DLLEXPORT void s_vector_divide( const int n, const float x[], const float y[], float result[] ){
	vsDiv( n, x, y, result );
}

DLLEXPORT void d_vector_add( const int n, const double x[], const double y[], double result[] ){
	vdAdd( n, x, y, result );
}

DLLEXPORT void d_vector_subtract( const int n, const double x[], const double y[], double result[] ){
	vdSub( n, x, y, result );
}

DLLEXPORT void d_vector_multiply( const int n, const double x[], const double y[], double result[] ){
	vdMul( n, x, y, result );
}

DLLEXPORT void d_vector_divide( const int n, const double x[], const double y[], double result[] ){
	vdDiv( n, x, y, result );
}

DLLEXPORT void c_vector_add( const int n, const Complex8 x[], const Complex8 y[], Complex8 result[] ){
	vcAdd( n, x, y, result );
}

DLLEXPORT void c_vector_subtract( const int n, const Complex8 x[], const Complex8 y[], Complex8 result[] ){
	vcSub( n, x, y, result );
}

DLLEXPORT void c_vector_multiply( const int n, const Complex8 x[], const Complex8 y[], Complex8 result[] ){
	vcMul( n, x, y, result );
}

DLLEXPORT void c_vector_divide( const int n, const Complex8 x[], const Complex8 y[], Complex8 result[] ){
	vcDiv( n, x, y, result );
}

DLLEXPORT void z_vector_add( const int n, const Complex16 x[], const Complex16 y[], Complex16 result[] ){
	vzAdd( n, x, y, result );
}

DLLEXPORT void z_vector_subtract( const int n, const Complex16 x[], const Complex16 y[], Complex16 result[] ){
	vzSub( n, x, y, result );
}

DLLEXPORT void z_vector_multiply( const int n, const Complex16 x[], const Complex16 y[], Complex16 result[] ){
	vzMul( n, x, y, result );
}

DLLEXPORT void z_vector_divide( const int n, const Complex16 x[], const Complex16 y[], Complex16 result[] ){
	vzDiv( n, x, y, result );
}
