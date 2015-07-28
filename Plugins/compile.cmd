@echo off

del /F /Q %~dps0*.log 2>NUL
reg.exe query "HKCR\VisualStudio.Solution\CLSID" >NUL || (
	@echo unable to find VisualStudio.Solution regkey
	pause
	exit 1
)
for /f "tokens=3*" %%a in ('reg.exe query "HKCR\VisualStudio.Solution\CLSID" /ve ^| find /I "REG_"') do (
	@echo %%b
	reg.exe query "HKLM\SOFTWARE\Classes\CLSID\%%b\LocalServer32" >NUL || (
		@echo unable to find CLSID VisualStudio.Solution regkey
		pause
		exit 1
	)
	for /f "tokens=3*" %%c in ('reg.exe query "HKLM\SOFTWARE\Classes\CLSID\%%b\LocalServer32" /ve ^| find /I "REG_"') do (
		@echo %%d
		for /f "tokens=*" %%e in ('dir %~dps0*.sln /B /S /A') do (
			@echo %%e
			%%d %%~se /Rebuild "Release|Any CPU" /Out %~dps0%%~ne.log || pause && exit 1
			find /I "===" %~dps0%%~ne.log
		)
	) 
)
@echo finished
if Not "%~1" == "batch" pause
exit /b 0