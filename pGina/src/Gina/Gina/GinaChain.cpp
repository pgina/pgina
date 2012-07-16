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
#include "HookedLoggedOutSAS.h"

// Dialog ID's for MSGINA's WlxLoggedOutSAS Dialog
#define IDD_WLXLOGGEDOUTSAS_DIALOG 1500 

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
			bool retval = m_wrappedGina->GetConsoleSwitchCredentials(pCredInfo);
			PWLX_CONSOLESWITCH_CREDENTIALS_INFO_V1_0 pInfo = (PWLX_CONSOLESWITCH_CREDENTIALS_INFO_V1_0) pCredInfo;
			pDEBUG(L"GetConsoleSwitchCredentials: CredInfo=%p PrivateDataLen: 0x%08x PrivateData: %p", pCredInfo, pInfo->PrivateDataLen, pInfo->PrivateData);
			return retval;
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
			// If configured to pass through to MS GINA, or auto-logon is enabled, we 
			// pass through, and let the MS GINA handle the logon.
			if(m_passthru || IsAutoLogonEnabled())
			{
				return m_wrappedGina->LoggedOutSAS(dwSasType, pAuthenticationId, pLogonSid, pdwOptions, phToken, pMprNotifyInfo, pProfile);				
			}

			// We auth'd in another session - this session is the real one though, need to tell the service to add!
			if(dwSasType == WLX_SAS_TYPE_AUTHENTICATED)
			{
				// We could do this, but we don't get password (without additional wrapping of our own in WlxGetConsoleSwitchCredentials
				//  So instead we grab it post SAS processing via msgina.				
				WLX_CONSOLESWITCH_CREDENTIALS_INFO_V1_0 credInfo;
				if(m_winlogon->WlxQueryConsoleSwitchCredentials(&credInfo))
				{
					pDEBUG(L"LoggedOutSAS: CredInfo=%p PrivateDataLen: 0x%08x PrivateData: %p", &credInfo, credInfo.PrivateDataLen, credInfo.PrivateData);
				}				
				
				int msresult = m_wrappedGina->LoggedOutSAS(dwSasType, pAuthenticationId, pLogonSid, pdwOptions, phToken, pMprNotifyInfo, pProfile);
				//pGina::Transactions::LoginInfo::Add(pMprNotifyInfo->pszUserName, pMprNotifyInfo->pszDomain, pMprNotifyInfo->pszPassword);
				return msresult;
			}

			// Gather username/password from user.  If remote, we get it from rdp client if provided,
			//  otherwise show the logged out sas dialog to get it.
			std::wstring username;
			std::wstring password;
			std::wstring domain;
						
			bool showDialog = true;

			if(pGina::Helpers::UserIsRemote())
			{
				WLX_CLIENT_CREDENTIALS_INFO_V2_0 creds;
				creds.dwType = WLX_CREDENTIAL_TYPE_V2_0;				
				if(m_winlogon->WlxQueryTsLogonCredentials(&creds))
				{
					if(creds.pszUserName) username = creds.pszUserName;
					if(creds.pszPassword) password = creds.pszPassword;
					if(creds.pszDomain) domain = creds.pszDomain;
					if(!creds.fPromptForPassword) showDialog = false;

					pDEBUG(L"fPromptForPassword: %s", creds.fPromptForPassword ? L"TRUE" : L"FALSE");
					pDEBUG(L"fDisconnectOnLogonFailure: %s", creds.fDisconnectOnLogonFailure ? L"TRUE" : L"FALSE");
				}				
			}
			
			if(showDialog)
			{
				DialogLoggedOutSAS dialog(m_winlogon);						
				dialog.Username(username);
				dialog.Password(password);

				int dialogResult = dialog.ShowDialog();
				if(dialogResult == DialogLoggedOutSAS::SAS_ACTION_LOGON)
				{
					// Harvest u/p for passing along to msgina
					username = dialog.Username();
					password = dialog.Password();
				}
				else if(dialogResult >= DialogLoggedOutSAS::SAS_ACTION_MIN && dialogResult <= DialogLoggedOutSAS::SAS_ACTION_MAX)
				{
					// Just do as told
					return dialogResult;
				}
				else
				{
					// Unknown ret value, default to no action
					return DialogLoggedOutSAS::SAS_ACTION_NONE;
				}
			}

			// We now have the login info, let's give it a shot!
			pDEBUG(L"GinaChain::LoggedOutSAS: Processing login for %s", username.c_str());
			pGina::Transactions::User::LoginResult result = pGina::Transactions::User::ProcessLoginForUser(username.c_str(), NULL, password.c_str(), pGina::Protocol::LoginRequestMessage::Login);
			if(!result.Result())
			{
				std::wstring failureMsg = result.Message();
				pERROR(L"GinaChain::LoggedOutSAS: %s", failureMsg.c_str());
				m_winlogon->WlxMessageBox(NULL, const_cast<wchar_t *>(failureMsg.c_str()), L"Login Failure", MB_ICONEXCLAMATION | MB_OK);
				return WLX_SAS_ACTION_NONE;					
			}			

			pDEBUG(L"inaChain::LoggedOutSAS: Successful, resulting username: %s", result.Username().c_str());						

			// Invoke the msgina logged out sas dialog, intercept it, set username/password, and hit ok!
			HookedLoggedOutSAS::Enabled(true);
			HookedLoggedOutSAS::SetLoginInfo(result);
			int msresult = m_wrappedGina->LoggedOutSAS(dwSasType, pAuthenticationId, pLogonSid, pdwOptions, phToken, pMprNotifyInfo, pProfile);
			HookedLoggedOutSAS::Enabled(false);				
			return msresult;			
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

		// Winlogon calls from MSGINA that we hook
		int GinaChain::WlxDialogBoxParam(HANDLE hInst, LPWSTR lpszTemplate, HWND hwndOwner, DLGPROC dlgprc, LPARAM dwInitParam)
		{
			// Is it a dialog id (integer)?
			if (!HIWORD(lpszTemplate)) 
			{
				// Make sure we only hook the dialogs we want
				switch ((DWORD) lpszTemplate) 
				{
					case IDD_WLXLOGGEDOUTSAS_DIALOG:
						// Thank god gina is single threaded... I think...
						HookedLoggedOutSAS::SetHookedDlgProc(dlgprc);
						return WinlogonProxy::WlxDialogBoxParam(hInst, lpszTemplate, hwndOwner, HookedLoggedOutSAS::GetDlgHookProc(), dwInitParam);
						break;
				}
			}			

			// Everyone else falls through
   			return WinlogonProxy::WlxDialogBoxParam(hInst, lpszTemplate, hwndOwner, dlgprc, dwInitParam);
		}

		// Check to see if the auto-logon registry settings are there.
		bool GinaChain::IsAutoLogonEnabled()
		{
			std::wstring subKeyName = L"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon";

			// We look for the values "AutoAdminLogon" and "ForceAutoLogon".  If either of them exist
			// and are non-zero, we return true.
			if( pGina::Registry::StringValueExistsAndIsNonZero(HKEY_LOCAL_MACHINE, subKeyName.c_str(), L"AutoAdminLogon") ||
				pGina::Registry::StringValueExistsAndIsNonZero(HKEY_LOCAL_MACHINE, subKeyName.c_str(), L"ForceAutoLogon") )
				return true;

			return false;
		}
	}
}