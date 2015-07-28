@echo off
call %~dps0..\pGina\src\compile.cmd batch || pause && exit 1
call %~dps0..\Plugins\compile.cmd batch || pause && exit 1

set inno=HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 5_is1
set ProgramFiles(x86) && set inno=HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 5_is1

reg.exe query "%inno%" /v DisplayIcon || (
	@echo cant find DisplayIcon in HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 5_is1
	pause
	exit 1
)
for /f "tokens=2*" %%a in ('reg.exe query "%inno%" /v DisplayIcon ^| find /I "REG_"') do set Compil32=%%~sb

%Compil32% /cc %~dps0installer.iss || pause && exit 1

@echo finished
pause
exit 0