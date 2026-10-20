$ErrorActionPreference = 'Stop'
$machine = Get-CimInstance Win32_ComputerSystem
if ($machine.Model -notmatch 'VirtualBox|VMware Virtual Platform') { throw 'VM_ONLY: Run inside the test VM.' }
$administrator = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $administrator) { throw 'Run PowerShell as administrator inside the VM.' }
if (Get-Process -Name 'OpenCredential.Configuration' -ErrorAction SilentlyContinue) { throw 'Close OpenCredential Configuration first.' }
$package = Join-Path $PSScriptRoot 'FinalUpdate'
$reportDirectory = Join-Path $env:ProgramData 'OpenCredential-QA'
$previous = Join-Path $reportDirectory 'validation-result.json'
if (Test-Path -LiteralPath $previous) {
    Copy-Item -LiteralPath $previous -Destination (Join-Path $reportDirectory ('validation-before-update-' + [Guid]::NewGuid().ToString('N') + '.json'))
}
& (Join-Path $PSScriptRoot 'Invoke-GuestValidation.ps1') -Install -AllowUnsignedReviewInstaller -Confirm:$false -InstallerPath (Join-Path $package 'OpenCredentialInstaller-review.exe')
$validation = Get-Content -LiteralPath $previous -Raw | ConvertFrom-Json
if (-not $validation.ServicePresentBefore -or -not $validation.InstallerExecuted -or -not $validation.ServiceRunningAfter) {
    throw 'Expected an upgrade of the existing running installation.'
}
$info = Get-CimInstance Win32_Service -Filter "Name='OpenCredential'"
if ($info.PathName -notmatch '^\s*"?(.+?\.exe)(?:"|\s|$)') { throw 'Cannot resolve installed service directory.' }
$exe = [Environment]::ExpandEnvironmentVariables($Matches[1])
if ([IO.Path]::GetFileName($exe) -ne 'OpenCredential.Service.ServiceHost.exe') { throw 'Unexpected service executable.' }
$core = Join-Path ([IO.Path]::GetDirectoryName($exe)) 'Plugins\Core'
$manifest = Get-Content -LiteralPath (Join-Path $package 'plugins.json') -Raw | ConvertFrom-Json
$checks = @()
foreach ($entry in $manifest) {
    if ($entry.Name -notin @('OpenCredential.Plugin.DatabaseAuth.dll','OpenCredential.Plugin.DatabaseLogger.dll')) { throw 'Unexpected review manifest entry.' }
    $actual = (Get-FileHash -LiteralPath (Join-Path $core $entry.Name) -Algorithm SHA256).Hash
    $checks += [pscustomobject]@{ Plugin=$entry.Name; HashMatches=($actual -eq $entry.SHA256) }
}
if ($checks.Count -ne 2 -or @($checks | Where-Object { -not $_.HashMatches }).Count -gt 0) { throw 'Installed plugin hashes do not match the final review build.' }
$report = [ordered]@{ RecordedAtUtc=[DateTime]::UtcNow.ToString('o'); UpgradeOfExistingInstallation=$true;
    InstallerExitCode=$validation.InstallerExitCode; DataMarkerPreserved=$validation.DataMarkerPreserved;
    RegistryMarkerPreserved=$validation.RegistryMarkerPreserved; ServiceRunning=$validation.ServiceRunningAfter;
    ProviderRegistered=$validation.ProviderRegistered; Plugins=$checks; PostUpgradeLogin='Pending' }
$reportPath = Join-Path $reportDirectory 'final-update-result.json'
$report | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $reportPath -Encoding UTF8
if (Test-Path -LiteralPath '\\VBOXSVR\OpenCredentialQAResults') { Copy-Item -LiteralPath $reportPath -Destination '\\VBOXSVR\OpenCredentialQAResults\final-update-result.json' }
Write-Output 'PASS Review installer upgrade: service Running, markers preserved, provider registered, both plugin hashes verified. Post-upgrade login still pending.'
