#define MyAppName "pGina"
#define MyAppVersion "3.0"
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
AppID={{3D8D0F0D-7DBF-400C-9C44-00BD21986138}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}-3
DefaultGroupName={#MyAppName}
AllowNoIcons=true
LicenseFile=C:\Code\pgina\LICENSE
OutputBaseFilename=pGinaSetup
SetupIconFile=C:\Code\pgina\Installer\pgina.ico
Compression=lzma/Max
SolidCompression=true
AppCopyright=pGina Team
AppVerName=pGina v3.0.0
ExtraDiskSpaceRequired=6

ArchitecturesInstallIn64BitMode=x64 ia64

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
;Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "C:\Code\pgina\pGina\src\bin\pGina.Configuration.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Code\pgina\pGina\src\bin\*.exe"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "C:\Code\pgina\pGina\src\bin\*.dll"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "C:\Code\pgina\pGina\src\bin\*.xml"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "C:\Code\pgina\pGina\src\bin\*.config"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
;Source: "C:\Code\pgina\Plugins\bin\*.exe"; DestDir: "{app}\Plugins"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "C:\Code\pgina\Plugins\bin\*.dll"; DestDir: "{app}\Plugins"; Flags: ignoreversion recursesubdirs createallsubdirs
;Source: "C:\Code\pgina\Plugins\bin\*.xml"; DestDir: "{app}\Plugins"; Flags: ignoreversion recursesubdirs createallsubdirs
;Source: "C:\Code\pgina\Plugins\bin\*.config"; DestDir: "{app}\Plugins"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\pGina.Service.ServiceHost.exe"; Parameters: "--install"
Filename: "{app}\pGina.Service.ServiceHost.exe"; Parameters: "--start"
Filename: "{app}\pGina.CredentialProvider.Registration.exe"; Parameters: "--mode install"; WorkingDir: "{app}" 
Filename: "{app}\pGina.CredentialProvider.Registration.exe"; Parameters: "--mode enable"; WorkingDir: "{app}"
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, "&", "&&")}}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{app}\pGina.Service.ServiceHost.exe"; Parameters: "--uninstall"
Filename: "{app}\pGina.Service.ServiceHost.exe"; Parameters: "--stop"
Filename: "{app}\pGina.CredentialProvider.Registration.exe"; Parameters: "--mode uninstall"; WorkingDir: "{app}"

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
	
	// If no .NET 4.0 framework found, install the smallest
#ifdef use_dotnetfx40
	dotnetfx40full(true);
#endif

	// Visual C++ 2010 Redistributable
#ifdef use_vc2010
	vc2010();
#endif
	
	Result := true;
end;
