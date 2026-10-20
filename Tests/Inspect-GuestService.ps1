$ErrorActionPreference = 'Stop'
$machine = Get-CimInstance Win32_ComputerSystem
if ($machine.Model -notmatch 'VirtualBox|VMware Virtual Platform') { throw 'VM_ONLY: Run this diagnostic inside the test VM.' }

# Observation only: do not install, start, stop or reconfigure any service.
$serviceObjects = @(Get-CimInstance Win32_Service | Where-Object {
    $_.Name -match 'OpenCredential|pGina' -or $_.DisplayName -match 'OpenCredential|pGina' -or $_.PathName -match 'OpenCredential|pGina'
})
$services = @($serviceObjects | Select-Object Name, DisplayName, State, StartMode, ExitCode, ServiceSpecificExitCode)
$controllers = @(Get-Service | Where-Object { $_.Name -match 'OpenCredential|pGina' -or $_.DisplayName -match 'OpenCredential|pGina' } |
    Select-Object Name, @{Name='Status';Expression={$_.Status.ToString()}})
$files = @()
$directories = @(@($env:ProgramFiles, ${env:ProgramFiles(x86)}) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | ForEach-Object { Join-Path $_ 'OpenCredential' })
foreach ($service in $serviceObjects) {
    if ($service.Name -eq 'OpenCredential' -and $service.PathName -match '^\s*"?(.+?\.exe)(?:"|\s|$)') {
        $directories += [IO.Path]::GetDirectoryName([Environment]::ExpandEnvironmentVariables($Matches[1]))
    }
}
foreach ($directory in $directories | Select-Object -Unique) {
    foreach ($name in @('OpenCredential.InstallUtil.exe', 'OpenCredential.Service.ServiceHost.exe', 'log4net.dll')) {
        $path = Join-Path $directory $name
        if (Test-Path -LiteralPath $path) {
            $item = Get-Item -LiteralPath $path
            $files += [pscustomobject]@{ Name=$name; Location=$item.DirectoryName; Length=$item.Length;
                FileVersion=$item.VersionInfo.FileVersion; SHA256=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash }
        }
    }
}
$events = @()
foreach ($log in @('System','Application')) {
    $recent = @(Get-WinEvent -FilterHashtable @{LogName=$log;StartTime=(Get-Date).AddHours(-2);Level=@(1,2,3)} -MaxEvents 250 -ErrorAction SilentlyContinue)
    foreach ($event in $recent) {
        if ($event.Message -match 'OpenCredential|pGina') {
            $types = @([regex]::Matches($event.Message, '\b(?:[A-Za-z_][A-Za-z0-9_]*\.)*[A-Za-z_][A-Za-z0-9_]*Exception\b') | ForEach-Object { $_.Value } | Select-Object -Unique)
            $events += [pscustomobject]@{ Log=$log; EventId=$event.Id; Provider=$event.ProviderName;
                TimeUtc=$event.TimeCreated.ToUniversalTime().ToString('o'); ExceptionTypes=$types }
        }
    }
}
$report = [ordered]@{ RecordedAtUtc=[DateTime]::UtcNow.ToString('o'); Services=$services; Controllers=$controllers; Files=$files; RelatedErrors=$events }
$directory = Join-Path $env:ProgramData 'OpenCredential-QA'
New-Item -ItemType Directory -Force -Path $directory | Out-Null
$path = Join-Path $directory 'service-diagnostic.json'
$json = $report | ConvertTo-Json -Depth 6
[IO.File]::WriteAllText($path, $json)
if (Test-Path -LiteralPath '\\VBOXSVR\OpenCredentialQAResults') { Copy-Item -LiteralPath $path -Destination '\\VBOXSVR\OpenCredentialQAResults\service-diagnostic.json' }
Write-Output $json
# Deliberately exclude raw logs, connection strings, registry settings, service
# command lines, account names, event messages and authentication data.
