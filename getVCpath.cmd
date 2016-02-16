@echo off
set WOW=
set ProgramFiles(x86) && set WOW=\Wow6432Node

for /f "tokens=*" %%a in ('reg.exe query "HKCR\._sln" /ve ^| find /I "REG_"') do set findDATA_="%%a"
set findDATA_=%findDATA_:<=%
set findDATA_=%findDATA_:>=%
set findDATA_=%findDATA_:)=%
set findDATA_=%findDATA_:(=%
set findDATA_=%findDATA_:"=%
call :findDATA %findDATA_%
set findDATA_=
@echo %findDATA%

@echo reg.exe query "HKCR\VisualStudio.Solution\CLSID"
reg.exe query "HKCR\VisualStudio.Solution\CLSID" >NUL || (
	@echo unable to find VisualStudio.Solution regkey
	pause
	exit /b 1
)

@echo reg.exe query "HKCR\VisualStudio.Solution\CLSID" /ve
for /f "tokens=%findDATA%*" %%a in ('reg.exe query "HKCR\VisualStudio.Solution\CLSID" /ve ^| find /I "REG_"') do set VCguid=%%~b

@echo reg.exe query "HKLM\SOFTWARE%WOW%\Classes\CLSID\%VCguid%\LocalServer32"
reg.exe query "HKLM\SOFTWARE%WOW%\Classes\CLSID\%VCguid%\LocalServer32" >NUL || (
	@echo unable to find CLSID VisualStudio.Solution regkey
	pause
	exit /b 1
)

@echo reg.exe query "HKLM\SOFTWARE%WOW%\Classes\CLSID\%VCguid%\LocalServer32" /ve
for /f "tokens=%findDATA%*" %%a in ('reg.exe query "HKLM\SOFTWARE%WOW%\Classes\CLSID\%VCguid%\LocalServer32" /ve ^| find /I "REG_"') do set VCpath=%%~sb

if "%VCpath%"=="" (
	@echo can't find Visual Studio Path
	exit /b 1
)
@echo VCpath=%VCpath%
exit /b 0

:findDATA
@echo %*
	if "%findDATA%"=="" set /A findDATA=0
	set /A findDATA+=1
	set args=%1"
	set args=%args:"=%

	@echo %args% | findstr /I /R /C:"^REG_" >NUL && exit /b 0

	SHIFT
	set args=%1"
	set args=%args:"=%
	if NOT "%args%" == "" GOTO findDATA
goto :EOF


:EOF