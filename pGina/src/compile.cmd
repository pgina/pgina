@echo off

del /F /Q %~dps0*.log 2>NUL
if "%VCpath%"=="" call %~dps0..\..\getVCpath.cmd || exit /b 1

@echo.
@echo compiling pgina 64bit
%VCpath% %~dps0pGina-3.x-vs2010.sln /Rebuild "Release|x64" /Out %~dps0x64.log || (
	@echo failed to compile
	pause
	exit /b 1
)
find /I "===" %~dps0x64.log

@echo.
@echo compiling pgina 32bit
%VCpath% %~dps0pGina-3.x-vs2010.sln /Rebuild "Release|Win32" /Out %~dps0Win32.log || (
	@echo failed to compile
	pause
	exit /b 1
)
find /I "===" %~dps0Win32.log

if Not "%~1" == "batch" (
	@echo finished
	pause
)
exit /b 0
