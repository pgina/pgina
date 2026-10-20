param([string]$MSBuildPath = '')
$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($env:OC_QA_LOGGER_CONNECTION)) { throw 'Set an explicit synthetic OC_QA_LOGGER_CONNECTION without printing it.' }
if ([string]::IsNullOrWhiteSpace($MSBuildPath)) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $MSBuildPath = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
}
if (-not $MSBuildPath) { throw 'MSBuild required.' }
& $MSBuildPath (Join-Path $PSScriptRoot 'DatabaseLogger.Tests\DatabaseLogger.Tests.csproj') /p:Configuration=Release /verbosity:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'Logger regression build failed.' }
# Resolve the installed plugin runtime dependencies without touching registry settings.
$bin = Join-Path (Split-Path $PSScriptRoot -Parent) 'Plugins\Core\bin'
$test = Join-Path $PSScriptRoot 'DatabaseLogger.Tests\bin\Release'
foreach ($file in Get-ChildItem -LiteralPath $bin -Filter '*.dll') {
    Copy-Item -LiteralPath $file.FullName -Destination $test -Force
}
& (Join-Path $test 'OpenCredential.DatabaseLogger.RegressionTests.exe')
if ($LASTEXITCODE -ne 0) { throw 'Logger SQL replay regression failed.' }
