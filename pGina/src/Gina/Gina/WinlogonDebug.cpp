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

#include "WinlogonDebug.h"

namespace pGina
{
	namespace GINA
	{
		void DebugWinlogonInterface::WlxUseCtrlAltDel() 
		{
		}

		void DebugWinlogonInterface::WlxSetContextPointer(void *newContext)
		{
		}

		void DebugWinlogonInterface::WlxSasNotify(DWORD sas)
		{
		}

		bool DebugWinlogonInterface::WlxSetTimeout(DWORD newTimeout)
		{
			return true;
		}

		int  DebugWinlogonInterface::WlxAssignShellProtection(HANDLE token, HANDLE process, HANDLE thread)
		{
			return 0;
		}

		int  DebugWinlogonInterface::WlxMessageBox(HWND owner, LPWSTR text, LPWSTR title, UINT style)
		{
			return MessageBox(owner, text, title, style);
		}

		int  DebugWinlogonInterface::WlxDialogBox(HANDLE hInst, LPWSTR lpszTemplate, HWND hwndOwner, DLGPROC dlgprc)
		{
			return (int) DialogBox((HINSTANCE) hInst, lpszTemplate, hwndOwner, dlgprc);
		}

		int  DebugWinlogonInterface::WlxDialogBoxParam(HANDLE hInst, LPWSTR lpszTemplate, HWND hwndOwner, DLGPROC dlgprc, LPARAM dwInitParam)
		{
			return (int) DialogBoxParam((HINSTANCE) hInst, lpszTemplate, hwndOwner, dlgprc, dwInitParam);
		}

		int  DebugWinlogonInterface::WlxDialogBoxIndirect(HANDLE hInst, LPCDLGTEMPLATE hDialogTemplate, HWND hwndOwner, DLGPROC dlgprc)
		{
			return (int) DialogBoxIndirect((HINSTANCE) hInst, hDialogTemplate, hwndOwner, dlgprc);
		}

		int  DebugWinlogonInterface::WlxDialogBoxIndirectParam(HANDLE hInst, LPCDLGTEMPLATE hDialogTemplate, HWND hwndOwner, DLGPROC dlgprc, LPARAM dwInitParam)
		{
			return (int) DialogBoxIndirectParam((HINSTANCE) hInst, hDialogTemplate, hwndOwner, dlgprc, dwInitParam);
		}

		int  DebugWinlogonInterface::WlxSwitchDesktopToUser()
		{
			return 0;
		}

		int  DebugWinlogonInterface::WlxSwitchDesktopToWinlogon()
		{
			return 0;
		}

		int  DebugWinlogonInterface::WlxChangePasswordNotify(PWLX_MPR_NOTIFY_INFO pMprInfo, DWORD dwChangeInfo)
		{
			return 0;
		}

		bool DebugWinlogonInterface::WlxGetSourceDesktop(PWLX_DESKTOP *ppDesktop)
		{
			return false;
		}

		bool DebugWinlogonInterface::WlxSetReturnDesktop(PWLX_DESKTOP pDesktop)
		{
			return true;
		}

		bool DebugWinlogonInterface::WlxCreateUserDesktop(HANDLE hToken, DWORD Flags, PWSTR pszDesktopName, PWLX_DESKTOP *ppDesktop)
		{
			return false;
		}

		int  DebugWinlogonInterface::WlxChangePasswordNotifyEx(PWLX_MPR_NOTIFY_INFO pMprInfo, DWORD dwChangeInfo, PWSTR ProviderName, PVOID Reserved)
		{
			return 0;
		}

		bool DebugWinlogonInterface::WlxCloseUserDesktop(PWLX_DESKTOP pDesktop, HANDLE hToken)
		{
			return false;
		}

		bool DebugWinlogonInterface::WlxSetOption(DWORD Option, ULONG_PTR Value, ULONG_PTR *OldValue)
		{
			return true;
		}

		bool DebugWinlogonInterface::WlxGetOption(DWORD Option, ULONG_PTR *Value)
		{
			return false;
		}

		void DebugWinlogonInterface::WlxWin31Migrate()
		{
		}

		bool DebugWinlogonInterface::WlxQueryClientCredentials(PWLX_CLIENT_CREDENTIALS_INFO_V1_0 pCred)
		{
			return false;
		}

		bool DebugWinlogonInterface::WlxQueryInetConnectorCredentials(PWLX_CLIENT_CREDENTIALS_INFO_V1_0 pCred)
		{
			return false;
		}

		bool DebugWinlogonInterface::WlxDisconnect()
		{
			return false;
		}

		int  DebugWinlogonInterface::WlxQueryTerminalServicesData(PWLX_TERMINAL_SERVICES_DATA pTSData, WCHAR *UserName, WCHAR *Domain)
		{
			return 0;
		}

		int  DebugWinlogonInterface::WlxQueryConsoleSwitchCredentials(PWLX_CONSOLESWITCH_CREDENTIALS_INFO_V1_0 pCred)
		{
			return 0;
		}

		bool DebugWinlogonInterface::WlxQueryTsLogonCredentials(PWLX_CLIENT_CREDENTIALS_INFO_V2_0 pCred)
		{
			return false;
		}
	}
}