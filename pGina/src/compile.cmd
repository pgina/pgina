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
		%%d %~dps0pGina-3.x-vs2010.sln /Rebuild "Release|x64" /Out %~dps0x64.log || pause && exit 1
		%%d %~dps0pGina-3.x-vs2010.sln /Rebuild "Release|Win32" /Out %~dps0Win32.log  || pause && exit 1
	) 
)
find /I "===" %~dps0x64.log
find /I "===" %~dps0Win32.log
@echo finished
pause
exit 0