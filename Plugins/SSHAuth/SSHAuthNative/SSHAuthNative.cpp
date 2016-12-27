#include <WinSock2.h>
#include <WS2tcpip.h>
#include <stdio.h>
#include <Windows.h>
#include "libssh2.h"
using namespace std;

extern "C" {
	__declspec(dllexport) int ssh_connect_and_pw_auth(const char *host, const char *port, const char *user, const char *password, char *errmsg, const int errlen)
	{
		int rc = 0;
		WSADATA wsa_data;
		LIBSSH2_SESSION *ssh_session = NULL;
		ADDRINFO *addr_info = NULL;
		SOCKET sock;
		char *ssh_err_desc;

		rc = WSAStartup(WINSOCK_VERSION, &wsa_data);
		if (rc != 0) {
			_snprintf_s(errmsg, errlen, _TRUNCATE, "WSAStartup failed");
			return 1;
		}

		rc = getaddrinfo(host, port, NULL, &addr_info);
		if (rc) {
			_snprintf_s(errmsg, errlen, _TRUNCATE, "Host name resolution failure (%d)", rc);
			if (addr_info != NULL)
				freeaddrinfo(addr_info);
			WSACleanup();
			return 2;
		}

		// TODO support iteration over entire list returned by getaddrinfo
		sock = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
		if (sock == INVALID_SOCKET) {
			_snprintf_s(errmsg, errlen, _TRUNCATE, "Failed to open socket");
			freeaddrinfo(addr_info);
			WSACleanup();
			return 3;
		}

		rc = connect(sock, addr_info->ai_addr, (int)addr_info->ai_addrlen);
		if (rc == SOCKET_ERROR) {
			_snprintf_s(errmsg, errlen, _TRUNCATE, "Failed to open TCP connection");
			closesocket(sock);
			freeaddrinfo(addr_info);
			WSACleanup();
			return 4;
		}

		rc = libssh2_init(0);
		if (rc) {
			_snprintf_s(errmsg, errlen, _TRUNCATE, "libssh2 initialization failed (%d)\n", rc);
			closesocket(sock);
			freeaddrinfo(addr_info);
			WSACleanup();
			return 5;
		}

		ssh_session = libssh2_session_init();
		if (ssh_session == NULL) {
			_snprintf_s(errmsg, errlen, _TRUNCATE, "Failed to allocate SSH session data structure (libssh2_session_init returned %d)", rc);
			closesocket(sock);
			freeaddrinfo(addr_info);
			WSACleanup();
			return 6;
		}

		rc = libssh2_session_handshake(ssh_session, (int)sock);
		if (rc) {
			libssh2_session_last_error(ssh_session, &ssh_err_desc, NULL, 0);
			_snprintf_s(errmsg, errlen, _TRUNCATE, "Failed SSH handshake (%d=%s)", rc, ssh_err_desc);
			libssh2_session_free(ssh_session);
			closesocket(sock);
			freeaddrinfo(addr_info);
			WSACleanup();
			return 7;
		}

		rc = libssh2_userauth_password(ssh_session, user, password);
		// now rc == 0 iff successful authentication; do cleanup and return this code.

		if (rc) {
			// retrieve error details (likely, incorrect password)
			libssh2_session_last_error(ssh_session, &ssh_err_desc, NULL, 0);
			_snprintf_s(errmsg, errlen, _TRUNCATE, "SSH authentication failed for user %s (%d: %s)", user, rc, ssh_err_desc);
		}

		libssh2_session_disconnect(ssh_session, "Finished");
		libssh2_session_free(ssh_session);
		closesocket(sock);
		freeaddrinfo(addr_info);
		WSACleanup();

		return rc;
	}
};