// requires Windows Server 2003 Service Pack 2, Windows Server 2008, Windows Vista Service Pack 1, Windows XP Service Pack 3
// requires windows installer 3.1
// WARNING: express setup (downloads and installs the components depending on your OS) if you want to deploy it on cd or network download the full bootsrapper on website below
// http://www.microsoft.com/downloads/en/details.aspx?FamilyID=9cfb2d51-5ff4-4491-b0e5-b386f32c0992&displaylang=en

[CustomMessages]
dotnetfx40full_title=.NET 4.0 Full Framework

dotnetfx40full_size=3 MB - 197 MB
// specifiy the full /lcid parameter, including a trailing space! or leave it an empty string if default or unknown
// en.dotnetfx40full_lcid='/lcid 1033 '
en.dotnetfx40full_lcid=''

#ifdef dotnet_Passive
#define dotnetfx40full_passive "'/passive '"
#else
#define dotnetfx40full_passive "''"
#endif

[Code]
const
	dotnetfx40full_url = 'http://download.microsoft.com/download/1/B/E/1BE39E79-7E39-46A3-96FF-047F95396215/dotNetFx40_Full_setup.exe';

function dotnetfx40full(checkOnly : boolean) : boolean;
var
	version: cardinal;
begin
    result := true;
	RegQueryDWordValue(HKLM, 'Software\Microsoft\NET Framework Setup\NDP\v4\full', 'Install', version);
	if version <> 1 then begin
        result := false;
        if not checkOnly then
    		AddProduct('dotNetFx40_Full_setup.exe',
    			CustomMessage('dotnetfx40full_lcid') + '/q ' + {#dotnetfx40full_passive} + '/norestart',
    			CustomMessage('dotnetfx40full_title'),
    			CustomMessage('dotnetfx40full_size'),
    			dotnetfx40full_url,false,false);
    end;
end;
