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
#include <credentialprovider.h>
#include <list>

#include <pGinaTransactions.h>
#include <Threading.h>

#include "TileUiTypes.h"
#include "Credential.h"
#include "Provider.h"

namespace pGina
{
	namespace Service
	{		
		class StateHelper
		{
		public:
			static void Start();
			static void Stop();
			
			static std::wstring GetStateText();

			static void AddTarget(pGina::CredProv::Credential *ptr);
			static void RemoveTarget(pGina::CredProv::Credential *ptr);
			static void AddTarget(pGina::CredProv::Provider *ptr);
			static void RemoveTarget(pGina::CredProv::Provider *ptr);
			
			static void NotifyStateChanged(bool newState);

		private:
			static pGina::Transactions::ServiceStateThread s_serviceStateThread;
			static std::list<pGina::CredProv::Provider *> s_providers;
			static std::list<pGina::CredProv::Credential *> s_creds;
			static pGina::Threading::Mutex s_mutex;
		};
	}
}