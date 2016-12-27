# SSHAuth plugin

This plugin uses a SSH server for authentication. The plugin connects
to the specified SSH server and attempts to authenticate as the given
user with the "password" authentication method. It is not necessary
for the user to be able to open an actual shell or execute commands on
the SSH server, but note that SSH authentication stage will return
failure if the user shell is set to "/sbin/nologin".

## Known issues

* Does not support the keyboard-interactive authentication method that
  many SSH servers offer instead of password authentication.

* Uses the first address returned by getaddrinfo for the SSH server
  hostname, so if multiple addresses are returned then it may not
  work.

* No support for changing passwords.

## Libraries

This plugin does most of its work in native code, which is built in a
DLL (SSHAuthNative.dll).

Building this DLL requires static libraries for *libssh2* and its
dependencies *OpenSSL* and *Zlib*.  Such static libraries are included
in this repository, but alternatively they can be built from source
and moved to the corresponding folders specified in the library
pragma directives of SSHAuthNative.cs before attempting to build this
plugin.

## Author

This SSHAuth plugin for pGina 3.x was developed by David Dumas.

This plugin was inspired by a [pGina 1.x and 2.x plugin also named SSHAuth developed by Ahmed Obied](http://sshauth.sourceforge.net/).  However, due to the significant plugin interface changes for pGina 3.x, this plugin was developed from scratch rather than being derived from that codebase.
