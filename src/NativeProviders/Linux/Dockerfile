FROM ubuntu:24.04

ENV DEBIAN_FRONTEND=noninteractive
ENV VERSION=2024.0
ENV INTEL=/opt/intel
ENV MKL=$INTEL/mkl/$VERSION
ENV OPENMP=$INTEL/compiler/$VERSION

# Install system dependencies
RUN apt-get update && apt-get install -y \
    wget \
    curl \
    g++ \
    g++-multilib \
    libc6-dev-i386 \
    ca-certificates \
    gnupg \
    lsb-release \
    software-properties-common \
    && rm -rf /var/lib/apt/lists/*

# Install Intel MKL using offline installer as it is not available in the apt repository
RUN mkdir -p /tmp/mkl \
    && cd /tmp/mkl \
    && wget https://registrationcenter-download.intel.com/akdlm/IRC_NAS/86d6a4c1-c998-4c6b-9fff-ca004e9f7455/l_onemkl_p_2024.0.0.49673.sh \
    && chmod +x l_onemkl_p_2024.0.0.49673.sh \
    && mkdir -p /opt/intel \
    && ./l_onemkl_p_2024.0.0.49673.sh -a -s --eula accept --install-dir /opt/intel \
    && rm -rf /tmp/mkl

# Set up environment variables for MKL
ENV LD_LIBRARY_PATH=""
ENV CPATH=""
ENV LIBRARY_PATH=""
ENV LD_LIBRARY_PATH=$MKL/lib/intel64:$OPENMP/lib:$LD_LIBRARY_PATH
ENV CPATH=$MKL/include:$CPATH
ENV LIBRARY_PATH=$MKL/lib/intel64:$OPENMP/lib:$LIBRARY_PATH

WORKDIR /workspace

# Copy project files
COPY . .

RUN chmod +x src/NativeProviders/Linux/mkl_build.sh

CMD ["sh", "-c", "cd src/NativeProviders/Linux && ./mkl_build.sh"]
