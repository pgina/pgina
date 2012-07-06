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
			m_dllVersion(0),
			m_negotiated(false),
			m_initialized(false),
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

		// TBD: Instead of checking function ptrs, we could just use m_dllVersion to infer which 
		//	*must* be available (at which point crashes would be the chained dll's fault.  That
		//	seems overly mean though perhaps...

		// Additional GINA exports that we implement (wrapped)
		bool GinaWrapper::Negotiate(DWORD dwWinlogonVersion)
		{
			if(!m_pfWlxNegotiate) return false;
			m_negotiated = m_pfWlxNegotiate(dwWinlogonVersion, &m_dllVersion) ? true : false;
			return m_negotiated;
		}

		bool GinaWrapper::Initialize(LPWSTR lpWinsta, HANDLE hWlx, PVOID pvReserved, PVOID pWinlogonFunctions)
		{
			if(!m_pfWlxInitialize) return false;
			m_initialized = m_pfWlxInitialize(lpWinsta, hWlx, pvReserved, pWinlogonFunctions, &m_ginaContext) ? true : false;
			return m_initialized;
		}
			
		// Standard Gina * interfaces, directly map to GINA exports
		bool GinaWrapper::IsLockOk()
		{
			if(!m_pfWlxIsLockOk || !m_negotiated || !m_initialized) return true;
			return (m_pfWlxIsLockOk(m_ginaContext) ? true : false);
		}

		bool GinaWrapper::IsLogoffOk()
		{
			if(!m_pfWlxIsLogoffOk || !m_negotiated || !m_initialized) return true;
			return (m_pfWlxIsLogoffOk(m_ginaContext) ? true : false);			
		}

		bool GinaWrapper::GetConsoleSwitchCredentials(PVOID pCredInfo)
		{
			if(!m_pfWlxGetConsoleSwitchCredentials || !m_negotiated || !m_initialized) return false;
			return m_pfWlxGetConsoleSwitchCredentials(m_ginaContext, pCredInfo) ? true : false;
		}

		void GinaWrapper::ReconnectNotify()
		{			
			if(m_pfWlxReconnectNotify && m_negotiated && m_initialized)
				m_pfWlxReconnectNotify(m_ginaContext);
		}

		void GinaWrapper::DisconnectNotify()
		{
			if(m_pfWlxDisconnectNotify && m_negotiated && m_initialized)
				m_pfWlxDisconnectNotify(m_ginaContext);
		}

		void GinaWrapper::Logoff()
		{
			if(m_pfWlxLogoff && m_negotiated && m_initialized)
				m_pfWlxLogoff(m_ginaContext);
		}

		bool GinaWrapper::ScreenSaverNotify(BOOL * pSecure)
		{
			if(!m_pfWlxScreenSaverNotify || !m_negotiated || !m_initialized) return false;
			return m_pfWlxScreenSaverNotify(m_ginaContext, pSecure) ? true : false;
		}

		void GinaWrapper::Shutdown(DWORD ShutdownType)
		{
			if(m_pfWlxShutdown && m_negotiated && m_initialized)
				m_pfWlxShutdown(m_ginaContext, ShutdownType);
		}

		void GinaWrapper::DisplaySASNotice()
		{
			if(m_pfWlxDisplaySASNotice && m_negotiated && m_initialized)
				m_pfWlxDisplaySASNotice(m_ginaContext);
		}

		void GinaWrapper::DisplayLockedNotice()
		{
			if(m_pfWlxDisplayLockedNotice && m_negotiated && m_initialized)
				m_pfWlxDisplayLockedNotice(m_ginaContext);
		}

		bool GinaWrapper::DisplayStatusMessage(HDESK hDesktop, DWORD dwOptions, PWSTR pTitle, PWSTR pMessage)
		{
			if(!m_pfWlxDisplayStatusMessage || !m_negotiated || !m_initialized) return false;
			return m_pfWlxDisplayStatusMessage(m_ginaContext, hDesktop, dwOptions, pTitle, pMessage) ? true : false;
		}

		bool GinaWrapper::GetStatusMessage(DWORD * pdwOptions, PWSTR pMessage, DWORD dwBufferSize)
		{
			if(!m_pfWlxGetStatusMessage || !m_negotiated || !m_initialized) return false;
			return m_pfWlxGetStatusMessage(m_ginaContext, pdwOptions, pMessage, dwBufferSize) ? true : false;
		}

		bool GinaWrapper::RemoveStatusMessage()
		{
			if(!m_pfWlxRemoveStatusMessage || !m_negotiated || !m_initialized) return false;
			return m_pfWlxRemoveStatusMessage(m_ginaContext) ? true : false;
		}

		int  GinaWrapper::LoggedOutSAS(DWORD dwSasType, PLUID pAuthenticationId, PSID pLogonSid, PDWORD pdwOptions,
								 PHANDLE phToken, PWLX_MPR_NOTIFY_INFO pMprNotifyInfo, PVOID *pProfile)
		{
			if(!m_pfWlxLoggedOutSAS || !m_negotiated || !m_initialized) return 0;
			return m_pfWlxLoggedOutSAS(m_ginaContext, dwSasType, pAuthenticationId, pLogonSid, pdwOptions, phToken, pMprNotifyInfo, pProfile);
		}

		int  GinaWrapper::LoggedOnSAS(DWORD dwSasType, PVOID pReserved)
		{
			if(!m_pfWlxLoggedOnSAS || !m_negotiated || !m_initialized) return 0;
			return m_pfWlxLoggedOnSAS(m_ginaContext, dwSasType, pReserved);
		}

		int  GinaWrapper::WkstaLockedSAS(DWORD dwSasType)
		{
			if(!m_pfWlxWkstaLockedSAS || !m_negotiated || !m_initialized) return 0;
			return m_pfWlxWkstaLockedSAS(m_ginaContext, dwSasType);
		}

		bool GinaWrapper::ActivateUserShell(PWSTR pszDesktopName, PWSTR pszMprLogonScript, PVOID pEnvironment)
		{
			if(!m_pfWlxActivateUserShell || !m_negotiated || !m_initialized) return false;
			return m_pfWlxActivateUserShell(m_ginaContext, pszDesktopName, pszMprLogonScript, pEnvironment) ? true : false;
		}

		bool GinaWrapper::StartApplication(PWSTR pszDesktopName, PVOID pEnvironment, PWSTR pszCmdLine)
		{
			if(!m_pfWlxStartApplication || !m_negotiated || !m_initialized) return false;
			return m_pfWlxStartApplication(m_ginaContext, pszDesktopName, pEnvironment, pszCmdLine) ? true : false;
		}

		bool GinaWrapper::NetworkProviderLoad(PWLX_MPR_NOTIFY_INFO pNprNotifyInfo)
		{
			if(!m_pfWlxNetworkProviderLoad || !m_negotiated || !m_initialized) return false;
			return m_pfWlxNetworkProviderLoad(m_ginaContext, pNprNotifyInfo) ? true : false;
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