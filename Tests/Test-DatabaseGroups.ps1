param([string]$MSBuildPath = '')
$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($env:OC_QA_GROUP_CONNECTION)) { throw 'Set an explicit synthetic OC_QA_GROUP_CONNECTION without printing it.' }
if (-not $MSBuildPath) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $MSBuildPath = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
}
if (-not $MSBuildPath) { throw 'MSBuild required.' }
& $MSBuildPath (Join-Path $PSScriptRoot 'DatabaseAuth.SqlTests\DatabaseAuth.SqlTests.csproj') /p:Configuration=Release /verbosity:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'Group regression build failed.' }
$bin = Join-Path (Split-Path $PSScriptRoot -Parent) 'Plugins\Core\bin'
$test = Join-Path $PSScriptRoot 'DatabaseAuth.SqlTests\bin\Release'
Get-ChildItem -LiteralPath $bin -Filter '*.dll' | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $test -Force }
& (Join-Path $test 'OpenCredential.DatabaseAuth.SqlRegressionTests.exe')
if ($LASTEXITCODE -ne 0) { throw 'PostgreSQL membership regression failed.' }
