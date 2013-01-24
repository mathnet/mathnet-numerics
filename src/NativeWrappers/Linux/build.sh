export INTEL=/opt/intel
export MKL=$INTEL/mkl

g++ --shared -fPIC -o ./x64/MathNet.Numerics.MKL.so -I$MKL/include -I../Common  ../MKL/vector_functions.c ../MKL/blas.c ../MKL/lapack.cpp $INTEL/lib/intel64/libiomp5.a $MKL/lib/intel64/libmkl_intel_lp64.a $MKL/lib/intel64/libmkl_intel_thread.a $MKL/lib/intel64/libmkl_core.a 

g++ -m32 --shared -fPIC -o ./x86/MathNet.Numerics.MKL.so -I$MKL/include -I../Common  ../MKL/vector_functions.c ../MKL/blas.c ../MKL/lapack.cpp $INTEL/lib/ia32/libiomp5.a $MKL/lib/ia32/libmkl_intel.a $MKL/lib/ia32/libmkl_intel_thread.a $MKL/lib/ia32/libmkl_core.a 
