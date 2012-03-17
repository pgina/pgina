In Master Branch
---------------------------
 - EmailAuth Plugin updates to 3.0.0.1

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

