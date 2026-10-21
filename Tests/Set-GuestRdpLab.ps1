param([ValidateSet('Enable','Restore')][string]$Action = 'Enable')
$ErrorActionPreference = 'Stop'
$machine = Get-CimInstance Win32_ComputerSystem
if ($machine.Model -notmatch 'VirtualBox|VMware Virtual Platform') { throw 'VM_ONLY: Run inside the test VM.' }
$admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $admin) { throw 'Run from the local administrator console.' }
if ($env:SESSIONNAME -like 'RDP-*') { throw 'Use the VirtualBox console, not an RDP session, to manage this temporary test.' }
$directory = Join-Path $env:ProgramData 'OpenCredential-QA'
$statePath = Join-Path $directory 'rdp-temporary-state.json'
$terminal = 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server'
$rdp = Join-Path $terminal 'WinStations\RDP-Tcp'
$rule = 'OpenCredential-QA-RDP-HostOnly'
$group = Get-LocalGroup -SID 'S-1-5-32-555'
$student = Get-LocalUser -Name ocqa_student
$member = @((Get-LocalGroupMember -Group $group) | Where-Object { $_.SID.Value -eq $student.SID.Value }).Count -gt 0
if ($Action -eq 'Enable') {
    if (Test-Path -LiteralPath $statePath) { throw 'RDP lab state already exists. Restore it before preparing a new test.' }
    if (Get-NetFirewallRule -Name $rule -ErrorAction SilentlyContinue) { throw 'The dedicated firewall rule already exists; no changes made.' }
    $deny = (Get-ItemProperty -LiteralPath $terminal -Name fDenyTSConnections).fDenyTSConnections
    $nla = (Get-ItemProperty -LiteralPath $rdp -Name UserAuthentication).UserAuthentication
    $port = (Get-ItemProperty -LiteralPath $rdp -Name PortNumber).PortNumber
    if ($nla -ne 1 -or $port -ne 3389 -or $deny -ne 1) { throw 'Expected disabled RDP on port 3389 with NLA enabled; no changes made.' }
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
    $state = [ordered]@{ RecordedAtUtc=[DateTime]::UtcNow.ToString('o'); OriginalDeny=$deny; OriginalNla=$nla;
        StudentSid=$student.SID.Value; OriginallyMember=$member; RuleName=$rule; Status='Preparing' }
    $state | ConvertTo-Json | Set-Content -LiteralPath $statePath -Encoding UTF8
    try {
        if (-not $member) { Add-LocalGroupMember -Group $group -Member $student }
        New-NetFirewallRule -Name $rule -DisplayName 'OpenCredential QA RDP from VirtualBox NAT host only' -Direction Inbound -Action Allow -Protocol TCP -LocalPort 3389 -RemoteAddress 10.0.2.2 -Profile Any -EdgeTraversalPolicy Block | Out-Null
        Set-ItemProperty -LiteralPath $terminal -Name fDenyTSConnections -Value 0
        Start-Service TermService
        (Get-Service TermService).WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Running, [TimeSpan]::FromSeconds(30))
        $state.Status = 'Enabled'
        $state | ConvertTo-Json | Set-Content -LiteralPath $statePath -Encoding UTF8
    } catch {
        Set-ItemProperty -LiteralPath $terminal -Name fDenyTSConnections -Value $deny
        Remove-NetFirewallRule -Name $rule -ErrorAction SilentlyContinue
        if (-not $member) { Remove-LocalGroupMember -Group $group -Member $student -ErrorAction SilentlyContinue }
        throw
    }
    $report = [ordered]@{ RecordedAtUtc=[DateTime]::UtcNow.ToString('o'); Enabled=$true; NlaEnabled=$true;
        RemoteAddressAllowed='10.0.2.2'; Port=3389; TemporaryGroupMembership=(-not $member); RestoreRequired=$true }
    Write-Output 'PASS Temporary RDP enabled for synthetic user; NLA retained; firewall limited to VirtualBox NAT host. Restore required after test.'
} else {
    $state = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
    if ($state.StudentSid -ne $student.SID.Value -or $state.RuleName -ne $rule) { throw 'Saved RDP state does not match this lab account/rule.' }
    Set-ItemProperty -LiteralPath $terminal -Name fDenyTSConnections -Value $state.OriginalDeny
    if (Get-NetFirewallRule -Name $rule -ErrorAction SilentlyContinue) { Remove-NetFirewallRule -Name $rule }
    if (-not $state.OriginallyMember -and $member) { Remove-LocalGroupMember -Group $group -Member $student }
    $restoredMember = @((Get-LocalGroupMember -Group $group) | Where-Object { $_.SID.Value -eq $student.SID.Value }).Count -gt 0
    $settingsRestored = ((Get-ItemProperty -LiteralPath $terminal -Name fDenyTSConnections).fDenyTSConnections -eq $state.OriginalDeny) -and
        ((Get-ItemProperty -LiteralPath $rdp -Name UserAuthentication).UserAuthentication -eq $state.OriginalNla)
    $ruleAbsent = $null -eq (Get-NetFirewallRule -Name $rule -ErrorAction SilentlyContinue)
    if (-not $settingsRestored -or -not $ruleAbsent -or $restoredMember -ne [bool]$state.OriginallyMember) { throw 'RDP settings restoration failed.' }
    $state.Status = 'Restored'
    $state | ConvertTo-Json | Set-Content -LiteralPath $statePath -Encoding UTF8
    $report = [ordered]@{ RecordedAtUtc=[DateTime]::UtcNow.ToString('o'); RdpSettingsRestored=$settingsRestored;
        TemporaryRuleRemoved=$ruleAbsent; GroupMembershipRestored=$true; RestoreRequired=$false }
    Write-Output 'PASS RDP disabled again; NLA unchanged; dedicated firewall rule and temporary user permission removed.'
}
$file = if ($Action -eq 'Enable') { 'rdp-prepared.json' } else { 'rdp-restored.json' }
$path = Join-Path $directory $file
$report | ConvertTo-Json | Set-Content -LiteralPath $path -Encoding UTF8
Copy-Item -LiteralPath $path -Destination (Join-Path '\\VBOXSVR\OpenCredentialQAResults' $file)
