$ErrorActionPreference = 'Stop'
$path = Join-Path $PSScriptRoot 'Invoke-GuestValidation.ps1'
$tokens = $null
$errors = $null
$ast = [Management.Automation.Language.Parser]::ParseFile($path, [ref]$tokens, [ref]$errors)
if ($errors.Count -ne 0) { throw 'Guest script contains syntax errors.' }

# Exercise the real parameter contract without executing the installation body.
# In particular, defaults must not use PSScriptRoot during parameter binding.
$binding = [ScriptBlock]::Create($ast.ParamBlock.Extent.Text + @'

if ($InstallerPath -ne '' -or $HashPath -ne '') {
    throw 'Artifact paths must be resolved in the script body, not in parameter defaults.'
}
'@)
& $binding
& $binding -Recheck
Write-Output 'PASS Guest script: parameter binding does not require PSScriptRoot.'

# Never run this negative test from inside the test VM.
$machine = Get-CimInstance Win32_ComputerSystem
if ($machine.Model -match 'VirtualBox|VMware Virtual Platform') { throw 'Run this host-safety regression on the development host only.' }
try {
    & $path
    throw 'Guest script did not reject the development host.'
} catch {
    if ($_.Exception.Message -notlike 'VM_ONLY:*') { throw }
}
Write-Output 'PASS Guest script: host rejected before writes or installation.'
