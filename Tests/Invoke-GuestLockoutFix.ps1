param([string]$PackageDirectory = '')
$ErrorActionPreference = 'Stop'

$machine = Get-CimInstance Win32_ComputerSystem
if ($machine.Model -notmatch 'VirtualBox|VMware Virtual Platform') { throw 'VM_ONLY: Run inside the test VM.' }
$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
if (-not ([Security.Principal.WindowsPrincipal]$identity).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Run PowerShell as administrator inside the VM.'
}
if ([string]::IsNullOrWhiteSpace($PackageDirectory)) { $PackageDirectory = Join-Path $PSScriptRoot 'LockoutUtcFix' }
if (Get-Process -Name 'OpenCredential.Configuration' -ErrorAction SilentlyContinue) {
    throw 'Close OpenCredential Configuration before updating the plugin.'
}
$name = 'OpenCredential.Plugin.DatabaseAuth.dll'
$source = Join-Path $PackageDirectory $name
$manifest = Get-Content -LiteralPath (Join-Path $PackageDirectory 'manifest.json') -Raw | ConvertFrom-Json
$expectedHash = [string]$manifest.SHA256
if ($expectedHash -notmatch '^[A-Fa-f0-9]{64}$' -or
    (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash -ne $expectedHash) {
    throw 'Review package hash mismatch; no changes made.'
}
$serviceInfo = Get-CimInstance Win32_Service -Filter "Name='OpenCredential'"
if ($null -eq $serviceInfo -or $serviceInfo.State -ne 'Running') { throw 'Expected OpenCredential service to be running before update.' }
if ($serviceInfo.PathName -notmatch '^\s*"?(.+?\.exe)(?:"|\s|$)') { throw 'Cannot resolve installed service directory.' }
$exe = [Environment]::ExpandEnvironmentVariables($Matches[1])
if ([IO.Path]::GetFileName($exe) -ne 'OpenCredential.Service.ServiceHost.exe') { throw 'Unexpected service executable.' }
$target = Join-Path (Join-Path ([IO.Path]::GetDirectoryName($exe)) 'Plugins\Core') $name
if (-not (Test-Path -LiteralPath $target -PathType Leaf)) { throw 'Expected installed core plugin was not found; no changes made.' }
$oldHash = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash
$backup = Join-Path (Join-Path $env:ProgramData 'OpenCredential-QA') ('LockoutUtcFix-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $backup | Out-Null
$backupFile = Join-Path $backup $name
Copy-Item -LiteralPath $target -Destination $backupFile
if ((Get-FileHash -LiteralPath $backupFile -Algorithm SHA256).Hash -ne $oldHash) { throw 'Backup verification failed; installed plugin unchanged.' }

$service = Get-Service OpenCredential
$copyAttempted = $false
try {
    Stop-Service -Name OpenCredential
    $service.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Stopped, [TimeSpan]::FromSeconds(30))
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
    $service.Refresh()
    if ($service.Status -eq 'Stopped') { Start-Service -Name OpenCredential }
    throw $failure
}
$report = [ordered]@{ RecordedAtUtc=[DateTime]::UtcNow.ToString('o'); Plugin=$name; PreviousSHA256=$oldHash;
    InstalledSHA256=$expectedHash; BackupDirectory=$backup; ServiceRunning=$true; LockoutRetest='Pending' }
$reportPath = Join-Path $backup 'lockout-fix-result.json'
$report | ConvertTo-Json | Set-Content -LiteralPath $reportPath -Encoding UTF8
if (Test-Path -LiteralPath '\\VBOXSVR\OpenCredentialQAResults') {
    Copy-Item -LiteralPath $reportPath -Destination '\\VBOXSVR\OpenCredentialQAResults\lockout-fix-result.json'
}
Write-Output 'PASS Plugin updated with verified backup; service Running. Lockout expiry must be retested.'
# No account/password/registry changes, deletion, host installation or VM reboot.
