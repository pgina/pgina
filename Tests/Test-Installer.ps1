param(
    [string]$CompilerPath = 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe',
    [string]$OutputDirectory = (Join-Path $env:LOCALAPPDATA 'Temp\opencode\OpenCredential-installer-review')
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$source = Join-Path $root 'Installer\installer.iss'
$text = Get-Content -LiteralPath $source -Raw
if ($text -match '(?im)^\s*\[(InstallDelete|UninstallDelete)\]') { throw 'Installer must not recursively purge configuration or cache directories.' }
if ($text -match '(?im)^Root:.*Subkey:.*(OpenCredential|pGina).*uninsdeletekey') { throw 'Installer must preserve authentication configuration on uninstall.' }
if ($text -notmatch 'DotNetRelease\s*<\s*528040') { throw '.NET Framework 4.8 requirement is missing.' }
if ($text -notmatch 'if ExitCode <> 0 then') { throw 'Post-install failure must not be silently ignored.' }
if (-not (Test-Path -LiteralPath $CompilerPath)) { throw 'Inno Setup 6 compiler is required.' }
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
& $CompilerPath '/Qp' ('/O' + $OutputDirectory) '/FOpenCredentialInstaller-review' $source
if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed.' }
$installer = Join-Path $OutputDirectory 'OpenCredentialInstaller-review.exe'
if (-not (Test-Path -LiteralPath $installer)) { throw 'Installer was not generated.' }
Write-Output 'PASS Installer: preservation policy, framework requirement, error checking and compilation.'
Get-Item -LiteralPath $installer | Select-Object FullName,Length
# Deliberately never execute this installer on the development host.
