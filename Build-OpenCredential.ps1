param(
    [string]$Configuration = "Release",
    [string]$Platform = "Mixed Platforms",
    [string]$Target = "BuildAll",
    [string]$MSBuildPath = "",
    [ValidatePattern('^(|v[0-9]+)$')]
    [string]$PlatformToolset = ""
)

$buildProjectPath = Join-Path $PSScriptRoot "OpenCredentialBuild.msbuild.xml"
$msbuildPath = $MSBuildPath
if ([string]::IsNullOrWhiteSpace($msbuildPath)) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $msbuildPath = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    }
}

if (-not (Test-Path $buildProjectPath)) {
    throw "Build project not found: $buildProjectPath"
}

if ([string]::IsNullOrWhiteSpace($msbuildPath) -or -not (Test-Path $msbuildPath)) {
    throw "MSBuild not found. Install Visual Studio C++/.NET Framework build tools or pass -MSBuildPath."
}

$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = $msbuildPath
$psi.Arguments = ('"{0}" /t:{1} /p:Configuration={2} /m:1' -f $buildProjectPath, $Target, $Configuration)
if ($PlatformToolset) { $psi.Arguments += ' /p:PlatformToolset=' + $PlatformToolset }
$psi.UseShellExecute = $false
$psi.RedirectStandardOutput = $true
$psi.RedirectStandardError = $true

# Normalize environment variable casing so CL.exe doesn't fail when both Path and PATH exist.
$envMap = [System.Environment]::GetEnvironmentVariables()
$normalized = @{}
foreach ($key in $envMap.Keys) {
    $lower = $key.ToString().ToLowerInvariant()
    if (-not $normalized.ContainsKey($lower)) {
        $normalized[$lower] = [string]$envMap[$key]
    }
}

$psi.Environment.Clear()
foreach ($entry in $normalized.GetEnumerator()) {
    $name = if ($entry.Key -eq "path") { "Path" } else { $entry.Key }
    $psi.Environment[$name] = $entry.Value
}

$process = [System.Diagnostics.Process]::Start($psi)
$stdoutTask = $process.StandardOutput.ReadToEndAsync()
$stderrTask = $process.StandardError.ReadToEndAsync()
$process.WaitForExit()
$stdout = $stdoutTask.Result
$stderr = $stderrTask.Result

Write-Output $stdout
if ($stderr) {
    Write-Error $stderr
}

exit $process.ExitCode
