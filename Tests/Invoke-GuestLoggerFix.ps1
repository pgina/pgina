param([string]$PackageDirectory = '')
$ErrorActionPreference = 'Stop'
$machine = Get-CimInstance Win32_ComputerSystem
if ($machine.Model -notmatch 'VirtualBox|VMware Virtual Platform') { throw 'VM_ONLY: Run inside the test VM.' }
$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
if (-not ([Security.Principal.WindowsPrincipal]$identity).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Run PowerShell as administrator inside the VM.'
}
if (Get-Process -Name 'OpenCredential.Configuration' -ErrorAction SilentlyContinue) { throw 'Close OpenCredential Configuration first.' }
if ([string]::IsNullOrWhiteSpace($PackageDirectory)) { $PackageDirectory = Join-Path $PSScriptRoot 'LoggerReplayFix' }
$name = 'OpenCredential.Plugin.DatabaseLogger.dll'
$source = Join-Path $PackageDirectory $name
$manifest = Get-Content -LiteralPath (Join-Path $PackageDirectory 'manifest.json') -Raw | ConvertFrom-Json
$expectedHash = [string]$manifest.SHA256
if ($expectedHash -notmatch '^[A-Fa-f0-9]{64}$' -or (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash -ne $expectedHash) {
    throw 'Review package hash mismatch; no changes made.'
}
$info = Get-CimInstance Win32_Service -Filter "Name='OpenCredential'"
if ($null -eq $info -or $info.State -ne 'Running') { throw 'Expected service Running before update.' }
if ($info.PathName -notmatch '^\s*"?(.+?\.exe)(?:"|\s|$)') { throw 'Cannot resolve installed service directory.' }
$exe = [Environment]::ExpandEnvironmentVariables($Matches[1])
if ([IO.Path]::GetFileName($exe) -ne 'OpenCredential.Service.ServiceHost.exe') { throw 'Unexpected service executable.' }
$target = Join-Path (Join-Path ([IO.Path]::GetDirectoryName($exe)) 'Plugins\Core') $name
$queue = Join-Path $env:ProgramData 'OpenCredential\DatabaseLogger\databaselogger-queue.sqlite'
if (-not (Test-Path -LiteralPath $target -PathType Leaf) -or -not (Test-Path -LiteralPath $queue -PathType Leaf)) {
    throw 'Expected installed plugin and lab queue required; no changes made.'
}
$backup = Join-Path (Join-Path $env:ProgramData 'OpenCredential-QA') ('LoggerReplayFix-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $backup | Out-Null
$oldHash = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash
$backupFile = Join-Path $backup $name
Copy-Item -LiteralPath $target -Destination $backupFile
if ((Get-FileHash -LiteralPath $backupFile -Algorithm SHA256).Hash -ne $oldHash) { throw 'Plugin backup verification failed.' }
$service = Get-Service OpenCredential
$copyAttempted = $false
$queueSnapshots = @()
try {
    Stop-Service -Name OpenCredential
    $service.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Stopped, [TimeSpan]::FromSeconds(30))
    # With the service stopped and configuration closed, preserve the queue and
    # any SQLite WAL/SHM sidecars before allowing the new service to replay it.
    foreach ($path in @($queue, ($queue + '-wal'), ($queue + '-shm'))) {
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { continue }
        $hash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
        $destination = Join-Path $backup ([IO.Path]::GetFileName($path))
        Copy-Item -LiteralPath $path -Destination $destination
        if ((Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash -ne $hash) { throw 'Queue backup verification failed; plugin unchanged.' }
        $queueSnapshots += [pscustomobject]@{ File=[IO.Path]::GetFileName($path); SHA256=$hash }
    }
    if ($queueSnapshots.Count -eq 0) { throw 'No queue backup created; plugin unchanged.' }
    $copyAttempted = $true
    Copy-Item -LiteralPath $source -Destination $target -Force
    if ((Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -ne $expectedHash) { throw 'Installed plugin hash mismatch.' }
    Start-Service -Name OpenCredential
    $service.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Running, [TimeSpan]::FromSeconds(30))
} catch {
    $failure = $_
    if ($copyAttempted) {
        Stop-Service -Name OpenCredential -ErrorAction SilentlyContinue
        $service.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Stopped, [TimeSpan]::FromSeconds(30))
        Copy-Item -LiteralPath $backupFile -Destination $target -Force
    }
    # Never restore the queue automatically: replay may already have committed
    # some entries, and restoring an older queue could duplicate those entries.
    $service.Refresh()
    if ($service.Status -eq 'Stopped') { Start-Service -Name OpenCredential }
    throw $failure
}
$report = [ordered]@{ RecordedAtUtc=[DateTime]::UtcNow.ToString('o'); Plugin=$name; PreviousSHA256=$oldHash;
    InstalledSHA256=$expectedHash; BackupDirectory=$backup; QueueSnapshots=$queueSnapshots; ServiceRunning=$true; ReplayRetest='Pending' }
$reportPath = Join-Path $backup 'logger-fix-result.json'
$report | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $reportPath -Encoding UTF8
if (Test-Path -LiteralPath '\\VBOXSVR\OpenCredentialQAResults') {
    Copy-Item -LiteralPath $reportPath -Destination '\\VBOXSVR\OpenCredentialQAResults\logger-fix-result.json'
}
Write-Output 'PASS Logger updated; plugin and queue backups verified; service Running. Check queue replay next.'
# No manual queue deletion, account/password/registry changes or Windows reboot.
