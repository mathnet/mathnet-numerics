#include "blas.h"

#ifdef _WINDOWS
	#define DLLEXPORT __declspec( dllexport )
#else
	#define DLLEXPORT
#endif

DLLEXPORT void s_axpy( const int n, const float alpha, const float x[], float y[]){
	cblas_saxpy(n, alpha, x, 1, y, 1);
}

DLLEXPORT void d_axpy( const int n, const double alpha, const double x[], double y[]){
	cblas_daxpy(n, alpha, x, 1, y, 1);
}

DLLEXPORT void c_axpy( const int n, const Complex8 alpha, const Complex8 x[], Complex8 y[]){
	cblas_caxpy(n, &alpha, x, 1, y, 1);
}

DLLEXPORT void z_axpy( const int n, const Complex16 alpha, const Complex16 x[], Complex16 y[]){
	cblas_zaxpy(n, &alpha, x, 1, y, 1);
}
