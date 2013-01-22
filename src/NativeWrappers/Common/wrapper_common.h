#ifndef WRAPPER_COMMON_H
#define WRAPPER_COMMON_H

#ifdef _WINDOWS
	#define DLLEXPORT __declspec( dllexport )
#else
	#define DLLEXPORT
#endif

#endif 
