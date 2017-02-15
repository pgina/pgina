/*
	Copyright (c) 2017, pGina Team
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

#include "ServiceStateHelper.h"

namespace pGina
{
	namespace Service
	{
		/* static */ bool StateHelper::s_LoginChangePassword;
		/* static */ std::wstring StateHelper::s_password;
		/* static */ std::wstring StateHelper::s_username;
		/* static */ pGina::Threading::Mutex StateHelper::s_mutex;
		/* static */ pGina::Transactions::ServiceStateThread StateHelper::s_serviceStateThread;
		/* static */ std::list<pGina::CredProv::Provider *> StateHelper::s_providers;
		/* static */ std::list<pGina::CredProv::Credential *> StateHelper::s_creds;

		/* static */
		void StateHelper::Start()
		{
			s_serviceStateThread.SetCallback(NotifyStateChanged);
			s_serviceStateThread.Start();
		}

		/* static */
		void StateHelper::Stop()
		{
			s_serviceStateThread.Stop();
		}

		/* static */
		std::wstring StateHelper::GetStateText()
		{
			if(s_serviceStateThread.IsServiceRunning())
			{
				return L"Service Status: Connected";
			}
			else
			{
				return L"Service Status: Disconnected";
			}
		}

		/* static */
		bool StateHelper::GetState()
		{
			if(s_serviceStateThread.IsServiceRunning())
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/* static */
		void StateHelper::AddTarget(pGina::CredProv::Provider *ptr)
		{
			pGina::Threading::ScopedLock lock(s_mutex);
			s_providers.push_back(ptr);
		}

		/* static */
		void StateHelper::RemoveTarget(pGina::CredProv::Provider *ptr)
		{
			pGina::Threading::ScopedLock lock(s_mutex);
			s_providers.remove(ptr);
		}

		/* static */
		void StateHelper::AddTarget(pGina::CredProv::Credential *ptr)
		{
			pGina::Threading::ScopedLock lock(s_mutex);
			s_creds.push_back(ptr);
		}

		/* static */
		void StateHelper::RemoveTarget(pGina::CredProv::Credential *ptr)
		{
			pGina::Threading::ScopedLock lock(s_mutex);
			s_creds.remove(ptr);
		}

		/* static */
		void StateHelper::NotifyStateChanged(bool newState)
		{
			pGina::Threading::ScopedLock lock(s_mutex);
			for(std::list<pGina::CredProv::Credential *>::iterator itr = s_creds.begin();
				itr != s_creds.end(); ++itr)
			{
				pGina::CredProv::Credential * ptr = *itr;
				ptr->ServiceStateChanged(newState);
			}

			for(std::list<pGina::CredProv::Provider *>::iterator itr = s_providers.begin();
				itr != s_providers.end(); ++itr)
			{
				pGina::CredProv::Provider * ptr = *itr;
				ptr->ServiceStateChanged(newState);
			}
		}

		/* static */
		std::wstring StateHelper::GetUsername()
		{
			pGina::Threading::ScopedLock lock(s_mutex);
			return StateHelper::s_username;
		}

		/* static */
		std::wstring StateHelper::GetPassword()
		{
			pGina::Threading::ScopedLock lock(s_mutex);
			return StateHelper::s_password;
		}

		/* static */
		bool StateHelper::GetLoginChangePassword()
		{
			pGina::Threading::ScopedLock lock(s_mutex);
			return StateHelper::s_LoginChangePassword;
		}

		/* static */
		void StateHelper::PushUsername(std::wstring username, std::wstring password, bool Login)
		{
			pGina::Threading::ScopedLock lock(s_mutex);
			StateHelper::s_username.clear();
			StateHelper::s_password.clear();
			StateHelper::s_username = username;
			StateHelper::s_password = password;
			StateHelper::s_LoginChangePassword = Login;
		}

		/* static */
		void StateHelper::SetProvScenario(CREDENTIAL_PROVIDER_USAGE_SCENARIO Scenario)
		{
			pGina::Threading::ScopedLock lock(s_mutex);
			for(std::list<pGina::CredProv::Provider *>::iterator itr = s_providers.begin();
				itr != s_providers.end(); ++itr)
			{
				pGina::CredProv::Provider * ptr = *itr;
				ptr->m_usageScenario = Scenario;
			}
		}
	}
}