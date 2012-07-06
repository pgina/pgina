// requires Windows Server 2003 Service Pack 2, Windows Server 2008, Windows Vista Service Pack 1, Windows XP Service Pack 3
// requires windows installer 3.1
// WARNING: express setup (downloads and installs the components depending on your OS) if you want to deploy it on cd or network download the full bootsrapper on website below
// http://www.microsoft.com/downloads/en/details.aspx?displaylang=en&FamilyID=5765d7a8-7722-4888-a970-ac39b33fd8ab

[CustomMessages]
dotnetfx40client_title=.NET 4.0 Client Framework

dotnetfx40client_size=3 MB - 197 MB
// specifiy the full /lcid parameter, including a trailing space! or leave it an empty string if default or unknown
// en.dotnetfx40client_lcid='/lcid 1033 '
en.dotnetfx40client_lcid=''

#ifdef dotnet_Passive
#define dotnetfx40client_passive "'/passive '"
#else
#define dotnetfx40client_passive "''"
#endif

[Code]
const
	dotnetfx40client_url = 'http://download.microsoft.com/download/7/B/6/7B629E05-399A-4A92-B5BC-484C74B5124B/dotNetFx40_Client_setup.exe';

function dotnetfx40client(checkOnly : boolean) : boolean;
var
	version: cardinal;
begin
    result := true;
	RegQueryDWordValue(HKLM, 'Software\Microsoft\NET Framework Setup\NDP\v4\client', 'Install', version);
	if version <> 1 then begin
        result := false;
        if not checkOnly then
    		AddProduct('dotNetFx40_Client_setup.exe',
    			CustomMessage('dotnetfx40client_lcid') + '/q ' + {#dotnetfx40client_passive} + '/norestart',
    			CustomMessage('dotnetfx40client_title'),
    			CustomMessage('dotnetfx40client_size'),
    			dotnetfx40client_url,false,false);
    end;
end;
