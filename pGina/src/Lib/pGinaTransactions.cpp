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
#include "Helpers.h"

namespace pGina
{
	namespace Transactions
	{			
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
		User::LoginResult User::ProcessLoginForUser(const wchar_t *username, const wchar_t *domain, const wchar_t *password)
		{			
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
					return LoginResult();
				
				// Then send a loging request message, expect a loginresult message
				pGina::Protocol::LoginRequestMessage request(username, domain, password);
				reply = pGina::Protocol::SendRecvPipeMessage(pipeClient, request);
				cleanup.Add(reply);

				if(reply && reply->Type() != pGina::Protocol::LoginResponse)
					return LoginResult();

				// Did they login?
				pGina::Protocol::LoginResponseMessage * responseMsg = static_cast<pGina::Protocol::LoginResponseMessage *>(reply);
				
				// Send disconnect, expect ack, then close
				pGina::Protocol::DisconnectMessage disconnect;
				reply = pGina::Protocol::SendRecvPipeMessage(pipeClient, disconnect);
				cleanup.Add(reply);		

				// We close regardless, no need to check reply type..
				pipeClient.Close();		

				// If a domain was not specified, we must set it to local machine else no u/p will work regardless
				if(responseMsg->Domain().length() == 0)
				{
					Log::Warn(L"Plugins did not set a domain name, assuming local machine!");
					responseMsg->Domain(pGina::Helpers::GetDomainName());
				}

				return LoginResult(responseMsg->Result(), responseMsg->Username(), responseMsg->Password(), responseMsg->Domain(), responseMsg->Message());				
			}
			else
			{
				Log::Warn(L"Unable to connect to pGina service pipe - LastError: 0x%08x, falling back on LogonUser()", GetLastError());
				
				std::wstring domainName = pGina::Helpers::GetDomainName();				
				Log::Debug(L"Using LogonUser(%s, %s, *****)", username, domainName.c_str());
				HANDLE token = NULL;
				if(LogonUser(username, domainName.c_str(), password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, &token) == TRUE)				
				{
					CloseHandle(token);
					return LoginResult(true, username ? username : L"", password ? password : L"", domainName, L"");					
				}				
			}

			return LoginResult();
		}								
	}
}