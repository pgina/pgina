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

#include "WinlogonReal.h"

namespace pGina
{
	namespace GINA
	{
		void RealWinlogonInterface::WlxUseCtrlAltDel() 
		{
			return ((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxUseCtrlAltDel(m_hWlx);
		}

		void RealWinlogonInterface::WlxSetContextPointer(void *newContext)
		{
			((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxSetContextPointer(m_hWlx, newContext);
		}

		void RealWinlogonInterface::WlxSasNotify(DWORD sas)
		{
			((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxSasNotify(m_hWlx, sas);
		}

		bool RealWinlogonInterface::WlxSetTimeout(DWORD newTimeout)
		{
			return ((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxSetTimeout(m_hWlx, newTimeout) ? true : false;
		}

		int  RealWinlogonInterface::WlxAssignShellProtection(HANDLE token, HANDLE process, HANDLE thread)
		{
			return ((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxAssignShellProtection(m_hWlx, token, process, thread);
		}

		int  RealWinlogonInterface::WlxMessageBox(HWND owner, LPWSTR text, LPWSTR title, UINT style)
		{
			return ((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxMessageBox(m_hWlx, owner, text, title, style);
		}

		int  RealWinlogonInterface::WlxDialogBox(HANDLE hInst, LPWSTR lpszTemplate, HWND hwndOwner, DLGPROC dlgprc)
		{
			return ((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxDialogBox(m_hWlx, hInst, lpszTemplate, hwndOwner, dlgprc);
		}

		int  RealWinlogonInterface::WlxDialogBoxParam(HANDLE hInst, LPWSTR lpszTemplate, HWND hwndOwner, DLGPROC dlgprc, LPARAM dwInitParam)
		{
			return ((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxDialogBoxParam(m_hWlx, hInst, lpszTemplate, hwndOwner, dlgprc, dwInitParam);
		}

		int  RealWinlogonInterface::WlxDialogBoxIndirect(HANDLE hInst, LPCDLGTEMPLATE hDialogTemplate, HWND hwndOwner, DLGPROC dlgprc)
		{
			return ((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxDialogBoxIndirect(m_hWlx, hInst, hDialogTemplate, hwndOwner, dlgprc);
		}

		int  RealWinlogonInterface::WlxDialogBoxIndirectParam(HANDLE hInst, LPCDLGTEMPLATE hDialogTemplate, HWND hwndOwner, DLGPROC dlgprc, LPARAM dwInitParam)
		{
			return ((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxDialogBoxIndirectParam(m_hWlx, hInst, hDialogTemplate, hwndOwner, dlgprc, dwInitParam);
		}

		int  RealWinlogonInterface::WlxSwitchDesktopToUser()
		{
			return ((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxSwitchDesktopToUser(m_hWlx);
		}

		int  RealWinlogonInterface::WlxSwitchDesktopToWinlogon()
		{
			return ((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxSwitchDesktopToWinlogon(m_hWlx);
		}

		int  RealWinlogonInterface::WlxChangePasswordNotify(PWLX_MPR_NOTIFY_INFO pMprInfo, DWORD dwChangeInfo)
		{
			return ((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxChangePasswordNotify(m_hWlx, pMprInfo, dwChangeInfo);
		}

		bool RealWinlogonInterface::WlxGetSourceDesktop(PWLX_DESKTOP *ppDesktop)
		{
			return ((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxGetSourceDesktop(m_hWlx, ppDesktop) ? true : false;
		}

		bool RealWinlogonInterface::WlxSetReturnDesktop(PWLX_DESKTOP pDesktop)
		{
			return ((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxSetReturnDesktop(m_hWlx, pDesktop) ? true : false;
		}

		bool RealWinlogonInterface::WlxCreateUserDesktop(HANDLE hToken, DWORD Flags, PWSTR pszDesktopName, PWLX_DESKTOP *ppDesktop)
		{
			return ((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxCreateUserDesktop(m_hWlx, hToken, Flags, pszDesktopName, ppDesktop) ? true : false;
		}

		int  RealWinlogonInterface::WlxChangePasswordNotifyEx(PWLX_MPR_NOTIFY_INFO pMprInfo, DWORD dwChangeInfo, PWSTR ProviderName, PVOID Reserved)
		{
			return ((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxChangePasswordNotifyEx(m_hWlx, pMprInfo, dwChangeInfo, ProviderName, Reserved);
		}

		bool RealWinlogonInterface::WlxCloseUserDesktop(PWLX_DESKTOP pDesktop, HANDLE hToken)
		{
			return ((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxCloseUserDesktop(m_hWlx, pDesktop, hToken) ? true : false;
		}

		bool RealWinlogonInterface::WlxSetOption(DWORD Option, ULONG_PTR Value, ULONG_PTR *OldValue)
		{
			return ((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxSetOption(m_hWlx, Option, Value, OldValue) ? true : false;
		}

		bool RealWinlogonInterface::WlxGetOption(DWORD Option, ULONG_PTR *Value)
		{
			return ((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxGetOption(m_hWlx, Option, Value) ? true : false;
		}

		void RealWinlogonInterface::WlxWin31Migrate()
		{
			((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxWin31Migrate(m_hWlx);
		}

		bool RealWinlogonInterface::WlxQueryClientCredentials(PWLX_CLIENT_CREDENTIALS_INFO_V1_0 pCred)
		{
			return ((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxQueryClientCredentials(pCred) ? true : false;
		}

		bool RealWinlogonInterface::WlxQueryInetConnectorCredentials(PWLX_CLIENT_CREDENTIALS_INFO_V1_0 pCred)
		{
			return ((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxQueryInetConnectorCredentials(pCred) ? true : false;
		}

		bool RealWinlogonInterface::WlxDisconnect()
		{
			return ((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxDisconnect() ? true : false;
		}

		int  RealWinlogonInterface::WlxQueryTerminalServicesData(PWLX_TERMINAL_SERVICES_DATA pTSData, WCHAR *UserName, WCHAR *Domain)
		{
			return ((PWLX_DISPATCH_VERSION_1_3)m_pFuncTable)->WlxQueryTerminalServicesData(m_hWlx, pTSData, UserName, Domain);
		}

		int  RealWinlogonInterface::WlxQueryConsoleSwitchCredentials(PWLX_CONSOLESWITCH_CREDENTIALS_INFO_V1_0 pCred)
		{
			// Must be at 1_4 for this call
			if(WinlogonInterface::Version() <= WLX_VERSION_1_3)
				return 0;
			
			return ((PWLX_DISPATCH_VERSION_1_4)m_pFuncTable)->WlxQueryConsoleSwitchCredentials(pCred);
		}

		bool RealWinlogonInterface::WlxQueryTsLogonCredentials(PWLX_CLIENT_CREDENTIALS_INFO_V2_0 pCred)
		{
			// Must be at 1_4 for this call
			if(WinlogonInterface::Version() <= WLX_VERSION_1_3)
				return 0;

			return ((PWLX_DISPATCH_VERSION_1_4)m_pFuncTable)->WlxQueryTsLogonCredentials(pCred) ? true : false;
		}
	}
}