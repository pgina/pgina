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

#include "Winlogon.h"

namespace pGina
{
	namespace GINA
	{
		class Gina
		{
		public:
			static bool InitializeFactory(HANDLE hWlx, void * pWinlogonFunctions, Gina **context);

			Gina(WinlogonInterface *pWinLogonIface);

			// Queries from winlogon
			virtual bool IsLockOk() = 0;
			virtual bool IsLogoffOk() = 0;
			virtual bool GetConsoleSwitchCredentials(PVOID pCredInfo) = 0;

			// Notifications from winlogon
			virtual void ReconnectNotify() = 0;
			virtual void DisconnectNotify() = 0;
			virtual void Logoff() = 0;
			virtual bool ScreenSaverNotify(BOOL * pSecure) = 0;
			virtual void Shutdown(DWORD ShutdownType) = 0;

			// Notices/Status messages
			virtual void DisplaySASNotice() = 0;
			virtual void DisplayLockedNotice() = 0;
			virtual bool DisplayStatusMessage(HDESK hDesktop, DWORD dwOptions, PWSTR pTitle, PWSTR pMessage) = 0;
			virtual bool GetStatusMessage(DWORD * pdwOptions, PWSTR pMessage, DWORD dwBufferSize) = 0;
			virtual bool RemoveStatusMessage() = 0;					
			
			// SAS handling
			virtual int  LoggedOutSAS(DWORD dwSasType, PLUID pAuthenticationId, PSID pLogonSid, PDWORD pdwOptions, 
								 	  PHANDLE phToken, PWLX_MPR_NOTIFY_INFO pMprNotifyInfo, PVOID *pProfile) = 0;			
			virtual int  LoggedOnSAS(DWORD dwSasType, PVOID pReserved) = 0;			
			virtual int  WkstaLockedSAS(DWORD dwSasType) = 0;
			
			// Things to do when winlogon says to...
			virtual bool ActivateUserShell(PWSTR pszDesktopName, PWSTR pszMprLogonScript, PVOID pEnvironment) = 0;			
			virtual bool StartApplication(PWSTR pszDesktopName, PVOID pEnvironment, PWSTR pszCmdLine) = 0;
			virtual bool NetworkProviderLoad(PWLX_MPR_NOTIFY_INFO pNprNotifyInfo) = 0;						

			virtual ~Gina();

		protected:			
			WinlogonInterface * m_winlogon;
		};
	}
}