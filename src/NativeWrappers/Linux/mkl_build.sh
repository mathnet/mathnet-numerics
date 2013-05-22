export INTEL=/opt/intel
export MKL=$INTEL/mkl
export OPENMP=$INTEL/composerxe/lib

g++ -std=c++11 -DGCC -m64 --shared -fPIC -o ../../../../MKL/Linux/x64/MathNet.Numerics.MKL.dll -I$MKL/include -I../Common ../MKL/vector_functions.c ../MKL/blas.c ../MKL/lapack.cpp -Wl,--start-group  $MKL/lib/intel64/libmkl_intel_lp64.a $MKL/lib/intel64/libmkl_intel_thread.a $MKL/lib/intel64/libmkl_core.a -Wl,--end-group -L$OPENMP/intel64 -liomp5 -lpthread -lm  

cp $OPENMP/intel64/libiomp5.so  ../../../../MKL/Linux/x64/

g++ -std=c++11 -DGCC -m32 --shared -fPIC -o ../../../../MKL/Linux/x86/MathNet.Numerics.MKL.dll -I$MKL/include -I../Common ../MKL/vector_functions.c ../MKL/blas.c ../MKL/lapack.cpp -Wl,--start-group $MKL/lib/ia32/libmkl_intel.a $MKL/lib/ia32/libmkl_intel_thread.a $MKL/lib/ia32/libmkl_core.a -Wl,--end-group -L$OPENMP/ia32 -liomp5 -lpthread -lm  

cp $OPENMP/ia32/libiomp5.so  ../../../../MKL/Linux/x86/
