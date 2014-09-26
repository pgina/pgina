# NSIS Installer config file for pGina
#
# Requires DotNetChecker plugin from: https://github.com/ProjectHuman/NsisDotNetChecker
#
!include LogicLib.nsh
!include WinVer.nsh
!include x64.nsh
!include DotNetChecker.nsh
!include MUI2.nsh

!define APPNAME "pGina"
!define VERSION "3.2.4.1"

RequestExecutionLevel admin  ; Require admin rights

Name "${APPNAME} - ${VERSION}"   ; Name in title bar
OutFile "pGina-${VERSION}-setup.exe" ; Output file

# UI configuration
!define MUI_ABORTWARNING
!define MUI_ICON "..\..\pGina\src\Configuration\Resources\pginaicon_redcircle.ico"
!define MUI_WELCOMEFINISHPAGE_BITMAP "images\welcome-finish.bmp"
!define MUI_UNWELCOMEFINISHPAGE_BITMAP "images\welcome-finish.bmp"
!define MUI_COMPONENTSPAGE_SMALLDESC

# Installer pages
!define MUI_PAGE_HEADER_TEXT "Install pGina ${VERSION}"
!define MUI_WELCOMEPAGE_TITLE "Install pGina ${VERSION}"
!insertmacro MUI_PAGE_WELCOME 
!insertmacro MUI_PAGE_LICENSE ..\..\LICENSE
!insertmacro MUI_PAGE_DIRECTORY 
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_COMPONENTS
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

# Custom function callbacks
!define MUI_CUSTOMFUNCTION_GUIINIT GuiInit

# Language files
!insertmacro MUI_LANGUAGE "English"

VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "${APPNAME} Setup"
VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "pGina Team"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "pGina installer"
VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "2014 pGina Team"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" ${VERSION} 
VIProductVersion ${VERSION}

########################
# Functions            #
########################
Function GuiInit
  # Determine the installation directory.  If were 64 bit, use
  # PROGRAMFILES64, otherwise, use PROGRAMFILES. 
  ${If} ${RunningX64}
    StrCpy $INSTDIR "$PROGRAMFILES64\pGina"
    SetRegView 64
  ${Else}
    StrCpy $INSTDIR "$PROGRAMFILES\pGina"
  ${EndIf}
FunctionEnd

#############################################
# Sections                                  #
#############################################

Section -Prerequisites
  # Check for and install .NET 4
  !insertmacro CheckNetFramework 40Full
SectionEnd

Section "pGina" InstallpGina 
  SectionIn RO ; Make this option read-only

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

Section "Core plugins" InstallCorePlugins
  SetOutPath $INSTDIR\Plugins\Core
  File "..\..\Plugins\Core\bin\*.dll"
SectionEnd

Section "Contributed plugins" InstallContribPlugins
  SetOutPath $INSTDIR\Plugins\Contrib
  File "..\..\Plugins\Contrib\bin\*.dll"
SectionEnd

Section /o "Visual C++ redistributable package" InstallVCRedist
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

#######################################
# Descriptions
#######################################
LangString DESC_InstallpGina ${LANG_ENGLISH} "Install pGina (required)."
LangString DESC_InstallCorePlugins ${LANG_ENGLISH} "Install pGina core plugins."
LangString DESC_InstallContribPlugins ${LANG_ENGLISH} "Install community contributed plugins."
LangString DESC_InstallVCRedist ${LANG_ENGLISH} "Visual C++ redistributable package is required. However, it may already be installed on your system."

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${InstallpGina} $(DESC_InstallpGina)
  !insertmacro MUI_DESCRIPTION_TEXT ${InstallCorePlugins} $(DESC_InstallCorePlugins)
  !insertmacro MUI_DESCRIPTION_TEXT ${InstallContribPlugins} $(DESC_InstallContribPlugins)
  !insertmacro MUI_DESCRIPTION_TEXT ${InstallVCRedist} $(DESC_InstallVCRedist)
!insertmacro MUI_FUNCTION_DESCRIPTION_END


