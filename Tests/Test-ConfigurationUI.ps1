param(
    [string]$MSBuildPath = '',
    [string]$ScreenshotDirectory = '',
    [ValidateRange(1, 600)]
    [int]$TestTimeoutSeconds = 60
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
if (-not $MSBuildPath) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $MSBuildPath = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
}
if (-not $MSBuildPath) { throw 'Visual Studio MSBuild required.' }
# Build only managed configuration dependencies; never launch Program.Main or the public UI constructor.
& $MSBuildPath (Join-Path $root 'OpenCredential\src\Configuration\Configuration.csproj') /p:Configuration=Release /p:Platform=AnyCPU /verbosity:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'Configuration UI build failed.' }
& $MSBuildPath (Join-Path $root 'Plugins\Core\DatabaseAuth\DatabaseAuth\DatabaseAuth.csproj') /p:Configuration=Release /p:Platform=AnyCPU /verbosity:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'Database Auth configuration UI build failed.' }
& $MSBuildPath (Join-Path $PSScriptRoot 'Configuration.UI.Tests\Configuration.UI.Tests.csproj') /p:Configuration=Release /verbosity:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'Configuration UI test build failed.' }
$arguments = @((Join-Path $root 'OpenCredential\src\bin\OpenCredential.Configuration.exe'))
if ($ScreenshotDirectory) { $arguments += $ScreenshotDirectory }
$start = New-Object System.Diagnostics.ProcessStartInfo
$start.FileName = Join-Path $PSScriptRoot 'Configuration.UI.Tests\bin\Release\OpenCredential.Configuration.UITests.exe'
# Windows paths cannot contain quotes. Reject them rather than build ambiguous native arguments.
foreach ($argument in $arguments) {
    if ($argument.Contains('"')) { throw 'UI test paths must not contain quotes.' }
}
$start.Arguments = ($arguments | ForEach-Object { '"' + ($_ -replace '(\\+)$', '$1$1') + '"' }) -join ' '
$start.UseShellExecute = $false
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$process = [System.Diagnostics.Process]::Start($start)
try {
    $output = $process.StandardOutput.ReadToEndAsync()
    $errorOutput = $process.StandardError.ReadToEndAsync()
    $timedOut = -not $process.WaitForExit($TestTimeoutSeconds * 1000)
    if ($timedOut) {
        # Stop only the synthetic executable created by this invocation, never an existing application.
        if (-not $process.HasExited) { $process.Kill() }
        $process.WaitForExit()
    }
    Write-Output $output.Result
    if ($errorOutput.Result) { Write-Output $errorOutput.Result }
    if ($timedOut) { throw "Configuration UI tests exceeded $TestTimeoutSeconds seconds. See the last TRACE stage above." }
    if ($process.ExitCode -ne 0) { throw 'Configuration accessibility/layout regression failed.' }
}
finally { $process.Dispose() }
