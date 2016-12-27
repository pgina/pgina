@echo off
if "%~1"=="" (
	@echo designed to run from Visual Studio
	pause
	exit 0
)

if Exist %~dpsn0.run (
	exit 0
) else (
	echo.>%~dpsn0.run
	call %* || (
		call :clean
		exit 1
	)
)
call :clean
exit 0


:clean
	del /F /Q %~dpsn0.run
	rd /S /Q %~dps0Release
	del /F /Q %~dps0Debug\Win32\*.lib
	del /F /Q %~dps0Debug\x64\*.lib
:EOF