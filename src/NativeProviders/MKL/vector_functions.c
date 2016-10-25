#include "mkl_vml.h"
#include "wrapper_common.h"

#if __cplusplus
extern "C" {
#endif

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

DLLEXPORT void s_vector_power(const int n, const float x[], const float y[], float result[]) {
	vsPow(n, x, y, result);
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

DLLEXPORT void d_vector_power(const int n, const double x[], const double y[], double result[]) {
	vdPow(n, x, y, result);
}

DLLEXPORT void c_vector_add( const int n, const MKL_Complex8 x[], const MKL_Complex8 y[], MKL_Complex8 result[] ){
	vcAdd( n, x, y, result );
}

DLLEXPORT void c_vector_subtract( const int n, const MKL_Complex8 x[], const MKL_Complex8 y[], MKL_Complex8 result[] ){
	vcSub( n, x, y, result );
}

DLLEXPORT void c_vector_multiply( const int n, const MKL_Complex8 x[], const MKL_Complex8 y[], MKL_Complex8 result[] ){
	vcMul( n, x, y, result );
}

DLLEXPORT void c_vector_divide( const int n, const MKL_Complex8 x[], const MKL_Complex8 y[], MKL_Complex8 result[] ){
	vcDiv( n, x, y, result );
}

DLLEXPORT void c_vector_power(const int n, const MKL_Complex8 x[], const MKL_Complex8 y[], MKL_Complex8 result[]) {
	vcPow(n, x, y, result);
}

DLLEXPORT void z_vector_add( const int n, const MKL_Complex16 x[], const MKL_Complex16 y[], MKL_Complex16 result[] ){
	vzAdd( n, x, y, result );
}

DLLEXPORT void z_vector_subtract( const int n, const MKL_Complex16 x[], const MKL_Complex16 y[], MKL_Complex16 result[] ){
	vzSub( n, x, y, result );
}

DLLEXPORT void z_vector_multiply( const int n, const MKL_Complex16 x[], const MKL_Complex16 y[], MKL_Complex16 result[] ){
	vzMul( n, x, y, result );
}

DLLEXPORT void z_vector_divide( const int n, const MKL_Complex16 x[], const MKL_Complex16 y[], MKL_Complex16 result[] ){
	vzDiv( n, x, y, result );
}

DLLEXPORT void z_vector_power(const int n, const MKL_Complex16 x[], const MKL_Complex16 y[], MKL_Complex16 result[]) {
	vzPow(n, x, y, result);
}

#if __cplusplus
}
#endif
