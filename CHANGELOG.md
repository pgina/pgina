# Changelog

All notable changes to this project will be documented in this file.

## [1.0.0] - 2026-03-09

### Added
- **MySQL 8.x and MariaDB 10.x/11.x Support**
  - Migrated from legacy `MySql.Data` to `MySqlConnector` library for modern database compatibility
  - Support for MySQL 8.x native authentication methods (`caching_sha2_password`, `mysql_native_password`)
  - Full MariaDB 10.x and 11.x compatibility
  - Added connection timeout and command timeout configuration options

- **Windows 10 and Windows 11 Compatibility**
  - Updated Credential Provider for Windows 10/11 compatibility
  - Tested and verified on Windows 10 21H2+ and Windows 11 22H2+
  - Proper x64 architecture support

- **Enhanced Installer**
  - Updated to Inno Setup with modern UI
  - Visual C++ 2013 Redistributable bundled (required for Credential Provider)
  - Proper Win32 and x64 DLL deployment
  - Improved registry ACL configuration
  - Automatic service installation and CP registration

- **New Configuration Options**
  - Connection timeout settings for slow networks
  - Command timeout for long-running queries
  - SSL/TLS connection support option

### Changed
- **Breaking**: Minimum .NET Framework version is now 4.8
- **Breaking**: MySQL connection library changed from `MySql.Data` to `MySqlConnector`
  - Namespace updated from `MySql.Data.MySqlClient` to `MySqlConnector`
- Updated `MySqlUserDataSource.cs` with modern connection string handling
- Improved error handling and logging for database connections
- Updated all project files to Visual Studio 2019/2022 format (Platform Toolset v142+)

### Fixed
- Connection issues with MySQL 8.x servers using `caching_sha2_password`
- Credential Provider registration on fresh Windows installations
- Memory leaks in long-running authentication sessions
- Incorrect namespace references after MySqlConnector migration
- Missing CLSID registration during installation

### Security
- Updated dependencies to address known vulnerabilities
- Modern TLS support for database connections

### Dependencies
- `MySqlConnector` 2.x (replaces `MySql.Data`)
- `log4net` 2.0.15+
- `BouncyCastle.Cryptography` 2.x (for encryption support)

---

## [3.2.4.1-beta] - 2014/09/26

### Fixed
- Single user plugin denies logon when single user account already has active session

---

## [3.2.4.0-beta] - 2014/06/30

### Fixed
- Issue with fields not appearing when service status is hidden (#231)

---

## [3.2.3.0-beta] - 2014/06/20

### Added
- LDAP plugin feature: option to use authentication bind when searching in authorization and gateway stages (#224)
- MySQL plugin: add option to prevent (or not) logon in gateway if server error occurs (#213)

---

## [3.2.2.0-beta] - 2014/06/17

### Fixed
- Credential provider not properly revealing the username/password fields on Windows 7 when the service becomes available (#231)

### Added
- Support for Start TLS in LDAP plugin (#214)
- Updated installer to the more modern UI

---

## [3.2.1.0-beta] - 2014/06/05

### Added
- New plugin: DriveMapper, maps drives after logon
- Option for hiding username/password fields (#219)
- Username/password fields are hidden when service is not available

### Changed
- Removed option to hide MOTD. Users can just leave text blank if desired

### Fixed
- Null pointer issues with blank username/passwords (#220)
- Issue with non GUID entries in CP list (#210)
- Bug with unlock scenario not processing plugins (#227)

### Changed
- Lots of updates to the RADIUS plugin (Oooska, #223)
- New installer based on NullSoft NSIS

---

## [3.2.0.0-beta] - 2013/10/17

### Added
- Support for changing passwords (#26)
- Change password plugin support: LDAP and LocalMachine
- Accepts credentials from RDP clients (#208) (weiss)
- RADIUS plugin: support for multiple servers (#205) (weiss)
- LDAP plugin: support for dereferencing

### Changed
- Switch to IDictionary in RPC

---

## [3.1.8.0] - 2013/06/03

### Added
- First stable release for 3.1
- Re-add support for the CredUI scenario (#195)
- Option to make the credential tile the default option (#182)
- Service is now dependent on RPC, improves startup time

### Fixed
- Improve resize behavior of configuration (#188)
- Various bug fixes (#146, #187, #177)

---

## [3.1.7.0-beta] - 2013/01/07

### Added
- Configurable login progress message while plugins are executing
- Plugins split into "Core" and "Contrib" directories
- New icon
- New tile image (Win 8 style)
- MySQL plugin support for group-based authorization

### Changed
- Removed support for the CredUI scenario
- Installer now installs the VS2012 redistributable package

---

## [3.1.6.0-beta] - 2012/10/24

### Added
- Support for filtering in CredUI scenario
- Simulator explains lack of logging when using OpenCredential service
- Support for using original username in the unlock scenario (CP only, #154)

### Fixed
- Session cache bug related to CredUI login (#153)

---

## [3.1.5.0-beta] - 2012/10/03

### Added
- Filtering of any credential provider (#144, #132)

### Fixed
- MySQL: Fix for problem when no hash alg is used (#145)
- Email Auth: IMAP fixes (#150, #151)

---

## [3.1.4.0-beta] - 2012/07/26

### Added
- MySQL Auth Plugin: support for groups in Gateway (#114)
- Show group list in simulator
- Support AutoAdminLogon in GINA (#99)

### Fixed
- Fixes for dependency loading (#142, #143)

---

## [3.1.3.0-beta] - 2012/07/12

### Changed
- RADIUS plugin: Improved logging and thread safety (Oooska)
- LocalMachine plugin: make options more flexible for password scrambling and profile deletion

### Fixed
- Crash when unresolvable SIDs exist in groups (#121)

---

## [3.1.2.0-beta] - 2012/07/02

### Added
- New RADIUS plugin (Oooksa)

### Fixed
- Install Cred Prov using env (#137)
- Configuration UI tweaks

---

## [3.1.1.0-beta] - 2012/06/23

### Changed
- LocalMachine plugin: change functionality of the scramble passwords option (#136)
- LDAP plugin: support groupOfUniqueNames and groupOfNames object classes (#135)
- LDAP plugin: better tool-tips
- GINA support for optional MOTD and service status

---

## [3.1.0.0-beta] - 2012/06/05

### Added
- Simulator reworked to include individual plugin information
- MySQL Logger plugin numerous changes (Oooska)
- Single User Login plugin provides more flexibility in options (Oooksa)
- LDAP plugin includes support for group authorization and adding/removing from local groups
- Add IStatefulPlugin interface to plugin API
- MySQL auth plugin includes configurable column names
- Make MOTD and service status display optional (in Credential Providers)

---

## [3.0.12.1] - 2012/06/05

### Fixed
- Fix for custom CA certs in Windows store (#107)
- Icon improvements

---

## [3.0.12.0] - 2012/05/29

### Added
- Installer enhancements: internal changes, less noisy at post install

### Fixed
- Issue with web services (#127)
- Issue with failure when network is disconnected (#128)

### Changed
- Change default setting for Local Machine authorization stage (#119)

---

## [3.0.11.2] - 2012/05/16

### Added
- Add some additional logging in install mode

---

## [3.0.11.1] - 2012/05/08

### Fixed
- Bug fix for systems with password security policies (#126)

---

## [3.0.11.0] - 2012/04/07

### Added
- LDAP plugin option to always fail on empty passwords (#118)

---

## [3.0.10.0] - 2012/03/25

### Added
- EmailAuth Plugin updates to 3.0.0.1
- Add UsernameMod plugin by Evan Horne

---

## [3.0.9.0-beta] - 2012/03/10

### Added
- Added EmailAuth plugin by Evan Horne

### Fixed
- GINA crashes if left at login window for several minutes (#93)
- Bug with SingleUser plugin password
- Trim whitespace around username prior to login (#98)

---

## [3.0.8.0-beta] - 2012/02/13

### Added
- Added install utility to manage all post install/uninstall tasks
- Install utility sets ACLs on registry key to only allow SYSTEM/Admin access
- Log files moved to separate directory (default)
- Service spawns thread to handle initialization so that service can respond immediately to the OS on startup

### Fixed
- Configuration bug in LDAP Auth plugin (issue #95)

---

## [3.0.7.0-beta] - 2012/01/25

### Added
- MySQL Auth plugin support for salted hashes and base 64 encoding

### Changed
- Configuration app now requires admin escalation in order to run

---

## [3.0.6.0-beta] - 2012/01/10

### Fixed
- Improved exception handling in user cleanup
- Bug in locked scenario (#80)
- Better "login failed" messages from LDAP auth plugin

---

## [3.0.5.0-beta] - 2012/01/03

### Changed
- Minor logging changes

---

## [3.0.4.0-beta] - 2012/01/02

### Fixed
- Error handling fixes (#79)

---

## [3.0.3.0-beta] - 2012/01/02

### Added
- Config UI improvements
- Improve external DLL loading (#71)
- Major speed improvements in LocalMachine plugin

### Fixed
- Installer fixes
- Fix GINA on XP

---

## [3.0.2.0-beta] - 2011/11/05

### Added
- Add msbuild file
- Add MySQL auth plugin

---

## [3.0.1.0-beta] - 2011/10/15

### Added
- Initial release
