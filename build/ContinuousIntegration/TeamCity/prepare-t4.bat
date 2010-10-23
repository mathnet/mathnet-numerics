IF DEFINED CommonProgramFiles(x86) GOTO x64

:x86
SET common=%CommonProgramFiles%
GOTO prepare

:x64
SET common=%CommonProgramFiles(x86)%
GOTO prepare

:prepare
"%common%\Microsoft Shared\TextTemplating\1.2\texttransform.exe" -out ..\..\..\src\Numerics\Version.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\..\..\src\Numerics\Version.tt
"%common%\Microsoft Shared\TextTemplating\1.2\texttransform.exe" -out ..\..\..\src\Silverlight\Version.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\..\..\src\Silverlight\Version.tt

"%common%\Microsoft Shared\TextTemplating\1.2\texttransform.exe" -out ..\..\..\src\Numerics\Algorithms\LinearAlgebra\Atlas\AtlasLinearAlgebraProvider.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\..\..\src\Numerics\Algorithms\LinearAlgebra\Atlas\AtlasLinearAlgebraProvider.tt
"%common%\Microsoft Shared\TextTemplating\1.2\texttransform.exe" -out ..\..\..\src\Numerics\Algorithms\LinearAlgebra\Atlas\SafeNativeMethods.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\..\..\src\Numerics\Algorithms\LinearAlgebra\Atlas\SafeNativeMethods.tt
"%common%\Microsoft Shared\TextTemplating\1.2\texttransform.exe" -out ..\..\..\src\Numerics\Algorithms\LinearAlgebra\Mkl\MklLinearAlgebraProvider.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\..\..\src\Numerics\Algorithms\LinearAlgebra\Mkl\MklLinearAlgebraProvider.tt
"%common%\Microsoft Shared\TextTemplating\1.2\texttransform.exe" -out ..\..\..\src\Numerics\Algorithms\LinearAlgebra\Mkl\SafeNativeMethods.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\..\..\src\Numerics\Algorithms\LinearAlgebra\Mkl\SafeNativeMethods.tt

set common= 