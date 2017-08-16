export INTEL=/opt/intel
export MKL=$INTEL/mkl
export OPENMP=$INTEL/lib
export OUT=../../../out/MKL/OSX

mkdir -p $OUT/x64
mkdir -p $OUT/x86

clang++ -std=c++11 -D_M_X64 -DGCC -m64 --shared -fPIC -o $OUT/x64/MathNet.Numerics.MKL.dll -I$MKL/include -I../Common -I../MKL ../MKL/memory.c ../MKL/capabilities.cpp ../MKL/vector_functions.c ../Common/blas.c ../Common/lapack.cpp ../MKL/fft.cpp  $MKL/lib/libmkl_intel_lp64.a $MKL/lib/libmkl_core.a $MKL/lib/libmkl_intel_thread.a -L$OPENMP -liomp5 -lpthread -lm

cp $OPENMP/libiomp5.dylib  $OUT/x64/

clang++ -std=c++11 -D_M_IX86 -DGCC -m32 --shared -fPIC -o $OUT/x86/MathNet.Numerics.MKL.dll -I$MKL/include -I../Common -I../MKL ../MKL/memory.c ../MKL/capabilities.cpp ../MKL/vector_functions.c ../Common/blas.c ../Common/lapack.cpp ../MKL/fft.cpp  $MKL/lib/libmkl_intel_lp64.a $MKL/lib/libmkl_core.a $MKL/lib/libmkl_intel_thread.a -L$OPENMP -liomp5 -lpthread -lm

cp $OPENMP/libiomp5.dylib  $OUT/x86/
