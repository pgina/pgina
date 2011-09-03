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

#include "GinaWrapper.h"

namespace pGina
{
	namespace GINA
	{
		GinaWrapper::GinaWrapper(const wchar_t * dll) :
			Gina(NULL),
			m_ginaContext(0),
			m_dll(0),
			m_pfWlxNegotiate(0),
			m_pfWlxInitialize(0),
			m_pfWlxDisplaySASNotice(0),
			m_pfWlxLoggedOutSAS(0),
			m_pfWlxActivateUserShell(0),
			m_pfWlxLoggedOnSAS(0),
			m_pfWlxDisplayLockedNotice(0),
			m_pfWlxWkstaLockedSAS(0),
			m_pfWlxIsLockOk(0),
			m_pfWlxIsLogoffOk(0),
			m_pfWlxLogoff(0),
			m_pfWlxShutdown(0),
			m_pfWlxStartApplication(0),
			m_pfWlxScreenSaverNotify(0),
			m_pfWlxNetworkProviderLoad(0),
			m_pfWlxDisplayStatusMessage(0),
			m_pfWlxGetStatusMessage(0),
			m_pfWlxRemoveStatusMessage(0),
			m_pfWlxGetConsoleSwitchCredentials(0),
			m_pfWlxReconnectNotify(0),
			m_pfWlxDisconnectNotify(0)
		{
			if(dll) m_dllname = dll;			
		}
		
		GinaWrapper::~GinaWrapper()
		{
			Unload();
		}

		// Additional GINA exports that we implement (wrapped)
		bool GinaWrapper::Negotiate(DWORD dwWinlogonVersion, DWORD *pdwDllVersion)
		{
			return false;
		}

		bool GinaWrapper::Initialize(LPWSTR lpWinsta, HANDLE hWlx, PVOID pvReserved, PVOID pWinlogonFunctions)
		{
			return false;
		}
			
		// Standard Gina * interfaces, directly map to GINA exports
		bool GinaWrapper::IsLockOk()
		{
			return false;
		}

		bool GinaWrapper::IsLogoffOk()
		{
			return false;
		}

		bool GinaWrapper::GetConsoleSwitchCredentials(PVOID pCredInfo)
		{
			return false;
		}

		void GinaWrapper::ReconnectNotify()
		{			
		}

		void GinaWrapper::DisconnectNotify()
		{
		}

		void GinaWrapper::Logoff()
		{
		}

		bool GinaWrapper::ScreenSaverNotify(BOOL * pSecure)
		{
			return false;
		}

		void GinaWrapper::Shutdown(DWORD ShutdownType)
		{
		}

		void GinaWrapper::DisplaySASNotice()
		{
		}

		void GinaWrapper::DisplayLockedNotice()
		{
		}

		bool GinaWrapper::DisplayStatusMessage(HDESK hDesktop, DWORD dwOptions, PWSTR pTitle, PWSTR pMessage)
		{
			return false;
		}

		bool GinaWrapper::GetStatusMessage(DWORD * pdwOptions, PWSTR pMessage, DWORD dwBufferSize)
		{
			return false;
		}

		bool GinaWrapper::RemoveStatusMessage()
		{
			return false;
		}

		int  GinaWrapper::LoggedOutSAS(DWORD dwSasType, PLUID pAuthenticationId, PSID pLogonSid, PDWORD pdwOptions,
								 PHANDLE phToken, PWLX_MPR_NOTIFY_INFO pMprNotifyInfo, PVOID *pProfile)
		{
			return 0;
		}

		int  GinaWrapper::LoggedOnSAS(DWORD dwSasType, PVOID pReserved)
		{
			return 0;
		}

		int  GinaWrapper::WkstaLockedSAS(DWORD dwSasType)
		{
			return 0;
		}

		bool GinaWrapper::ActivateUserShell(PWSTR pszDesktopName, PWSTR pszMprLogonScript, PVOID pEnvironment)
		{
			return false;
		}

		bool GinaWrapper::StartApplication(PWSTR pszDesktopName, PVOID pEnvironment, PWSTR pszCmdLine)
		{
			return false;
		}

		bool GinaWrapper::NetworkProviderLoad(PWLX_MPR_NOTIFY_INFO pNprNotifyInfo)
		{
			return false;
		}

		bool GinaWrapper::Load()
		{			
			if(!(m_dll = LoadLibraryW(m_dllname.c_str())))
				return false;
			
			m_pfWlxNegotiate = (PFWLXNEGOTIATE) GetProcAddress(m_dll, "WlxNegotiate");
			m_pfWlxInitialize = (PFWLXINITIALIZE) GetProcAddress(m_dll, "WlxInitialize");
			m_pfWlxDisplaySASNotice = (PFWLXDISPLAYSASNOTICE) GetProcAddress(m_dll, "WlxDisplaySASNotice");
			m_pfWlxLoggedOutSAS = (PFWLXLOGGEDOUTSAS) GetProcAddress(m_dll, "WlxLoggedOutSAS");
			m_pfWlxActivateUserShell = (PFWLXACTIVATEUSERSHELL) GetProcAddress(m_dll, "WlxActivateUserShell");
			m_pfWlxLoggedOnSAS = (PFWLXLOGGEDONSAS) GetProcAddress(m_dll, "WlxLoggedOnSAS");
			m_pfWlxDisplayLockedNotice = (PFWLXDISPLAYLOCKEDNOTICE) GetProcAddress(m_dll, "WlxDisplayLockedNotice");
			m_pfWlxWkstaLockedSAS = (PFWLXWKSTALOCKEDSAS) GetProcAddress(m_dll, "WlxWkstaLockedSAS");
			m_pfWlxIsLockOk = (PFWLXISLOCKOK) GetProcAddress(m_dll, "WlxIsLockOk");
			m_pfWlxIsLogoffOk = (PFWLXISLOGOFFOK) GetProcAddress(m_dll, "WlxIsLogoffOk");
			m_pfWlxLogoff = (PFWLXLOGOFF) GetProcAddress(m_dll, "WlxLogoff");
			m_pfWlxShutdown = (PFWLXSHUTDOWN) GetProcAddress(m_dll, "WlxShutdown");
			m_pfWlxStartApplication = (PFWLXSTARTAPPLICATION) GetProcAddress(m_dll, "WlxStartApplication");
			m_pfWlxScreenSaverNotify = (PFWLXSCREENSAVERNOTIFY) GetProcAddress(m_dll, "WlxScreenSaverNotify");
			m_pfWlxNetworkProviderLoad = (PFWLXNETWORKPROVIDERLOAD) GetProcAddress(m_dll, "WlxNetworkProviderLoad");
			m_pfWlxDisplayStatusMessage = (PFWLXDISPLAYSTATUSMESSAGE) GetProcAddress(m_dll, "WlxDisplayStatusMessage");
			m_pfWlxGetStatusMessage = (PFWLXGETSTATUSMESSAGE) GetProcAddress(m_dll, "WlxGetStatusMessage");
			m_pfWlxRemoveStatusMessage = (PFWLXREMOVESTATUSMESSAGE) GetProcAddress(m_dll, "WlxRemoveStatusMessage");
			m_pfWlxGetConsoleSwitchCredentials = (PFWLXGETCONSOLESWITCHCREDENTIALS) GetProcAddress(m_dll, "WlxGetConsoleSwitchCredentials");
			m_pfWlxReconnectNotify = (PFWLXRECONNECTNOTIFY) GetProcAddress(m_dll, "WlxReconnectNotify");
			m_pfWlxDisconnectNotify = (PFWLXDISCONNECTNOTIFY) GetProcAddress(m_dll, "WlxDisconnectNotify");

			return true;
		}

		void GinaWrapper::Unload()
		{
			if(!IsLoaded()) return;

			if(FreeLibrary(m_dll))
			{
				m_ginaContext = 0;
				m_dll = 0;

				m_pfWlxNegotiate = 0;
				m_pfWlxInitialize = 0;
				m_pfWlxDisplaySASNotice = 0;
				m_pfWlxLoggedOutSAS = 0;
				m_pfWlxActivateUserShell = 0;
				m_pfWlxLoggedOnSAS = 0;
				m_pfWlxDisplayLockedNotice = 0;
				m_pfWlxWkstaLockedSAS = 0;
				m_pfWlxIsLockOk = 0;
				m_pfWlxIsLogoffOk = 0;
				m_pfWlxLogoff = 0;
				m_pfWlxShutdown = 0;
				m_pfWlxStartApplication = 0;
				m_pfWlxScreenSaverNotify = 0;
				m_pfWlxNetworkProviderLoad = 0;
				m_pfWlxDisplayStatusMessage = 0;
				m_pfWlxGetStatusMessage = 0;
				m_pfWlxRemoveStatusMessage = 0;
				m_pfWlxGetConsoleSwitchCredentials = 0;
				m_pfWlxReconnectNotify = 0;
				m_pfWlxDisconnectNotify = 0;
			}			
		}
	}
}