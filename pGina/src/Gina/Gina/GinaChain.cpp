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

#include "GinaChain.h"

namespace pGina
{
	namespace GINA
	{
		// Queries from winlogon
		bool GinaChain::IsLockOk()
		{
			return false;
		}

		bool GinaChain::IsLogoffOk()
		{
			return false;
		}

		bool GinaChain::GetConsoleSwitchCredentials(PVOID pCredInfo)
		{
			return false;
		}

		// Notifications from winlogon
		void GinaChain::ReconnectNotify()
		{
		}

		void GinaChain::DisconnectNotify()
		{
		}

		void GinaChain::Logoff()
		{
		}

		bool GinaChain::ScreenSaverNotify(BOOL * pSecure)
		{
			return false;
		}

		void GinaChain::Shutdown(DWORD ShutdownType)
		{
		}


		// Notices/Status messages
		void GinaChain::DisplaySASNotice()
		{
		}

		void GinaChain::DisplayLockedNotice()
		{
		}

		bool GinaChain::DisplayStatusMessage(HDESK hDesktop, DWORD dwOptions, PWSTR pTitle, PWSTR pMessage)
		{
			return false;
		}

		bool GinaChain::GetStatusMessage(DWORD * pdwOptions, PWSTR pMessage, DWORD dwBufferSize)
		{
			return false;
		}

		bool GinaChain::RemoveStatusMessage()
		{
			return false;
		}

			
		// SAS handling
		int  GinaChain::LoggedOutSAS(DWORD dwSasType, PLUID pAuthenticationId, PSID pLogonSid, PDWORD pdwOptions, 
									 PHANDLE phToken, PWLX_MPR_NOTIFY_INFO pMprNotifyInfo, PVOID *pProfile)
		{
			return 0;
		}

		int  GinaChain::LoggedOnSAS(DWORD dwSasType, PVOID pReserved)
		{
			return 0;
		}

		int  GinaChain::WkstaLockedSAS(DWORD dwSasType)
		{
			return 0;
		}

			
		// Things to do when winlogon says to...
		bool GinaChain::ActivateUserShell(PWSTR pszDesktopName, PWSTR pszMprLogonScript, PVOID pEnvironment)
		{
			return false;
		}

		bool GinaChain::StartApplication(PWSTR pszDesktopName, PVOID pEnvironment, PWSTR pszCmdLine)
		{
			return false;
		}

		bool GinaChain::NetworkProviderLoad(PWLX_MPR_NOTIFY_INFO pNprNotifyInfo)
		{
			return false;
		}
	}
}