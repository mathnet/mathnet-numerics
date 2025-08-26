# Build Native Libraries for MathNet.Numerics MKL for Linux

### Option 1: Manual Installation

**Install MKL**

There are many options. At time of writing this is a good source:
https://www.intel.com/content/www/us/en/developer/tools/oneapi/onemkl-download.html

**Install g++**

```bash
sudo apt-get install g++ g++-multilib libc6-dev-i386
```

**Build**

```bash
./mkl_build.sh
```

**Note:** You may have to update MKL's version number inside the script. See `VERSION` environment variable.

### Option 2: Docker

**Build the native libraries**

Run following commands from the project root.

```bash
docker build -f src/NativeProviders/Linux/Dockerfile -t mathnet-mkl-builder .
docker run --rm -v $(pwd)/out:/workspace/out mathnet-mkl-builder
```

## Output

The build creates native libraries in:
- `out/MKL/Linux/x64/libMathNetNumericsMKL.so` (64-bit)
- `out/MKL/Linux/x86/libMathNetNumericsMKL.so` (32-bit)
- Plus OpenMP runtime libraries (`libiomp5.so`)
