Install MKL:
tar -zxvf l_mkl_11.3.0.109.tgz
sudo ./install.sh
Make sure to include IA32 as well

Install g++:
sudo apt-get install g++ g++-multilib libc6-dev-i386

Build:
./mkl_build.sh
