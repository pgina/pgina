
[CustomMessages]
vc14x86_title=Microsoft Visual C++ Redistributable (latest supported) (x86)
vc14x64_title=Microsoft Visual C++ Redistributable (latest supported) (x64)

en.vc14x86_size=Latest
en.vc14x64_size=Latest

#ifdef dotnet_Passive
#define vc14_passive "'/passive '"
#else
#define vc14_passive "''"
#endif

[Code]
const
    vc14x86_url = 'https://aka.ms/vc14/vc_redist.x86.exe';
    vc14x64_url = 'https://aka.ms/vc14/vc_redist.x64.exe';

function IsVc14Installed(arch: string): boolean;
var
    installed: cardinal;
begin
    installed := 0;
    if not RegQueryDWordValue(HKLM32, 'SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\' + arch, 'Installed', installed) then
        RegQueryDWordValue(HKLM64, 'SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\' + arch, 'Installed', installed);

    Result := installed = 1;
end;

procedure vc14();
begin
    if not IsVc14Installed('x86') then begin
        ExtractTemporaryFile('VC_redist.x86.exe');
        AddProduct(
            'VC_redist.x86.exe',
            '/install /quiet ' + {#vc14_passive} + '/norestart',
            CustomMessage('vc14x86_title'),
            CustomMessage('vc14x86_size'),
            vc14x86_url,
            false,
            false);
    end;

    if isX64 and (not IsVc14Installed('x64')) then begin
        ExtractTemporaryFile('VC_redist.x64.exe');
        AddProduct(
            'VC_redist.x64.exe',
            '/install /quiet ' + {#vc14_passive} + '/norestart',
            CustomMessage('vc14x64_title'),
            CustomMessage('vc14x64_size'),
            vc14x64_url,
            false,
            false);
    end;
end;
