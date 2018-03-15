#define MyAppName "pGina.fork"
#define MyAppVersion "3.9.9.12"
#define MyAppPublisher "pGina Team"
#define MyAppURL "http://www.pgina.org/"
#define MyAppExeName "pGina.Configuration.exe"
#define MyAppSetupName 'pGina'
#define SetupScriptVersion '0'


; Use some useful packaging stuff from: http://tonaday.blogspot.com/2010/12/innosetup.html
// dotnet_Passive enabled shows the .NET/VC2010 installation progress, as it can take quite some time
#define dotnet_Passive
#define use_dotnetfx40
#define use_vc2010

// Enable the required define(s) below if a local event function (prepended with Local) is used
//#define haveLocalPrepareToInstall
//#define haveLocalNeedRestart
//#define haveLocalNextButtonClick


[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
UsePreviousAppDir=no
AppID={{3D8D0F0D-7DBF-400C-9C44-00BD21986139}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} v{#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=true
LicenseFile=..\LICENSE
OutputBaseFilename=pGinaSetup-{#MyAppVersion}
SetupIconFile=..\Installer\pgina_icons.ico
Compression=lzma/Max
SolidCompression=true
AppCopyright=pGina Team
ExtraDiskSpaceRequired=6
DisableDirPage=auto
AlwaysShowDirOnReadyPage=yes
AlwaysShowGroupOnReadyPage=yes
DisableProgramGroupPage=auto
AlwaysRestart=yes

ArchitecturesInstallIn64BitMode=x64 ia64

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
;Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\pGina\src\bin\pGina.Configuration.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\pGina\src\bin\*.exe"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\pGina\src\bin\*.dll"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\pGina\src\bin\*.xml"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\pGina\src\bin\*.config"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\Plugins\bin\*.exe"; DestDir: "{app}\Plugins"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\Plugins\bin\*.dll"; DestDir: "{app}\Plugins"; Flags: ignoreversion recursesubdirs createallsubdirs
;Source: "..\Plugins\bin\*.xml"; DestDir: "{app}\Plugins"; Flags: ignoreversion recursesubdirs createallsubdirs
;Source: "..\Plugins\bin\*.config"; DestDir: "{app}\Plugins"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\pGina.InstallUtil.exe"; Parameters: "post-install"; StatusMsg: "Installing service, CP/GINA, and setting permissions..."; WorkingDir: "{app}"; Flags: runhidden
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, "&", "&&")}}"; Flags: nowait postinstall skipifsilent runascurrentuser
Filename: "sc"; Parameters: "failure pgina reset= 0 actions= restart/5000"

[UninstallRun]
Filename: "{app}\pGina.InstallUtil.exe"; Parameters: "post-uninstall"; StatusMsg: "Removing service and CP/GINA..."; WorkingDir: "{app}"; Flags: runhidden

; More custom stuff from [] for ensuring user gets everything needed
#include "scripts\products.iss"

#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"

#ifdef use_dotnetfx40
#include "scripts\products\dotnetfx40client.iss"
#include "scripts\products\dotnetfx40full.iss"
#endif
#ifdef use_vc2010
#include "scripts\products\vc2010.iss"
#endif

#include "scripts\services.iss"

[CustomMessages]
win2000sp3_title=Windows 2000 Service Pack 3
winxpsp2_title=Windows XP Service Pack 2
winxpsp3_title=Windows XP Service Pack 3

#expr SaveToFile(AddBackslash(SourcePath) + "Preprocessed"+MyAppSetupname+SetupScriptVersion+".iss")

[Code]
var isVerySilent: Boolean;

function InitializeSetup(): Boolean;
var ResultCode: integer;
begin
  isVerySilent := False;
  for ResultCode := 1 to ParamCount do
    if CompareText(ParamStr(ResultCode), '/verysilent') = 0 then
    begin
      isVerySilent := True;
      Break;
    end;

  //init windows version
  initwinversion();

  //check if dotnetfx20 can be installed on this OS
  //if not minwinspversion(5, 0, 3) then begin
  //	MsgBox(FmtMessage(CustomMessage('depinstall_missing'), [CustomMessage('win2000sp3_title')]), mbError, MB_OK);
  //	exit;
  //end;
  if not minwinspversion(5, 1, 3) then begin
    if not isVerySilent then begin
      MsgBox(FmtMessage(CustomMessage('depinstall_missing'), [CustomMessage('winxpsp3_title')]), mbError, MB_OK);
      end;
    Abort;
  end;
	
  // If no .NET 4.0 framework found, install the full thing
#ifdef use_dotnetfx40
  dotnetfx40full(false);
#endif

  // Visual C++ 2010 Redistributable
#ifdef use_vc2010
  vc2010();
#endif
	
  Result := true;
end;

procedure DoPreInstall();
var ResultCode: Integer;
begin
  // If our service is already installed, stop it!
  if IsServiceInstalled('pGina') = true then begin
    Exec('net', 'stop pgina', '', SW_HIDE, ewWaitUntilTerminated, ResultCode)
    StopService('pGina');		
  end
end;

procedure DoPostInstall();
begin
  if IsWin64 then begin
    RestartReplace(ExpandConstant('{app}') + '\x64\pGinaCredentialProvider.dll', 'C:\WINDOWS\SYSTEM32\pGinaCredentialProvider.dll')
  end else begin
    RestartReplace(ExpandConstant('{app}') + '\Win32\pGinaCredentialProvider.dll', 'C:\WINDOWS\SYSTEM32\pGinaCredentialProvider.dll')
  end
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then begin
    DoPreInstall();
  end else if CurStep = ssPostInstall then begin
    DoPostInstall();
  end;
end;
