param([string]$MSBuildPath = "")
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
if (-not $MSBuildPath) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $MSBuildPath = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
}
if (-not $MSBuildPath) { throw 'MSBuild is required.' }
& $MSBuildPath (Join-Path $root 'OpenCredential\src\Service\Impl\Impl.csproj') /p:Configuration=Release /p:Platform=AnyCPU /verbosity:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'Service formatting regression build failed.' }
$serviceAssembly = [Reflection.Assembly]::LoadFrom((Join-Path $root 'OpenCredential\src\bin\OpenCredential.Service.Impl.dll'))
# Invoke only the standalone formatter, never Service (whose static constructor initializes real settings).
$formatter = $serviceAssembly.GetType('OpenCredential.Service.Impl.MotdVersion', $true).GetMethod('Expand', [Reflection.BindingFlags]'Static,NonPublic')
$releaseVersion = @($serviceAssembly.GetCustomAttributes([Reflection.AssemblyInformationalVersionAttribute], $false))[0].InformationalVersion
$expected = 'OpenCredential Version: ' + $releaseVersion
$actual = $formatter.Invoke($null, [object[]]@('OpenCredential Version: %v'))
if ($actual -ne $expected) { throw "MOTD must display release version '$expected', not '$actual'." }
$actual = $formatter.Invoke($null, [object[]]@('%v / %v | %m %d %i %n | synthetic-text'))
if ($actual -ne ($releaseVersion + ' / ' + $releaseVersion + ' | %m %d %i %n | synthetic-text')) { throw 'MOTD version substitution must preserve custom text and other tokens.' }
$literal = 'Custom welcome: 1.0.0.0'
if ($formatter.Invoke($null, [object[]]@($literal)) -ne $literal) { throw 'Literal administrator messages must not be rewritten.' }
Write-Output 'PASS MOTD uses the published release version, preserving assembly identity, other tokens and custom text; service not initialized.'
& $MSBuildPath (Join-Path $PSScriptRoot 'DatabaseAuth.Tests\DatabaseAuth.Tests.csproj') /p:Configuration=Release /verbosity:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'Desktop regression build failed.' }
& (Join-Path $PSScriptRoot 'DatabaseAuth.Tests\bin\Release\OpenCredential.DatabaseAuth.RegressionTests.exe')
if ($LASTEXITCODE -ne 0) { throw 'Desktop offline regression failed.' }

# Reflection only loads metadata/types; it does not register a Credential Provider,
# install a service, authenticate a real account or initialize plugin settings.
$pluginDirectory = Join-Path $root 'Plugins\Core\bin'
$resolve = [ResolveEventHandler] {
    param($sender, $event)
    $candidate = Join-Path $pluginDirectory (([Reflection.AssemblyName]$event.Name).Name + '.dll')
    if (Test-Path $candidate) { return [Reflection.Assembly]::LoadFrom($candidate) }
    return $null
}
[AppDomain]::CurrentDomain.add_AssemblyResolve($resolve)
try {
    foreach ($file in Get-ChildItem $pluginDirectory -Filter 'OpenCredential.Plugin.*.dll') {
        $assembly = [Reflection.Assembly]::LoadFrom($file.FullName)
        $null = $assembly.GetTypes()
        Write-Output "PASS Plugin types: $($file.Name)"
    }
    $authAssembly = [Reflection.Assembly]::LoadFrom((Join-Path $pluginDirectory 'OpenCredential.Plugin.DatabaseAuth.dll'))
    $utc = [DateTime]::SpecifyKind([DateTime]::Parse('2026-10-20T13:39:35', [Globalization.CultureInfo]::InvariantCulture), [DateTimeKind]::Utc)
    foreach ($provider in @('PostgreSqlUserDataSource', 'MySqlUserDataSource')) {
        $type = $authAssembly.GetType(('OpenCredential.Plugin.DatabaseAuth.' + $provider), $true)
        # Bypass constructors: no SQL connection or real registry settings.
        $instance = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($type)
        $method = $type.GetMethod('ReadUtcDateTime', [Reflection.BindingFlags]'Instance,NonPublic')
        foreach ($value in @([DateTime]::SpecifyKind($utc, [DateTimeKind]::Unspecified), $utc, $utc.ToLocalTime(), '2026-10-20 13:39:35', '2026-10-20T08:39:35-05:00')) {
            $actual = $method.Invoke($instance, [object[]]@($value))
            if ($actual.Ticks -ne $utc.Ticks -or $actual.Kind -ne [DateTimeKind]::Utc) { throw "$provider returned an incorrect UTC lockout timestamp." }
        }
        Write-Output "PASS Compiled provider UTC: $provider"
    }
    $mysqlType = $authAssembly.GetType('OpenCredential.Plugin.DatabaseAuth.MySqlUserDataSource', $true)
    $closedSource = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($mysqlType)
    $rejected = $false
    try { $null = $mysqlType.GetMethod('GetAllUserEntriesForCache').Invoke($closedSource, [object[]]@()) }
    catch { $rejected = $_.Exception.GetBaseException() -is [InvalidOperationException] }
    if (-not $rejected) { throw 'A closed MySQL source must not produce an apparently successful empty snapshot.' }
    Write-Output 'PASS Closed MySQL source rejects full-copy requests without touching registry or SQL.'
} finally {
    [AppDomain]::CurrentDomain.remove_AssemblyResolve($resolve)
}
