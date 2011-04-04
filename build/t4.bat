IF DEFINED CommonProgramFiles(x86) GOTO x64

:x86
SET common=%CommonProgramFiles%
GOTO prepare

:x64
SET common=%CommonProgramFiles(x86)%
GOTO prepare

:prepare
"%common%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out ..\src\Numerics\Version.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\src\Numerics\Version.tt
"%common%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out ..\src\Silverlight\Version.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\src\Silverlight\Version.tt

"%common%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out ..\src\Numerics\Algorithms\LinearAlgebra\Mkl\MklLinearAlgebraProvider.Common.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\src\Numerics\Algorithms\LinearAlgebra\Mkl\MklLinearAlgebraProvider.Common.tt
"%common%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out ..\src\Numerics\Algorithms\LinearAlgebra\Mkl\MklLinearAlgebraProvider.Complex.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\src\Numerics\Algorithms\LinearAlgebra\Mkl\MklLinearAlgebraProvider.Complex.tt
"%common%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out ..\src\Numerics\Algorithms\LinearAlgebra\Mkl\MklLinearAlgebraProvider.Complex32.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\src\Numerics\Algorithms\LinearAlgebra\Mkl\MklLinearAlgebraProvider.Complex32.tt
"%common%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out ..\src\Numerics\Algorithms\LinearAlgebra\Mkl\MklLinearAlgebraProvider.double.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\src\Numerics\Algorithms\LinearAlgebra\Mkl\MklLinearAlgebraProvider.double.tt
"%common%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out ..\src\Numerics\Algorithms\LinearAlgebra\Mkl\MklLinearAlgebraProvider.float.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\src\Numerics\Algorithms\LinearAlgebra\Mkl\MklLinearAlgebraProvider.float.tt
"%common%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out ..\src\Numerics\Algorithms\LinearAlgebra\Mkl\SafeNativeMethods.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\src\Numerics\Algorithms\LinearAlgebra\Mkl\SafeNativeMethods.tt

"%common%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out ..\src\Numerics\Algorithms\LinearAlgebra\GotoBlas\GotoBlasLinearAlgebraProvider.Common.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\src\Numerics\Algorithms\LinearAlgebra\GotoBlas\GotoBlasLinearAlgebraProvider.Common.tt
"%common%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out ..\src\Numerics\Algorithms\LinearAlgebra\GotoBlas\GotoBlasLinearAlgebraProvider.Complex.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\src\Numerics\Algorithms\LinearAlgebra\GotoBlas\GotoBlasLinearAlgebraProvider.Complex.tt
"%common%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out ..\src\Numerics\Algorithms\LinearAlgebra\GotoBlas\GotoBlasLinearAlgebraProvider.Complex32.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\src\Numerics\Algorithms\LinearAlgebra\GotoBlas\GotoBlasLinearAlgebraProvider.Complex32.tt
"%common%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out ..\src\Numerics\Algorithms\LinearAlgebra\GotoBlas\GotoBlasLinearAlgebraProvider.double.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\src\Numerics\Algorithms\LinearAlgebra\GotoBlas\GotoBlasLinearAlgebraProvider.double.tt
"%common%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out ..\src\Numerics\Algorithms\LinearAlgebra\GotoBlas\GotoBlasLinearAlgebraProvider.float.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\src\Numerics\Algorithms\LinearAlgebra\GotoBlas\GotoBlasLinearAlgebraProvider.float.tt
"%common%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out ..\src\Numerics\Algorithms\LinearAlgebra\GotoBlas\SafeNativeMethods.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\src\Numerics\Algorithms\LinearAlgebra\GotoBlas\SafeNativeMethods.tt

"%common%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out ..\src\Numerics\Algorithms\LinearAlgebra\Acml\AcmlLinearAlgebraProvider.Common.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\src\Numerics\Algorithms\LinearAlgebra\Acml\AcmlLinearAlgebraProvider.Common.tt
"%common%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out ..\src\Numerics\Algorithms\LinearAlgebra\Acml\AcmlLinearAlgebraProvider.Complex.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\src\Numerics\Algorithms\LinearAlgebra\Acml\AcmlLinearAlgebraProvider.Complex.tt
"%common%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out ..\src\Numerics\Algorithms\LinearAlgebra\Acml\AcmlLinearAlgebraProvider.Complex32.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\src\Numerics\Algorithms\LinearAlgebra\Acml\AcmlLinearAlgebraProvider.Complex32.tt
"%common%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out ..\src\Numerics\Algorithms\LinearAlgebra\Acml\AcmlLinearAlgebraProvider.double.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\src\Numerics\Algorithms\LinearAlgebra\Acml\AcmlLinearAlgebraProvider.double.tt
"%common%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out ..\src\Numerics\Algorithms\LinearAlgebra\Acml\AcmlLinearAlgebraProvider.float.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\src\Numerics\Algorithms\LinearAlgebra\Acml\AcmlLinearAlgebraProvider.float.tt
"%common%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out ..\src\Numerics\Algorithms\LinearAlgebra\Acml\SafeNativeMethods.cs -P "%ProgramFiles%\Reference Assemblies\Microsoft\Framework\v3.5" ..\src\Numerics\Algorithms\LinearAlgebra\Acml\SafeNativeMethods.tt

set common = 