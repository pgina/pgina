[CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact='High')]
param([switch]$Uninstall)
$ErrorActionPreference = 'Stop'
$machine = Get-CimInstance Win32_ComputerSystem
if ($machine.Model -notmatch 'VirtualBox|VMware Virtual Platform') { throw 'VM_ONLY: Run inside the test VM.' }
$administrator = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $administrator) { throw 'Run as administrator in the VM, keeping the alternate Windows sign-in available.' }
if (-not $Uninstall) { throw 'Inspection complete: VM/admin confirmed. Use -Uninstall only after a cold snapshot and explicit approval.' }
if (Get-Process -Name 'OpenCredential.Configuration' -ErrorAction SilentlyContinue) { throw 'Close OpenCredential Configuration first.' }
$info = Get-CimInstance Win32_Service -Filter "Name='OpenCredential'"
if ($null -eq $info -or $info.PathName -notmatch '^\s*"?(.+?\.exe)(?:"|\s|$)') { throw 'Expected installed service required.' }
$exe = [Environment]::ExpandEnvironmentVariables($Matches[1])
if ([IO.Path]::GetFileName($exe) -ne 'OpenCredential.Service.ServiceHost.exe') { throw 'Unexpected service executable.' }
$directory = [IO.Path]::GetDirectoryName($exe)
$uninstaller = Join-Path $directory 'unins000.exe'
if (-not (Test-Path -LiteralPath $uninstaller -PathType Leaf)) { throw 'Expected Inno uninstaller not found.' }
if (-not $PSCmdlet.ShouldProcess('Test VM OpenCredential only', 'Uninstall service/provider; verify persistent files and registry; do not reboot')) { return }

$reportDirectory = Join-Path $env:ProgramData 'OpenCredential-QA'
New-Item -ItemType Directory -Force -Path $reportDirectory | Out-Null
$reportPath = Join-Path $reportDirectory 'uninstall-result.json'
$dataDirectory = Join-Path $env:ProgramData 'OpenCredential'
$registryPath = 'HKLM:\SOFTWARE\OpenCredential3'
$markerName = 'QA_Uninstall_' + [Guid]::NewGuid().ToString('N')
$markerValue = [Guid]::NewGuid().ToString('N')
$markerPath = Join-Path $dataDirectory ($markerName + '.marker')
if (-not (Test-Path -LiteralPath $dataDirectory) -or -not (Test-Path -LiteralPath $registryPath)) { throw 'Existing persistent data and settings required.' }
[IO.File]::WriteAllText($markerPath, $markerValue)
New-ItemProperty -LiteralPath $registryPath -Name $markerName -Value $markerValue -PropertyType String | Out-Null
$service = Get-Service OpenCredential
Stop-Service OpenCredential
$service.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Stopped, [TimeSpan]::FromSeconds(30))
# Preserve a manifest, not copies of credential/configuration data. The cold
# VirtualBox snapshot is the recovery copy and is managed from the host.
$files = @(Get-ChildItem -LiteralPath $dataDirectory -Recurse -File | ForEach-Object {
    [pscustomobject]@{ Path=$_.FullName; SHA256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
})
$report = [ordered]@{ RecordedAtUtc=[DateTime]::UtcNow.ToString('o'); InstallerExecuted=$false; UninstallerExitCode=$null;
    ServiceRemoved=$false; ProviderRemoved=$false; DataMarkerPreserved=$false; RegistryMarkerPreserved=$false;
    PersistentFileCount=$files.Count; PersistentFilesPreserved=$false; ManualAlternateLogin='Pending'; ManualReboot='Pending' }
try {
    $log = Join-Path $reportDirectory 'uninstall-review.log'
    $process = Start-Process -FilePath $uninstaller -ArgumentList @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART',('/LOG="' + $log + '"')) -Wait -PassThru
    $report.InstallerExecuted = $true
    $report.UninstallerExitCode = $process.ExitCode
    $deadline = (Get-Date).AddSeconds(30)
    while ((Get-Service OpenCredential -ErrorAction SilentlyContinue) -and (Get-Date) -lt $deadline) { Start-Sleep -Milliseconds 500 }
    $report.ServiceRemoved = $null -eq (Get-Service OpenCredential -ErrorAction SilentlyContinue)
    $report.ProviderRemoved = -not (Test-Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{D0BEFEFB-3D2C-44DA-BBAD-3B2D04557246}')
    $report.DataMarkerPreserved = (Test-Path -LiteralPath $markerPath) -and ((Get-Content -LiteralPath $markerPath -Raw) -eq $markerValue)
    $stored = Get-ItemProperty -LiteralPath $registryPath -Name $markerName -ErrorAction SilentlyContinue
    $report.RegistryMarkerPreserved = ($null -ne $stored) -and ($stored.$markerName -eq $markerValue)
    $report.PersistentFilesPreserved = $files.Count -gt 0
    foreach ($file in $files) {
        if (-not (Test-Path -LiteralPath $file.Path -PathType Leaf) -or (Get-FileHash -LiteralPath $file.Path -Algorithm SHA256).Hash -ne $file.SHA256) { $report.PersistentFilesPreserved = $false }
    }
} finally {
    $report | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $reportPath -Encoding UTF8
    if (Test-Path -LiteralPath '\\VBOXSVR\OpenCredentialQAResults') { Copy-Item -LiteralPath $reportPath -Destination '\\VBOXSVR\OpenCredentialQAResults\uninstall-result.json' }
}
if ($report.UninstallerExitCode -notin @(0,3010) -or -not $report.ServiceRemoved -or -not $report.ProviderRemoved -or
    -not $report.DataMarkerPreserved -or -not $report.RegistryMarkerPreserved -or -not $report.PersistentFilesPreserved) {
    throw 'Uninstall validation failed. Keep the admin session open and restore the protected VM snapshot before further sign-in tests.'
}
Write-Output 'PASS Uninstall: service/provider removed; data/registry markers and persistent files preserved. Alternate Windows login still requires manual validation.'
