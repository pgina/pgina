// requires Windows Server 2003 Service Pack 2, Windows Server 2008, Windows Vista Service Pack 1, Windows XP Service Pack 3
// requires windows installer 3.1
// http://www.microsoft.com/downloads/en/details.aspx?FamilyID=9cfb2d51-5ff4-4491-b0e5-b386f32c0992&displaylang=en

[CustomMessages]
vc2010x86_title=MS Visual C++ 2010 Redistributable package (x86)
vc2010x64_title=MS Visual C++ 2010 Redistributable package (x64)

en.vc2010x86_size=4.8 MB
en.vc2010x64_size=5.5 MB
// specifiy the full /lcid parameter, including a trailing space! or leave it an empty string if default or unknown
// en.dotnetfx40client_lcid='/lcid 1033 '
en.vc2010_lcid=''

#ifdef dotnet_Passive
#define vc2010_passive "'/passive '"
#else
#define vc2010_passive "''"
#endif

[Code]
const
    vc2010x86_url = 'http://download.microsoft.com/download/5/B/C/5BC5DBB3-652D-4DCE-B14A-475AB85EEF6E/vcredist_x86.exe';
    vc2010x64_url = 'http://download.microsoft.com/download/3/2/2/3224B87F-CFA0-4E70-BDA3-3DE650EFEBA5/vcredist_x64.exe';

procedure vc2010();
var
	version: cardinal;
begin
    // x86 (32 bit) runtime
    if not RegQueryDWordValue(HKLM32, 'SOFTWARE\Microsoft\VisualStudio\10.0\VC\VCRedist\x86', 'Installed', version) then
        RegQueryDWordValue(HKLM32, 'SOFTWARE\Microsoft\VisualStudio\10.0\VC\Runtimes\x86', 'Installed', version);
        if version <> 1 then
    		AddProduct('vcredist_x86.exe',
    			CustomMessage('vc2010_lcid') + '/q ' + {#vc2010_passive} + '/norestart',
    			CustomMessage('vc2010x86_title'),
    			CustomMessage('vc2010x86_size'),
    			vc2010x86_url,false,false);
    if isX64 then begin
        version := 0;
        // x64 (64 bit) runtime also.
	    if not RegQueryDWordValue(HKLM32, 'SOFTWARE\Microsoft\VisualStudio\10.0\VC\VCRedist\x64', 'Installed', version) then
            RegQueryDWordValue(HKLM32, 'SOFTWARE\Microsoft\VisualStudio\10.0\VC\Runtimes\x64', 'Installed', version);
        if version <> 1 then
    		AddProduct('vcredist_x64.exe',
    			CustomMessage('vc2010_lcid') + '/q ' + {#vc2010_passive} + '/norestart',
    			CustomMessage('vc2010x64_title'),
    			CustomMessage('vc2010x64_size'),
    			vc2010x64_url,false,false);
    end;
end;
