$ErrorActionPreference = 'Stop'
$machine = Get-CimInstance Win32_ComputerSystem
if ($machine.Model -notmatch 'VirtualBox|VMware Virtual Platform') { throw 'VM_ONLY: Run inside the test VM.' }
$admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $admin) { throw 'Run from the local administrator console in the VM.' }
$user = Get-LocalUser -Name ocqa_student
$rdpGroup = Get-LocalGroup -SID 'S-1-5-32-555'
$member = @((Get-LocalGroupMember -Group $rdpGroup) | Where-Object { $_.SID.Value -eq $user.SID.Value }).Count -gt 0
$account = Get-CimInstance Win32_UserAccount -Filter "LocalAccount=True AND Name='ocqa_student'"
$directory = Join-Path $env:ProgramData 'OpenCredential-QA'
New-Item -ItemType Directory -Force -Path $directory | Out-Null
# Export only user-rights policy locally for inspection. Never apply/import it
# or copy the raw security-policy export to the shared folder.
$policy = Join-Path $directory ('rdp-rights-' + [Guid]::NewGuid().ToString('N') + '.ini')
& secedit.exe /export /cfg $policy /areas USER_RIGHTS /quiet | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Could not inspect local user-rights policy.' }
$allow = @(); $deny = @()
foreach ($line in Get-Content -LiteralPath $policy) {
    if ($line -match '^SeRemoteInteractiveLogonRight\s*=\s*(.*)$') { $allow = @($Matches[1].Split(',') | ForEach-Object { $_.Trim().TrimStart('*') }) }
    if ($line -match '^SeDenyRemoteInteractiveLogonRight\s*=\s*(.*)$') { $deny = @($Matches[1].Split(',') | ForEach-Object { $_.Trim().TrimStart('*') }) }
}
$candidateSids = @($user.SID.Value,'S-1-1-0','S-1-5-11','S-1-5-4','S-1-5-14','S-1-5-113')
$unreadable = 0
foreach ($group in Get-LocalGroup) {
    try { if (@((Get-LocalGroupMember -Group $group) | Where-Object { $_.SID.Value -eq $user.SID.Value }).Count -gt 0) { $candidateSids += $group.SID.Value } }
    catch { $unreadable++ }
}
$report = [ordered]@{ RecordedAtUtc=[DateTime]::UtcNow.ToString('o'); ComputerName=$env:COMPUTERNAME;
    SyntheticUserEnabled=$user.Enabled; SyntheticUserLocked=[bool]$account.Lockout;
    MemberOfRemoteDesktopUsers=$member; RemoteDesktopGroupName=$rdpGroup.Name;
    RdpGroupAllowedByPolicy=($allow -contains 'S-1-5-32-555');
    CandidateAllowMatches=@($candidateSids | Where-Object { $allow -contains $_ } | Select-Object -Unique).Count;
    CandidateDenyMatches=@($candidateSids | Where-Object { $deny -contains $_ } | Select-Object -Unique).Count;
    UnreadableLocalGroups=$unreadable }
$path = Join-Path $directory 'rdp-rights.json'
$json = $report | ConvertTo-Json -Depth 4
[IO.File]::WriteAllText($path, $json)
Copy-Item -LiteralPath $path -Destination '\\VBOXSVR\OpenCredentialQAResults\rdp-rights.json'
Write-Output $json
# No group membership, firewall, service, registry or user-rights policy changes.
