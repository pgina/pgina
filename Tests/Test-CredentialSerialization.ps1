param(
    [string]$MSBuildPath = '',
    [ValidatePattern('^(|v[0-9]+)$')]
    [string]$PlatformToolset = ''
)
$ErrorActionPreference = 'Stop'
if (-not $MSBuildPath) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $MSBuildPath = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
}
if (-not $MSBuildPath) { throw 'MSBuild required.' }
foreach ($platform in @('x64','Win32')) {
    $arguments = @((Join-Path $PSScriptRoot 'CredentialProvider.Tests\CredentialProvider.Tests.vcxproj'), '/p:Configuration=Release', "/p:Platform=$platform", '/verbosity:minimal', '/nologo')
    if ($PlatformToolset) { $arguments += '/p:PlatformToolset=' + $PlatformToolset }
    & $MSBuildPath @arguments
    if ($LASTEXITCODE -ne 0) { throw 'Credential serialization regression build failed.' }
    & (Join-Path $PSScriptRoot "CredentialProvider.Tests\bin\$platform\CredentialSerializationTests.exe")
    if ($LASTEXITCODE -ne 0) { throw 'Credential serialization regression failed.' }
}
