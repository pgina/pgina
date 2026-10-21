$ErrorActionPreference = 'Stop'
$machine = Get-CimInstance Win32_ComputerSystem
if ($machine.Model -notmatch 'VirtualBox|VMware Virtual Platform') { throw 'VM_ONLY: Run inside the test VM.' }
$admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $admin) { throw 'Run from the alternate local administrator session after reboot.' }
$path = Join-Path $env:ProgramData 'OpenCredential-QA\uninstall-result.json'
$report = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
$bootUtc = (Get-CimInstance Win32_OperatingSystem).LastBootUpTime.ToUniversalTime()
if ($bootUtc -le [DateTime]::Parse($report.RecordedAtUtc).ToUniversalTime()) { throw 'A reboot after the recorded uninstall has not been observed.' }
if (Get-Service OpenCredential -ErrorAction SilentlyContinue) { throw 'OpenCredential service still exists after reboot.' }
if (Test-Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{D0BEFEFB-3D2C-44DA-BBAD-3B2D04557246}') { throw 'OpenCredential provider still registered after reboot.' }
if ($report.UninstallerExitCode -ne 0 -or -not $report.PersistentFilesPreserved -or -not $report.RegistryMarkerPreserved) { throw 'Prior uninstall preservation checks were not successful.' }
$report.ManualAlternateLogin = 'PASS: user confirmed local administrator desktop; elevated session verified'
$report.ManualReboot = 'PASS: boot after uninstall; service/provider absent'
$report | Add-Member -NotePropertyName PostUninstallBootUtc -NotePropertyValue $bootUtc.ToString('o') -Force
$report | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $path -Encoding UTF8
Copy-Item -LiteralPath $path -Destination '\\VBOXSVR\OpenCredentialQAResults\uninstall-result.json'
Write-Output 'PASS Post-uninstall reboot and alternate administrator access verified; service/provider absent.'
