# NSIS Installer config file for OpenCredential
#
# Requires DotNetChecker plugin from: https://github.com/ProjectHuman/NsisDotNetChecker
#
!include LogicLib.nsh
!include WinVer.nsh
!include x64.nsh
!include DotNetChecker.nsh
!include MUI2.nsh

!define APPNAME "OpenCredential"
!define VERSION "1.0.0.0"

RequestExecutionLevel admin  ; Require admin rights

Name "${APPNAME} - ${VERSION}"   ; Name in title bar
OutFile "OpenCredential-${VERSION}-setup.exe" ; Output file

# UI configuration
!define MUI_ABORTWARNING
!define MUI_ICON "..\..\OpenCredential\src\Configuration\Resources\OpenCredentialApp.ico"
!define MUI_WELCOMEFINISHPAGE_BITMAP "images\welcome-finish.bmp"
!define MUI_UNWELCOMEFINISHPAGE_BITMAP "images\welcome-finish.bmp"
!define MUI_COMPONENTSPAGE_SMALLDESC

# Installer pages
!define MUI_PAGE_HEADER_TEXT "Install OpenCredential ${VERSION}"
!define MUI_WELCOMEPAGE_TITLE "Install OpenCredential ${VERSION}"
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
VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "OpenCredential Project"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "OpenCredential installer"
VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "OpenCredential Project"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" ${VERSION} 
VIProductVersion ${VERSION}

########################
# Functions            #
########################
Function GuiInit
  # Determine the installation directory.  If were 64 bit, use
  # PROGRAMFILES64, otherwise, use PROGRAMFILES. 
  ${If} ${RunningX64}
    StrCpy $INSTDIR "$PROGRAMFILES64\OpenCredential"
    SetRegView 64
  ${Else}
    StrCpy $INSTDIR "$PROGRAMFILES\OpenCredential"
  ${EndIf}
FunctionEnd

#############################################
# Sections                                  #
#############################################

Section -Prerequisites
  # Check for and install .NET 4
  !insertmacro CheckNetFramework 40Full
SectionEnd

Section "OpenCredential" InstallOpenCredential 
  SectionIn RO ; Make this option read-only

  SetOutPath $INSTDIR
  File "..\..\OpenCredential\src\bin\*.exe"
  File "..\..\OpenCredential\src\bin\*.dll"
  File "..\..\OpenCredential\src\bin\log4net.xml"
  File "..\..\OpenCredential\src\bin\*.config"

  ${If} ${AtLeastWin7}
    SetOutPath $INSTDIR\Win32
    File "..\..\OpenCredential\src\bin\Win32\OpenCredentialCredentialProvider.dll"
    SetOutPath $INSTDIR\x64
    File "..\..\OpenCredential\src\bin\x64\OpenCredentialCredentialProvider.dll"
  ${Else}
    SetOutPath $INSTDIR\Win32
    File "..\..\OpenCredential\src\bin\Win32\OpenCredentialGINA.dll"
    SetOutPath $INSTDIR\x64
    File "..\..\OpenCredential\src\bin\x64\OpenCredentialGINA.dll"
  ${EndIf}

   WriteUninstaller "$INSTDIR\OpenCredential-Uninstall.exe"
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
  ExecWait '"$INSTDIR\OpenCredential.InstallUtil.exe" post-install'
SectionEnd

Section "un.OpenCredential" ; Uninstall OpenCredential
  ${If} ${RunningX64}
    SetRegView 64
  ${EndIf}

  SetOutPath $INSTDIR
  ExecWait '"$INSTDIR\OpenCredential.InstallUtil.exe" post-uninstall'
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
    Delete "$INSTDIR\Win32\OpenCredentialCredentialProvider.dll"
    Delete "$INSTDIR\x64\OpenCredentialCredentialProvider.dll"
  ${Else}
    Delete "$INSTDIR\Win32\OpenCredentialGINA.dll"
    Delete "$INSTDIR\x64\OpenCredentialGINA.dll"
  ${EndIf}
  RmDir $INSTDIR\Win32
  RmDir $INSTDIR\x64
SectionEnd

Section "un.Delete OpenCredential configuration"
  DeleteRegKey HKLM "SOFTWARE\OpenCredential3"
SectionEnd

Section "un.Delete OpenCredential logs"
  Delete $INSTDIR\log\*.txt
  RmDir $INSTDIR\log
SectionEnd

Section "un."
  RmDir $INSTDIR
SectionEnd

#######################################
# Descriptions
#######################################
LangString DESC_InstallOpenCredential ${LANG_ENGLISH} "Install OpenCredential (required)."
LangString DESC_InstallCorePlugins ${LANG_ENGLISH} "Install OpenCredential core plugins."
LangString DESC_InstallContribPlugins ${LANG_ENGLISH} "Install community contributed plugins."
LangString DESC_InstallVCRedist ${LANG_ENGLISH} "Visual C++ redistributable package is required. However, it may already be installed on your system."

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${InstallOpenCredential} $(DESC_InstallOpenCredential)
  !insertmacro MUI_DESCRIPTION_TEXT ${InstallCorePlugins} $(DESC_InstallCorePlugins)
  !insertmacro MUI_DESCRIPTION_TEXT ${InstallContribPlugins} $(DESC_InstallContribPlugins)
  !insertmacro MUI_DESCRIPTION_TEXT ${InstallVCRedist} $(DESC_InstallVCRedist)
!insertmacro MUI_FUNCTION_DESCRIPTION_END


