#include "authfuncs.h"

extern "C"{
	__declspec(dllexport) int auth_user(wchar_t* Username, wchar_t* Password, wchar_t* Domain, wchar_t* Ticket)
	{
		/*
			Creates a new SEC_WINNT_AUTH_IDENTITY structure using the given user name, password, and domain.  These fields are supplied by pGina, and the configuration
			tool for the krb5 plugin.
		*/
		SEC_WINNT_AUTH_IDENTITY auth;
		ZeroMemory( &auth, sizeof(auth) );

		auth.Domain = reinterpret_cast<unsigned short*>( Domain );
		auth.DomainLength = (unsigned long)wcslen( Domain );
		auth.User = reinterpret_cast<unsigned short*>( Username );
		auth.UserLength = (unsigned long)wcslen( Username );
		auth.Password = reinterpret_cast<unsigned short*>( Password );
		auth.PasswordLength = (unsigned long)wcslen( Password );
		auth.Flags = SEC_WINNT_AUTH_IDENTITY_UNICODE;

		char clientOutBufferData[8192];

		SecBuffer     clientOutBuffer;
		SecBufferDesc clientOutBufferDesc;

		///////////////////////////////////////////
		// Get the client and server credentials //
		///////////////////////////////////////////

		CredHandle clientCredentials;

		SECURITY_STATUS status;

		/*
			Acquires a HANDLE to the credentials for the given user
		*/
		status = ::AcquireCredentialsHandle( NULL,
											 L"Kerberos",
											 SECPKG_CRED_OUTBOUND,
											 NULL,
											 &auth,
											 NULL,
											 NULL,
											 &clientCredentials,
											 NULL );

		if(status != SEC_E_OK)
			return status;

		//////////////////////////////////////
		// Initialize the security contexts //
		//////////////////////////////////////

		CtxtHandle clientContext = {};
		unsigned long clientContextAttr = 0;

		CtxtHandle serverContext = {};
		unsigned long serverContextAttr = 0;

		/////////////////////////////
		// Clear the client buffer //
		/////////////////////////////

		clientOutBuffer.BufferType = SECBUFFER_TOKEN;
		clientOutBuffer.cbBuffer   = sizeof clientOutBufferData;
		clientOutBuffer.pvBuffer   = clientOutBufferData;

		clientOutBufferDesc.cBuffers  = 1;
		clientOutBufferDesc.pBuffers  = &clientOutBuffer;
		clientOutBufferDesc.ulVersion = SECBUFFER_VERSION;

		///////////////////////////////////
		// Initialize the client context //
		///////////////////////////////////

		status = InitializeSecurityContext( &clientCredentials,
											NULL,
											Ticket,// the (service/domain) spn target that will authenticate this user "krbtgt/ad.utah.edu",
											0,
											0,
											SECURITY_NATIVE_DREP,
											NULL,
											0,
											&clientContext,
											&clientOutBufferDesc,
											&clientContextAttr,
											NULL );

		return status;
	}
}