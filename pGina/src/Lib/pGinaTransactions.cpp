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