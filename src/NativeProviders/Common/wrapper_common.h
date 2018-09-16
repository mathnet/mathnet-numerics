#ifndef WRAPPER_COMMON_H
#define WRAPPER_COMMON_H

#ifndef __MS_VC_INSTALL_PATH
#define __MS_VC_INSTALL_PATH C:/Program Files (x86)/Microsoft Visual Studio/2017/Professional/VC/Tools/MSVC/14.15.26726
#endif

#ifdef _WINDOWS
	#define DLLEXPORT __declspec( dllexport )
#else
	#define DLLEXPORT
#endif

#endif
