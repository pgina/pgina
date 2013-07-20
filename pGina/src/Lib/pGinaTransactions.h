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
#pragma once

#include <Windows.h>
#include <string>
#include <pGinaMessages.h>
#include <Threading.h>

namespace pGina
{
	namespace Transactions
	{
		class Log
		{
		public:
			static void Debug(const wchar_t *format, ...);
			static void Info(const wchar_t *format, ...);
			static void Warn(const wchar_t *format, ...);
			static void Error(const wchar_t *format, ...);

		private:
			static void LogInternal(const wchar_t *level, const wchar_t *message);
		};

		class Service
		{
		public:
			static bool Ping();
		};

		class User
		{
		public:			
			class LoginResult
			{
				public:
					LoginResult() 
						: m_result(false) {}
					LoginResult(bool result, std::wstring const& user, std::wstring const& pass, std::wstring const& domain, std::wstring const& msg)
						: m_username(user), m_domain(domain), m_password(pass), m_message(msg), m_result(result) {}						

					std::wstring Username() { return m_username; }
					void Username(std::wstring const& v) { m_username = v; }

					std::wstring Password() { return m_password; }
					void Password(std::wstring const& v) { m_password = v; }

					std::wstring Domain() { return m_domain; }
					void Domain(std::wstring const& v) { m_domain = v; }
			
					bool Result() { return m_result; }
					void Result(bool v) { m_result = v; }

					std::wstring Message() { return m_message; }
					void Message(std::wstring const& v) { m_message = v; }

					void Clear() { 
						Username(L""); Password(L""); Domain(L""); 
						Result(false); Message(L""); 
					}

				private:
					std::wstring m_username;
					std::wstring m_domain;
					std::wstring m_password;
					std::wstring m_message;
					bool m_result;
			};			

			static LoginResult ProcessLoginForUser(const wchar_t *username, const wchar_t *domain, const wchar_t *password, pGina::Protocol::LoginRequestMessage::LoginReason reason);
			static bool LocalLoginForUser(const wchar_t *username, const wchar_t *password);

			static LoginResult ProcessChangePasswordForUser( const wchar_t *username, const wchar_t *domain,
				const wchar_t *oldPassword, const wchar_t *newPassword );
		};

		/* Generic transaction for receiving some text for a field in the UI. */
		class TileUi
		{
		public:
			static std::wstring GetDynamicLabel(const wchar_t *labelName);
		};

		class LoginInfo
		{
		public:

			class UserInformation 
			{
			public:
				std::wstring OriginalUsername() { return m_orig_uname; }
				void OriginalUsername(std::wstring const &v) { m_orig_uname = v; }

				std::wstring Username() { return m_username; }
				void Username(std::wstring const& v) { m_username = v; }

				std::wstring Domain() { return m_domain; }
				void Domain(std::wstring const& v) { m_domain = v; }

				UserInformation() {}
				UserInformation(const std::wstring & orig_uname, const std::wstring & uname, const std::wstring& dom): 
					m_orig_uname(orig_uname), m_username(uname), m_domain(dom) {}

			private:
				std::wstring m_username;
				std::wstring m_domain;
				std::wstring m_orig_uname;
			};

			static UserInformation GetUserInformation( int session_id );
			static void Move(const wchar_t *username, const wchar_t *domain, const wchar_t *password, int old_session, int new_session);
		};
		
		class ServiceStateThread : public pGina::Threading::Thread
		{
		public:
			typedef void (*NOTIFY_STATE_CHANGE_CALLBACK)(bool newState);

			ServiceStateThread();			

			bool IsServiceRunning();			
			void SetCallback(NOTIFY_STATE_CHANGE_CALLBACK callback);

		protected:
			virtual DWORD ThreadMain();

		private:
			void SetServiceRunning(bool b);

		private:
			bool m_serviceRunning;
			NOTIFY_STATE_CHANGE_CALLBACK m_callback;
		};		
	}
}