1) Install MKL.

There are many options. At time of writing this is a good source:
https://www.intel.com/content/www/us/en/developer/tools/oneapi/onemkl-download.html

2) Install g++:
sudo apt-get install g++ g++-multilib libc6-dev-i386

3) Build:
./mkl_build.sh
PS: you may have to update MKL's version number inside the script. See VERSION environment variable.
