#pragma once

#include <Windows.h>
#include <string>

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

		class User
		{
		public:
			static bool ProcessLoginForUser(const wchar_t *username, const wchar_t *domain, const wchar_t *password);
			static const wchar_t * AuthenticatedUsername();
			static const wchar_t * AuthenticatedDomain();
			static const wchar_t * AuthenticatedPassword();
			static const wchar_t * AuthenticationMessage();

		private:
			static std::wstring s_authenticatedUsername;
			static std::wstring s_authenticatedDomain;
			static std::wstring s_authenticatedPassword;
			static std::wstring s_authenticationMessage;
		};		
	}
}