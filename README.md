# OpenCredential - MySQL / MariaDB / PostgreSQL Edition

[![GitHub release](https://img.shields.io/github/release/pedropablobm/OpenCredential.svg)](https://github.com/pedropablobm/OpenCredential/releases)
[![License](https://img.shields.io/badge/license-BSD%203--Clause-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-lightgrey.svg)](https://github.com/pedropablobm/OpenCredential)

## Overview

OpenCredential is a pluggable open source Credential Provider (and GINA) replacement for Windows. Plugins are written in managed code and allow user authentication, authorization, session management, and login-time actions.

This repository is an unofficial fork of the original [pGina](https://github.com/pgina/pgina) project, updated and rebranded for modern Windows and modern database backends.

Current branch status:

- MySQL 8.x and MariaDB 10.x/11.x authentication and logging validated in build and runtime tests
- PostgreSQL authentication and logging implemented, compiling successfully, and validated in simulation tests
- Standard English schema creation validated for both MySQL/MariaDB and PostgreSQL
- Plugin projects renamed to `DatabaseAuth` and `DatabaseLogger` to reflect multi-provider support

## Features

- MySQL 8.x support
- MariaDB 10.x and 11.x support
- PostgreSQL support for authentication and logger
- Windows 10 and Windows 11 compatibility
- Multiple authentication plugins: database, LDAP, LocalMachine
- Group-based authorization and gateway rules
- Password hash support: BCrypt, MD5, SHA1, SHA256, SHA384, SHA512, salted variants
- Optional active-user validation through a configurable status column
- Optional login lockout with configurable columns
- SQLite-based offline cache for authentication
- SQLite-based offline queue for logger replay

## Requirements

### Operating System

- Windows 10 version 21H2 or later
- Windows 11 version 22H2 or later

### Runtime Dependencies

- .NET Framework 4.8 or later
- Latest supported Microsoft Visual C++ v14 Redistributable

Install these runtimes on target machines:

- x86: [vc_redist.x86.exe](https://aka.ms/vc14/vc_redist.x86.exe)
- x64: [vc_redist.x64.exe](https://aka.ms/vc14/vc_redist.x64.exe)

### Database Servers

- MySQL 8.0 or later
- MariaDB 10.x or 11.x
- PostgreSQL 12 or later recommended

## Installation

1. Download the latest release from [Releases](https://github.com/pedropablobm/OpenCredential/releases).
2. Run the generated installer as Administrator.
3. Complete the setup wizard.
4. Let `OpenCredential.InstallUtil.exe post-install` finish successfully.
5. Open the OpenCredential configuration tool and configure your authentication plugin.

Note:

- In current local builds, the installer filename is `OpenCredentialInstaller-1.0.0.0.exe`.

## Production Security Notes

Before promoting a build to production or broad internal rollout:

- store database credentials outside the repository, preferably in environment variables or a secrets manager
- do not version local runtime files such as `OpenCredential.AdminWeb/App_Data/`, `keys/`, cookies, logs, installer outputs, or copied SDK binaries
- rotate any credential that was ever stored in a local runtime JSON, test compose file, or shared screenshot
- use dedicated least-privilege database accounts for `Database Auth`, `Database Logger`, and any admin web integration
- enable HTTPS or a reverse proxy with TLS for any admin web exposure

## Database Provider Configuration

The database plugins are now presented in OpenCredential as:

- `Database Auth`
- `Database Logger`

For backward compatibility, legacy setting keys and existing offline cache paths are migrated automatically when possible.

The authentication plugin now supports provider selection from the UI:

- `MySql`
- `PostgreSql`

The logger plugin also supports provider selection from the UI:

- `MySql`
- `PostgreSql`

## Recommended Schema

For new deployments, the recommended schema is the English schema:

- `users`
- `groups`
- `user_groups`
- `careers`
- `levels`
- `login_events`
- `login_sessions`

This schema is clearer for future maintenance and matches the latest PostgreSQL work.

### Recommended Authentication Schema

```sql
CREATE TABLE users (
  id INTEGER NOT NULL,
  username VARCHAR(50) NOT NULL,
  first_name VARCHAR(100),
  last_name VARCHAR(100),
  document_id VARCHAR(15),
  email VARCHAR(200),
  status INTEGER NOT NULL DEFAULT 1,
  career_id INTEGER,
  level_id INTEGER,
  hash_method TEXT NOT NULL,
  password_hash TEXT,
  failed_attempts INTEGER NOT NULL DEFAULT 0,
  locked_until TIMESTAMP NULL,
  last_attempt_at TIMESTAMP NULL,
  PRIMARY KEY (username),
  UNIQUE (id)
);

CREATE TABLE groups (
  group_id BIGINT PRIMARY KEY,
  group_name VARCHAR(128) NOT NULL UNIQUE
);

CREATE TABLE user_groups (
  user_id INTEGER NOT NULL,
  group_id BIGINT NOT NULL,
  PRIMARY KEY (user_id, group_id)
);

CREATE TABLE careers (
  id INTEGER PRIMARY KEY,
  name VARCHAR(255) NOT NULL,
  status INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE levels (
  id INTEGER PRIMARY KEY,
  name VARCHAR(100) NOT NULL,
  status INTEGER NOT NULL DEFAULT 1
);
```

When you use `Create Tables...` in `Database Auth` with the standard English schema (`users`, `groups`, `user_groups`), the plugin now also:

- creates `careers`
- creates `levels`
- adds `first_name`, `last_name`, `document_id`, `email`, `career_id`, `level_id`
- adds `failed_attempts`, `locked_until`, `last_attempt_at`
- adds indexes and foreign keys from `users` to `careers` and `levels`

### Lockout Columns

Login lockout is available but disabled by default.

For the standard English schema created from the plugin UI, these columns are now created automatically. If you are integrating with an existing schema, your user table must include:

```sql
ALTER TABLE users
  ADD COLUMN failed_attempts INTEGER NOT NULL DEFAULT 0,
  ADD COLUMN locked_until TIMESTAMP NULL,
  ADD COLUMN last_attempt_at TIMESTAMP NULL;
```

If you are integrating with an existing schema, table and column names remain configurable from the UI, but the recommended default for new deployments is the English schema shown above.

## Password Hashing

Supported values in the hash-method column include:

- `BCRYPT`
- `MD5`
- `SMD5`
- `SHA1`
- `SSHA1`
- `SHA256`
- `SSHA256`
- `SHA384`
- `SSHA384`
- `SHA512`
- `SSHA512`

For legacy unsalted hashes, the plugin can compare values stored as:

- hexadecimal
- Base64

This behavior depends on the `Hash Encoding` setting in the configuration UI.

## MySQL / MariaDB Configuration

### Authentication Plugin

Typical settings:

```text
Provider: MySql
Host: 192.168.1.100
Port: 3306
Database: your_database
User: your_db_user
Password: your_db_password
TLS Mode: None, Required, VerifyCA, or VerifyFull
```

Recommended field mapping for the English schema:

```text
Table: users
Username Column: username
Password Column: password_hash
Hash Method Column: hash_method
User Table Primary Key Column: id
Status Column: status
Active Value: 1
Failed Attempts Column: failed_attempts
Blocked Until Column: locked_until
Last Attempt Column: last_attempt_at
Group Table Name: groups
Group Name Column: group_name
Group Table Primary Key Column: group_id
User-Group Table Name: user_groups
User Foreign Key Column: user_id
Group Foreign Key Column: group_id
```

### Logger Plugin

Typical settings:

```text
Provider: MySql
Host: 192.168.1.100
Port: 3306
Database: your_database
User: your_db_user
Password: your_db_password
Event Table: login_events
Session Table: login_sessions
```

## PostgreSQL Configuration

### Authentication Plugin

Typical settings:

```text
Provider: PostgreSql
Host: 192.168.1.100
Port: 5432
Database: your_database
User: opencredential_client
Password: your_db_password
```

Recommended field mapping:

```text
Table: users
Username Column: username
Password Column: password_hash
Hash Method Column: hash_method
User Table Primary Key Column: id
Status Column: status
Active Value: 1
Failed Attempts Column: failed_attempts
Blocked Until Column: locked_until
Last Attempt Column: last_attempt_at
Group Table Name: groups
Group Name Column: group_name
Group Table Primary Key Column: group_id
User-Group Table Name: user_groups
User Foreign Key Column: user_id
Group Foreign Key Column: group_id
```

### Logger Plugin

Typical settings:

```text
Provider: PostgreSql
Host: 192.168.1.100
Port: 5432
Database: your_database
User: opencredential_client
Password: your_db_password
Event Table: login_events
Session Table: login_sessions
```

### PostgreSQL TLS Note

Current implementation supports PostgreSQL through `Npgsql 4.1.14`.

Important note:

- In the current implementation, MySQL-style UI values `VerifyCA` and `VerifyFull` are mapped internally to PostgreSQL `Require`.
- If you need strict PostgreSQL certificate validation, that should be implemented and tested separately before documenting it as supported.

## Build from Source

### Prerequisites

- Visual Studio 2022 or later
- .NET Framework 4.8 SDK
- NuGet package restore enabled
- Inno Setup 6.x for installer generation

### Build Steps

```powershell
git clone https://github.com/pedropablobm/OpenCredential.git
cd OpenCredential

# Recommended full build for installer generation
powershell -ExecutionPolicy Bypass -File .\Build-OpenCredential.ps1 -Configuration Release

# This wrapper builds:
# - OpenCredential\src\OpenCredential-1.0.0.0.sln (x64 + Win32)
# - supported core plugin solutions under Plugins\Core\**\*.sln

# If you only want the main solution for development work:
msbuild OpenCredential\src\OpenCredential-1.0.0.0.sln /p:Configuration=Release /p:Platform=x64
msbuild OpenCredential\src\OpenCredential-1.0.0.0.sln /p:Configuration=Release /p:Platform=Win32

# Optional: build legacy contrib plugins separately
msbuild OpenCredentialBuild.msbuild.xml /t:BuildContribPlugins /p:Configuration=Release
```

### Installer

Open `Installer\installer.iss` with Inno Setup and compile the package.

Before generating the installer, run `Build-OpenCredential.ps1` so the plugin DLLs under `Plugins\Core\bin` are refreshed. Building only the main solution can leave stale plugin binaries in the package.

If command-line C++ builds fail with a `Path` / `PATH` duplication error from `CL.exe`, use `Build-OpenCredential.ps1`. It normalizes the process environment before invoking MSBuild.

## Current Validation Status

Validated in this fork:

- Full solution build in Visual Studio
- Installer generation and installation
- MySQL/MariaDB connection tests
- PostgreSQL connection tests
- Standard English schema creation for `users`, `groups`, `user_groups`, `careers`, `levels`
- Simulated login chain for authentication, authorization, and gateway in MySQL/MariaDB and PostgreSQL
- `DatabaseAuth` compiles with MySQL, MariaDB, and PostgreSQL provider support
- `DatabaseLogger` compiles with MySQL, MariaDB, and PostgreSQL provider support

Still pending:

- End-to-end real Windows login validation for PostgreSQL in all target environments
- End-to-end PostgreSQL logger runtime validation outside simulation
- Full PostgreSQL offline-cache and offline-queue runtime validation

## Troubleshooting

### Login screen does not appear

- Ensure the required Visual C++ Redistributables are installed
- Run `OpenCredential.InstallUtil.exe post-install` as Administrator
- Check Windows Event Viewer

### Some plugins do not appear in OpenCredential Configuration

- Rebuild with `Build-OpenCredential.ps1` before compiling the installer
- Confirm the installed plugin folders contain fresh `OpenCredential.Plugin.*.dll` files
- If only `Database Auth`, `Database Logger`, and `Local Machine` appear, the installer was likely built from stale plugin binaries

### Database connection fails

- Verify host, port, database, user, and password
- Verify the database server accepts remote connections from the client IP
- Verify the database account has only the required privileges
- For PostgreSQL, verify the `pg_hba.conf` rules allow the client

### Authentication fails

- Verify the username column and password column mapping
- Verify the hash method value matches the stored hash
- For PostgreSQL, ensure `System.Text.Json.dll` and `System.Text.Encodings.Web.dll` are present after package restore and build
- Verify the active-user filter matches your configured active value
- If lockout is enabled, verify the lockout columns exist and are writable

### Logger fails

- Verify `login_events` and `login_sessions` exist
- Verify the logger database user can `INSERT` and `UPDATE` as required
- Check the offline SQLite queue status in the plugin test output

### Logs Location

- Windows Event Viewer -> Application -> OpenCredential
- Log files under `C:\ProgramData\OpenCredential\` or migrated legacy paths under `C:\ProgramData\pGina\`

## Changes from Original pGina

| Feature | Original pGina | This Fork |
|---------|----------------|-----------|
| MySQL connector | MySql.Data 6.5.4 | MySqlConnector 2.x |
| MySQL 8.x | No | Yes |
| MariaDB 10.x/11.x | Limited | Yes |
| PostgreSQL | No | Implemented in current branch |
| Windows 10 | Partial | Yes |
| Windows 11 | No | Yes |
| .NET Framework | 4.0 | 4.8 |

## License

This repository is an unofficial fork of pGina.

- The original pGina-derived code remains under the BSD-3-Clause license in
  [LICENSE](LICENSE).
- OpenCredential fork attribution and redistribution notice are described in
  [NOTICE](NOTICE).
- The installer shows a combined fork notice and original license text from
  [Installer/OpenCredential-License.txt](Installer/OpenCredential-License.txt).
- Recommended source file header templates for new and heavily modified files
  are documented in [SOURCE_HEADER_TEMPLATES.md](SOURCE_HEADER_TEMPLATES.md).

## Credits

- Original pGina Team - https://github.com/pgina/pgina
- MySqlConnector contributors - https://github.com/mysqlconnector/net
- Npgsql contributors - https://github.com/npgsql/npgsql

## Support

- **Issues**: [GitHub Issues](https://github.com/pedropablobm/OpenCredential/issues)
