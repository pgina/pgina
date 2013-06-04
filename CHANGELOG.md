3.1.8.0 (2013/06/03)
---------------------------
 - First stable release for 3.1
 - Re-add support for the CredUI scenario.  (#195)
 - Option to make pGina the default tile. (#182)
 - Service is now dependent on RPC, improves startup time.
 - Improve resize behavior of configuration. (#188)
 - Various bug fixes (#146, #187, #177)

3.1.7.0 BETA (2013/01/07)
---------------------------
 - Configurable login progress message while plugins are executing.
 - Plugins split into "Core" and "Contrib" directories. 
 - New icon
 - New tile image (Win 8 style)
 - Remove support for the CredUI scenario.
 - Installer now installs the VS2012 redistributable package.
 - MySQL plugin support for group-based authorization.

3.1.6.0 BETA (2012/10/24)
---------------------------
 - Support for filtering in CredUI scenario.
 - Simulator explains lack of logging when using pGina service.
 - Support for using original username in the unlock scenario (CP only, #154).
 - Fix session cache bug related to CredUI login (#153).

3.1.5.0 BETA (2012/10/03)
---------------------------
 - Filtering of any credential provider (#144, #132)
 - MySQL:  Fix for problem when no hash alg is used (#145)
 - Email Auth:  IMAP fixes (#150. #151)

3.1.4.0 BETA (2012/07/26)
---------------------------
 - MySQL Auth Plugin: support for groups in Gateway (#114)
 - Show group list in simulator.
 - Support AutoAdminLogon in GINA (#99)
 - Fixes for dependency loading (#142,#143)

3.1.3.0 BETA (2012/07/12)
---------------------------
 - RADIUS plugin: Improved logging and thread safety (Oooska)
 - Fix: crash when unresolvable SIDs exist in groups (#121)
 - LocalMachine plugin: make options more flexible for password scrambling
   and profile deletion.

3.1.2.0 BETA (2012/07/02)
---------------------------
 - New RADIUS plugin (Oooksa)
 - Fix: install Cred Prov using env (#137)
 - Configuration UI tweaks

3.1.1.0 BETA (2012/06/23)
---------------------------
 - LocalMachine plugin: change functionality of the scramble passwords
   option.  (#136)
 - LDAP plugin: support groupOfUniqueNames and groupOfNames object
   classes.  (#135)
 - LDAP plugin: better tool-tips
 - GINA support for optional MOTD and service status.

3.1.0.0 BETA (2012/06/05)
---------------------------
 - Simulator reworked to include individual plugin information
 - MySQL Logger plugin numerous changes (Oooska)
 - Single User Login plugin provides more flexibility in options (Oooksa)
 - LDAP plugin includes support for group authorization and adding/removing
   from local groups.
 - Add IStatefulPlugin interface to plugin API
 - MySQL auth plugin includes configurable column names
 - Make MOTD and service status display optional (in Credential Providers)

3.0.12.1 (2012/06/05)
---------------------------
 - Fix for custom CA certs in Windows store (#107)
 - Icon improvements

3.0.12.0 (2012/05/29)
---------------------------
 - Installer enhancements: internal changes, less noisy at post install
 - Fix issue with web services (#127)
 - Fix issue with failure when network is disconnected (#128)
 - Change default setting for Local Machine authorization stage (#119)

3.0.11.2 (2012/05/16)
---------------------------
 - Add some additional logging in install mode.

3.0.11.1 (2012/05/08)
---------------------------
 - Bug fix for systems with password security policies (#126)

3.0.11.0 (2012/04/07)
---------------------------
 - LDAP plugin option to always fail on empty passwords (#118)

3.0.10.0 (2012/03/25)
---------------------------
 - EmailAuth Plugin updates to 3.0.0.1
 - Add UsernameMod plugin by Evan Horne

3.0.9.0 BETA (2012/03/10)
----------------------------
 - Added EmailAuth plugin by Evan Horne
 - Fixed Issue #93 - GINA crashes if left at login window for several minutes
 - Fixed Bug with SingleUser plugin password
 - Fixed Issue #98 - Trim whitespace around username prior to login

3.0.8.0 BETA (2012/02/13)
----------------------------
 - Added install utility to manage all post install/uninstall tasks.
 - Install utility sets ACLs on registry key to only allow SYSTEM/Admin access.
 - Log files moved to separate directory (default).
 - Service spawns thread to handle initialization so that service can
   respond immediately to the OS on startup.
 - Fix configuration bug in LDAP Auth plugin (issue #95)

3.0.7.0 BETA (2012/01/25)
----------------------------
 - MySQL Auth plugin support for salted hashes and base 64 encoding.
 - Configuration app now requires admin escalation in order to run.

3.0.6.0 BETA (2012/01/10)
----------------------------
 - Improved exception handling in user cleanup.
 - Fix bug in locked scenario (#80)
 - Better "login failed" messages from LDAP auth plugin.

3.0.5.0 BETA (2012/01/03)
----------------------------
 - Minor logging changes.

3.0.4.0 BETA (2012/01/02)
----------------------------
 - Error handling fixes (#79)

3.0.3.0 BETA (2012/01/02)
----------------------------
 - Config UI improvments.
 - Installer fixes
 - Improve external DLL loading (#71).
 - Major speed improvements in LocalMachine plugin.
 - Fix GINA on XP.


3.0.2.0-BETA (2011/11/05)
----------------------------
 - Add msbuild file.
 - Add MySQL auth plugin.

3.0.1.0 BETA (2011/10/15)
----------------------------
 - Initial release.

