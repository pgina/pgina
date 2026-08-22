#define MyAppName "OpenCredential"
#define MyAppVersion "1.0.0.0"
#define MyAppPublisher "OpenCredential Project"
#define MyAppURL "https://github.com/pedropablobm/OpenCredential"
#define MyAppExeName "OpenCredential.Configuration.exe"
#define MyAppSetupName 'OpenCredential'
#define SetupScriptVersion '0'


; Use some useful packaging stuff from: http://toneday.blogspot.com/2010/12/innosetup.html
; dotnet_Passive enabled shows the .NET/VC runtime installation progress, as it can take quite some time
#define dotnet_Passive
#define use_dotnetfx40
#define use_vc14

; Enable the required define(s) below if a local event function (prepended with Local) is used
;#define haveLocalPrepareToInstall
;#define haveLocalNeedRestart
;#define haveLocalNextButtonClick


[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppID={{3D8D0F0D-7DBF-400C-9C44-00BD21986138}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} v{#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={commonpf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=true
LicenseFile=OpenCredential-License.txt
OutputBaseFilename=OpenCredentialInstaller-{#MyAppVersion}
SetupIconFile=..\OpenCredential\src\Configuration\Resources\OpenCredentialApp.ico
Compression=lzma/Max
SolidCompression=true
AppCopyright=OpenCredential Project
ExtraDiskSpaceRequired=6
DisableDirPage=auto
AlwaysShowDirOnReadyPage=yes
AlwaysShowGroupOnReadyPage=yes
DisableProgramGroupPage=auto

ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
;Name: "english"; MessagesFile: "compiler:Default.isl"

[Registry]
; Remove legacy registry keys from earlier pGina-based installs
Root: HKLM; Subkey: "SOFTWARE\pGina"; Flags: uninsdeletekey
Root: HKCU; Subkey: "SOFTWARE\pGina"; Flags: uninsdeletekey
; Also remove older compatibility keys if they exist
Root: HKLM; Subkey: "SOFTWARE\pGina3"; Flags: uninsdeletekey
Root: HKCU; Subkey: "SOFTWARE\pGina3"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\OpenCredential"; Flags: uninsdeletekey
Root: HKCU; Subkey: "SOFTWARE\OpenCredential"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\OpenCredential3"; Flags: uninsdeletekey
Root: HKCU; Subkey: "SOFTWARE\OpenCredential3"; Flags: uninsdeletekey

[InstallDelete]
; Remove previous configuration before installing (clean install)
Type: filesandordirs; Name: "{commonappdata}\pGina"
Type: filesandordirs; Name: "{commonappdata}\pGina3"
Type: filesandordirs; Name: "{userappdata}\pGina"
Type: filesandordirs; Name: "{userappdata}\pGina3"
Type: filesandordirs; Name: "{localappdata}\pGina"
Type: filesandordirs; Name: "{localappdata}\pGina3"
Type: filesandordirs; Name: "{commonappdata}\OpenCredential"
Type: filesandordirs; Name: "{commonappdata}\OpenCredential3"
Type: filesandordirs; Name: "{userappdata}\OpenCredential"
Type: filesandordirs; Name: "{userappdata}\OpenCredential3"
Type: filesandordirs; Name: "{localappdata}\OpenCredential"
Type: filesandordirs; Name: "{localappdata}\OpenCredential3"

[UninstallDelete]
; Remove configuration during uninstall
Type: filesandordirs; Name: "{commonappdata}\pGina"
Type: filesandordirs; Name: "{commonappdata}\pGina3"
Type: filesandordirs; Name: "{userappdata}\pGina"
Type: filesandordirs; Name: "{userappdata}\pGina3"
Type: filesandordirs; Name: "{localappdata}\pGina"
Type: filesandordirs; Name: "{localappdata}\pGina3"
Type: filesandordirs; Name: "{commonappdata}\OpenCredential"
Type: filesandordirs; Name: "{commonappdata}\OpenCredential3"
Type: filesandordirs; Name: "{userappdata}\OpenCredential"
Type: filesandordirs; Name: "{userappdata}\OpenCredential3"
Type: filesandordirs; Name: "{localappdata}\OpenCredential"
Type: filesandordirs; Name: "{localappdata}\OpenCredential3"
; Remove installation directory completely
Type: filesandordirs; Name: "{app}"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "bin\VC_redist.x86.exe"; DestDir: "{tmp}"; Flags: dontcopy
Source: "bin\VC_redist.x64.exe"; DestDir: "{tmp}"; Flags: dontcopy
Source: "..\OpenCredential\src\bin\OpenCredential.Configuration.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\OpenCredential\src\bin\*.exe"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "pGina*,FakeWinlogon.exe,NativeLibTest.exe"
Source: "..\OpenCredential\src\bin\*.dll"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "pGina*"
Source: "..\OpenCredential\src\bin\*.xml"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\OpenCredential\src\bin\*.config"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "pGina*"
; Keep wildcard packaging for core dependencies, but exclude legacy plugin binaries
; that may still be present in bin from older builds and would duplicate PluginUuid.
Source: "..\Plugins\Core\bin\*.dll"; DestDir: "{app}\Plugins\Core"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "pGina.Plugin.*,pGina.Shared.dll,Abstractions.dll"
Source: "..\Plugins\Core\bin\*.xml"; DestDir: "{app}\Plugins\Core"; Flags: ignoreversion recursesubdirs createallsubdirs
;Source: "..\Plugins\Contrib\bin\*.dll"; DestDir: "{app}\Plugins\Contrib"; Flags: ignoreversion recursesubdirs createallsubdirs


[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\OpenCredential.InstallUtil.exe"; Parameters: "post-install"; StatusMsg: "Installing OpenCredential service, credential provider, and permissions..."; WorkingDir: "{app}"; Flags: runhidden
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, "&", "&&")}}"; Flags: nowait postinstall skipifsilent runascurrentuser

[UninstallRun]
Filename: "{app}\OpenCredential.InstallUtil.exe"; Parameters: "post-uninstall"; StatusMsg: "Removing OpenCredential service, credential provider, and permissions..."; WorkingDir: "{app}"; Flags: runhidden; RunOnceId: "UninstallService"

; More custom stuff from [] for ensuring user gets everything needed
#include "scripts\products.iss"

#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"

#ifdef use_dotnetfx40
#include "scripts\products\dotnetfx40client.iss"
#include "scripts\products\dotnetfx40full.iss"
#endif
#ifdef use_vc14
#include "scripts\products\vc14.iss"
#endif

#include "scripts\services.iss"

[CustomMessages]
win2000sp3_title=Windows 2000 Service Pack 3
winxpsp2_title=Windows XP Service Pack 2
winxpsp3_title=Windows XP Service Pack 3

#expr SaveToFile(AddBackslash(SourcePath) + "Preprocessed"+MyAppSetupname+SetupScriptVersion+".iss")

[Code]
function InitializeSetup(): Boolean;
begin
    //init windows version
    initwinversion();
    
    //check if dotnetfx20 can be installed on this OS
    //if not minwinspversion(5, 0, 3) then begin
    //	MsgBox(FmtMessage(CustomMessage('depinstall_missing'), [CustomMessage('win2000sp3_title')]), mbError, MB_OK);
    //	exit;
    //end;
    if not minwinspversion(5, 1, 3) then begin
        MsgBox(FmtMessage(CustomMessage('depinstall_missing'), [CustomMessage('winxpsp3_title')]), mbError, MB_OK);
        exit;
    end;
    
    // If no .NET 4.0 framework found, install the full thing
#ifdef use_dotnetfx40
    dotnetfx40full(false);
#endif

    // Latest supported Visual C++ v14 Redistributable
#ifdef use_vc14
    vc14();
#endif
    
    Result := true;
end;

procedure DoPreInstall();
begin
  // Stop either the current OpenCredential service or a legacy pGina service.
  if IsServiceInstalled('OpenCredential') = true then begin
        StopService('OpenCredential');
  end else if IsServiceInstalled('pGina') = true then begin
        StopService('pGina');
  end;
end;

procedure DoPostInstall();
begin
  // ...
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then begin
    DoPreInstall();
  end else if CurStep = ssPostInstall then begin
    DoPostInstall();
  end;
end;
