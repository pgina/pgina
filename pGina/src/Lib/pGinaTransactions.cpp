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
			// Call outputdebugstring, after appending a \n and truncating
			wchar_t ods_buffer[4096 - 6];	// Max output to OutputDebugString
			_snwprintf_s(ods_buffer, sizeof(ods_buffer) / sizeof(WORD), _TRUNCATE, L"%s\n", message);
			OutputDebugString(ods_buffer);

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
		bool Service::Ping()
		{
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
					return false;
												
				// Send disconnect, expect ack, then close
				pGina::Protocol::DisconnectMessage disconnect;
				reply = pGina::Protocol::SendRecvPipeMessage(pipeClient, disconnect);
				cleanup.Add(reply);		

				// We close regardless, no need to check reply type..
				pipeClient.Close();		
				return true;
			}

			return false;
		}

		/* static */
		User::LoginResult User::ProcessLoginForUser(const wchar_t *username, const wchar_t *domain, const wchar_t *password, pGina::Protocol::LoginRequestMessage::LoginReason reason)
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
				pGina::Protocol::LoginRequestMessage request(username, domain, password, reason);
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
					responseMsg->Domain(pGina::Helpers::GetMachineName());
				}

				// If we failed, and the 'LocalAdminFallback' option is on, try this with LogonUser iff the username is an 
				//	admin locally.  In fact, it is so rare that this should be turned off, that we don't expose it in the UI
				//  even..  woah!
				if(!responseMsg->Result())					
				{
					if(pGina::Registry::GetBool(L"LocalAdminFallback", true))
					{
						Log::Warn(L"Unable to authenticate %s, checking to see if local admin fallback applies", request.Username().c_str());
						if(pGina::Helpers::IsUserLocalAdmin(request.Username()))
						{
							Log::Info(L"%s is a local admin, falling back to system auth", request.Username().c_str());
							if(LocalLoginForUser(request.Username().c_str(), request.Password().c_str()))
							{
								Log::Info(L"Local login succeeded");
								return LoginResult(true, request.Username(), request.Password(), pGina::Helpers::GetMachineName(), L"");
							}
							else
							{
								Log::Error(L"Local login failed");
							}
						}
					}
				}

				return LoginResult(responseMsg->Result(), responseMsg->Username(), responseMsg->Password(), responseMsg->Domain(), responseMsg->Message());				
			}
			else
			{
				Log::Warn(L"Unable to connect to pGina service pipe - LastError: 0x%08x, falling back on LogonUser()", GetLastError());
				if(LocalLoginForUser(username, password))
					return LoginResult(true, username ? username : L"", password ? password : L"", pGina::Helpers::GetMachineName(), L"");					
			}

			return LoginResult();
		}	

		/* static */
		bool User::LocalLoginForUser(const wchar_t *username, const wchar_t *password)
		{
			std::wstring domainName = pGina::Helpers::GetMachineName();				
			Log::Debug(L"Using LogonUser(%s, %s, *****)", username, domainName.c_str());
			HANDLE token = NULL;
			if(LogonUser(username, domainName.c_str(), password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, &token) == TRUE)				
			{
				CloseHandle(token);
				return true;
			}

			return false;
		}

		/* static */
		std::wstring TileUi::GetDynamicLabel(const wchar_t *labelName)
		{
			// Write a log message to the service
			std::wstring pipeName = pGina::Registry::GetString(L"ServicePipeName", L"Unknown");
			std::wstring pipePath = L"\\\\.\\pipe\\";
			pipePath += pipeName;

			pGina::NamedPipes::PipeClient pipeClient(pipePath, 100);	
			std::wstring labelText = L"";

			if( pipeClient.Connect() )
			{
				// Start a cleanup pool for messages we collect along the way
				pGina::Memory::ObjectCleanupPool cleanup;

				// Always send hello first, expect hello in return
				pGina::Protocol::HelloMessage hello;
				pGina::Protocol::MessageBase * reply = pGina::Protocol::SendRecvPipeMessage(pipeClient, hello);		
				cleanup.Add(reply);

				if(reply && reply->Type() != pGina::Protocol::Hello)
					return labelText;

				// Then send a label request message, expect a labelresponse message
				pGina::Protocol::DynamicLabelRequestMessage request(labelName);
				reply = pGina::Protocol::SendRecvPipeMessage(pipeClient, request);
				cleanup.Add(reply);

				if(reply && reply->Type() != pGina::Protocol::DynLabelResponse)
					return labelText;

				// Get the reply
				pGina::Protocol::DynamicLabelResponseMessage * responseMsg = 
					static_cast<pGina::Protocol::DynamicLabelResponseMessage *>(reply);
				labelText = responseMsg->Text();

				// Send disconnect, expect ack, then close
				pGina::Protocol::DisconnectMessage disconnect;
				reply = pGina::Protocol::SendRecvPipeMessage(pipeClient, disconnect);
				cleanup.Add(reply);		

				// We close regardless, no need to check reply type..
				pipeClient.Close();
			}
			else
			{
				Log::Warn(L"Unable to connect to pGina service pipe - LastError: 0x%08x, giving up.", GetLastError());
			}

			return labelText;
		}

		/* static */
		LoginInfo::UserInformation LoginInfo::GetUserInformation(int session_id)
		{
			// Write a log message to the service
			std::wstring pipeName = pGina::Registry::GetString(L"ServicePipeName", L"Unknown");
			std::wstring pipePath = L"\\\\.\\pipe\\";
			pipePath += pipeName;

			pGina::NamedPipes::PipeClient pipeClient(pipePath, 100);	
			
			if( pipeClient.Connect() )
			{
				// Start a cleanup pool for messages we collect along the way
				pGina::Memory::ObjectCleanupPool cleanup;

				// Always send hello first, expect hello in return
				pGina::Protocol::HelloMessage hello;
				pGina::Protocol::MessageBase * reply = pGina::Protocol::SendRecvPipeMessage(pipeClient, hello);		
				cleanup.Add(reply);

				if(reply && reply->Type() != pGina::Protocol::Hello)
					return UserInformation();

				// Then send a request message, expect a response message
				pGina::Protocol::UserInformationRequestMessage request(session_id);
				reply = pGina::Protocol::SendRecvPipeMessage(pipeClient, request);
				cleanup.Add(reply);

				if(reply && reply->Type() != pGina::Protocol::UserInfoResponse)
					return UserInformation();

				// Get the reply
				pGina::Protocol::UserInformationResponseMessage * responseMsg = 
					static_cast<pGina::Protocol::UserInformationResponseMessage *>(reply);

				// Send disconnect, expect ack, then close
				pGina::Protocol::DisconnectMessage disconnect;
				reply = pGina::Protocol::SendRecvPipeMessage(pipeClient, disconnect);
				cleanup.Add(reply);		

				// We close regardless, no need to check reply type..
				pipeClient.Close();

				return UserInformation(responseMsg->OriginalUsername(), responseMsg->Username(), responseMsg->Domain());
			}
			else
			{
				Log::Warn(L"Unable to connect to pGina service pipe - LastError: 0x%08x, giving up.", GetLastError());
			}

			return UserInformation();
		}

		/* static */
		void LoginInfo::Move(const wchar_t *username, const wchar_t *domain, const wchar_t *password, int old_session, int new_session)
		{
			std::wstring pipeName = pGina::Registry::GetString(L"ServicePipeName", L"Unknown");
			std::wstring pipePath = L"\\\\.\\pipe\\";
			pipePath += pipeName;

			pGina::NamedPipes::PipeClient pipeClient(pipePath, 100);	
			std::wstring labelText = L"";

			if( pipeClient.Connect() )
			{
				// Start a cleanup pool for messages we collect along the way
				pGina::Memory::ObjectCleanupPool cleanup;

				// Always send hello first, expect hello in return
				pGina::Protocol::HelloMessage hello;
				pGina::Protocol::MessageBase * reply = pGina::Protocol::SendRecvPipeMessage(pipeClient, hello);		
				cleanup.Add(reply);

				if(reply && reply->Type() != pGina::Protocol::Hello)
					return;

				pGina::Protocol::LoginInfoChangeMessage request(username, domain, password);
				request.FromSession(old_session);
				request.ToSession(new_session);

				reply = pGina::Protocol::SendRecvPipeMessage(pipeClient, request);
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
			else
			{
				Log::Warn(L"Unable to connect to pGina service pipe - LastError: 0x%08x, giving up.", GetLastError());
			}		
		}	

		ServiceStateThread::ServiceStateThread() :		
			m_serviceRunning(false),
			m_callback(NULL)
		{
		}

		void ServiceStateThread::SetCallback(NOTIFY_STATE_CHANGE_CALLBACK callback)
		{
			m_callback = callback;
		}

		DWORD ServiceStateThread::ThreadMain()
		{
			while(Running())
			{				
				bool runningNow = Service::Ping();				
				bool runningWas = IsServiceRunning();
				SetServiceRunning(runningNow);

				if(runningNow != runningWas && m_callback != NULL)
				{
					m_callback(runningNow);
				}

				SetServiceRunning(runningNow);
				Sleep(pGina::Registry::GetDword(L"PingSleepTime", 5000));
			}

			return 0;
		}

		void ServiceStateThread::SetServiceRunning(bool b)
		{
			pGina::Threading::ScopedLock lock(m_mutex);
			m_serviceRunning = b;
		}

		bool ServiceStateThread::IsServiceRunning()
		{
			pGina::Threading::ScopedLock lock(m_mutex);
			return m_serviceRunning;
		}	

		/* static */
		User::LoginResult User::ProcessChangePasswordForUser(const wchar_t *username, const wchar_t *domain,
				const wchar_t *oldPassword, const wchar_t *newPassword)
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
				
				// Then send a change password request message, expect a loginresult message
				pGina::Protocol::ChangePasswordRequestMessage request(username, domain, oldPassword, newPassword);
				reply = pGina::Protocol::SendRecvPipeMessage(pipeClient, request);
				cleanup.Add(reply);

				if(reply && reply->Type() != pGina::Protocol::ChangePasswordResponse)
					return LoginResult();

				pGina::Protocol::ChangePasswordResponseMessage * responseMsg = 
					static_cast<pGina::Protocol::ChangePasswordResponseMessage *>(reply);
				
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
					responseMsg->Domain(pGina::Helpers::GetMachineName());
				}

				// Password is ignored by credential provider.
				return LoginResult(responseMsg->Result(), 
					responseMsg->Username(), 
					responseMsg->OldPassword(),
					responseMsg->Domain(),
					responseMsg->Message());				
			}
			else
			{
				Log::Warn(L"Unable to connect to pGina service pipe - LastError: 0x%08x, giving up.", GetLastError());
			}

			return LoginResult();
		}
	}
}