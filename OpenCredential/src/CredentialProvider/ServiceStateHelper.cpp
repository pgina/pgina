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

#include "ServiceStateHelper.h"

namespace OpenCredential
{
	namespace Service
	{
		/* static */ OpenCredential::Threading::Mutex StateHelper::s_mutex;
		/* static */ OpenCredential::Transactions::ServiceStateThread StateHelper::s_serviceStateThread;
		/* static */ std::list<OpenCredential::CredProv::Provider *> StateHelper::s_providers;
		/* static */ std::list<OpenCredential::CredProv::Credential *> StateHelper::s_creds;

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
				return L"OpenCredential service connected.";
			}
			else
			{
				return L"OpenCredential service is starting or temporarily unavailable. You can continue signing in, and offline sign-in will work when cached credentials are available.";
			}
		}			

		/* static */
		void StateHelper::AddTarget(OpenCredential::CredProv::Provider *ptr)
		{
			OpenCredential::Threading::ScopedLock lock(s_mutex);
			s_providers.push_back(ptr);
		}

		/* static */
		void StateHelper::RemoveTarget(OpenCredential::CredProv::Provider *ptr)
		{
			OpenCredential::Threading::ScopedLock lock(s_mutex);
			s_providers.remove(ptr);
		}

		/* static */
		void StateHelper::AddTarget(OpenCredential::CredProv::Credential *ptr)
		{
			OpenCredential::Threading::ScopedLock lock(s_mutex);
			s_creds.push_back(ptr);
		}

		/* static */
		void StateHelper::RemoveTarget(OpenCredential::CredProv::Credential *ptr)
		{
			OpenCredential::Threading::ScopedLock lock(s_mutex);
			s_creds.remove(ptr);
		}

		/* static */
		void StateHelper::NotifyStateChanged(bool newState)
		{
			OpenCredential::Threading::ScopedLock lock(s_mutex);
			for(std::list<OpenCredential::CredProv::Credential *>::iterator itr = s_creds.begin();
				itr != s_creds.end(); ++itr)
			{
				OpenCredential::CredProv::Credential * ptr = *itr;
				ptr->ServiceStateChanged(newState);
			}

			for(std::list<OpenCredential::CredProv::Provider *>::iterator itr = s_providers.begin();
				itr != s_providers.end(); ++itr)
			{
				OpenCredential::CredProv::Provider * ptr = *itr;
				ptr->ServiceStateChanged(newState);
			}
		}
	}
}

