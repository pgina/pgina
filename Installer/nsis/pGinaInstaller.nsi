# NSIS Installer config file for pGina
#
# Requires DotNetChecker plugin from: https://github.com/ProjectHuman/NsisDotNetChecker
#
!include LogicLib.nsh
!include WinVer.nsh
!include x64.nsh
!include DotNetChecker.nsh

!define APPNAME "pGina"
!define VERSION "3.2.1.0"

RequestExecutionLevel admin  ; Require admin rights
LicenseData "..\..\LICENSE"

# Name in app title bar
Name "${APPNAME} - ${VERSION}"
Icon "..\..\pGina\src\Configuration\Resources\pginaicon_redcircle.ico"
OutFile "pGina-${VERSION}-setup.exe"

VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "${APPNAME} Setup"
VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "pGina Team"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "pGina installer"
VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "2014 pGina Team"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" ${VERSION} 
VIProductVersion ${VERSION}

Page license
Page directory findInstallDir
Page components
Page instfiles

UninstPage components
UninstPage uninstConfirm
UninstPage instfiles

Section -Prerequisites

  # Check for and install .NET 4
  !insertmacro CheckNetFramework 40Full

SectionEnd

Section "-pGina" 
  SetOutPath $INSTDIR
  File "..\..\pGina\src\bin\*.exe"
  File "..\..\pGina\src\bin\*.dll"
  File "..\..\pGina\src\bin\log4net.xml"
  File "..\..\pGina\src\bin\*.config"

  ${If} ${AtLeastWin7}
    SetOutPath $INSTDIR\Win32
    File "..\..\pGina\src\bin\Win32\pGinaCredentialProvider.dll"
    SetOutPath $INSTDIR\x64
    File "..\..\pGina\src\bin\x64\pGinaCredentialProvider.dll"
  ${Else}
    SetOutPath $INSTDIR\Win32
    File "..\..\pGina\src\bin\Win32\pGinaGINA.dll"
    SetOutPath $INSTDIR\x64
    File "..\..\pGina\src\bin\x64\pGinaGINA.dll"
  ${EndIf}

   WriteUninstaller "$INSTDIR\pGina-Uninstall.exe"
SectionEnd

Section "Core plugins"
  SetOutPath $INSTDIR\Plugins\Core
  File "..\..\Plugins\Core\bin\*.dll"
SectionEnd

Section "Contributed plugins"
  SetOutPath $INSTDIR\Plugins\Contrib
  File "..\..\Plugins\Contrib\bin\*.dll"
SectionEnd

Section /o "Visual C++ redistributable package"
  SetOutPath $INSTDIR 
  ${If} ${RunningX64}
     File "vcredist_x64.exe"
     ExecWait "$INSTDIR\vcredist_x64.exe"
     Delete $INSTDIR\vcredist_x64.exe
  ${Else}
     File "vcredist_x86.exe"
     ExecWait "$INSTDIR\vcredist_x86.exe"
     Delete $INSTDIR\vcredist_x86.exe
  ${EndIf}
SectionEnd

Section ; Run installer script
  SetOutPath $INSTDIR
  ExecWait '"$INSTDIR\pGina.InstallUtil.exe" post-install'
SectionEnd

Section "un.pGina" ; Uninstall pGina
  ${If} ${RunningX64}
    SetRegView 64
  ${EndIf}

  SetOutPath $INSTDIR
  ExecWait '"$INSTDIR\pGina.InstallUtil.exe" post-uninstall'
  Delete $INSTDIR\*.exe
  Delete $INSTDIR\*.dll
  Delete $INSTDIR\log4net.xml
  Delete $INSTDIR\*.config
  Delete $INSTDIR\*.InstallLog

  # Delete plugins
  Delete $INSTDIR\Plugins\Core\*.dll
  RmDir $INSTDIR\Plugins\Core
  Delete $INSTDIR\Plugins\Contrib\*.dll
  RmDir $INSTDIR\Plugins\Contrib
  RmDir $INSTDIR\Plugins

  ${If} ${AtLeastWin7}
    Delete "$INSTDIR\Win32\pGinaCredentialProvider.dll"
    Delete "$INSTDIR\x64\pGinaCredentialProvider.dll"
  ${Else}
    Delete "$INSTDIR\Win32\pGinaGINA.dll"
    Delete "$INSTDIR\x64\pGinaGINA.dll"
  ${EndIf}
  RmDir $INSTDIR\Win32
  RmDir $INSTDIR\x64
SectionEnd

Section "un.Delete pGina configuration"
  DeleteRegKey HKLM "SOFTWARE\pGina3"
SectionEnd

Section "un.Delete pGina logs"
  Delete $INSTDIR\log\*.txt
  RmDir $INSTDIR\log
SectionEnd

Section "un."
  RmDir $INSTDIR
SectionEnd

Function findInstallDir
  ${If} ${RunningX64}
    StrCpy $INSTDIR "$PROGRAMFILES64\pGina"
    SetRegView 64
  ${Else}
    StrCpy $INSTDIR "$PROGRAMFILES\pGina"
  ${EndIf}
FunctionEnd
