
[CustomMessages]
vc2012x86_title=MS Visual C++ 2012 Redistributable package (x86)
vc2012x64_title=MS Visual C++ 2012 Redistributable package (x64)

en.vc2012x86_size=6.3 MB
en.vc2012x64_size=6.9 MB

#ifdef dotnet_Passive
#define vc2012_passive "'/passive '"
#else
#define vc2012_passive "''"
#endif

[Code]
const
    vc2012x86_url = 'http://download.microsoft.com/download/1/6/B/16B06F60-3B20-4FF2-B699-5E9B7962F9AE/VSU1/vcredist_x86.exe';
    vc2012x64_url = 'http://download.microsoft.com/download/1/6/B/16B06F60-3B20-4FF2-B699-5E9B7962F9AE/VSU1/vcredist_x64.exe';

procedure vc2012();
var
	version: cardinal;
begin
    // x86 (32 bit) runtime
    if not RegQueryDWordValue(HKLM32, 'SOFTWARE\Microsoft\VisualStudio\11.0\VC\VCRedist\x86', 'Installed', version) then
        RegQueryDWordValue(HKLM32, 'SOFTWARE\Microsoft\VisualStudio\11.0\VC\Runtimes\x86', 'Installed', version);
        if version <> 1 then
    		AddProduct('vcredist_x86.exe',
    			'/q ' + {#vc2012_passive} + '/norestart',
    			CustomMessage('vc2012x86_title'),
    			CustomMessage('vc2012x86_size'),
    			vc2012x86_url,false,false);
    if isX64 then begin
        version := 0;
        // x64 (64 bit) runtime also.
	    if not RegQueryDWordValue(HKLM32, 'SOFTWARE\Microsoft\VisualStudio\11.0\VC\VCRedist\x64', 'Installed', version) then
            RegQueryDWordValue(HKLM32, 'SOFTWARE\Microsoft\VisualStudio\11.0\VC\Runtimes\x64', 'Installed', version);
        if version <> 1 then
    		AddProduct('vcredist_x64.exe',
    			'/q ' + {#vc2012_passive} + '/norestart',
    			CustomMessage('vc2012x64_title'),
    			CustomMessage('vc2012x64_size'),
    			vc2012x64_url,false,false);
    end;
end;
