export INTEL=/opt/intel
export MKL=$INTEL/mkl
export OPENMP=$INTEL/composerxe/lib
export OUT=../../../out/MKL/OSX

mkdir -p $OUT/x64
mkdir -p $OUT/x86

clang++ -D_M_X64 -DGCC -m64 --shared -fPIC -o $OUT/x64/MathNet.Numerics.MKL.dll -I$MKL/include -I../Common ../MKL/memory.c ../MKL/capabilities.cpp ../MKL/vector_functions.c ../MKL/blas.c ../MKL/lapack.cpp $MKL/lib/libmkl_intel_lp64.a $MKL/lib/libmkl_core.a $MKL/lib/libmkl_intel_thread.a -L$OPENMP -liomp5 -lpthread -lm  

cp $OPENMP/libiomp5.dylib  $OUT/x64/

clang++ -D_M_IX86 -DGCC -m32 --shared -fPIC -o $OUT/x86/MathNet.Numerics.MKL.dll -I$MKL/include -I../Common ../MKL/memory.c ../MKL/capabilities.cpp ../MKL/vector_functions.c ../MKL/blas.c ../MKL/lapack.cpp $MKL/lib/libmkl_intel.a $MKL/lib/libmkl_core.a $MKL/lib/libmkl_intel_thread.a  -L$OPENMP -liomp5 -lpthread -lm  

cp $OPENMP/libiomp5.dylib  $OUT/x86/
