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

#include "Gina.h"
#include "GinaWrapper.h"
#include "Winlogon.h"
#include "WinlogonProxy.h"

namespace pGina
{
	namespace GINA
	{
		// Gina implementation that loads another gina dll that hooks 
		//	(and stubs) msgina.dll for customization (ui etc).  We are both
		//  a Gina() interface (for winlogon) and a WinlogonProxy (for our 
		//  chained GINA).
		class GinaChain : public Gina, public WinlogonProxy
		{
		public:
			GinaChain(WinlogonInterface *pWinLogonIface);
			virtual ~GinaChain();

			// Queries from winlogon
			virtual bool IsLockOk();
			virtual bool IsLogoffOk();
			virtual bool GetConsoleSwitchCredentials(PVOID pCredInfo);

			// Notifications from winlogon
			virtual void ReconnectNotify();
			virtual void DisconnectNotify();
			virtual void Logoff();
			virtual bool ScreenSaverNotify(BOOL * pSecure);
			virtual void Shutdown(DWORD ShutdownType);

			// Notices/Status messages
			virtual void DisplaySASNotice();
			virtual void DisplayLockedNotice();
			virtual bool DisplayStatusMessage(HDESK hDesktop, DWORD dwOptions, PWSTR pTitle, PWSTR pMessage);
			virtual bool GetStatusMessage(DWORD * pdwOptions, PWSTR pMessage, DWORD dwBufferSize);
			virtual bool RemoveStatusMessage();						
			
			// SAS handling
			virtual int  LoggedOutSAS(DWORD dwSasType, PLUID pAuthenticationId, PSID pLogonSid, PDWORD pdwOptions, 
									 PHANDLE phToken, PWLX_MPR_NOTIFY_INFO pMprNotifyInfo, PVOID *pProfile);
			virtual int  LoggedOnSAS(DWORD dwSasType, PVOID pReserved);
			virtual int  WkstaLockedSAS(DWORD dwSasType);
			
			// Things to do when winlogon says to...
			virtual bool ActivateUserShell(PWSTR pszDesktopName, PWSTR pszMprLogonScript, PVOID pEnvironment);
			virtual bool StartApplication(PWSTR pszDesktopName, PVOID pEnvironment, PWSTR pszCmdLine);
			virtual bool NetworkProviderLoad(PWLX_MPR_NOTIFY_INFO pNprNotifyInfo);		
		
			// Winlogon callbacks that we hook
			virtual int  WlxDialogBoxParam(HANDLE hInst, LPWSTR lpszTemplate, HWND hwndOwner, DLGPROC dlgprc, LPARAM dwInitParam);
		private:
			GinaWrapper * m_wrappedGina;
			bool m_passthru;

			// Check for auto-logon registry settings.
			bool IsAutoLogonEnabled();
		};
	}
}