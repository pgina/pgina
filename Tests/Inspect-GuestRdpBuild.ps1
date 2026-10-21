$ErrorActionPreference = 'Stop'
$machine = Get-CimInstance Win32_ComputerSystem
if ($machine.Model -notmatch 'VirtualBox|VMware Virtual Platform') { throw 'VM_ONLY: Run inside the test VM.' }
$admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $admin) { throw 'Run from the alternate local administrator session.' }
if (-not [Environment]::Is64BitProcess) { throw 'Use 64-bit Windows PowerShell to verify System32 and SysWOW64 independently.' }
$package = Join-Path $PSScriptRoot 'RdpFix'
$manifest = Get-Content -LiteralPath (Join-Path $package 'native-manifest.json') -Raw | ConvertFrom-Json
$checks = @()
foreach ($entry in $manifest.NativeLibraries) {
    if ($entry.Directory -notin @('System32','SysWOW64') -or $entry.Name -ne 'OpenCredentialCredentialProvider.dll') { throw 'Unexpected native library manifest.' }
    $path = Join-Path (Join-Path $env:WINDIR $entry.Directory) $entry.Name
    $match = (Test-Path -LiteralPath $path -PathType Leaf) -and ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -eq $entry.SHA256)
    $checks += [pscustomobject]@{ Check=('Installed native DLL ' + $entry.Directory); Pass=$match }
}
$checks += [pscustomobject]@{ Check='Both expected native DLLs verified'; Pass=(@($manifest.NativeLibraries).Count -eq 2) }
$service = Get-Service OpenCredential -ErrorAction SilentlyContinue
$checks += [pscustomobject]@{ Check='OpenCredential service Running'; Pass=($null -ne $service -and $service.Status -eq 'Running') }
$checks += [pscustomobject]@{ Check='Provider registered'; Pass=(Test-Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{D0BEFEFB-3D2C-44DA-BBAD-3B2D04557246}') }
$nla = (Get-ItemProperty -LiteralPath 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp' -Name UserAuthentication).UserAuthentication
$checks += [pscustomobject]@{ Check='NLA remains enabled'; Pass=($nla -eq 1) }
$boot = (Get-CimInstance Win32_OperatingSystem).LastBootUpTime.ToUniversalTime()
$checks += [pscustomobject]@{ Check='Reboot occurred after review package build'; Pass=($boot -gt [DateTime]::Parse($manifest.BuiltAtUtc).ToUniversalTime()) }
$report = [ordered]@{ RecordedAtUtc=[DateTime]::UtcNow.ToString('o'); BootUtc=$boot.ToString('o');
    Checks=$checks; Passed=(@($checks | Where-Object { -not $_.Pass }).Count -eq 0); RdpAcceptance='Pending' }
$directory = Join-Path $env:ProgramData 'OpenCredential-QA'
New-Item -ItemType Directory -Force -Path $directory | Out-Null
$path = Join-Path $directory 'rdp-build-result.json'
$report | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $path -Encoding UTF8
Copy-Item -LiteralPath $path -Destination '\\VBOXSVR\OpenCredentialQAResults\rdp-build-result.json'
if (-not $report.Passed) { $checks | Format-Table -AutoSize; throw 'Native RDP review build verification failed. Do not repeat sign-in attempts yet.' }
Write-Output 'PASS Native RDP build hashes, service, provider, NLA and reboot checks. Real RDP login still pending.'
