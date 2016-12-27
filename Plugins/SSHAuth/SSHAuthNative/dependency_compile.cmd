@echo off
cd /d %~dps0
if "%INCLUDE%"=="" (
	@echo please Open Visual Studio Command Prompt first
	pause
	exit 1
)
nasm.exe -v || for %%a in ("%ProgramFiles%" "%ProgramFiles(x86)%") do for /f "tokens=*" %%b in ('dir %%~dpnxsa\nasm.exe /B /S /A') do %%~dpsbnasm -v && set path=%path%;%%~dpsb
nasm.exe -v || (
	@echo please install nasm first
	pause
	exit
)

set zlib=zlib
set openssl=openssl
set libssh2=libssh2
set slashpath=%~dps0
set slashpath=%slashpath:\=/%
@echo %slashpath%

for %%a in (%zlib% %openssl% %libssh2%) do for /f "tokens=*" %%b in ('dir * /B /A:D') do @echo %%b | find /I "%%a" >NUL && set %%a=%%b
@echo zlib=%zlib%
@echo openssl=%openssl%
@echo libssh2=%libssh2%

@echo.
@echo verify zlibstat.vcxproj
type %~dps0%zlib%\contrib\vstudio\vc10\zlibstat.vcxproj | find /I "ZLIB_WINAPI" && (
	@echo remove ZLIB_WINAPI from PreprocessorDefinitions in zlibstat.vcxproj
	pause
	exit 1
)
@echo compiling zlib
if "%Platform%"=="x64" devenv.exe "%~dps0%zlib%\contrib\vstudio\vc10\zlibvc.sln" /Project zlibstat /Rebuild "Release|x64" /Out %~dps0zlibx64.log || (
	@echo error during build of zlib
	pause
	exit 1
)
if "%Platform%"=="" devenv.exe "%~dps0%zlib%\contrib\vstudio\vc10\zlibvc.sln" /Project zlibstat /Rebuild "Release|Win32" /Out %~dps0zlibx86.log || (
	@echo error during build of zlib
	pause
	exit 1
)
@echo done

@echo.
@echo configure openssl
cd /d %openssl%
if "%Platform%"=="x64" perl.exe Configure VC-WIN64A no-shared --release --with-zlib-lib=%~dps0%zlib%contrib\vstudio\vc10\x64\ZlibStatRelease --prefix=%~dps0%openssl%\VC-64A || (
	@echo error during config of openssl
	pause
	exit 1
)
if "%Platform%"=="" perl.exe Configure VC-WIN32 no-shared --release --with-zlib-lib=%~dps0%zlib%contrib\vstudio\vc10\x86\ZlibStatRelease --prefix=%~dps0%openssl%\VC-32 || (
	@echo error during config of openssl
	pause
	exit 1
)
@echo done
@echo.
@echo compile openssl
nmake clean
nmake || (
	@echo error during compile of openssl
	pause
	exit 1
)
nmake test || (
	@echo error during test of openssl
	pause
	exit 1
)
nmake install || (
	@echo error during install of openssl
	pause
	exit 1
)
@echo done

@echo.
@echo cmake libssh2
md %~dps0%libssh2%\x64
md %~dps0%libssh2%\x86
if "%Platform%"=="x64" cd /d %~dps0%libssh2%\x64
if "%Platform%"=="" cd /d %~dps0%libssh2%\x86
if "%Platform%"=="x64" cmake -G "Visual Studio 10 2010 Win64" -D"OPENSSL_INCLUDE_DIR=%slashpath%%openssl%/VC-64A/include" -D"OPENSSL_ROOT_DIR=%slashpath%%openssl%/VC-64A/lib" -D"ENABLE_ZLIB_COMPRESSION=1" -D"ZLIB_LIBRARY=%slashpath%%zlib%/contrib/vstudio/vc10/x64/ZlibStatRelease/zlibstat.lib" -D"BUILD_SHARED_LIBS=0" -D"ZLIB_INCLUDE_DIR=%slashpath%%zlib%" %slashpath%%libssh2% || (
	@echo error during cmake of libssh2
	pause
	exit 1
)
if "%Platform%"=="" cmake -G "Visual Studio 10 2010" -D"OPENSSL_INCLUDE_DIR=%slashpath%%openssl%/VC-32/include" -D"OPENSSL_ROOT_DIR=%slashpath%%openssl%/VC-32/lib" -D"ENABLE_ZLIB_COMPRESSION=1" -D"ZLIB_LIBRARY=%slashpath%%zlib%/contrib/vstudio/vc10/x86/ZlibStatRelease/zlibstat.lib" -D"BUILD_SHARED_LIBS=0" -D"ZLIB_INCLUDE_DIR=%slashpath%%zlib%" %slashpath%%libssh2% || (
	@echo error during cmake of libssh2
	pause
	exit 1
)
@echo done
@echo.
@echo compiling libssh2
if "%Platform%"=="x64" devenv.exe "%~dps0%libssh2%\x64\libssh2.sln" /Project libssh2 /Rebuild "Release|x64" /Out %~dps0libssh2x64.log || (
	@echo error during build of libssh2
	pause
	exit 1
)
if "%Platform%"=="" devenv.exe "%~dps0%libssh2%\x86\libssh2.sln" /Project libssh2 /Rebuild "Release|Win32" /Out %~dps0libssh2x86.log || (
	@echo error during build of libssh2
	pause
	exit 1
)
@echo done

@echo.
@echo copy libs
cd /d %~dps0
rd /s /q %~dps0x64
rd /s /q %~dps0x86
md %~dps0x64
md %~dps0x86
copy /y %~dps0\%zlib%\contrib\vstudio\vc10\x64\ZlibStatRelease\zlibstat.lib %~dps0x64
copy /y %~dps0\%zlib%\contrib\vstudio\vc10\x86\ZlibStatRelease\zlibstat.lib %~dps0x86
copy /y %~dps0\%openssl%\VC-64A\lib\libcrypto.lib %~dps0x64
copy /y %~dps0\%openssl%\VC-32\lib\libcrypto.lib %~dps0x86
copy /y %~dps0\%openssl%\VC-64A\lib\libssl.lib %~dps0x64
copy /y %~dps0\%openssl%\VC-32\lib\libssl.lib %~dps0x86
copy /y %~dps0\%libssh2%\x64\src\Release\libssh2.lib %~dps0x64
copy /y %~dps0\%libssh2%\x86\src\Release\libssh2.lib %~dps0x86
copy /y %~dps0\%libssh2%\include\libssh2.h %~dps0x64
copy /y %~dps0\%libssh2%\include\libssh2.h %~dps0x86
@echo.>%~dps0x64\%zlib%
@echo.>%~dps0x86\%zlib%
@echo.>%~dps0x64\%openssl%
@echo.>%~dps0x86\%openssl%
@echo.>%~dps0x64\%libssh2%
@echo.>%~dps0x86\%libssh2%
color 0A
@echo all done

pause