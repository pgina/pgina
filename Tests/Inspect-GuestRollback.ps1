param([Parameter(Mandatory=$true)][ValidateSet('Original','Validated')][string]$State)
$ErrorActionPreference = 'Stop'
$machine = Get-CimInstance Win32_ComputerSystem
if ($machine.Model -notmatch 'VirtualBox|VMware Virtual Platform') { throw 'VM_ONLY: Run inside the test VM.' }
$admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $admin) { throw 'Run from the alternate local administrator session.' }
$service = Get-CimInstance Win32_Service -Filter "Name='OpenCredential'"
$student = Get-LocalUser -Name ocqa_student -ErrorAction SilentlyContinue
$checks = @()
if ($State -eq 'Original') {
    $checks += [pscustomobject]@{Check='OpenCredential service absent before initial installation';Pass=($null -eq $service)}
    $checks += [pscustomobject]@{Check='Synthetic student absent before test account creation';Pass=($null -eq $student)}
} else {
    $checks += [pscustomobject]@{Check='Validated service Running';Pass=($null -ne $service -and $service.State -eq 'Running')}
    $checks += [pscustomobject]@{Check='Synthetic student restored and enabled';Pass=($null -ne $student -and $student.Enabled)}
    $checks += [pscustomobject]@{Check='Provider registered';Pass=(Test-Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{D0BEFEFB-3D2C-44DA-BBAD-3B2D04557246}')}
    if ($null -ne $service -and $service.PathName -match '^\s*"?(.+?\.exe)(?:"|\s|$)') {
        $exe = [Environment]::ExpandEnvironmentVariables($Matches[1])
        if ([IO.Path]::GetFileName($exe) -ne 'OpenCredential.Service.ServiceHost.exe') { throw 'Unexpected service executable.' }
        $core = Join-Path ([IO.Path]::GetDirectoryName($exe)) 'Plugins\Core'
        $manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'FinalUpdate\plugins.json') -Raw | ConvertFrom-Json
        foreach ($entry in $manifest) {
            if ($entry.Name -notin @('OpenCredential.Plugin.DatabaseAuth.dll','OpenCredential.Plugin.DatabaseLogger.dll')) { throw 'Unexpected plugin manifest entry.' }
            $path = Join-Path $core $entry.Name
            $same = (Test-Path -LiteralPath $path) -and ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -eq $entry.SHA256)
            $checks += [pscustomobject]@{Check=('Restored plugin hash: ' + $entry.Name);Pass=$same}
        }
        $checks += [pscustomobject]@{Check='Both expected plugin checks present';Pass=(@($manifest).Count -eq 2)}
    }
    foreach ($relative in @('DatabaseAuth\databaseauth-cache.sqlite','DatabaseLogger\databaselogger-queue.sqlite','DatabaseLogger\databaselogger-state.sqlite')) {
        $path = Join-Path (Join-Path $env:ProgramData 'OpenCredential') $relative
        $checks += [pscustomobject]@{Check=('Persistent cache/queue restored: ' + $relative);Pass=(Test-Path -LiteralPath $path -PathType Leaf)}
    }
}
$report = [ordered]@{ RecordedAtUtc=[DateTime]::UtcNow.ToString('o'); ExpectedState=$State;
    AdministratorSessionVerified=$true; Checks=$checks; Passed=(@($checks | Where-Object { -not $_.Pass }).Count -eq 0) }
$directory = Join-Path $env:ProgramData 'OpenCredential-QA'
New-Item -ItemType Directory -Force -Path $directory | Out-Null
$file = 'rollback-' + $State.ToLowerInvariant() + '.json'
$path = Join-Path $directory $file
$report | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $path -Encoding UTF8
Copy-Item -LiteralPath $path -Destination (Join-Path '\\VBOXSVR\OpenCredentialQAResults' $file)
if (-not $report.Passed) { throw 'Snapshot validation failed; keep the admin session and network disconnected.' }
Write-Output "PASS Snapshot $State checks and local administrator session verified. No service/account settings changed."
