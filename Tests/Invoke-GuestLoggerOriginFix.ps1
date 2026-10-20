$ErrorActionPreference = 'Stop'
# Reuse the VM/admin guards, hash checks, DLL/queue backups and service waits.
& (Join-Path $PSScriptRoot 'Invoke-GuestLoggerFix.ps1') -PackageDirectory (Join-Path $PSScriptRoot 'LoggerOriginFix')
