param(
    [string]$Configuration = "Release",
    [string]$Platform = "Mixed Platforms",
    [string]$Target = "BuildAll"
)

$buildProjectPath = Join-Path $PSScriptRoot "OpenCredentialBuild.msbuild.xml"
$msbuildPath = "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"

if (-not (Test-Path $buildProjectPath)) {
    throw "Build project not found: $buildProjectPath"
}

if (-not (Test-Path $msbuildPath)) {
    throw "MSBuild not found: $msbuildPath"
}

$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = $msbuildPath
$psi.Arguments = ('"{0}" /t:{1} /p:Configuration={2} /m:1' -f $buildProjectPath, $Target, $Configuration)
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
$stdout = $process.StandardOutput.ReadToEnd()
$stderr = $process.StandardError.ReadToEnd()
$process.WaitForExit()

Write-Output $stdout
if ($stderr) {
    Write-Error $stderr
}

exit $process.ExitCode
