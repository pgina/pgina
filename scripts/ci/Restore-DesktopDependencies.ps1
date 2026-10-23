param([string]$NuGetPath = 'nuget.exe')
$ErrorActionPreference = 'Stop'
$root = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$configs = @(git -C $root ls-files -- 'OpenCredential/src/**/packages.config' 'Plugins/Core/**/packages.config')
if ($LASTEXITCODE -ne 0 -or $configs.Count -eq 0) { throw 'Tracked Desktop NuGet manifests required.' }
if (-not (Get-Command $NuGetPath -ErrorAction SilentlyContinue)) { throw 'NuGet CLI required; use the GitHub-hosted Windows image or install it separately.' }
foreach ($relative in $configs) {
    if ($relative -like 'OpenCredential/src/*') {
        $packages = Join-Path $root 'OpenCredential\src\packages'
    } elseif ($relative -match '^Plugins/Core/([^/]+)/') {
        $packages = Join-Path $root ('Plugins\Core\' + $Matches[1] + '\packages')
    } else { throw 'Unexpected dependency manifest path.' }
    # Restore packages.config directly: no native project/toolset evaluation,
    # floating versions, repository credentials or private feeds are needed.
    & $NuGetPath restore (Join-Path $root $relative) -PackagesDirectory $packages -ConfigFile (Join-Path $PSScriptRoot 'NuGet.config') -NonInteractive -Verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "NuGet restore failed: $relative" }
}
Write-Output "PASS Restored $($configs.Count) tracked Desktop dependency manifests from nuget.org."
