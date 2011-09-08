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

#include <pGinaNativeLib.h>
#include <Macros.h>

#include "GinaChain.h"
#include "GinaWrapper.h"
#include "WinlogonRouter.h"

#include "DialogLoggedOutSAS.h"

namespace pGina
{
	namespace GINA
	{
		GinaChain::GinaChain(WinlogonInterface *pWinLogonIface) : 
			Gina(pWinLogonIface), WinlogonProxy(pWinLogonIface), m_passthru(false)
		{
			// Are we in passthru mode?
			m_passthru = pGina::Registry::GetBool(L"GinaPassthru", false);

			// When we use the winlogon router table, we want it to 
			//	direct all winlogon calls to us (via our WinlogonProxy 
			//	implementation).  We sit between winlogon and a real
			//	GINA, so our Gina interface let's us have first shot
			//	at Winlogon->Gina direction calls, and WinlogonRouter+
			//	WinlogonProxy let's us have first shot at Gina->Winlogon
			//	direction traffic.
			WinlogonRouter::Interface(this);

			// Init and load our chained/wrapped gina
			std::wstring ginaName = pGina::Registry::GetString(L"ChainedGinaPath", L"MSGINA.DLL");
			pDEBUG(L"Wrapping gina: %s", ginaName.c_str());

			m_wrappedGina = new GinaWrapper(ginaName.c_str());
			if(!m_wrappedGina->Load())
			{
				pERROR(L"Failed to load wrapped gina: 0x%08x", GetLastError());
				return;
			}

			// Now negotiate and initialize our wrapped gina			
			if(!m_wrappedGina->Negotiate(WinlogonInterface::Version()))
			{	
				pERROR(L"Failed to negotiate with wrapped gina using version: %d (0x%08x)", WinlogonInterface::Version(), WinlogonInterface::Version());
				return;
			}

			if(!m_wrappedGina->Initialize(L"Winsta0", WinlogonRouter::Interface(), NULL, WinlogonRouter::DispatchTable()))
			{
				pERROR(L"Failed to initialize wrapped gina");
				return;
			}
		}

		GinaChain::~GinaChain()
		{
			if(m_wrappedGina)
			{
				delete m_wrappedGina;
				m_wrappedGina = NULL;
			}
		}

		// Queries from winlogon
		bool GinaChain::IsLockOk()
		{
			return m_wrappedGina->IsLockOk();
		}

		bool GinaChain::IsLogoffOk()
		{
			return m_wrappedGina->IsLogoffOk();
		}

		bool GinaChain::GetConsoleSwitchCredentials(PVOID pCredInfo)
		{
			return m_wrappedGina->GetConsoleSwitchCredentials(pCredInfo);
		}

		// Notifications from winlogon
		void GinaChain::ReconnectNotify()
		{
			m_wrappedGina->ReconnectNotify();
		}

		void GinaChain::DisconnectNotify()
		{
			m_wrappedGina->DisconnectNotify();
		}

		void GinaChain::Logoff()
		{
			m_wrappedGina->Logoff();
		}

		bool GinaChain::ScreenSaverNotify(BOOL * pSecure)
		{
			return m_wrappedGina->ScreenSaverNotify(pSecure);
		}

		void GinaChain::Shutdown(DWORD ShutdownType)
		{
			m_wrappedGina->Shutdown(ShutdownType);
		}


		// Notices/Status messages
		void GinaChain::DisplaySASNotice()
		{
			m_wrappedGina->DisplaySASNotice();
		}

		void GinaChain::DisplayLockedNotice()
		{
			m_wrappedGina->DisplayLockedNotice();
		}

		bool GinaChain::DisplayStatusMessage(HDESK hDesktop, DWORD dwOptions, PWSTR pTitle, PWSTR pMessage)
		{
			return m_wrappedGina->DisplayStatusMessage(hDesktop, dwOptions, pTitle, pMessage);
		}

		bool GinaChain::GetStatusMessage(DWORD * pdwOptions, PWSTR pMessage, DWORD dwBufferSize)
		{
			return m_wrappedGina->GetStatusMessage(pdwOptions, pMessage, dwBufferSize);
		}

		bool GinaChain::RemoveStatusMessage()
		{
			return m_wrappedGina->RemoveStatusMessage();
		}

			
		// SAS handling
		int  GinaChain::LoggedOutSAS(DWORD dwSasType, PLUID pAuthenticationId, PSID pLogonSid, PDWORD pdwOptions, 
									 PHANDLE phToken, PWLX_MPR_NOTIFY_INFO pMprNotifyInfo, PVOID *pProfile)
		{
			if(m_passthru)
			{
				return m_wrappedGina->LoggedOutSAS(dwSasType, pAuthenticationId, pLogonSid, pdwOptions, phToken, pMprNotifyInfo, pProfile);				
			}

			DialogLoggedOutSAS dialog(m_winlogon);
			int ourDialogResult = dialog.ShowDialog();	
			
			if(ourDialogResult == DialogLoggedOutSAS::PGINA_EMERGENCY_ESCAPE_HATCH)
				return m_wrappedGina->LoggedOutSAS(dwSasType, pAuthenticationId, pLogonSid, pdwOptions, phToken, pMprNotifyInfo, pProfile);
			
			switch(ourDialogResult)
			{
			case DialogLoggedOutSAS::SAS_ACTION_LOGON:
				// Invoke the msgina logged out sas dialog, intercept it, set username/password, and hit ok!
				break;
			case DialogLoggedOutSAS::PGINA_LOGIN_FAILED:
				std::wstring failureMessage = dialog.LoginResult().Message();
				m_winlogon->WlxMessageBox(NULL, const_cast<wchar_t *>(failureMessage.c_str()), L"Login Failure", MB_ICONEXCLAMATION | MB_OK);
				return WLX_SAS_ACTION_NONE;	
				break;			
			}

			// If we make it here, just return none
			return WLX_SAS_ACTION_NONE;
		}

		int  GinaChain::LoggedOnSAS(DWORD dwSasType, PVOID pReserved)
		{
			return m_wrappedGina->LoggedOnSAS(dwSasType, pReserved);
		}

		int  GinaChain::WkstaLockedSAS(DWORD dwSasType)
		{
			return m_wrappedGina->WkstaLockedSAS(dwSasType);
		}

			
		// Things to do when winlogon says to...
		bool GinaChain::ActivateUserShell(PWSTR pszDesktopName, PWSTR pszMprLogonScript, PVOID pEnvironment)
		{
			return m_wrappedGina->ActivateUserShell(pszDesktopName, pszMprLogonScript, pEnvironment);
		}

		bool GinaChain::StartApplication(PWSTR pszDesktopName, PVOID pEnvironment, PWSTR pszCmdLine)
		{
			return m_wrappedGina->StartApplication(pszDesktopName, pEnvironment, pszCmdLine);
		}

		bool GinaChain::NetworkProviderLoad(PWLX_MPR_NOTIFY_INFO pNprNotifyInfo)
		{
			return m_wrappedGina->NetworkProviderLoad(pNprNotifyInfo);
		}							
	}
}