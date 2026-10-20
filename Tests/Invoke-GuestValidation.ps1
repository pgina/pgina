[CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact='High')]
param(
    [string]$InstallerPath = '',
    [string]$HashPath = '',
    [switch]$Install,
    [switch]$Recheck,
    [switch]$AllowUnsignedReviewInstaller
)
$ErrorActionPreference = 'Stop'
if ($Install -and $Recheck) { throw 'Choose either -Install or -Recheck; recheck never runs the installer.' }
$machine = Get-CimInstance Win32_ComputerSystem
if ($machine.Model -notmatch 'VirtualBox|VMware Virtual Platform') {
    throw 'VM_ONLY: This validation script refuses to install or write test markers on the development host.'
}
$administrator = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $administrator) { throw 'Run Windows PowerShell as Administrator inside the disposable VM.' }

$reportDirectory = Join-Path $env:ProgramData 'OpenCredential-QA'
New-Item -ItemType Directory -Force -Path $reportDirectory | Out-Null
$service = Get-Service OpenCredential -ErrorAction SilentlyContinue
$release = (Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full' -Name Release).Release
$report = [ordered]@{
    RecordedAtUtc = [DateTime]::UtcNow.ToString('o')
    VirtualMachine = $true
    WindowsBuild = [Environment]::OSVersion.Version.ToString()
    FrameworkRelease = $release
    ServicePresentBefore = ($null -ne $service)
    InstallerExecuted = $false
    RecheckOnly = [bool]$Recheck
    InstallerExitCode = $null
    DataMarkerPreserved = $null
    RegistryMarkerPreserved = $null
    ServiceRunningAfter = $null
    ServiceStateAfter = $null
    ProviderRegistered = $null
    ManualChecks = @(
        @{ Scenario='Alternate local administrator sign-in'; Status='Pending' },
        @{ Scenario='Real Windows logon using synthetic database account'; Status='Pending' },
        @{ Scenario='Wrong password and account lockout'; Status='Pending' },
        @{ Scenario='Windows lock/unlock and logoff visible in AdminWeb'; Status='Pending' },
        @{ Scenario='Password reset in AdminWeb followed by new Windows logon'; Status='Pending' },
        @{ Scenario='Offline logon and queue replay with isolated test database'; Status='Pending' },
        @{ Scenario='RDP sign-in where licensed and available'; Status='Pending' },
        @{ Scenario='Uninstall and rollback preserve configuration'; Status='Pending' }
    )
}

if ($Install) {
    # Windows PowerShell 5.1 may not initialize PSScriptRoot while binding defaults
    # for a script launched with -File from a UNC share. Resolve paths here instead.
    if ([string]::IsNullOrWhiteSpace($InstallerPath)) {
        $scriptDirectory = $PSScriptRoot
        if ([string]::IsNullOrWhiteSpace($scriptDirectory) -and -not [string]::IsNullOrWhiteSpace($PSCommandPath)) {
            $scriptDirectory = [IO.Path]::GetDirectoryName($PSCommandPath)
        }
        if ([string]::IsNullOrWhiteSpace($scriptDirectory) -and -not [string]::IsNullOrWhiteSpace($MyInvocation.MyCommand.Path)) {
            $scriptDirectory = [IO.Path]::GetDirectoryName($MyInvocation.MyCommand.Path)
        }
        if ([string]::IsNullOrWhiteSpace($scriptDirectory)) { throw 'Cannot resolve the script directory. Supply -InstallerPath and -HashPath explicitly.' }
        $InstallerPath = Join-Path $scriptDirectory 'OpenCredentialInstaller-review.exe'
    }
    if ([string]::IsNullOrWhiteSpace($HashPath)) { $HashPath = [IO.Path]::ChangeExtension($InstallerPath, '.sha256') }
    if ($release -lt 528040) { throw '.NET Framework 4.8 or later is required.' }
    if (-not (Test-Path -LiteralPath $InstallerPath)) { throw 'Review installer not found.' }
    if (-not (Test-Path -LiteralPath $HashPath)) { throw 'Expected SHA256 sidecar is required.' }
    $expected = (Get-Content -LiteralPath $HashPath -Raw).Trim()
    if ($expected -notmatch '^[A-Fa-f0-9]{64}$' -or (Get-FileHash -LiteralPath $InstallerPath -Algorithm SHA256).Hash -ne $expected) {
        throw 'Installer hash does not match the reviewed package.'
    }
    $signature = Get-AuthenticodeSignature -LiteralPath $InstallerPath
    if ($signature.Status -ne 'Valid' -and -not ($AllowUnsignedReviewInstaller -and $signature.Status -eq 'NotSigned')) {
        throw 'Installer signature is not valid. Unsigned review builds require explicit lab-only consent.'
    }
    if ($PSCmdlet.ShouldProcess('Disposable VM only: OpenCredential service and Windows Credential Provider', 'Install/update reviewed package; keep network disconnected until synthetic database configuration is verified')) {
        $markerName = 'QA_Preservation_' + [Guid]::NewGuid().ToString('N')
        $markerValue = [Guid]::NewGuid().ToString('N')
        $dataDirectory = Join-Path $env:ProgramData 'OpenCredential'
        New-Item -ItemType Directory -Force -Path $dataDirectory | Out-Null
        $markerPath = Join-Path $dataDirectory ($markerName + '.marker')
        [IO.File]::WriteAllText($markerPath, $markerValue)
        $registryPath = 'HKLM:\SOFTWARE\OpenCredential3'
        if (-not (Test-Path -LiteralPath $registryPath)) { New-Item -Path $registryPath | Out-Null }
        New-ItemProperty -Path $registryPath -Name $markerName -Value $markerValue -PropertyType String | Out-Null
        $setupLog = Join-Path $reportDirectory 'installer-review.log'
        $process = Start-Process -FilePath $InstallerPath -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', ('/LOG="' + $setupLog + '"')) -Wait -PassThru
        $report.InstallerExecuted = $true
        $report.InstallerExitCode = $process.ExitCode
        $report.DataMarkerPreserved = (Test-Path -LiteralPath $markerPath) -and ((Get-Content -LiteralPath $markerPath -Raw) -eq $markerValue)
        $stored = Get-ItemProperty -Path $registryPath -Name $markerName -ErrorAction SilentlyContinue
        $report.RegistryMarkerPreserved = ($null -ne $stored) -and ($stored.$markerName -eq $markerValue)
        $service = Get-Service OpenCredential -ErrorAction SilentlyContinue
        if ($null -ne $service) {
            try { $service.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Running, [TimeSpan]::FromSeconds(30)) }
            catch [System.ServiceProcess.TimeoutException] { }
            $service.Refresh()
            $report.ServiceStateAfter = $service.Status.ToString()
        }
        $report.ServiceRunningAfter = ($null -ne $service) -and ($service.Status -eq 'Running')
        $report.ProviderRegistered = Test-Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{D0BEFEFB-3D2C-44DA-BBAD-3B2D04557246}'
    }
}

if ($Recheck) {
    $previousPath = Join-Path $reportDirectory 'validation-result.json'
    if (-not (Test-Path -LiteralPath $previousPath)) { throw 'No prior installation report found; recheck will not install anything.' }
    $previous = Get-Content -LiteralPath $previousPath -Raw | ConvertFrom-Json
    if (-not $previous.InstallerExecuted) { throw 'The prior report does not record an executed installer.' }
    $report.InstallerExecuted = $previous.InstallerExecuted
    $report.InstallerExitCode = $previous.InstallerExitCode
    $report.ManualChecks = $previous.ManualChecks
    $markers = @(Get-ChildItem -LiteralPath (Join-Path $env:ProgramData 'OpenCredential') -Filter 'QA_Preservation_*.marker' -File -ErrorAction SilentlyContinue |
        Where-Object { $_.BaseName -match '^QA_Preservation_[A-Fa-f0-9]{32}$' })
    $report.DataMarkerPreserved = $markers.Count -gt 0
    $report.RegistryMarkerPreserved = $markers.Count -gt 0
    foreach ($marker in $markers) {
        $value = Get-Content -LiteralPath $marker.FullName -Raw
        $stored = Get-ItemProperty -LiteralPath 'HKLM:\SOFTWARE\OpenCredential3' -Name $marker.BaseName -ErrorAction SilentlyContinue
        if ($null -eq $stored -or $stored.($marker.BaseName) -ne $value) { $report.RegistryMarkerPreserved = $false }
    }
    $service = Get-Service OpenCredential -ErrorAction SilentlyContinue
    if ($null -ne $service) {
        try { $service.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Running, [TimeSpan]::FromSeconds(30)) }
        catch [System.ServiceProcess.TimeoutException] { }
        $service.Refresh()
        $report.ServiceStateAfter = $service.Status.ToString()
    }
    $report.ServiceRunningAfter = ($null -ne $service) -and ($service.Status -eq 'Running')
    $report.ProviderRegistered = Test-Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{D0BEFEFB-3D2C-44DA-BBAD-3B2D04557246}'
}

$reportPath = Join-Path $reportDirectory 'validation-result.json'
$json = $report | ConvertTo-Json -Depth 6
[IO.File]::WriteAllText($reportPath, $json)
$sharedResults = '\\VBOXSVR\OpenCredentialQAResults'
if (Test-Path -LiteralPath $sharedResults) { Copy-Item -LiteralPath $reportPath -Destination (Join-Path $sharedResults 'validation-result.json') }
Write-Output $json
if ($report.InstallerExecuted -and ($report.InstallerExitCode -notin @(0,3010) -or -not $report.DataMarkerPreserved -or -not $report.RegistryMarkerPreserved -or -not $report.ServiceRunningAfter -or -not $report.ProviderRegistered)) {
    throw 'Guest installation validation failed. Do not change sign-in settings or proceed to release; inspect the local setup log.'
}
# No automatic reboot, real-account login, uninstall, snapshot restore or secret export.
