# OpenCredential

[![GitHub release](https://img.shields.io/github/release/pedropablobm/OpenCredential.svg)](https://github.com/pedropablobm/OpenCredential/releases)
[![License](https://img.shields.io/badge/license-BSD%203--Clause-blue.svg)](LICENSE)

OpenCredential is an open-source Windows Credential Provider with managed plugins for authentication, authorization and session logging. It is an **unofficial fork of [pGina](https://github.com/pgina/pgina)**, updated for modern Windows and database backends.

## Features

- MySQL, MariaDB and PostgreSQL authentication and session logging.
- LDAP and local Windows account integration.
- Group-based access rules, active-user checks and configurable login lockout.
- Optional DPAPI-protected offline authentication and queued session logging.
- English/Spanish configuration and sign-in text, importable configuration language packs and developer/license credits.

## Install and evaluate

**Current stable release: [v1.0.1.0](https://github.com/pedropablobm/OpenCredential/releases/tag/v1.0.1.0). The installer is unsigned.** Trusted signing is pending provider approval, not yet available. The earlier `v1.0.1.0-rc.1` prerelease remains available unchanged.

Requirements: Windows 10 21H2+ or Windows 11 22H2+, .NET Framework 4.8 and Microsoft Visual C++ v14 runtimes. The installer includes the x86/x64 VC++ redistributables.

1. Keep a working local administrator account and back up existing configuration.
2. Download the installer and `SHA256SUMS` from [Releases](https://github.com/pedropablobm/OpenCredential/releases); verify the SHA256 checksum.
3. Install as Administrator **in a test VM first** and configure the required plugins.
4. Test authentication, lockout and recovery before deploying to other computers.

Windows 11 VM tests cover online/offline login, logging, RDP with NLA, update and uninstall/rollback. Windows 10 and third-party plugins still need acceptance testing. See [validation status](QA_READINESS.md) for scope and remaining checks.

## Build and test

Use Windows PowerShell, NuGet CLI, Visual Studio C++ build tools, the .NET Framework 4.8 SDK and Inno Setup 6 for packaging. These commands generate local build/test outputs; they do **not** install the credential provider or service.

```powershell
# Restore dependencies from nuget.org
.\scripts\ci\Restore-DesktopDependencies.ps1

# Visual Studio 2022: explicitly use v143
.\Build-OpenCredential.ps1 -Configuration Release -PlatformToolset v143
.\Tests\Test-CredentialSerialization.ps1 -PlatformToolset v143
.\Tests\Test-Desktop.ps1
.\Tests\Test-GuestScript.ps1
```

Projects default to `v145`; omit `-PlatformToolset` when using that installed toolset. Compile all core plugins before building `Installer/installer.iss`; the main solution alone is not sufficient. The proposed GitHub Actions workflow builds/tests and produces **unsigned** artifacts only.

## Documentation

| Topic | Guide |
|---|---|
| Database setup, schema, TLS, hashing and troubleshooting | [Configuration reference](docs/REFERENCE.md) |
| Upgrade, offline policy and rollback precautions | [Stabilization](STABILIZATION.md) |
| Complete offline copy, synchronization status and first-login limits | [Offline synchronization](docs/OFFLINE_SYNCHRONIZATION.md) |
| Interface languages, importing translations and developer credits | [Languages and About](docs/LANGUAGES.md) |
| Validation evidence and pending acceptance | [QA readiness](QA_READINESS.md) · [VM testing](Tests/Guest-README.md) |
| Signing status and manual approval policy | [Code signing](CODE_SIGNING.md) · [Setup guide](docs/signing/README.md) |
| Data processing and retention | [Privacy draft](PRIVACY.md) |
| Release history | [Changelog](CHANGELOG.md) |

Use least-privilege database accounts and verified TLS. Never commit credentials, caches or runtime data. Offline group-dependent rules fail closed; uninstall preserves configuration and persistent data.

AdminWeb is a **separate repository** at [pedropablobm/OpenCredential.AdminWeb](https://github.com/pedropablobm/OpenCredential.AdminWeb). When checked out locally under `OpenCredential.AdminWeb/`, run its Git/build commands separately.

## License and support

The pGina-derived code retains the [BSD-3-Clause license](LICENSE) and upstream attribution; see [NOTICE](NOTICE) and the [installer license](Installer/OpenCredential-License.txt). Thanks to the pGina, MySqlConnector and Npgsql contributors.

Report bugs through [GitHub Issues](https://github.com/pedropablobm/OpenCredential/issues), without credentials or private authentication logs.
