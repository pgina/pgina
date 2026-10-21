$ErrorActionPreference = 'Stop'
$machine = Get-CimInstance Win32_ComputerSystem
if ($machine.Model -notmatch 'VirtualBox|VMware Virtual Platform') { throw 'VM_ONLY: Run inside the test VM.' }
$admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $admin) { throw 'Run from the local administrator console.' }
$auth = 'A89DF410-53CA-4FE1-A6CA-4479B841CA19'
$local = '12FA152D-A2E3-4C8D-9535-5DCD49DFCB6D'
$root = Get-Item -LiteralPath 'HKLM:\SOFTWARE\OpenCredential3'
$plugin = Get-Item -LiteralPath ('HKLM:\SOFTWARE\OpenCredential3\Plugins\' + $auth)
$authMask = [int]$root.GetValue($auth, 0)
$localMask = [int]$root.GetValue($local, 0)
$rules = @($plugin.GetValue('GroupGatewayRules', [string[]]@()))
$student = Get-LocalUser -Name ocqa_student
$group = Get-LocalGroup -SID 'S-1-5-32-555'
$member = @((Get-LocalGroupMember -Group $group) | Where-Object { $_.SID.Value -eq $student.SID.Value }).Count -gt 0
$terminal = 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server'
$nla = (Get-ItemProperty -LiteralPath ($terminal + '\WinStations\RDP-Tcp') -Name UserAuthentication).UserAuthentication
$checks = @(
    [pscustomobject]@{ Check='Database Auth authentication retained'; Pass=(($authMask -band 2) -ne 0) }
    [pscustomobject]@{ Check='Temporary Database Auth gateway disabled'; Pass=(($authMask -band 8) -eq 0) }
    [pscustomobject]@{ Check='Local Machine gateway retained'; Pass=(($localMask -band 8) -ne 0) }
    [pscustomobject]@{ Check='Temporary database gateway mapping removed'; Pass=(@($rules | Where-Object { $_ -like '*OC_QA_RDP_Only*' }).Count -eq 0) }
    [pscustomobject]@{ Check='Temporary RDP membership removed'; Pass=(-not $member) }
    [pscustomobject]@{ Check='RDP disabled'; Pass=((Get-ItemProperty -LiteralPath $terminal -Name fDenyTSConnections).fDenyTSConnections -eq 1) }
    [pscustomobject]@{ Check='NLA retained'; Pass=($nla -eq 1) }
    [pscustomobject]@{ Check='Dedicated temporary firewall rule absent'; Pass=($null -eq (Get-NetFirewallRule -Name 'OpenCredential-QA-RDP-HostOnly' -ErrorAction SilentlyContinue)) }
    [pscustomobject]@{ Check='OpenCredential service Running'; Pass=((Get-Service OpenCredential).Status -eq 'Running') }
)
$report = [ordered]@{ RecordedAtUtc=[DateTime]::UtcNow.ToString('o'); Checks=$checks;
    Passed=(@($checks | Where-Object { -not $_.Pass }).Count -eq 0) }
$directory = Join-Path $env:ProgramData 'OpenCredential-QA'
New-Item -ItemType Directory -Force -Path $directory | Out-Null
$path = Join-Path $directory 'rdp-final-cleanup.json'
$report | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $path -Encoding UTF8
Copy-Item -LiteralPath $path -Destination '\\VBOXSVR\OpenCredentialQAResults\rdp-final-cleanup.json'
if (-not $report.Passed) { $checks | Format-Table -AutoSize; throw 'Temporary RDP configuration not fully removed; inspect failed checks.' }
Write-Output 'PASS RDP temporary configuration removed; original authentication/gateway retained; NLA and service intact.'
# Read only except status reports. Never exports other registry values/secrets.
