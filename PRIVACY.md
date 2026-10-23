# Privacy policy — Desktop

**Draft for owner review before publication.** This describes OpenCredential Desktop and its bundled core plugins, not every third-party extension or a separately deployed AdminWeb service.

## Network destinations

This program will not transfer any information to other networked systems unless specifically requested by the user or the person installing or operating it.

An administrator may configure database, LDAP, mapped-drive and session-management integrations. Those features communicate with the destinations chosen by that administrator, including background synchronization/heartbeat while enabled. The project does not implement a default developer-hosted telemetry endpoint. A configured database/LDAP server is not a developer analytics service.

## Data processed and retained

- Authentication processes usernames and supplied credentials. Depending on the configured plugin, it verifies credentials with local Windows accounts, database hashes or an LDAP directory. Administrators must choose appropriate TLS settings; configuration choices may permit unencrypted transport and must not be treated as automatically secure.
- The Local Machine gateway may create/synchronize the local Windows account and password and manage its groups as configured. It is not a passive read-only check.
- Database Logger can record usernames, computer/host identifiers, IP addresses, session times/states and configured login, logoff, lock/unlock or connection events in the chosen database. Heartbeat and offline replay run in the background when enabled.
- Optional offline authentication stores a machine-protected cache of authentication material. Optional logger queues/presence data persist locally for later synchronization. Local diagnostics and Windows event records may also contain operational/account information; do not share raw logs without review.
- The operator controls database/local retention, access and backups. Uninstall is intentionally designed to preserve configuration and persistent caches/history; removing the program is not a request to erase that data. Any purge must be a separate, explicitly authorized operation after backup.

## Third parties and signing

Review the policies of the database/directory operators and any additional plugin, service or vendor affected by your deployment. AdminWeb/password recovery and SMTP have their own deployment considerations and are not fully covered here.

If hosted build/signing is enabled, GitHub and the approved signing provider process source/build artifacts and build-origin metadata. Only clean distributable code should be submitted: never production configuration, environment files, user databases, credentials, caches or raw authentication logs. SignPath integration is not yet enabled and no signing provider approval is claimed.

For project questions, contact the maintainer through <https://github.com/pedropablobm/OpenCredential>. Do not post credentials, personal authentication records or private keys in public issues.
