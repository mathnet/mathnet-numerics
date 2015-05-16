#ifndef WRAPPER_CUDA_H
#define WRAPPER_CUDA_H

#include "wrapper_common.h"

#define SAFECUDACALL(error,call) {*error = call; if(*error){goto exit;}}

#endif