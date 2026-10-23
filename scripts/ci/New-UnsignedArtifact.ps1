param(
    [Parameter(Mandatory=$true)][string]$OutputDirectory,
    [string]$CompilerPath = 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe',
    [ValidatePattern('^(|v[0-9]+)$')][string]$PlatformToolset = '',
    [switch]$AllowDirtySource
)
$ErrorActionPreference = 'Stop'
$root = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$commit = git -C $root rev-parse HEAD
if ($LASTEXITCODE -ne 0) { throw 'Git source commit required.' }
$dirty = @(git -C $root status --porcelain).Count -gt 0
if ($LASTEXITCODE -ne 0) { throw 'Cannot inspect source state.' }
if ($env:GITHUB_ACTIONS -eq 'true' -and $AllowDirtySource) { throw 'Dirty-source bypass is forbidden in GitHub Actions.' }
if ($dirty -and -not $AllowDirtySource) { throw 'Unsigned artifact requires clean source; local previews may explicitly use -AllowDirtySource.' }
if ($env:GITHUB_ACTIONS -eq 'true' -and $commit.Trim() -ne $env:GITHUB_SHA) { throw 'Checkout does not match the workflow commit.' }
$output = [IO.Path]::GetFullPath($OutputDirectory)
if ((Test-Path -LiteralPath $output) -and @(Get-ChildItem -LiteralPath $output -Force).Count -gt 0) { throw 'Output directory must be new or empty; no artifacts overwritten.' }
$source = Join-Path $root 'Installer\installer.iss'
$text = Get-Content -LiteralPath $source -Raw
if ($text -notmatch '(?m)^#define MyAppReleaseVersion "([A-Za-z0-9._-]+)"\s*$') { throw 'Cannot determine safe release version.' }
$version = $Matches[1]
if (-not (Test-Path -LiteralPath $CompilerPath -PathType Leaf)) { throw 'Inno Setup compiler required.' }
foreach ($name in @('OpenCredential.Configuration.exe','OpenCredential.InstallUtil.exe')) {
    if (-not (Test-Path -LiteralPath (Join-Path $root ('OpenCredential\src\bin\' + $name)))) { throw 'Build Desktop and run regression tests before packaging.' }
}
New-Item -ItemType Directory -Force -Path $output | Out-Null
$base = 'OpenCredentialInstaller-' + $version + '-unsigned'
& $CompilerPath /Qp ('/O' + $output) ('/F' + $base) $source
if ($LASTEXITCODE -ne 0) { throw 'Unsigned installer compilation failed.' }
$installer = Join-Path $output ($base + '.exe')
if ((Get-AuthenticodeSignature -LiteralPath $installer).Status -ne 'NotSigned') { throw 'Expected a newly compiled unsigned installer, not a signed artifact.' }
$info = [ordered]@{
    Product='OpenCredential'; Version=$version; Commit=$commit.Trim(); DirtySource=$dirty;
    SigningStatus='unsigned; SignPath approval/integration pending'; PlatformToolset=$PlatformToolset;
    Installer=[IO.Path]::GetFileName($installer); InstallerSHA256=(Get-FileHash -LiteralPath $installer -Algorithm SHA256).Hash.ToLowerInvariant();
    InnoSetupVersion=(Get-Item -LiteralPath $CompilerPath).VersionInfo.FileVersion;
    GitHubRepository=$env:GITHUB_REPOSITORY; GitHubRunId=$env:GITHUB_RUN_ID; GitHubRunAttempt=$env:GITHUB_RUN_ATTEMPT;
    RunnerImageVersion=$env:ImageVersion
}
$infoPath = Join-Path $output 'build-info.json'
$info | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $infoPath -Encoding UTF8
$sums = @($installer, $infoPath) | ForEach-Object { (Get-FileHash -LiteralPath $_ -Algorithm SHA256).Hash.ToLowerInvariant() + ' *' + [IO.Path]::GetFileName($_) }
$sums | Set-Content -LiteralPath (Join-Path $output 'SHA256SUMS') -Encoding ASCII
Write-Output "PASS Unsigned evaluation artifact created for $version. No installation, signing request or release publication performed."
