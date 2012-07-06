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

#include "Gina.h"
#include "Winlogon.h"
#include "WinlogonProxy.h"

namespace pGina
{
	namespace GINA
	{
		// Define function prototypes for all potential GINA exports
		typedef BOOL (WINAPI * PFWLXNEGOTIATE)  (DWORD, DWORD *);
		typedef BOOL (WINAPI * PFWLXINITIALIZE) (LPWSTR, HANDLE, PVOID, PVOID, PVOID *);
		typedef VOID (WINAPI * PFWLXDISPLAYSASNOTICE) (PVOID);
		typedef int  (WINAPI * PFWLXLOGGEDOUTSAS) (PVOID, DWORD, PLUID, PSID, PDWORD, PHANDLE, PWLX_MPR_NOTIFY_INFO, PVOID *);
		typedef BOOL (WINAPI * PFWLXACTIVATEUSERSHELL) (PVOID, PWSTR, PWSTR, PVOID);
		typedef int  (WINAPI * PFWLXLOGGEDONSAS) (PVOID, DWORD, PVOID);
		typedef VOID (WINAPI * PFWLXDISPLAYLOCKEDNOTICE) (PVOID);
		typedef int  (WINAPI * PFWLXWKSTALOCKEDSAS) (PVOID, DWORD);
		typedef BOOL (WINAPI * PFWLXISLOCKOK) (PVOID);
		typedef BOOL (WINAPI * PFWLXISLOGOFFOK) (PVOID);
		typedef VOID (WINAPI * PFWLXLOGOFF) (PVOID);
		typedef VOID (WINAPI * PFWLXSHUTDOWN) (PVOID, DWORD);
		typedef BOOL (WINAPI * PFWLXSCREENSAVERNOTIFY) (PVOID, BOOL *);
		typedef BOOL (WINAPI * PFWLXSTARTAPPLICATION) (PVOID, PWSTR, PVOID, PWSTR);
		typedef BOOL (WINAPI * PFWLXNETWORKPROVIDERLOAD) (PVOID, PWLX_MPR_NOTIFY_INFO);
		typedef BOOL (WINAPI * PFWLXDISPLAYSTATUSMESSAGE) (PVOID, HDESK, DWORD, PWSTR, PWSTR);
		typedef BOOL (WINAPI * PFWLXGETSTATUSMESSAGE) (PVOID, DWORD *, PWSTR, DWORD);
		typedef BOOL (WINAPI * PFWLXREMOVESTATUSMESSAGE) (PVOID);
		typedef BOOL (WINAPI * PFWLXGETCONSOLESWITCHCREDENTIALS) (PVOID, PVOID); 
		typedef VOID (WINAPI * PFWLXRECONNECTNOTIFY) (PVOID);
		typedef VOID (WINAPI * PFWLXDISCONNECTNOTIFY) (PVOID); 

		// Gina implementation that loads another gina dll only (no stubbing/hooking), 
		//	allowing a caller to 'be' winlogon.
		class GinaWrapper : public Gina
		{
		public:			
			GinaWrapper(const wchar_t * dll);
			~GinaWrapper();

			DWORD Version() { return m_dllVersion; }			

			// Additional GINA exports that we implement
			bool Negotiate(DWORD dwWinlogonVersion);
			bool Initialize(LPWSTR lpWinsta, HANDLE hWlx, PVOID pvReserved, PVOID pWinlogonFunctions);
			
			// Standard Gina * interfaces, directly map to GINA exports
			virtual bool IsLockOk();
			virtual bool IsLogoffOk();
			virtual bool GetConsoleSwitchCredentials(PVOID pCredInfo);
			virtual void ReconnectNotify();
			virtual void DisconnectNotify();
			virtual void Logoff();
			virtual bool ScreenSaverNotify(BOOL * pSecure);
			virtual void Shutdown(DWORD ShutdownType);
			virtual void DisplaySASNotice();
			virtual void DisplayLockedNotice();
			virtual bool DisplayStatusMessage(HDESK hDesktop, DWORD dwOptions, PWSTR pTitle, PWSTR pMessage);
			virtual bool GetStatusMessage(DWORD * pdwOptions, PWSTR pMessage, DWORD dwBufferSize);
			virtual bool RemoveStatusMessage();						
			virtual int  LoggedOutSAS(DWORD dwSasType, PLUID pAuthenticationId, PSID pLogonSid, PDWORD pdwOptions, 
									 PHANDLE phToken, PWLX_MPR_NOTIFY_INFO pMprNotifyInfo, PVOID *pProfile);
			virtual int  LoggedOnSAS(DWORD dwSasType, PVOID pReserved);
			virtual int  WkstaLockedSAS(DWORD dwSasType);
			virtual bool ActivateUserShell(PWSTR pszDesktopName, PWSTR pszMprLogonScript, PVOID pEnvironment);
			virtual bool StartApplication(PWSTR pszDesktopName, PVOID pEnvironment, PWSTR pszCmdLine);
			virtual bool NetworkProviderLoad(PWLX_MPR_NOTIFY_INFO pNprNotifyInfo);		
				
			// Call this to load (and verify it loaded) the wrapped gina
			bool Load();			
			void Unload();
			bool IsLoaded() { return m_dll != 0; }

		private:			
			std::wstring m_dllname;
			void * m_ginaContext;
			HINSTANCE m_dll;
			DWORD m_dllVersion;
			bool m_negotiated;
			bool m_initialized;

			// Pointers to the real MSGINA functions.
			PFWLXNEGOTIATE                		m_pfWlxNegotiate;
			PFWLXINITIALIZE                     m_pfWlxInitialize;
			PFWLXDISPLAYSASNOTICE               m_pfWlxDisplaySASNotice;
			PFWLXLOGGEDOUTSAS                   m_pfWlxLoggedOutSAS;
			PFWLXACTIVATEUSERSHELL              m_pfWlxActivateUserShell;
			PFWLXLOGGEDONSAS                    m_pfWlxLoggedOnSAS;
			PFWLXDISPLAYLOCKEDNOTICE            m_pfWlxDisplayLockedNotice;
			PFWLXWKSTALOCKEDSAS                 m_pfWlxWkstaLockedSAS;
			PFWLXISLOCKOK                       m_pfWlxIsLockOk;
			PFWLXISLOGOFFOK                     m_pfWlxIsLogoffOk;
			PFWLXLOGOFF                         m_pfWlxLogoff;
			PFWLXSHUTDOWN                       m_pfWlxShutdown;
			PFWLXSTARTAPPLICATION               m_pfWlxStartApplication;
			PFWLXSCREENSAVERNOTIFY              m_pfWlxScreenSaverNotify;
			PFWLXNETWORKPROVIDERLOAD            m_pfWlxNetworkProviderLoad;
			PFWLXDISPLAYSTATUSMESSAGE           m_pfWlxDisplayStatusMessage;
			PFWLXGETSTATUSMESSAGE               m_pfWlxGetStatusMessage;
			PFWLXREMOVESTATUSMESSAGE      		m_pfWlxRemoveStatusMessage;
			PFWLXGETCONSOLESWITCHCREDENTIALS	m_pfWlxGetConsoleSwitchCredentials; 
			PFWLXRECONNECTNOTIFY                m_pfWlxReconnectNotify;
			PFWLXDISCONNECTNOTIFY               m_pfWlxDisconnectNotify;
		};
	}
}