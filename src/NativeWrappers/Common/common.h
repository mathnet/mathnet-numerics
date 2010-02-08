#ifndef COMMON_H
#define COMMON_H

#ifdef _WINDOWS
	#define DLLEXPORT __declspec( dllexport )
#else
	#define DLLEXPORT
#endif

#endif 
