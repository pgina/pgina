/*
	Copyright (c) 2011, pGina Team
	All rights reserved.

	Redistribution and use in source and binary forms, with or without
	modification, are permitted provided that the following conditions are met:
		* Redistributions of source code must retain the above copyright
		  notice, this list of conditions and the following disclaimer.
		* Redistributions in binary form must reproduce the above copyright
		  notice, this list of conditions and the following disclaimer in the
		  documentation and/or other materials provided with the distribution.
		* Neither the name of the pGina Team nor the names of its contributors 
		  may be used to endorse or promote products derived from this software without 
		  specific prior written permission.

	THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
	ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
	DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY
	DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
	(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
	LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
	ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
	(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
	SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#include <stdio.h>

#include "pGinaTransactions.h"
#include "ObjectCleanupPool.h"
#include "PipeClient.h"
#include "Message.h"
#include "pGinaMessages.h"
#include "Registry.h"

namespace pGina
{
	namespace Transactions
	{
		std::wstring User::s_authenticatedUsername;
		std::wstring User::s_authenticatedDomain;
		std::wstring User::s_authenticatedPassword;
		std::wstring User::s_authenticationMessage;
			
		/* static */
		void Log::Debug(const wchar_t *format, ...)
		{
			wchar_t buffer[2048];
			memset(buffer, 0, sizeof(buffer));
			
			va_list args;
			va_start(args, format);
			_vsnwprintf_s( buffer, sizeof(buffer) / sizeof(WORD), _TRUNCATE, format, args);

			LogInternal(L"Debug", buffer);
		}

		/* static */
		void Log::Info(const wchar_t *format, ...)
		{
			wchar_t buffer[2048];
			memset(buffer, 0, sizeof(buffer));
			
			va_list args;
			va_start(args, format);
			_vsnwprintf_s( buffer, sizeof(buffer) / sizeof(WORD), _TRUNCATE, format, args);

			LogInternal(L"Info", buffer);
		}

		/* static */
		void Log::Warn(const wchar_t *format, ...)
		{
			wchar_t buffer[2048];
			memset(buffer, 0, sizeof(buffer));
			
			va_list args;
			va_start(args, format);
			_vsnwprintf_s( buffer, sizeof(buffer) / sizeof(WORD), _TRUNCATE, format, args);

			LogInternal(L"Warn", buffer);
		}

		/* static */
		void Log::Error(const wchar_t *format, ...)
		{
			wchar_t buffer[2048];
			memset(buffer, 0, sizeof(buffer));
			
			va_list args;
			va_start(args, format);
			_vsnwprintf_s( buffer, sizeof(buffer) / sizeof(WORD), _TRUNCATE, format, args);

			LogInternal(L"Error", buffer);
		}
		
		/* static */
		void Log::LogInternal(const wchar_t *level, const wchar_t *message)
		{
			// Call outputdebugstring
			OutputDebugString(message);

			// Write a log message to the service
			std::wstring pipeName = pGina::Registry::GetString(L"ServicePipeName", L"Unknown");
			std::wstring pipePath = L"\\\\.\\pipe\\";
			pipePath += pipeName;
						
			pGina::NamedPipes::PipeClient pipeClient(pipePath, 100);
			if(pipeClient.Connect())
			{
				// Start a cleanup pool for messages we collect along the way
				pGina::Memory::ObjectCleanupPool cleanup;

				// Always send hello first, expect hello in return
				pGina::Protocol::HelloMessage hello;
				pGina::Protocol::MessageBase * reply = pGina::Protocol::SendRecvPipeMessage(pipeClient, hello);		
				cleanup.Add(reply);

				if(reply && reply->Type() != pGina::Protocol::Hello)
					return;
				
				// Then send a log message, expect ack in return
				pGina::Protocol::LogMessage log(L"NativeLib", level, message);				
				reply = pGina::Protocol::SendRecvPipeMessage(pipeClient, log);
				cleanup.Add(reply);

				if(reply && reply->Type() != pGina::Protocol::Ack)
					return;
								
				// Send disconnect, expect ack, then close
				pGina::Protocol::DisconnectMessage disconnect;
				reply = pGina::Protocol::SendRecvPipeMessage(pipeClient, disconnect);
				cleanup.Add(reply);		

				// We close regardless, no need to check reply type..
				pipeClient.Close();		
			}
		}

		/* static */
		bool User::ProcessLoginForUser(const wchar_t *username, const wchar_t *domain, const wchar_t *password)
		{
			bool result = false;

			// Write a log message to the service
			std::wstring pipeName = pGina::Registry::GetString(L"ServicePipeName", L"Unknown");
			std::wstring pipePath = L"\\\\.\\pipe\\";
			pipePath += pipeName;
			
			// Start a cleanup pool for messages we collect along the way
			pGina::Memory::ObjectCleanupPool cleanup;

			pGina::NamedPipes::PipeClient pipeClient(pipePath, 100);			
			if(pipeClient.Connect())
			{				
				// Always send hello first, expect hello in return
				pGina::Protocol::HelloMessage hello;
				pGina::Protocol::MessageBase * reply = pGina::Protocol::SendRecvPipeMessage(pipeClient, hello);		
				cleanup.Add(reply);

				if(reply && reply->Type() != pGina::Protocol::Hello)
					return false;
				
				// Then send a loging request message, expect a loginresult message
				pGina::Protocol::LoginRequestMessage request(username, domain, password);
				reply = pGina::Protocol::SendRecvPipeMessage(pipeClient, request);
				cleanup.Add(reply);

				if(reply && reply->Type() != pGina::Protocol::LoginResponse)
					return false;

				// Did they login?
				pGina::Protocol::LoginResponseMessage * responseMsg = static_cast<pGina::Protocol::LoginResponseMessage *>(reply);
				result = responseMsg->Result();
				if(result)
				{
					s_authenticatedUsername = responseMsg->Username();
					s_authenticatedDomain = responseMsg->Domain();
					s_authenticatedPassword = responseMsg->Password();
				}
				s_authenticationMessage = responseMsg->Message();

				// Send disconnect, expect ack, then close
				pGina::Protocol::DisconnectMessage disconnect;
				reply = pGina::Protocol::SendRecvPipeMessage(pipeClient, disconnect);
				cleanup.Add(reply);		

				// We close regardless, no need to check reply type..
				pipeClient.Close();		
			}
			else
			{
				Log::Warn(L"Unable to connect to pGina service pipe - LastError: 0x%08x, falling back on LogonUser()", GetLastError());
				WCHAR computerName[MAX_COMPUTERNAME_LENGTH+1];
				DWORD computerNameLen = ARRAYSIZE(computerName);
				std::wstring domainName;

				if (!GetComputerNameW(computerName, &computerNameLen))
					domainName = L".";
				else
					domainName = computerName;

				Log::Debug(L"Using LogonUser(%s, %s, *****)", username, domainName.c_str());
				HANDLE token = NULL;
				result = LogonUser(username, domainName.c_str(), password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, &token) == TRUE;
				if(result)
				{
					CloseHandle(token);
					s_authenticatedUsername = username;
					s_authenticatedPassword = password ? password : L"";
					s_authenticatedDomain = domainName;
				}				
			}

			return result;
		}
		
		/* static */
		const wchar_t * User::AuthenticatedUsername() { return s_authenticatedUsername.c_str(); }
		/* static */
		const wchar_t * User::AuthenticatedDomain() { return s_authenticatedDomain.c_str(); }
		/* static */
		const wchar_t * User::AuthenticatedPassword() { return s_authenticatedPassword.c_str(); }
		/* static */
		const wchar_t * User::AuthenticationMessage() { return s_authenticationMessage.c_str(); }
				
	}
}